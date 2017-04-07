module Var

open System
open System.Diagnostics
open System.ComponentModel

open Microsoft.FSharp.Reflection

open MIL82Gui
open my


type KNumAttribute(n:uint16) =     
    inherit Attribute()
    let n = n
    member this.N = n

type ModbusRegisterAttribute(n:uint16) =     
    inherit Attribute()
    let n = n
    member this.Reg = n

type ValueSource =  
    | Modbus3 of uint16 
    | NoSource
    member this.What = 
        match this with
        | Modbus3(reg) -> sprintf "рег.%d" reg
        | _ -> ""


type DevVal = 
    |   [<Description("Концентрация")>] 
        [<ModbusRegister(12us)>]        
                                    Conc
    |   [<Description("Температура МК,\"С")>]    
        [<ModbusRegister(54us)>]     
                                    Tk
    |   [<Description("Напряжение термометра МК")>]   
        [<ModbusRegister(0us)>]
                                    Utk
    |   [<Description("Напряжение термометра ДВ")>]   
        [<ModbusRegister(2us)>]
                                    Utw
    |   [<Description("Напряжение ДАД")>]             
        [<ModbusRegister(4us)>]
                                    Up
    |   [<Description("Напряжение ДВ")>]              
        [<ModbusRegister(6us)>]
                                    Uw    
    |   [<Description("Сигнал МК")>]  
        [<ModbusRegister(10us)>]
                                    Var1        
    |   [<Description("Поправка ухода нуля от температуры")>]       
        [<ModbusRegister(14us)>]
                                    Var2
    |   [<Description("Значение сигнала МК с поправкой ухода нуля МК от темп. и влаги")>]  
        [<ModbusRegister(16us)>]
                                    Var3
    |   [<Description("Поправка ухода чувствительности от температуры МК")>]  
        [<ModbusRegister(18us)>]
                                    Var4
    |   [<Description("Поправка ухода чувствительности от температуры МК")>]  
        [<ModbusRegister(20us)>]
                                    Var5
    |   [<Description("Значение сигнала МК с поправкой чувствительности от темп. МК")>]  
        [<ModbusRegister(22us)>]
                                    Var6    
    |   [<Description("Поправка ухода чувствительности МК от сигнала ДАД")>]  
        [<ModbusRegister(28us)>]
                                    Var9
    |   [<Description("Поправка сигнала МК на коэффициент чувствительности")>]  
        [<ModbusRegister(30us)>]
                                    Var10
    |   [<Description("Значение сигнала ДАД с поправкой от температуры МК")>]  
        [<ModbusRegister(32us)>]
                                    Var11
    |   [<Description("Абсолютная влажность пробы, г/куб. м")>]  
        [<ModbusRegister(60us)>]
                                    SensorTGM
    |   [<Description("Результат проверки погрешности")>]               
        TestConcResult

    |   [<Description("Дата проверки")>]               
        TestDate 

    | UserVar of string*ValueSource
    
    member this.What = 
        match this with 
        | UserVar(s,_) -> s
        | _ -> sprintf "%s, %s" (my.unionToString this) (my.getUnionCaseDescription this)
    member x.WhatShort = 
        match x with
        | UserVar(s,_) -> 
            Text.RegularExpressions.Regex.Match( s, "^([^,$]+)" ).Groups.[1].Value            
        | _ -> my.unionToString x

    member this.Reg3 =
        match my.getUnionCaseAttribute<ModbusRegisterAttribute> this with
        | Some(mdbs) -> mdbs.Reg
        | _ -> 0us

    member this.Source =
        match my.getUnionCaseAttribute<ModbusRegisterAttribute> this with
        | Some(mdbs) -> Modbus3(mdbs.Reg)
        | _ ->
            match this with             
            | UserVar(_,Modbus3(reg)) -> Modbus3(reg)
            | _ -> NoSource
            
let getModbus3RegOfVar var = my.unionCaseAttributeValue<ModbusRegisterAttribute>(var).Reg

