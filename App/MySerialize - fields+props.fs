module MySerialize

open System
open System.Collections.Generic
open System.Reflection
open System.Xml
open System.Xml.Linq   
open System.ComponentModel

let xname thing = XName.Get( thing.ToString() )
            
let private excludeNames = 
    []
    //("SerialPort", "BreakState"::[])::[]

let private isNotExcludeName (t:Type) name =  
    excludeNames |> 
    List.tryFind ( fun (typeName,names) ->  
        typeName=t.Name &&
        names |> List.tryFind( fun name' -> name'=name  ) |> Option.isSome )  
    |> Option.isNone

let private strT' = typeof<string>

let private getConv = TypeDescriptor.GetConverter

let private canStrConv (typeConverter:TypeConverter) =
    typeConverter.CanConvertTo(strT') && typeConverter.CanConvertFrom(strT')            

let private isStrConv t = t |> getConv |> canStrConv 

let private isStruct (t:Type) = t.IsValueType && (not t.IsEnum)

let private getProps (t:Type) =     
    t.GetProperties() |> Array.toList |> List.filter( fun p ->         
        p.CanRead && ( p.CanWrite || not (isStruct p.PropertyType) ) && p.GetMethod.GetParameters().Length=0 && isNotExcludeName t p.Name)

let private getFields (t:Type) =   
    let fields = t.GetFields() |> Array.toList  
    fields |> List.filter( fun f -> f.IsPublic && (isNotExcludeName t f.Name) )

let private celem thing  = fun (x:'a) -> new XElement( xname thing, x)

let private isXEmpt (x:XElement) = not <| ( x.HasAttributes || x.HasElements )


let serialize (item:obj) (name:string) =     
    let getV obj (prop:PropertyInfo) = try Some( prop.GetValue(obj) ) with _ -> None
    let rec serializeProperty item (p:PropertyInfo) = 
        [|  let tpConv = getConv p.PropertyType
            match getV item p with // если можно прочитать значение свойства без эксепшенов
            | None -> ()  
            | Some(propVal) ->
            if p.CanWrite && tpConv.CanConvertTo strT' then
                yield XAttribute( xname p.Name, tpConv.ConvertToString(propVal) ) :> XObject
            if ( not <| canStrConv tpConv ) && p.PropertyType.IsClass then 
                yield! xmembers propVal p.Name |]
    and serializeField item (f:FieldInfo) = 
        [|  let tpConv = getConv f.FieldType
            let v = f.GetValue(item)
            if tpConv.CanConvertTo strT' then
                yield XAttribute( xname f.Name, tpConv.ConvertToString  v ) :> XObject
            else   
                yield! xmembers v f.Name |]
    and xprops item = [| for p in getProps (item.GetType() ) do yield! serializeProperty item p  |] 
    and xfields item = [| for f in  getFields (item.GetType() ) do yield! serializeField item f  |]     
    and xmembers' v = Array.append (xprops v) (xfields v)
    and xmembers v name = 
        let addx (e:XElement) = [| if e.HasAttributes || e.HasElements then yield e :> XObject |]
        [| yield! addx <|celem name ( xmembers' v ) |]
    //and serializeProperties (item:obj) name  : XElement = celem name (xprops item)
    //and serializeFields (item:obj) name = celem name (xfields item) 

    celem name (xmembers' item)

let private (|XAttrVal|_|) (e:XElement) name = 
            match [ for atr in e.Attributes() do yield atr ] |> List.tryFind( fun atr -> atr.Name=xname name ) with
            | None -> None
            | Some(atr) -> Some( atr.Value )

let private (|XChildElemByName|_|) (e:XElement) name = 
            match [ for e in e.Elements() do yield e ] |> List.tryFind( fun e -> e.Name=xname name ) with
            | None -> None
            | Some(e) -> Some(e)


let deserialize (e:XElement) ob =
    let rec deserializeProperty e ob (prop:PropertyInfo)=
        let tpConv = getConv prop.PropertyType
        match prop.Name with 
        | XAttrVal e val' when prop.CanWrite && canStrConv tpConv -> 
            prop.SetValue(ob, tpConv.ConvertFromString val') 
        | XChildElemByName e e -> deserializeMembers e ( prop.GetValue( ob ) )
        | _ -> ()
    and deserializeField e ob (field:FieldInfo)=
        let tpConv = getConv field.FieldType
        match field.Name with 
        | XAttrVal e val' when canStrConv tpConv && not <| field.IsLiteral -> 
            field.SetValue(ob, tpConv.ConvertFromString val')
        | XChildElemByName e e -> deserializeMembers e ( field.GetValue( ob ) )
        | _ -> ()
    and deserializeMembers e ob =  
        let props = getProps (ob.GetType())
        for p in props do
            deserializeProperty e ob p
        let fields = getFields (ob.GetType())
        for field in fields do
            deserializeField e ob field

    deserializeMembers e ob




 