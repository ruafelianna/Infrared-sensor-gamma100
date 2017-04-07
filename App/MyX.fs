module MyX

open System.Xml
open System.Xml.Linq

let xname thing = XName.Get( thing.ToString() )
let children (e:XElement) = [ for e in e.Elements() -> e ]
let child e name = (children e) |> List.tryFind( fun e -> e.Name=xname name ) 

let attrs (e:XElement) = [ for atr in e.Attributes() do yield atr ]

let attrv e name = 
    match (attrs e)|> List.tryFind( fun atr -> atr.Name=xname name ) with None -> None | Some(atr) -> Some( atr.Value )

let tryGetAttrValue e (name,func) = 
    match (attrs e)|> List.tryFind( fun atr -> atr.Name=xname name ) with 
    | None -> None 
    | Some(atr) -> 
        let b,v = func atr
        if b then Some( v ) else None





let celem thing  = fun (x:'a) -> new XElement( xname thing, x)

let empt (x:XElement) = not <| ( x.HasAttributes || x.HasElements )

let rec getElem (x:XElement) (path: string list) = 
    match path with
    | [] -> x
    | name::rest -> match child x name with
                    |  Some(elem) -> getElem elem rest
                    | _ ->  let elem = new XElement( xname name)
                            x.Add  elem
                            getElem elem rest

let getXAttr e path name = 
    let x = getElem e path
    match (attrs x)|> List.tryFind( fun atr -> atr.Name=xname name ) with 
    | None ->   let atr = new XAttribute( xname name, "")
                x.Add ( atr )
                atr
    | Some(atr) -> atr