let vars = 
    let varsCfgFileName = "vars.cfg"
    let varsXmlFileName = "vars.xml"
    let varsCfgExists = IO.File.Exists varsCfgFileName
    let vars = 
        [   let cases = FSharpType.GetUnionCases typeof<DevVal> |> Array.mapi( fun i case -> i,case)
            for i,case in cases do                        
                if i<cases.Length-1 then
                    yield FSharpValue.MakeUnion(case,[||]) :?> DevVal
            // проверка равенства строк без учёта регистра
            let eq a b = String.Compare(a.ToString(),b.ToString(),ignoreCase=true)=0            
            let (|Elem|_|) name (e:Xml.Linq.XElement) = if eq e.Name name then Some(e) else None
            let (|AttrV|_|) name e = 
                match (MyX.attrs e)|> List.tryFind( fun atr -> eq atr.Name name ) with 
                | None -> None 
                | Some(atr) -> Some( atr.Value )
            let (|AttrVal|_|) (name,func) e = 
                match e with
                | AttrV name v ->   let b,v = func v
                                    if b then Some( v ) else None
                | _ -> None
            if IO.File.Exists varsXmlFileName then
                let xdoc = Xml.Linq.XDocument.Load varsXmlFileName
                for e in xdoc.Root.Elements() do 
                    match e with
                    | Elem "modbusvar1" (AttrVal ("reg",UInt16.TryParse) reg ) ->
                        yield UserVar(e.Value,Modbus3(reg))
                    | _ -> ()
            
            if varsCfgExists then 
                use strm = new IO.StreamReader( varsCfgFileName, Text.Encoding.Default)
                yield!  
                    [ while not strm.EndOfStream do yield strm.ReadLine() ] 
                    |>  List.choose( fun s -> 
                        let m = Text.RegularExpressions.Regex.Match( s, "^\\s*(\\d+)\\s*([^\\s]+)\\s*([^$]+)\\s*$" )
                        let v n = 
                            let v = m.Groups.[int n].Value
                            v
                        if m.Success && m.Groups.Count=4 then
                            let b,reg = UInt16.TryParse (v 1)
                            if not b then None else
                                Some( UserVar(sprintf "%s, %s" (v 2) (v 3), Modbus3(reg) ) ) 
                        else None) ]

    let vars = 
        vars |> Seq.distinctBy( fun var -> var.Source ) |> Seq.toList
        |> List.sortBy( fun var -> 
            match var.Source with
            | Modbus3(reg) -> int reg
            | _ -> -1 ) 
    if varsCfgExists then IO.File.Delete varsCfgFileName
    let cx = MyX.celem
    let mainvars = 
        [| for var in vars do
            match var with
            | UserVar(s, Modbus3(reg) ) -> ()
            | _ -> 
                match var.Source with
                | NoSource -> ()
                | src -> yield sprintf "%s\t%s" src.What var.What |]
        |> Array.fold( fun acc a -> (if acc |> String.IsNullOrEmpty then "" else acc + "\n")+a ) ""
    let mainvars = 
        new Xml.Linq.XComment( "Зарезервированные переменные:\n"+mainvars+"\n" )
    let uservars = 
        [| for var in vars do
            match var with
            | UserVar(s, Modbus3(reg) ) -> yield cx "modbusvar1" [| s :> obj; new Xml.Linq.XAttribute( MyX.xname "reg", reg ) :> obj |]
            | _ -> ()   |]

    let cntnt = 
        [|  for var in vars do
                match var with
                | UserVar(s, Modbus3(reg) ) -> 
                    yield cx "modbusvar1" [| s :> obj; new Xml.Linq.XAttribute( MyX.xname "reg", reg ) :> obj |]
                | _ -> () |] 
        
    let xdoc = new Xml.Linq.XDocument [| yield mainvars :> obj;  yield cx "uservars" cntnt :> obj |]                    
        
    xdoc.Save varsXmlFileName
    IO.File.WriteAllText( "vars.fs", formatList vars "\r\n" (fun var -> 
        sprintf "let %s = var %d \"%s\"" var.WhatShort var.Reg3 var.What ) )
    vars

let modbus_vars =
    vars  |> List.choose( fun case -> 
        match case.Source with 
        | Modbus3(reg) -> Some(reg,case)
        | _ -> None)

let var_index x = vars |> List.findIndex( fun var -> var=x )
     

