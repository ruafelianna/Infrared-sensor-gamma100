module FSSerialize

open Microsoft.FSharp.Reflection

open System
open System.IO
open System.Reflection
open System.Runtime.Serialization
open System.Text
open System.Xml
open System.Xml.Linq
open System.Xml.Serialization
open System.Runtime.Serialization.Formatters.Binary

let serializeXml<'a> (x : 'a) =
    let toString = System.Text.Encoding.UTF8.GetString
    let xmlSerializer = new DataContractSerializer(typedefof<'a>)
    use stream = new MemoryStream()
    xmlSerializer.WriteObject(stream, x)
    toString <| stream.ToArray() 

let deserializeXml<'a> (xml : string) =
    let toBytes (x : string) = System.Text.Encoding.UTF8.GetBytes x
    let xmlSerializer = new DataContractSerializer(typedefof<'a>)
    use stream = new MemoryStream(toBytes xml)
    xmlSerializer.ReadObject(stream) :?> 'a

let deserializeXmlDef<'a> xml (def:'a) = try deserializeXml<'a> xml with _ -> def

let getFromXmlFile<'a> fileName (def:'a)=    
    let ret =  try deserializeXml<'a> (System.IO.File.ReadAllText fileName) with _ -> def
    System.IO.File.WriteAllText( fileName, serializeXml<'a> ret )
    ret

let serializeBinary<'a> (x :'a) =
    let binFormatter = new BinaryFormatter()
    use stream = new MemoryStream()
    binFormatter.Serialize(stream, x)
    stream.ToArray() 

let deserializeBinary<'a> (arr : byte[]) =
    let binFormatter = new BinaryFormatter()
    use stream = new MemoryStream(arr)
    binFormatter.Deserialize(stream) :?> 'a

let deserializeBinaryDef<'a> binDt (def:'a) = try deserializeBinary<'a> binDt with _ -> def