module my

open System
open System.ComponentModel
open System.Threading
open Microsoft.FSharp.Reflection   
open System.Runtime.InteropServices
open System.Text.RegularExpressions
open System.Globalization

let safelyWinForm (ctrl:System.Windows.Forms.Control) func =
    if ctrl.InvokeRequired then 
        ctrl.Invoke <| new  System.Windows.Forms.MethodInvoker( func ) |> ignore
    else func()

module simpleProperties = 
    open System.Windows.Forms
    open System.Drawing

    let dialog objt title = 
        let dlg = new Form( StartPosition=FormStartPosition.CenterScreen, 
                            Width=500,  Height=600, Text=title,
                            FormBorderStyle=FormBorderStyle.SizableToolWindow,
                            Font = new Font("Tahoma", 10.f),
                            ShowInTaskbar = false )
        
        let panel = new Panel( Dock=DockStyle.Right, Text="", Width=105, 
                                BorderStyle=BorderStyle.None, Parent=dlg  ) 
        panel.SendToBack() 
        dlg.AcceptButton <- new Button(Text="Принять", Left=10, Width=90, Height=40, Top=5, Parent=panel, DialogResult=DialogResult.OK) 
        dlg.CancelButton <- new Button(Text="Отмена", Left=10, Width=90, Height=40, Top=55, Parent=panel, DialogResult=DialogResult.Cancel)
            
        let grd = new PropertyGrid( Parent=dlg, Dock=Windows.Forms.DockStyle.Fill, SelectedObject=objt )
        grd.BringToFront()
        dlg.ShowDialog() |> ignore

let getExePath = System.IO.Path.GetDirectoryName( System.Windows.Forms.Application.ExecutablePath )

let forceDirectories path =
    let path = IO.Path.GetDirectoryName path
    if IO.Directory.Exists path |> not then IO.Directory.CreateDirectory path |> ignore

let formatList collection delimString conv = 
    collection |> Seq.fold( fun acc x ->
        acc + (if acc |> String.IsNullOrEmpty then acc else delimString) + (conv x) ) ""

let setPropertyValue target propName value = 
    let t = target.GetType()
    let propInfo = t.GetProperty(propName)
    if propInfo=null then
        sprintf "Упс! Свойство %s не найдено в типе %s" propName (t.Name) |> failwith
    else
        propInfo.SetValue(target, value,null)

let getPropertyValue target propName = 
    let t = target.GetType()
    let propInfo = t.GetProperty(propName)
    if propInfo=null then
        sprintf "Упс! Свойство %s не найдено в типе %s" propName (t.Name) |> failwith
    else
        propInfo.GetValue(target, null)

let укоротитьСтроку (s:string) maxLen s1 = 
    let len = s.Length
    if len < maxLen then s else s1+s.Substring(len-maxLen)