type Kefs = 
    
    |   [<Description("Коэффициент чувствительности термометра ДВ")>] 
        [<KNum(32us)>]   
        Ktw

    |   [<Description("Коэффициент функции термокомпенсации ухода нуля МК")>] 
        [<KNum(4us)>]   
        A0 
    |   [<Description("Коэффициент функции термокомпенсации ухода нуля МК")>] 
        [<KNum(5us)>]   
        A1
    |   [<Description("Коэффициент функции термокомпенсации ухода нуля МК")>] 
        [<KNum(6us)>]   
        A2

    |   [<Description("Коэффициент функции термокомпенсации ухода чувствительности МК")>] 
        [<KNum(7us)>]   
        B0_T 
    |   [<Description("Коэффициент функции термокомпенсации ухода чувствительности МК")>] 
        [<KNum(8us)>]   
        B1_T
    |   [<Description("Коэффициент функции термокомпенсации ухода чувствительности МК")>] 
        [<KNum(9us)>]   
        B2_T

    |   [<Description("Коэффициент функции термокомпенсации ухода нуля ДАД")>] 
        [<KNum(10us)>]   
        C0 
    |   [<Description("Коэффициент функции термокомпенсации ухода нуля ДАД")>] 
        [<KNum(11us)>]   
        C1
    |   [<Description("Коэффициент функции термокомпенсации ухода нуля ДАД")>] 
        [<KNum(12us)>]   
        C2

    |   [<Description("Единицы измерения")>] 
        [<KNum(56us)>]   
        Units 
    |   [<Description("Определяемый компонент")>] 
        [<KNum(53us)>]   
        Gas 
    |   [<Description("Шкала")>] 
        [<KNum(48us)>]   
        Scale 
    |   [<Description("Год выпуска")>] 
        [<KNum(54us)>]   
        Year
    |   [<Description("Серийный номер")>] 
        [<KNum(55us)>]   
        Serial

    |   [<Description("Смещение фазы синхронного детектора")>] 
        [<KNum(31us)>]   
        Phase_Start

    |   [<Description("Нулевое смещение, вычисленное при корректировке нулевых показаний")>] 
        [<KNum(0us)>]   
        Knul
    |   [<Description("Коэффициент чувствительности вычисленный при корректировке по PGS")>] 
        [<KNum(2us)>]       
        KSkale

    |   [<Description("Коэффициент функции компенсации ухода чувствительности МК по давлению")>] 
        [<KNum(13us)>]   
        D0
    |   [<Description("Коэффициент функции компенсации ухода чувствительности МК по давлению")>] 
        [<KNum(14us)>]   
        D1
    |   [<Description("Коэффициент функции компенсации ухода чувствительности МК по давлению")>] 
        [<KNum(15us)>]   
        D2

    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 1) ПГС№2")>] 
        [<KNum(16us)>]   
        K2
    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 1) ПГС№2")>] 
        [<KNum(17us)>]   
        B2
    |   [<Description("ПГС использованная при построении отрезка 1 линеаризатора МК")>] 
        [<KNum(18us)>]   
        PGS2
    |   [<Description("Значение сигнала var10 при построении отрезка 1 линеаризатора МК")>] 
        [<KNum(19us)>]   
        FR2

    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 2) ПГС№5")>] 
        [<KNum(20us)>]   
        K3
    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 2) ПГС№5")>] 
        [<KNum(21us)>]   
        B3
    |   [<Description("ПГС использованная при построении отрезка 2 линеаризатора МК")>] 
        [<KNum(22us)>]   
        PGS3
    |   [<Description("Значение сигнала var10 при построении отрезка 2 линеаризатора МК")>] 
        [<KNum(23us)>]   
        FR3

    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 3) ПГС№3")>] 
        [<KNum(24us)>]   
        K4
    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 3) ПГС№3")>] 
        [<KNum(25us)>]   
        B4
    |   [<Description("ПГС использованная при построении отрезка 3 линеаризатора МК")>] 
        [<KNum(26us)>]   
        PGS4
    |   [<Description("Значение сигнала var10 при построении отрезка 3 линеаризатора МК")>] 
        [<KNum(27us)>]   
        FR4

    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 4) ПГС№4")>] 
        [<KNum(28us)>]   
        K5
    |   [<Description("Коэффициент функции линеаризации характеристики МК (отрезок 4) ПГС№4")>] 
        [<KNum(29us)>]   
        B5
    |   [<Description("ПГС использованная при построении отрезка 4 линеаризатора МК")>] 
        [<KNum(30us)>]   
        PGS5
    
    |   [<Description("Т-комп.нуля.0")>] 
        [<KNum(23us)>]
        Termo0_1 
    |   [<Description("Т-комп.нуля.1")>] 
        [<KNum(24us)>]
        Termo0_2 
    |   [<Description("Т-комп.нуля.2")>] 
        [<KNum(25us)>]
        Termo0_3 

    |   [<Description("Т-комп.шкалы.0")>] 
        [<KNum(26us)>]
        TermoK_1 
    |   [<Description("Т-комп.шкалы.1")>] 
        [<KNum(27us)>]
        TermoK_2 
    |   [<Description("Т-комп.шкалы.2")>] 
        [<KNum(28us)>]
        TermoK_3    

    |   [<Description("Коэффициент функции компенсации нуля МК от влаги")>] 
        [<KNum(57us)>]
        W0
    |   [<Description("Коэффициент функции компенсации нуля МК от влаги")>] 
        [<KNum(58us)>]
        W1
    |   [<Description("Коэффициент функции компенсации нуля МК от влаги")>] 
        [<KNum(59us)>]
        W2

    | UserKef of string*uint16

    member x.What = 
        match x with 
        | UserKef(s,n) -> sprintf "%d %s" n s
        | _ -> sprintf "%d %s, %s" x.N (my.unionToString x) (my.getUnionCaseDescription x)
    member this.N = 
        match this with 
        | UserKef(_,n) -> n
        | _ -> 
            let knum = unionCaseAttributeValue<KNumAttribute>(this)
            knum.N

    member this.Reg = 224us + 2us*this.N
    member this.Cmd = ( 0x80us <<< 8 ) + this.N

let kefs =
    let koefsCfgFileName = "koefs.cfg"
    let koefsXmlfileName = "kefs.xml"
    let koefsCfgExists = IO.File.Exists koefsCfgFileName

    let kefs = 
        [   let cases = FSharpType.GetUnionCases typeof<Kefs> |> Array.mapi( fun i case -> i,case)
            for i,case in cases do                        
                if i<cases.Length-1 then
                    yield FSharpValue.MakeUnion(case,[||]) :?> Kefs             
            
            if IO.File.Exists koefsXmlfileName then
                let xdoc = Xml.Linq.XDocument.Load koefsXmlfileName
                for e in xdoc.Root.Elements() do 
                    let n =[ for atr in e.Attributes() do 
                                if atr.Name.ToString()="n" then 
                                    let b,v = UInt16.TryParse atr.Value
                                    if b then yield v ]
                    if n |> List.isEmpty |> not then
                        yield UserKef(e.Value, n.[0] )
            
            if koefsCfgExists then 
                use strm = new IO.StreamReader( koefsCfgFileName, Text.Encoding.Default)
                yield!  
                    [ while not strm.EndOfStream do yield strm.ReadLine() ] 
                    |>  List.choose( fun s -> 
                        let m = Text.RegularExpressions.Regex.Match( s, "^\\s*([^\\s]+)\\s*(?:[^\\s]+)\\s*([^$]+)\\s*$" )
                        if m.Success && m.Groups.Count=3 then Some( sprintf "%s, %s" m.Groups.[1].Value m.Groups.[2].Value ) else None)  
                    |> List.mapi( fun i s -> 
                        UserKef(s, uint16 i) ) ]
    let kefs = kefs |> Seq.distinctBy( fun kef -> kef.N ) |> Seq.toList |> List.sortBy( fun kef -> kef.N ) 
    if koefsCfgExists then IO.File.Delete koefsCfgFileName
    let cx = MyX.celem
    let mainkefs = 
        [| for kef in kefs do
            match kef with
            | UserKef(_) -> ()
            | _ -> yield kef.What |]
        |> Array.fold( fun acc a -> (if acc |> String.IsNullOrEmpty then "" else acc + "\n")+a ) ""
    let head = 
        new Xml.Linq.XComment( "Зарезервированные коэффициенты:\n"+mainkefs+"\n" )
    let userkefs = 
        [| for kef in kefs do
            match kef with
            | UserKef(s, n ) -> yield cx "kef" [| s :> obj; new Xml.Linq.XAttribute( MyX.xname "n", n ) :> obj |]
            | _ -> ()   |]
    let xdoc = new Xml.Linq.XDocument [| yield head :> obj; yield cx "userkefs" userkefs :> obj |]                    
    xdoc.Save koefsXmlfileName

    IO.File.WriteAllText( "kefs.fs", formatList kefs "\r\n" (fun x -> 
        sprintf "let %s = kef %d \"%s\"" (my.unionToString x) x.N (my.getUnionCaseDescription x) ) )
    kefs

