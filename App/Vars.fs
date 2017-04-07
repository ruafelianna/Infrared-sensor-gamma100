module Vars

    open System
    open System.Diagnostics
    open System.ComponentModel

    open MIL82Gui
    open my

    type Kefs = 
            | SoftVer | Knd | Year| Serial| Units | Gas | Scale | Scale0 | ScaleEnd | PGS1 | PGS3
            | Isp | Diap | Porog1 | Porog2 
    
    type Grp = Lin | ScaleNullTemp | ScaleEndTemp | MainPogr | Correct | Kefs | Indctn | Addy_

    type ChcknVal = Data | Conc | Pgs | T | Var2 | Uw | Ur | Kef of Kefs | Addy

   
    type ибял = 
    | [<Description("00.00")>] И'0'0 = 0 
    | [<Description("00.01")>] И'0'1 = 1 
    | [<Description("00.02")>] И'0'2 = 2 
    | [<Description("01.00")>] И'1'0 = 3 
    | [<Description("01.01")>] И'1'1 = 4 
    | [<Description("02.00")>] И'2'0 = 5 
    | [<Description("02.01")>] И'2'1 = 6 
    | [<Description("03.00")>] И'3'0 = 7 
    | [<Description("03.01")>] И'3'1 = 8 
    | [<Description("03.02")>] И'3'2 = 9
       

    type private Node = DevExpress.XtraTreeList.Nodes.TreeListNode
    type private Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes
    type private Column = DevExpress.XtraTreeList.Columns.TreeListColumn

    let private form = MIL82MainForm.form
    let private devGrd = form.treeList1
    let private log = NLog.LogManager.GetCurrentClassLogger()
    

    // дерево параметров прибора
    let private varsTree = [
        let (+++) (nds:Nodes) s = nds.Add [|s|]
        let (++) (nd:Node) s = nd.Nodes +++ s
        let ndN n (nd:Node) = [ for i in 1..n do yield nd ++ Convert.ToString(i) ]
        let nd3 = ndN 3
        let ndGas6 (nd:Node) = nd++"1"::nd++"2"::nd++"3"::nd++"2"::nd++"1"::nd++"3"::[]

        let nds = devGrd.Nodes
        nds.Clear()
        let (~%%) s = nds +++ s

        yield Addy_, [ yield Addy, %% "Адрес"::[] ]

        let nd = %% "Показания"
        yield Indctn,  [
            yield Conc,     nd++"Концентрация"::[]
            yield T,        nd++"T\"C"::[]
            yield Var2,     nd++"Var2"::[]

            yield Uw,     nd++"Uw"::[]
            yield Ur,     nd++"Ur"::[]]

        let nd = %% "Линеаризатор"
        yield Lin,  [
            yield Data, nd::[]
            yield Conc,    nd3 (nd ++ "Концентрация") 
            yield Pgs,     nd3 (nd ++ "ПГС") ]

        let nd = %% "Компенсация влияния температуры на нулевые показания"
        yield ScaleNullTemp, [
            yield Data, nd::[]
            yield Var2,    nd3 (nd ++ "Var2") 
            yield T,       nd3 (nd ++ "T\"C") ]

        let nd = %% "Компенсация влияния температуры на чувствительность"
        yield ScaleEndTemp, [
            yield Data, nd::[]
            yield Var2,    nd3 (nd ++ "Var2")
            yield T,       nd3 (nd ++ "T\"C") ]

        let nd = %% "Снятие основной погрешности"
        yield MainPogr, [
            yield Data, nd::[]
            yield Conc, ndGas6 (nd++"Концентрация")
            yield Pgs, ndGas6 (nd++"ПГС") ]

        let nd = %% "Коэффициенты"
    
        yield Grp.Kefs, [        
            yield Kef(SoftVer),        nd++"01 Версия программы"::[]
            yield Kef(Knd),            nd++"02 Тип прибора"::[]
            yield Kef(Year),           nd++"03 Год выпуска"::[]
            yield Kef(Serial),         nd++"04 Серийный номер"::[]
            yield Kef(Units),          nd++"05 Единицы измерения"::[]
            yield Kef(Gas),            nd++"06 Тип газа"::[]
            yield Kef(Scale),          nd++"07 Тип шкалы"::[]
            yield Kef(Scale0),         nd++"08 Начало шкалы"::[]
            yield Kef(ScaleEnd),       nd++"09 Конец шкалы"::[]
            yield Kef(PGS1),           nd++"10 ПГС1"::[]
            yield Kef(PGS3),           nd++"11 ПГС3"::[]
            yield Kef(Isp),            nd++"323 Исполнение"::[]
            yield Kef(Diap),           nd++"324 Диапазон измерений"::[]
            yield Kef(Porog1),         nd++"325 Порог1"::[]        
            yield Kef(Porog2),         nd++"328 Порог2"::[] ]]

    // список параметров прибора
    let lst = [ for (grp,vars) in varsTree do                    
                            for (val', nds) in vars do                            
                                let s = sprintf "%s.%s" (unionToString(grp)) (unionToString(val'))
                                match val', nds with
                                | Kef(kef), nd::[] -> yield grp, val', 0, nd, "Kef."+unionToString(kef)
                                | _, nd::[] -> yield grp, val', 0, nd, s
                                |_ -> for i, nd in nds |> List.mapi( fun i nd -> i, nd  ) do                                    
                                        yield grp, val', i, nd, sprintf "%s.%d" s i ]

    // соответствие номера регистра модбас параметру
    let modbusReg = 
        
        let (~%%) n = 224us + 2us*n

        (0us,   Indctn, Conc)::
        (2us,   Indctn, T)::
        (18us,  Indctn, Var2)::
        (8us,   Indctn, Uw)::
        (10us,  Indctn, Ur)::
        
        (%% 0us, Kefs, Kef(SoftVer) ):: 
        (%% 1us, Kefs, Kef(Knd) ):: 
        (%% 2us, Kefs, Kef(Year) ):: 
        (%% 3us, Kefs, Kef(Serial) ):: 
        (%% 5us, Kefs, Kef(Units) ):: 
        (%% 6us, Kefs, Kef(Gas) ):: 
        (%% 7us, Kefs, Kef(Scale) ):: 
        (%% 8us, Kefs, Kef(Scale0) ):: 
        (%% 9us, Kefs, Kef(ScaleEnd) ):: 
        (%% 10us, Kefs, Kef(PGS1) ):: 
        (%% 11us, Kefs, Kef(PGS3) ):: 
        (%% 323us, Kefs, Kef(Isp) ):: 
        (%% 324us, Kefs, Kef(Diap) ):: 
        (%% 325us, Kefs, Kef(Porog1) ):: 
        (%% 328us, Kefs, Kef(Porog2) ):: []

    let kefs = lst |> List.choose( function (Kefs, Kef(kef), _, _, _) -> Some(kef) | _ -> None )

    let kefReg kef = 
        match modbusReg |> List.tryFind( function |(_,Kefs, Kef(kef')) when kef'=kef -> true | _ -> false  ) with
        | Some(reg,_,_) -> reg
        | _ -> failwith( sprintf "Не найден номер к-та %O"  kef )

    let devVars = 
        modbusReg |>  List.choose( function  ( reg, Indctn, some ) -> Some( reg, Indctn, some )  | _ -> None )

    // проверка индекса параметра во врмя выполнения 
    let private failWithVar (grp:Grp) (prm:ChcknVal) idx = 
        let msg = sprintf "Не найден элемент %s.%s.%d" (unionToString grp) (unionToString prm) idx
        Debug.Assert(false, msg )
        failwith(msg)

    // обращение к параметру по индексу
    let getn (grp:Grp) (prm:ChcknVal) idx = 
        match lst |> 
            List.tryFind( function grp',prm', idx',_,_ when grp'=grp && prm'=prm && idx'=idx -> true | _ -> false  )
            with Some(_,_,_,nd,name) -> (nd,name) | _ -> failWithVar grp prm idx
    let get' grp prm = getn grp prm 0

    let getNodeN grp prm idx = getn grp prm idx |> fst
    let getNode grp prm = getNodeN grp prm 0

    let getNameN grp prm idx = getn grp prm idx |> snd
    let getName grp prm = getNameN grp prm 0

    let getCaptionN grp prm idx = (getNodeN grp prm idx ).[0].ToString()
    let getCaption grp prm = getCaptionN grp prm 0

    let getKefCaption kef = getCaption Kefs ( Kef(kef) )
    
    
    // выбран ли данный параметр
    let isSelectedN grp prm idx = 
        let nd,_ = getn grp prm idx 
        not <| form.IsMarkedOutNode nd

    let isSelected grp prm = isSelectedN grp prm 0

    let getSelectedKefs = [for kef in kefs do if isSelected Kefs (Kef(kef)) then yield kef ]

    // получить значение параметра
    let getValN grp prm idx = 
        let nd,_ = getn grp prm idx
        fun nDevice ->  
            let v = nd.GetValue( devGrd.Columns.[nDevice+1] )
            if v<>null then v.ToString() else ""
    let getVal grp prm = getValN grp prm 0

    // установить значение параметра по индексу и номеру прибора в таблице
    let setValN s grp prm idx = 
        let nd,_ = getn grp prm idx
        fun nDevice -> safelyWinForm devGrd ( fun () -> nd.SetValue( devGrd.Columns.[nDevice+1], s ) ) 
                    
    let set'val s grp prm = setValN s grp prm 0

    let (|Var|_|) (grp,val',idx) = 
        match lst |> Seq.tryFind ( fun (grp', val'', idx', _,_ )-> grp=grp' && val'=val'' && idx=idx') with
        | Some(_, _, _, nd,name ) -> Some(nd,name) | None -> None

    let (|RegToVar|_|) reg = 
        match modbusReg |> Seq.tryFind ( fun (reg',_, _)-> reg'=reg ) with
        | Some(_,grp,val') -> 
            match grp,val',0 with 
            | Var(nd,name) -> Some(grp,val',nd,name) 
            | _ -> None
        | None -> None

    let (|VarToReg|_|) (grp,val') =          
        match modbusReg |> Seq.tryFind ( fun (_,grp',val'')-> grp=grp' && val'=val'' ), (grp,val',0) with
        | Some(reg,_,_), Var(nd,name) ->            
            Some(reg, nd, name) 
        | _ -> None