let укоротитьПуть (s:string) =       
    (укоротитьСтроку (IO.Path.GetDirectoryName s) 15 @"...\")+"\\"+(IO.Path.GetFileName s)

let relativePath path0 path = 
    let pat = sprintf "^%s([^$]+)$" ( Text.RegularExpressions.Regex.Escape path0 )
    let m = Text.RegularExpressions.Regex.Match( path, pat)
    if m.Success then m.Groups.[1].Value else path

///Returns the case names of union type 'ty.
let unionToStrings<'ty>() =    
    FSharpType.GetUnionCases(typeof<'ty>) |> Array.map (fun info -> info.Name)

let unionToString (x:'a) = 
    match FSharpValue.GetUnionFields(x, typeof<'a>) with
    | case, _ -> case.Name

let parseUnion<'T> s =
    match FSharpType.GetUnionCases typeof<'T>  |> Array.filter (fun case -> case.Name = s) with
    | [|case|] -> Some( FSharpValue.MakeUnion(case,[||]) :?> 'T ) 
    |_ -> None

let unionCases<'T> =
    FSharpType.GetUnionCases typeof<'T> |> 
    Array.toList |> 
    List.map( fun case -> FSharpValue.MakeUnion(case,[||]) :?> 'T )

let parseUnionDef<'T> s =
    let type'of ty ob =  ob.GetType() = ty
    match parseUnion<'T> s with 
    | Some(v) -> v
    | None ->   let cases = FSharpType.GetUnionCases typeof<'T>
                FSharpValue.MakeUnion(cases.[0],[||]) :?> 'T


let getUnionCaseAttribute<'T> x = 
    let case,_ = FSharpValue.GetUnionFields(x, x.GetType() )
    case.GetCustomAttributes() |> 
    Seq.tryFind( fun e -> e.GetType()=typeof< 'T > ) |> 
    Option.map( fun atr -> atr :?> 'T )

let getUnionCaseDescription x = match getUnionCaseAttribute<DescriptionAttribute> x with None -> "" | Some(d) -> d.Description

let unionCaseAttributeValue<'T> x = 
    match getUnionCaseAttribute<'T> x with 
    | None -> sprintf "Упс! %s Gas.Code" (getUnionCaseDescription x) |> failwith  
    | Some(d) -> d



let getEnumDescription value = 
    let st = typeof<DescriptionAttribute>
    let attributes = value.GetType().GetField(value.ToString()).GetCustomAttributes( typeof<DescriptionAttribute>, false)
    let da = 
        attributes
        |> Seq.tryFind( fun e -> e.GetType()=typeof<DescriptionAttribute> ) 
        |> Option.map( fun atr -> atr :?> DescriptionAttribute )
    match da with None -> "" | Some(d) -> d.Description

let exn (exn:System.Exception) = 
    let exn = 
        match exn with  
        | :?  System.AggregateException as exn -> exn.InnerException
        | _ as exn -> exn
    sprintf "%s, %O, %s" exn.Message (exn.GetType()) exn.StackTrace

let milliseconds (dttm:System.DateTime) = 
    let r = dttm.Millisecond + 1000 * ( dttm.Second + 60*( dttm.Minute + 60*dttm.Hour ) )
    r
    

module Hex =

    let digToHex n =
        if n < 10 then char (n + 0x30) else char (n + 0x37)
    
    let hexToDig c =
        if c >= '0' && c <= '9' then Some(int c - int '0')
        elif c >= 'A' && c <= 'F' then Some( (int c - int 'A') + 10 )
        elif c >= 'a' && c <= 'f' then Some( (int c - int 'a') + 10 )
        else None
     
    let encode (buf:byte array) (prefix:bool) =
        let hex = Array.zeroCreate (buf.Length * 2)
        let mutable n = 0
        for i = 0 to buf.Length - 1 do
            hex.[n] <- digToHex ((int buf.[i] &&& 0xF0) >>> 4)
            n <- n + 1
            hex.[n] <- digToHex (int buf.[i] &&& 0xF)
            n <- n + 1
        if prefix then String.Concat("0x", new String(hex)) 
        else new String(hex)
    
    let tryParse (s:string) =
        if String.IsNullOrEmpty s then None else        
        let rec hexx acc (s:string) = 
            let len = s.Length
            if len=0 then acc else
            match hexToDig s.[0], acc with                 
            | Some(v), Some(acc) ->  hexx ( Some( (acc <<< 4) + v ) ) (s.Substring(1, len-1))
            | _ -> None
        let s = let len = s.Length
                if len >= 2 && s.[0]='0' && (s.[1]='x' || s.[1] = 'X') then  (s.Substring(2, len-2)) else s
        match hexx (Some(0)) s with
        | Some(v) -> Some( int16 v )
        | _ -> None
            
            
let private excludePropertiesList = new Collections.Generic.List<Type*string>()
let addExcludeProperty typeName propName = 
    excludePropertiesList.Add(typeName,propName)

let formatProperties (record:obj) = 
    
    let sdelim s delim = s + (if s |> String.IsNullOrEmpty |> not then delim else "")    
    let rec sarr (ob:obj) = 
        [| for i in (ob :?> Array) -> i |] 
        |> Array.fold ( fun acc s ->  sprintf "%s%s" (sdelim acc "; ") (formatval s) )  ""        
    and formatval ob = 
        if ob=null then "null" else
        let pit = ob.GetType()
        
        if pit.IsPrimitive then
            sprintf "%O" ob
        elif FSharpType.IsTuple pit then             
            FSharpValue.GetTupleFields ob |> sarr |> sprintf "(%s)"
        elif FSharpType.IsUnion pit then 
            let case, vals =  FSharpValue.GetUnionFields(ob, pit) 
            if vals |> Array.isEmpty then case.Name else sprintf "%s(%s)" case.Name ( sarr vals  ) 
        elif pit.IsArray then 
            sarr ob |> sprintf "[%s]"
        elif pit=typeof<string> then
            sprintf "\"%O\"" ob        
        elif pit.IsClass then
            pit.GetProperties() 
            |> Array.filter ( fun pi -> 
                pi.CanRead && 
                (excludePropertiesList.Exists( fun (tn,pn) -> tn=pit && pn=pi.Name ) |> not) )
            |> Array.fold ( fun acc pi ->   
                let s = 
                    match (try  Some( pi.GetValue(ob, null) ) with _ -> None ) with
                    | Some(null) -> "null"
                    | Some(v) -> formatval v
                    | _ -> "<...>"
                sprintf "%s%s=%O" (sdelim acc "; ") pi.Name s )  ""
            |> sprintf "{%s}"
        else sprintf "%s:%O" pit.Name ob 
    formatval record


module Conv = 
    type UnionNamesConverter<'T > () =                
        inherit  System.ComponentModel.StringConverter()
        let T = typeof<'T>
        let S = typeof<string>        

        override this.GetStandardValuesSupported (context:ITypeDescriptorContext) = true
        override this.GetStandardValuesExclusive (context:ITypeDescriptorContext) = true
        override this.GetStandardValues (context:ITypeDescriptorContext) =
            new TypeConverter.StandardValuesCollection( unionToStrings<'T>() )
        override this.CanConvertTo(ctx:ITypeDescriptorContext, t:Type) = 
            if t=S || t=T then true else false
        override this.CanConvertFrom(ctx:ITypeDescriptorContext, t:Type) = 
            if t=S || t=T then true else false
        override this.ConvertFrom(ctx:ITypeDescriptorContext, ci:System.Globalization.CultureInfo, value:obj) : obj = 
            parseUnionDef<'T> ( value.ToString() ) :> obj        
        override this.ConvertTo(ctx:ITypeDescriptorContext, ci:System.Globalization.CultureInfo, 
                                value:obj, destinationType:Type ) : obj =             
            match value with
            | :? string as s when destinationType=T -> parseUnionDef<'T>  s :> obj
            | :? string as s when destinationType=S ->  s :> obj
            | :? 'T as t when destinationType=S -> unionToString t :> obj
            | :? 'T as t when destinationType=T -> t :> obj
            | _ -> base.ConvertTo(ctx, ci, value, destinationType)

    [<AbstractClass>]
    type КовертерСписка() = 
        inherit  System.ComponentModel.StringConverter()
        override this.GetStandardValuesSupported (context:ITypeDescriptorContext) = true
        override this.GetStandardValuesExclusive (context:ITypeDescriptorContext) = true
        override this.GetStandardValues (context:ITypeDescriptorContext) =         
            new TypeConverter.StandardValuesCollection( this.GetList() )
        abstract GetList : unit->string array

    type ComPortNamesConverter() = 
        inherit КовертерСписка()        
        override this.GetList () = 
            System.IO.Ports.SerialPort.GetPortNames() |> Seq.cast |> Seq.toArray

let tryParseFloat (s:String) = Regex.Replace( s, "[,\\.]", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) |> Double.TryParse


let withCanceletion (ct:CancellationToken) canceled' fun' = 
    
    while not <| ( ct.IsCancellationRequested || canceled'() ) do fun'()

let withCanceletionTime (ct:CancellationToken) t canceled'  = 
    let t' = System.Environment.TickCount
    withCanceletion ct ( fun () -> (System.Environment.TickCount - t'>=t) || canceled'() ) 

let inline nop() = System.Threading.Thread.Sleep 1

let delayms (ct:CancellationToken) t =     
    withCanceletionTime ct t ( fun() -> false ) nop 

let perform'timed ct t = withCanceletionTime ct t ( fun () -> false ) 

//let crc16 bytes = 
//    Seq.fold  ( fun acc (b:byte) ->
//        let acc = acc ^^^ ( uint16 b )
//        let bit = (acc &&& 0x0001us) <> 0us
//        let acc = (acc >>> 1) &&& 0x7FFFus
//        if bit then acc ^^^ 0xA001us else acc) 0xFFFFus bytes 

let private auchCRCHi = 
    [|  0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy;
        0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy;
        0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy;
        0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy;
        0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy;
        0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy;
        0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy;
        0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy;
        0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy;
        0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy;
        0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy;
        0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy;
        0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy;
        0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy;
        0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy;
        0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy;
        0x00uy; 0xC1uy; 0x81uy; 0x40uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x01uy; 0xC0uy; 0x80uy; 0x41uy; 0x00uy; 0xC1uy; 0x81uy;
        0x40uy |]

let private auchCRCLo = 
    [|  0x00uy; 0xC0uy; 0xC1uy; 0x01uy; 0xC3uy; 0x03uy; 0x02uy; 0xC2uy; 0xC6uy; 0x06uy; 0x07uy; 0xC7uy; 0x05uy; 0xC5uy; 0xC4uy;
        0x04uy; 0xCCuy; 0x0Cuy; 0x0Duy; 0xCDuy; 0x0Fuy; 0xCFuy; 0xCEuy; 0x0Euy; 0x0Auy; 0xCAuy; 0xCBuy; 0x0Buy; 0xC9uy; 0x09uy;
        0x08uy; 0xC8uy; 0xD8uy; 0x18uy; 0x19uy; 0xD9uy; 0x1Buy; 0xDBuy; 0xDAuy; 0x1Auy; 0x1Euy; 0xDEuy; 0xDFuy; 0x1Fuy; 0xDDuy;
        0x1Duy; 0x1Cuy; 0xDCuy; 0x14uy; 0xD4uy; 0xD5uy; 0x15uy; 0xD7uy; 0x17uy; 0x16uy; 0xD6uy; 0xD2uy; 0x12uy; 0x13uy; 0xD3uy;
        0x11uy; 0xD1uy; 0xD0uy; 0x10uy; 0xF0uy; 0x30uy; 0x31uy; 0xF1uy; 0x33uy; 0xF3uy; 0xF2uy; 0x32uy; 0x36uy; 0xF6uy; 0xF7uy;
        0x37uy; 0xF5uy; 0x35uy; 0x34uy; 0xF4uy; 0x3Cuy; 0xFCuy; 0xFDuy; 0x3Duy; 0xFFuy; 0x3Fuy; 0x3Euy; 0xFEuy; 0xFAuy; 0x3Auy;
        0x3Buy; 0xFBuy; 0x39uy; 0xF9uy; 0xF8uy; 0x38uy; 0x28uy; 0xE8uy; 0xE9uy; 0x29uy; 0xEBuy; 0x2Buy; 0x2Auy; 0xEAuy; 0xEEuy;
        0x2Euy; 0x2Fuy; 0xEFuy; 0x2Duy; 0xEDuy; 0xECuy; 0x2Cuy; 0xE4uy; 0x24uy; 0x25uy; 0xE5uy; 0x27uy; 0xE7uy; 0xE6uy; 0x26uy;
        0x22uy; 0xE2uy; 0xE3uy; 0x23uy; 0xE1uy; 0x21uy; 0x20uy; 0xE0uy; 0xA0uy; 0x60uy; 0x61uy; 0xA1uy; 0x63uy; 0xA3uy; 0xA2uy;
        0x62uy; 0x66uy; 0xA6uy; 0xA7uy; 0x67uy; 0xA5uy; 0x65uy; 0x64uy; 0xA4uy; 0x6Cuy; 0xACuy; 0xADuy; 0x6Duy; 0xAFuy; 0x6Fuy;
        0x6Euy; 0xAEuy; 0xAAuy; 0x6Auy; 0x6Buy; 0xABuy; 0x69uy; 0xA9uy; 0xA8uy; 0x68uy; 0x78uy; 0xB8uy; 0xB9uy; 0x79uy; 0xBBuy;
        0x7Buy; 0x7Auy; 0xBAuy; 0xBEuy; 0x7Euy; 0x7Fuy; 0xBFuy; 0x7Duy; 0xBDuy; 0xBCuy; 0x7Cuy; 0xB4uy; 0x74uy; 0x75uy; 0xB5uy;
        0x77uy; 0xB7uy; 0xB6uy; 0x76uy; 0x72uy; 0xB2uy; 0xB3uy; 0x73uy; 0xB1uy; 0x71uy; 0x70uy; 0xB0uy; 0x50uy; 0x90uy; 0x91uy;
        0x51uy; 0x93uy; 0x53uy; 0x52uy; 0x92uy; 0x96uy; 0x56uy; 0x57uy; 0x97uy; 0x55uy; 0x95uy; 0x94uy; 0x54uy; 0x9Cuy; 0x5Cuy;
        0x5Duy; 0x9Duy; 0x5Fuy; 0x9Fuy; 0x9Euy; 0x5Euy; 0x5Auy; 0x9Auy; 0x9Buy; 0x5Buy; 0x99uy; 0x59uy; 0x58uy; 0x98uy; 0x88uy;
        0x48uy; 0x49uy; 0x89uy; 0x4Buy; 0x8Buy; 0x8Auy; 0x4Auy; 0x4Euy; 0x8Euy; 0x8Fuy; 0x4Fuy; 0x8Duy; 0x4Duy; 0x4Cuy; 0x8Cuy;
        0x44uy; 0x84uy; 0x85uy; 0x45uy; 0x87uy; 0x47uy; 0x46uy; 0x86uy; 0x82uy; 0x42uy; 0x43uy; 0x83uy; 0x41uy; 0x81uy; 0x80uy;
        0x40uy |]

let crc16 bytes = 
    let hi,lo = Seq.fold  ( fun (hi,uchCRCLo) (b:byte) ->
                    let i = int (hi ^^^ b)
                    let hi = uchCRCLo ^^^ auchCRCHi.[i]
                    let lo = auchCRCLo.[i]
                    (hi,lo) ) (0xFFuy,0xFFuy) bytes 
    (uint16 hi<<<8)+(uint16 lo)

let addcrc16 bytes = 
    let u = crc16 bytes
    Seq.append bytes [| byte(u >>> 8); byte(u) |]

let inline pow b e =
    let rec loop acc = function
                        | e when e < LanguagePrimitives.GenericOne<_> -> acc
                        | e -> loop (b*acc) (e-LanguagePrimitives.GenericOne<_>)
    loop LanguagePrimitives.GenericOne e

let bytesToStrs bytes =      
    Seq.fold ( fun (acc,i) b ->
        let s = sprintf "%s%X" (if b<0x10uy then "0" else "") b
        if i%16=0 then (s::acc, i+1) else                     
        match acc with 
        | [] -> ( s::[], i+1) 
        | s'::[] -> ((s'+" "+s)::[], i+1)
        | s'::acc  -> ((s'+" "+s)::acc, i+1)) ( [], 0 ) bytes |> fst |> List.rev

let bytesToStr bytes = Seq.fold ( fun acc s -> if acc="" then s else acc + " "+s) "" (bytesToStrs bytes)

let (|Bcd6Float|_|) (bcd:byte seq) = 
    let (|B|) (b:byte) = double(b >>> 4), double(b &&& 0x0Fuy)
    let (|D|_|) (B (b1,b2)) = if b1<10. && b2<10. then Some(b1,b2) else None
    let (|SC|_|) (B (sign,coma)) = if coma<7. then Some( (if sign=0. then 1. else -1.), double(coma) ) else None
    match Seq.toList bcd with 
    | SC(sign,coma)::D(d'100'000,d10'000)::D(d1000,d100)::D(d10,d1)::_ ->
        Some( sign*(d'100'000*100000. + d10'000*10000. + d1000*1000. + d100*100. + d10*10. + d1 )/( pow 10. coma ) ) 
    | _ -> None

let bcd6Float  = function (Bcd6Float v) -> Some( v ) | _ -> None

let floatBcd6 (value:double) =

    let comapos =
        let v = abs value
        let vin v1 v2 = v>=v1 && v<v2
        if vin 0. 1. then 6
        elif vin 1. 10. then 5
        elif vin 10. 100. then 4
        elif vin 100. 1000. then 3
        elif vin 1000. 10000. then 2
        elif vin 10000. 100000. then 1
        else 0
    let v =         
        let rec cv v = if v<100000. then v*10. |> cv else v
        let vv v = v*1000000.
        let absv = value |> abs
        absv |> ( if absv<1. then vv else cv) |> round |> int
    
    let b8 = ( (if value<0. then 1uy else 0uy ) <<< 3 ) <<< 4
    let b7 = byte comapos
           
    let b6 = byte( v/100000 ) <<< 4
    let v = v % 100000

    let b5 = byte ( v/10000 )
    let v = v % 10000

    let b4 = byte( v/1000 )  <<< 4
    let v = v % 1000

    let b3 = byte( v/100 )
    let v = v % 100

    let b2 = byte( v/10 )  <<< 4
    let v = v % 10

    let b1 = byte(v)

    [| b8+b7; b6+b5; b4+b3; b2+b1 |]

let casync f =  
    let dlgt = new System.Func<'a> ( f )
    Async.FromBeginEnd(dlgt.BeginInvoke, dlgt.EndInvoke)

module TreeList = 
    open System.Windows.Forms
    type TreeList = DevExpress.XtraTreeList.TreeList
    type Node = DevExpress.XtraTreeList.Nodes.TreeListNode
    type Column = DevExpress.XtraTreeList.Columns.TreeListColumn

    let calcNodeRange (nd0:Node) (nd1:Node) = 
        let rec loop (nd:Node) acc = 
            if nd=null then acc else 
                if nd=nd1 then nd1::acc else loop nd.NextVisibleNode (nd::acc)
        let nodes = loop nd0 [] |> List.rev
        match nodes |> List.tryFind( fun nd -> nd=nd1 ) with
        | Some(_) -> nodes
        | _ -> []

    let clear (nodes:Node list) (columns:Column list) = 
        for node in nodes do
            for column in columns do
                if column.OptionsColumn.AllowEdit then
                    node.SetValue( column, null )

    let copy nodes columns = 
        nodes
        |> List.map ( fun (node:Node) ->
            columns
            |> List.fold ( fun acc (column:Column) ->  
                let v = node.GetValue column
                let s = if v=null then "" else sprintf "%O" v
                if acc=null then s else sprintf "%s\t%s" acc s )  null )
        |> List.fold ( fun acc s ->
            if acc=null then s else sprintf "%s\r\n%s" acc s )  null 
        |> fun s -> if s |> String.IsNullOrEmpty |> not then Clipboard.SetText s

    let past (treeList:TreeList) = 
        
        let setnode (node:Node) (line:string) =          
            let items = line.Split('\t')
            let len = items.Length
            let col0 = treeList.FocusedColumn
            if col0<>null && len>0 then
                let nfocused = treeList.FocusedColumn.AbsoluteIndex
                let rec loop n = 
                    let coln = nfocused+n
                    if n<len && coln<treeList.Columns.Count then 
                        let column = treeList.Columns.[coln]
                        if column.OptionsColumn.AllowEdit then node.SetValue( column, items.[n] )
                        loop (n+1)
                loop 0

//            for n in treeList.FocusedColumn.AbsoluteIndex..treeList.Columns.Count-1 do
//                let column = treeList.Columns.[n]
//                if column.Visible && n<items.Length then
//                    node.SetValue( column, items.[n] )           
                
        let rec loop node lines = 
            match lines with
            | line::lines when node<>null ->
                setnode node line
                loop node.NextVisibleNode lines
            | _ -> ()
            
        Clipboard.GetText().Split( [|"\r\n"|], StringSplitOptions.None)
        |> Array.toList
        |> loop treeList.FocusedNode 

        
    type TreeListAction = 
        | ActionCopy | ActionClear
        member x.Do = 
            match x with
            | ActionCopy -> copy
            | ActionClear -> clear
    type DialogState = DialogShown of Node*Column*TreeListAction | DialogHiden
    type TreeListsHashset = System.Collections.Generic.HashSet<TreeList>

    
    
    let addCopyPastMenu = 
        let state = ref DialogHiden
        let panel = new Panel(Dock=DockStyle.Bottom, Height=40, Text = "Выберите последнюю ячейку дипазаона копирования")
        let lbl = new Label(Text="Выберите последнюю ячейку дипазаона", Parent=panel, Top=5, AutoSize=true)
        let buttonCancel = new Button(  Parent=panel, Width=100, Left=50, Height = 30, Top=5, Text="Скрыть")
        buttonCancel.Anchor <- AnchorStyles.Right ||| AnchorStyles.Top
        buttonCancel.Click.AddHandler( fun _ _ -> 
                state:=DialogHiden
                panel.Hide() )

        let treelistDialog =
            let addedTreeLists = new TreeListsHashset()
            fun (treeList:TreeList) action ->
            let node0 = treeList.FocusedNode 
            if node0<>null && !state=DialogHiden then
                panel.Parent <- treeList.Parent
                panel.Show()
                match action with
                | ActionCopy -> copy (treeList.FocusedNode::[]) (treeList.FocusedColumn::[])
                | _ -> () 
                state := DialogShown( treeList.FocusedNode, treeList.FocusedColumn,action)
                if addedTreeLists.Contains treeList |> not then
                    treeList.Click.AddHandler( fun _ _ ->
                    match !state with
                    | DialogHiden -> ()
                    | DialogShown(node0,column0,action) ->
                        let nodes = calcNodeRange node0 treeList.FocusedNode
                        let columns = 
                            [ for n in column0.AbsoluteIndex..treeList.FocusedColumn.AbsoluteIndex do
                                let column = treeList.Columns.[n]
                                if column.Visible then yield column]
                        action.Do nodes columns
                        state := DialogHiden
                        panel.Hide() )

        fun (treeList:TreeList) ->             
            let menu = new Windows.Forms.ContextMenu()
            treeList.ContextMenu <- menu
            menu.MenuItems.Add("Копировать").Click.AddHandler( fun _ _ -> treelistDialog treeList ActionCopy)
            menu.MenuItems.Add("Вставить").Click.AddHandler( fun _ _ -> past treeList )
            menu.MenuItems.Add("Очистить").Click.AddHandler( fun _ _ -> treelistDialog treeList ActionClear )