type Grp = 
    | [<Description("Корректировка показаний")>] 
        Adjust
    | [<Description("Проверка показаний")>] 
        IndicationTest 
    | [<Description("Настройка по давлению")>] 
        Press
    | [<Description("Настройка по влаге")>] 
        Humidity
    | [<Description("Корректировка показаний перед настройкой по давлению")>] 
        AdjustBeforPressure
    | [<Description("Корректировка показаний перед настройкой по влаге")>] 
        AdjustBeforHumidity
    | [<Description("Корректировка показаний перед настройкой климатики")>] 
        AdjustBeforTermo

    | [<Description("Настройка нуля шкалы по температуре")>] 
        Termo0
    | [<Description("Настройка чувствительности по температуре")>] 
        TermoE
    | [<Description("Коэфициент чувствительности термометра датчика влажности")>] 
        TermoKtw
    | [<Description("Сдача ОТК")>] 
        OTKPresent
    | [<Description("Проверка неизмеряемых")>] 
        TestNotMeasured

    | [<Description("Проверка нуля шкалы после термокомпенсации")>] 
        Test_Termo0
    | [<Description("Проверка чувствительности после термокомпенсации")>] 
        Test_TermoE

    | [<Description("Дата проверки")>] 
        Test_Date
        
    
    
    | [<Description("Коэфициенты")>] Koefs 
    | [<Description("Опрос")>] Interrogate
    member this.what = my.getUnionCaseDescription this

module Temperature =    
    type Index =
        |   [<Description("Н.к.у.")>]       Nku
        |   [<Description("Повышенная")>]   High        
        member x.What = my.getUnionCaseDescription x
        member public x.N =         
            my.unionCases<Index>
            |> List.mapi( fun n case ->  n,case) 
            |> List.find( fun (n,case) -> case=x )
            |> fst

type Prm =     
    | Val of DevVal 
    | Kef of Kefs 
    member x.What   = 
        match x with
        | Val(x) -> my.unionToString x
        | Kef(x) -> my.unionToString x

let prmGetReg3 = function 
    | Kef(kef) ->kef.Reg
    | Val(var) ->
        match var.Source with 
        | Modbus3(reg) -> reg
        | _ -> sprintf "Упс! prmGetReg3, %O" var |> failwith
module Reg3 = 

    let tryGetKef reg = 
        let x = kefs |> List.tryFind( function | kef when kef.Reg=reg -> true | _ -> false  )
        x

    let (|TryGetKef|_|) = tryGetKef 

    let (|TryGetKefByCmdCode|_|) cmdCode = 
        let c = 0x80us <<< 8
        if cmdCode<c then None else
            let n = cmdCode - c
            Some( n*2us + 224us )

    let tryGetDevVal reg = 
         modbus_vars |> List.tryFind( fun(areg,_) -> areg=reg )
    let (|TryGetDevVal|_|) = tryGetDevVal

type CodeAttribute(c) = 
    inherit Attribute()
    member this.Code = c

let codeval v = (unionCaseAttributeValue<CodeAttribute>(v)).Code

type Gas = 
    | [<Code(3us)>]   CO
    | [<Code(4us)>]   CO2
    | [<Code(5us)>]   CH4
    | [<Code(8us)>]   SO2
    | [<Code(10us)>]  NO2
    | [<Code(11us)>]  NO
    member this.Code = codeval this
    member this.What = my.unionToString this

type Units = 
    | [<Description("%")>]      [<Code(7us)>]   Percent
    | [<Description("мг/м3")>]  [<Code(2us)>]   MGpM3
    | [<Description("ppm")>]    [<Code(3us)>]   PPM
    | [<Description("г/м3")>]   [<Code(4us)>]   GpM3
    member this.Code = codeval this

type ScaleValueAttribute(code:uint16, scale:double, mid:double) = 
    inherit Attribute()
    member this.Code = code
    member this.Scale = scale
    member this.Mid = mid
    member this.What = sprintf "0-%g" scale




type Scale = 
    | [<ScaleValue(1us,  0.1, 0.05)>]       Scale01
    | [<ScaleValue(2us,  0.2, 0.1)>]        Scale02
    | [<ScaleValue(3us,  0.5, 0.2)>]        Scale05
    | [<ScaleValue(4us,  1., 0.5)>]         Scale1
    | [<ScaleValue(5us,  2., 1.)>]          Scale2
    | [<ScaleValue(6us,  5., 2.)>]          Scale5
    | [<ScaleValue(7us,  10., 5.)>]         Scale10
    | [<ScaleValue(8us,  15., 5.)>]         Scale15
    | [<ScaleValue(9us,  20., 10.)>]        Scale20
    | [<ScaleValue(10us, 30., 10.)>]        Scale30
    | [<ScaleValue(11us, 50., 20.)>]        Scale50
    | [<ScaleValue(12us, 60., 30.)>]        Scale60
    | [<ScaleValue(13us, 70., 30.)>]        Scale70    
    | [<ScaleValue(15us, 100., 50.)>]       Scale100
    | [<ScaleValue(16us, 200., 100.)>]      Scale200
    | [<ScaleValue(17us, 500., 200.)>]      Scale500
    | [<ScaleValue(18us, 1000., 500.)>]     Scale1000
    | [<ScaleValue(19us, 2000., 1000.)>]    Scale2000

    member this.Value = my.unionCaseAttributeValue<ScaleValueAttribute>(this)
    member this.Code = this.Value.Code
    member this.Scale = this.Value.Scale
    member this.Mid = this.Value.Mid
    member this.What = this.Value.What

let scalecode   (s:Scale) = s.Code
let scale       (s:Scale) = s.Scale
let scalewhat   (s:Scale) = s.What
let scalemid    (s:Scale) = s.Mid

let saveScalesFS() =
    let s = my.formatList my.unionCases<Scale> "\r\n" <| fun x ->
        sprintf "let %s = cm %d %g %g" (my.unionToString x) x.Code x.Scale x.Mid
    IO.File.WriteAllText( "scales.fs", s)
     

module Kind = 
    type D = 
        | [<Description("ИБЯЛ.413341.001")>] D0 = 0
        | [<Description("ИБЯЛ.413341.001.1")>] D1 = 1 
        | [<Description("ИБЯЛ.413341.001.2")>] D2 = 2 
        | [<Description("ИБЯЛ.413341.001.3")>] D3 = 3 
        | [<Description("ИБЯЛ.413341.001.6")>] D6 = 6 
        | [<Description("ИБЯЛ.413341.001.7")>] D7 = 7 
        | [<Description("ИБЯЛ.413341.001.10")>] D10 = 10 
        | [<Description("ИБЯЛ.413341.001.11")>] D11 = 11 
        | [<Description("ИБЯЛ.413341.001.12")>] D12 = 12 
        | [<Description("ИБЯЛ.413341.001.13")>] D13 = 13
        | [<Description("ИБЯЛ.413341.001.14")>] D14 = 14
        | [<Description("ИБЯЛ.413341.001.16")>] D16 = 16
        | [<Description("ИБЯЛ.413341.001.17")>] D17 = 17
        | [<Description("ИБЯЛ.413341.001.20")>] D20 = 20
        | [<Description("ИБЯЛ.413341.001.21")>] D21 = 21
        | [<Description("ИБЯЛ.413341.001.22")>] D22 = 22
        | [<Description("ИБЯЛ.413341.001.24")>] D24 = 24
        | [<Description("ИБЯЛ.413341.001.25")>] D25 = 25

        | [<Description("ИБЯЛ.413341.001.30")>] D30 = 30
        | [<Description("ИБЯЛ.413341.001.33")>] D33 = 33
        | [<Description("ИБЯЛ.413341.001.34")>] D34 = 34
        | [<Description("ИБЯЛ.413341.001.35")>] D35 = 35
        | [<Description("ИБЯЛ.413341.001.36")>] D36 = 36
        | [<Description("ИБЯЛ.413341.001.37")>] D37 = 37
        | [<Description("ИБЯЛ.413341.001.38")>] D38 = 38
        | [<Description("ИБЯЛ.413341.001.39")>] D39 = 39
        | [<Description("ИБЯЛ.413341.001.40")>] D40 = 40
        | [<Description("ИБЯЛ.413341.001.41")>] D41 = 41
        | [<Description("ИБЯЛ.413341.001.42")>] D42 = 42
        | [<Description("ИБЯЛ.413341.001.43")>] D43 = 43
        | [<Description("ИБЯЛ.413341.001.44")>] D44 = 44
        | [<Description("ИБЯЛ.413341.001.45")>] D45 = 45
        | [<Description("ИБЯЛ.413341.001.46")>] D46 = 46
        | [<Description("ИБЯЛ.413341.001.47")>] D47 = 47
        | [<Description("ИБЯЛ.413341.001.48")>] D48 = 48
        | [<Description("ИБЯЛ.413341.001.49")>] D49 = 49

        | [<Description("ИБЯЛ.413341.001.50")>] D50 = 50
        | [<Description("ИБЯЛ.413341.001.51")>] D51 = 51
        | [<Description("ИБЯЛ.413341.001.52")>] D52 = 52
        | [<Description("ИБЯЛ.413341.001.55")>] D55 = 55
        | [<Description("ИБЯЛ.413341.001.56")>] D56 = 56
        | [<Description("ИБЯЛ.413341.001.57")>] D57 = 57
        | [<Description("ИБЯЛ.413341.001.58")>] D58 = 58
        | [<Description("ИБЯЛ.413341.001.59")>] D59 = 59

        | [<Description("ИБЯЛ.413341.001.60")>] D60 = 60
        | [<Description("ИБЯЛ.413341.001.61")>] D61 = 61
        | [<Description("ИБЯЛ.413341.001.62")>] D62 = 62
        | [<Description("ИБЯЛ.413341.001.65")>] D65 = 65
        | [<Description("ИБЯЛ.413341.001.66")>] D66 = 66
        | [<Description("ИБЯЛ.413341.001.67")>] D67 = 67
        | [<Description("ИБЯЛ.413341.001.68")>] D68 = 68
        | [<Description("ИБЯЛ.413341.001.69")>] D69 = 69

        | [<Description("ИБЯЛ.413341.001.70")>] D70 = 70
        | [<Description("ИБЯЛ.413341.001.71")>] D71 = 71
        | [<Description("ИБЯЛ.413341.001.72")>] D72 = 72

    let li =
        [   yield D.D0, CO, PPM, Scale200,      5 
            yield D.D1, CO, PPM, Scale500,      5  
            yield D.D2, CO, PPM, Scale1000,     5  
            yield D.D3, CO, PPM, Scale2000,     5 

            yield D.D6, CO, Percent, Scale05,   5  
            yield D.D7, CO, Percent, Scale1,    5 

            yield D.D10, CO2, PPM, Scale100,    10
            yield D.D11, CO2, PPM, Scale200,    10
            yield D.D12, CO2, PPM, Scale500,    10
            yield D.D13, CO2, PPM, Scale1000,    10
            yield D.D14, CO2, PPM, Scale2000,    10

            yield D.D16, CO2, Percent, Scale05,    10
            yield D.D17, CO2, Percent, Scale1,    10

            yield D.D20, CH4, PPM, Scale500,    5
            yield D.D21, CH4, PPM, Scale1000,    5
            yield D.D22, CH4, PPM, Scale2000,    5

            yield D.D24, CH4, Percent, Scale05,    5
            yield D.D25, CH4, Percent, Scale1,    5

            yield D.D30, CO, GpM3, Scale15,    5
        
            yield D.D33, NO, GpM3, Scale2,    10
            yield D.D34, NO, GpM3, Scale1,    10
            yield D.D35, NO2, GpM3, Scale2,    5
            yield D.D36, NO2, GpM3, Scale1,    5

            yield D.D37, SO2, GpM3, Scale2,    10
            yield D.D38, SO2, GpM3, Scale5,    7
            yield D.D39, SO2, GpM3, Scale10,    7
            yield D.D40, SO2, GpM3, Scale20,    7
            yield D.D41, SO2, GpM3, Scale60,    7

            yield D.D45, CO, Percent, Scale2,    2
            yield D.D46, CO, Percent, Scale5,    2
            yield D.D47, CO, Percent, Scale10,    2
            yield D.D48, CO, Percent, Scale20,    2
            yield D.D49, CO, Percent, Scale30,    2
            yield D.D50, CO, Percent, Scale50,    2
            yield D.D51, CO, Percent, Scale70,    2
            yield D.D52, CO, Percent, Scale100,    2

            yield D.D55, CO2, Percent, Scale2,    2
            yield D.D56, CO2, Percent, Scale5,    2
            yield D.D57, CO2, Percent, Scale10,    2
            yield D.D58, CO2, Percent, Scale20,    2
            yield D.D59, CO2, Percent, Scale30,    2
            yield D.D60, CO2, Percent, Scale50,    2
            yield D.D61, CO2, Percent, Scale70,    2
            yield D.D62, CO2, Percent, Scale100,    2

            yield D.D65, CH4, Percent, Scale2,    2
            yield D.D66, CH4, Percent, Scale5,    2
            yield D.D67, CH4, Percent, Scale10,    2
            yield D.D68, CH4, Percent, Scale20,    2
            yield D.D69, CH4, Percent, Scale30,    2
            yield D.D70, CH4, Percent, Scale50,    2
            yield D.D71, CH4, Percent, Scale70,    2
            yield D.D72, CH4, Percent, Scale100,    2 ]

    let tryget d = 
        let found = li |> List.tryFind( fun (dn,_,_,_,_) -> dn=d)
        match found with 
        | Some(_,gas,uits,scale,errorlimit) -> Some(gas,uits,scale,errorlimit)
        | _ -> None
    let get d = 
        match tryget d with | Some(i) -> i | _ -> d.ToString() |> sprintf "Var.Kind.get %s - недопустимый аргумент" |> failwith

type NotMeasured = 
    | CO2_N2
    | CH4_N2
    | SO2_N2
    | NO_N2

type Cmd =
    | [<Code(1us)>]
      [<Description("Калибровка нуля")>] 
      Adj0 
    | [<Code(2us)>]
      [<Description("Калибровка чувствительности")>] 
      AdjE 
    | [<Code(7us)>]
      [<Description("Установка адреса")>] 
      SetAddy 
    | [<Code(8us)>]
      [<Description("Нормировка")>] 
      MkNorm 
    member this.Code = match (my.getUnionCaseAttribute<CodeAttribute> this) with Some(atr) -> atr.Code | _ -> failwith "Упс! Cmd Code"
    member this.What = sprintf "%s, %d" (my.getUnionCaseDescription this) this.Code
    static member cases = my.unionCases<Cmd>
    static member getCaseOfCode code = Cmd.cases |> List.tryFind ( fun i -> i.Code=code )

let (|CmdOfCode|_|) = Cmd.getCaseOfCode

type private Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type private Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes
type private Column = DevExpress.XtraTreeList.Columns.TreeListColumn

let private form = MIL82MainForm.form
let private log = NLog.LogManager.GetCurrentClassLogger()

let testMainErrorList = 1::2::5::3::4::[]




// дерево параметров прибора
let private varsTree = [
    let (+++) (nds:Nodes) s = nds.Add [|s|]
    let (++) (nd:Node) s = nd.Nodes +++ s
    let ndN n (nd:Node) = [ for i in 1..n do yield nd ++ ( sprintf "Точка %d" i ) ]
    let nd3 = ndN 3
    
    let ndGas5 nd = 
        1::2::5::3::4::[]
        |> List.mapi( fun i d -> nd++( sprintf "Точка %d, ПГС%d" (i+1) d ) )

    
    form.treeListData.Nodes.Clear()

    let nd = form.treeListData.Nodes +++ "Коэффициенты"    
    yield Grp.Koefs, [ for kef in kefs -> Kef(kef), nd++kef.What::[] ]

    let nd = form.treeListData.Nodes +++ Interrogate.what
    yield Grp.Interrogate, 
        [ for var in vars do
            match var.Source with
            | NoSource -> ()
            | _ -> yield Val(var), nd++var.What::[] ]

    let nd = form.treeListData.Nodes +++ "Данные настройки"
    let nds = nd.Nodes

    let cgrp v = nds +++ (my.getUnionCaseDescription v)

    let addAdjustGrp (grp:Grp) =
        let nd = cgrp grp 
        grp,[   yield Val(Conc), 
                    (nd ++ "Показания ГСО ПГС №1")::
                    (nd ++ "Показания ГСО ПГС №4")::[]
                yield Val(TestConcResult), 
                    (nd ++ "Проверка показаний после калибровки нуля")::
                    (nd ++ "Проверка показаний после калибровки чувствительности")::[] ]
    let addTestVariation nd = Val(TestConcResult), (nd ++ "проверка вариации показаний")::[]

    yield addAdjustGrp Adjust

    let nd = nds +++ IndicationTest.what    
    yield Grp.IndicationTest, 
        [   yield Val( Conc ),  testMainErrorList |> List.map( fun x -> nd++(sprintf "Показания при ГСО ПГС №%d" x) )
            yield Val( TestConcResult ), testMainErrorList |> List.map( fun x -> nd++(sprintf "Проверка показаний при ГСО ПГС №%d" x) ) ]

    yield addAdjustGrp AdjustBeforPressure
    let nd = cgrp Press
    yield Press, [
        let ptsP nd = [ for s in "Нормальное давдение"::"Избыточное давление"::[] -> nd++s ]
        yield Val(Var6 ),   ptsP (nd ++ "var6")
        yield Val(Var11),   ptsP (nd ++ "var11") 
        yield Val(Conc),    ptsP (nd ++ "Концентрация")  
        yield addTestVariation nd ]

    yield addAdjustGrp AdjustBeforHumidity
    let nd = cgrp Humidity
    yield Humidity, [
        let ptsP nd = [ for s in "Сухой газ"::"Влажный газ"::[] -> nd++s ]
        yield Val(Var1 ),       ptsP (nd ++ "var1")
        yield Val(SensorTGM),   ptsP (nd ++ "SensorTGM") 
        yield Val(Conc),        ptsP (nd ++ "Концентрация")  
        yield addTestVariation nd]
    
    
    yield addAdjustGrp AdjustBeforTermo

    let ndT = nds +++ "Настройка по температуре"
    
    let ptsT nd = [ for s in [ "Н.к.у."; "Повышенная температура"; "Пониженная температура"] -> nd++s ]
    let pts0 nd = [ for v in Conc::Var1::Up::Utw::Tk::[] -> Val(v), ptsT (nd ++ v.What) ]
    let ptsE nd = [ for v in Var4::Tk::[] -> Val(v), ptsT (nd ++ v.What) ]

    yield Termo0, 
        [   let nd =  ndT ++ Termo0.what
            yield! pts0 nd ]
    yield TermoE, 
        [   let nd =  ndT ++ TermoE.what
            yield! ptsE nd ]

    let nd = form.treeListData.Nodes +++ "Сдача ОТК"
    let nds = nd.Nodes

    let nd = nds +++ "Проверка после термокомпенсации"
    yield Test_Termo0, 
        [   let nd =  nd ++ "ПГС1"            
            yield Val(Conc), (nd ++ "Показания")::[]
            yield Val(TestConcResult), (nd ++ "Проверка")::[]  ]
    yield Test_TermoE, 
        [   let nd =  nd ++ "ПГС4"
            yield Val(Conc), (nd ++ "Показания")::[]
            yield Val(TestConcResult), (nd ++ "Проверка")::[] ]

    let ndOTK = nds +++ "Техпрогон"
    yield OTKPresent,
        [   let cnd var name = 
                let npts = [ for nday in 1..4 do for ngas in 1::3::1::[] do yield nday,ngas ]
                let nd = ndOTK ++ name
                Val(var), [ for nday,ngas in npts -> nd++(sprintf "День %d, ГСО-ПГС №%d" nday ngas) ]
            yield cnd Conc "Показания" 
            yield cnd TestConcResult "Проверка" ]
    let ndTestNotMeasured = nds +++ TestNotMeasured.what
    yield TestNotMeasured,
        [   //for x in "CO2-N2"::"NO-N2"::"CH4-N2"::"SO2-N2"::[] do
            let cnd var name =                 
                let nd = ndTestNotMeasured ++ name
                Val(var), [ for x in "ГСО-ПГС №1"::"ГСО-ПГС №5(неизм.)"::"ГСО-ПГС №6(неизм.)"::"ГСО-ПГС №7(неизм.)"::"ГСО-ПГС №8(неизм.)"::[] ->  nd++x ]
            yield cnd Conc "Показания" 
            yield cnd TestConcResult "Проверка" ]

    let nd = nds +++ "Дата проверки"
    yield Test_Date,
        [   yield Val(TestDate), [  nd++"Ввод термокомпенсации"
                                    nd++"Техпрогон, день 1"
                                    nd++"Техпрогон, день 2"
                                    nd++"Техпрогон, день 3"
                                    nd++"Техпрогон, день 4"] ] ]

// список параметров прибора
let grp_vars = 
    [ for (grp,vars) in varsTree do                    
        for (val', nds) in vars do                            
            let s = sprintf "%s.%s" (unionToString(grp)) (val'.What)
            match val', nds with
            | Kef(kef), nd::[] -> yield grp, val', 0, nd, "Kef."+unionToString(kef)
            | _, nd::[] -> yield grp, val', 0, nd, s
            |_ -> for i, nd in nds |> List.mapi( fun i nd -> i, nd  ) do                                    
                    yield grp, val', i, nd, sprintf "%s.%d" s i ]
    
let tryGetNode grp prm idx = 
    match grp_vars |> List.tryFind( fun (agrp,aprm, aidx,_,_)  -> agrp=grp && aprm=prm && aidx=idx   ) with 
    | Some(_,_,_,nd,name) -> Some(nd) 
    | _ -> None

// проверка индекса параметра во врмя выполнения 
let failWithVar (grp:Grp) (prm:Prm) idx = 
    let msg = sprintf "Не найден элемент %s.%s.%d" (unionToString grp) (unionToString prm) idx
    Debug.Assert(false, msg )
    failwith(msg)

// обращение к параметру по индексу
let getn (grp:Grp) (prm:Prm) idx = 
    match tryGetNode grp prm idx with 
    | Some(nd) -> nd 
    | _ -> failWithVar grp prm idx
let get' grp prm = getn grp prm 0

let getNodeN grp prm idx = getn grp prm idx 
let getNode grp prm = getNodeN grp prm 0

let getCaptionN grp prm idx = 
    let vnd (nd:Node) = nd.GetValue(form.columnParams).ToString()
    let nd = getNodeN grp prm idx
    let ndPrm = nd.ParentNode
    let ndGrp = ndPrm.ParentNode
    if ndGrp<>null then sprintf "%s.%s.%s" (vnd ndGrp) (vnd ndPrm) (vnd nd) else
        sprintf "%s.%s" (vnd ndPrm) (vnd nd)

let getNameOfPoint grp npt = 
    let fnd = 
        grp_vars |> 
        List.tryFind( function 
            | grpa,Val(_), npta,_,_ -> grpa=grp && npta=npt
            | _ -> false )
    match fnd with 
    | Some(_,_,_,nd,name) -> nd.GetValue( form.columnParams ).ToString()
    | _ -> 
        let msg = sprintf "Не найден элемент %s, точка %d" (unionToString grp) npt
        Debug.Assert(false, msg )
        failwith msg
    

let getCaption grp prm =
    let vnd (nd:Node) = nd.GetValue(form.columnParams).ToString()
    let nd = getNodeN grp prm 0    
    let ndGrp = nd.ParentNode
    sprintf "%s.%s" (vnd ndGrp) (vnd nd)

let cmdDescriptionByCode = function    
    | CmdOfCode(cmd) ->  cmd.What
    | Reg3.TryGetKefByCmdCode ( Reg3.TryGetKef(kef) )  -> sprintf "K%d" kef.N
    | _ -> ""
let (|CmdDescriptionByCode|) = cmdDescriptionByCode

// получить значение параметра
let getValN grp prm idx = 
    let nd = getn grp prm idx
    fun nDevice ->  
        let v = nd.GetValue( form.treeListData.Columns.[nDevice+1] )
        if v<>null then v.ToString() else ""
let getVal grp prm = getValN grp prm 0

// установить значение параметра по индексу и номеру прибора в таблице
let setValN s grp prm idx = 
    let nd = getn grp prm idx
    fun nDevice -> safelyWinForm form.treeListData ( fun () -> 
            let col = form.treeListData.Columns.[nDevice+1]
            try
                nd.SetValue( col, null ) 
                nd.SetValue( col, s )                    
            with exn  ->  exn |> my.exn |> log.Error ) 
                    
let setVal s grp prm = setValN s grp prm 0

let setCellStyle grp prm idx (style:UI.LogKind) nDevice = 
    let nd = getn grp prm idx
    safelyWinForm form.treeListData <| fun () -> 
        let col = form.treeListData.Columns.[nDevice+1]           
        UI.setTreeListCellStyle nd col style

let getCellStyle = 
    let ret = ref UI.LogKind.Info 
    fun grp prm idx nDevice ->
        let nd = getn grp prm idx
        safelyWinForm form.treeListData <| fun () -> 
            let col = form.treeListData.Columns.[nDevice+1]           
            ret := UI.getTreeListCellStyle nd col
        !ret

let (|Var|_|) (grp,val',idx) = 
    match grp_vars |> Seq.tryFind ( fun (grp', val'', idx', _,_ )-> grp=grp' && val'=val'' && idx=idx') with
    | Some(_, _, _, nd,name ) -> Some(nd,name) | None -> None

let getKef kef = getValN Koefs (Kef(kef)) 0
let setKef value kef = setValN value Koefs (Kef(kef)) 0

