module DevOp

open System

type Rx = MyIO.Modbus.Result.Rx
type OkRx = MyIO.Modbus.Result.OkResult

let private form = MIL82Gui.MIL82MainForm.form
let private log = NLog.LogManager.GetCurrentClassLogger()

type Color = System.Drawing.Color

// список неответивших адресов
let private notResponsedAddys = new System.Collections.Generic.HashSet<byte>()

let isNotResponsed = function Dev.TryGetAddy(addy) -> notResponsedAddys.Contains addy | _ -> false

let getAllGood() = [ for n in 0..Dev.count()-1 do if Dev.isSelected n && (not <| isNotResponsed n) then yield n]

let hasOne () = getAllGood() |> List.isEmpty |> not

let canCont() = (not form.Cancellation.IsCancellationRequested) && hasOne()

let cantCont() = not <| canCont()

let log2 level msg msg2 = Scn.Perform.CurrentOp.log level [] msg msg2
let log1 level msg = log2 level msg ""


let setDeviceSatus n (kind:UI.LogKind) (s:string) =
    my.safelyWinForm form.treeListDevices <| fun _ -> 
        if n>=form.treeListDevices.Nodes.Count then 
            failwith "setDeviceSatus n - индекс вне границ массива"
        let node = form.treeListDevices.Nodes.[n]
        node.SetValue( form.columnStatus, s ) 
        node.Tag <- [| new MIL82Gui.TreeLsitCellCustomDraw( kind=kind, ColumnIndex = form.columnStatus.AbsoluteIndex ) |]
        form.treeListDevices.RefreshCell( node, form.columnStatus) 
    let lbl = form.labelScenaryCurrentActionStatus
    my.safelyWinForm lbl.Owner <| fun _ ->
        lbl.ForeColor <- UI.ccellColor kind 
        lbl.Visible <- true
        lbl.Text <- if s.Length<50 then s else s.Substring( 0, 49)

type ModbusReq = Read3 of uint16 | Write16 of uint16*double
let private nothing _ _ = ()

let setTreeListDataFocus ndev grp prm  = 
    let treelist = UI.form.treeListData
    my.safelyWinForm treelist <| fun _ ->
        treelist.FocusedColumn <- treelist.Columns.[ndev+1]
        treelist.FocusedNode <- Var.getNode grp prm

let reqInfo  = 
    let nothing _ _ _ = ()
    function    
    | Read3( Var.Reg3.TryGetKef(kef) )->         
        let setkef kef n rx style = 
            let prm = Var.Kef(kef)
            Var.setVal (rx.ToString()) Var.Koefs prm n
            Var.setCellStyle (Var.Koefs) prm 0 style n
            setTreeListDataFocus n Var.Grp.Koefs prm
        setkef kef, kef.What        
    | Read3( Var.Reg3.TryGetDevVal(_,var) )-> 
        let svar = sprintf "%s.%d" var.What var.Reg3
        let setprm n rx style = 
            let prm = Var.Val(var)
            //setTreeListDataFocus n Var.Interrogate prm
            Var.setCellStyle Var.Interrogate prm 0 style n
            Var.setVal (rx.ToString()) Var.Interrogate prm n
        setprm, svar 
    | Read3(reg) -> 
        nothing, sprintf "рег.%d" reg
    | Write16(Var.Reg3.TryGetKefByCmdCode ( Var.Reg3.TryGetKef(kef) ), arg) ->
        let setprm n rx = 
            match rx with
            | Rx.Ok(OkRx.OkWrite) -> Var.Kef(kef) |> setTreeListDataFocus n Var.Koefs
            | _ -> ()            
        nothing, sprintf "Запись к-та №%d = %g" kef.N arg
    | Write16(Var.CmdDescriptionByCode( what ),arg) ->
        nothing, sprintf "%s <- %g" what arg
    | Write16(cmd,arg) ->
        nothing, sprintf "Команда.%d <- %g" cmd arg

let private processModbus ( rx:Rx) addy enableLog (req:ModbusReq)  = 
    match Dev.tryGetByAddy addy with 
    | None -> ()
    | Some(n,_) ->  
        let cont, what = reqInfo req
        let style, loglev = 
            match rx with 
            | _ when Scn.userBreaked() ->   
                UI.LogKind.Warn, Scn.LogWarn
            | Rx.Ok(_) ->                   
                UI.LogKind.Info, Scn.LogInfo
            | _ ->                          
                UI.LogKind.Error, Scn.LogError
        let srx = rx.ToString()

        sprintf "%s - %s" what srx |> setDeviceSatus n style
        if enableLog then log2 loglev what srx
        cont n rx style

  
// настройки приёмопередачи
let private commSets(): MyIO.Sets = 
    let p = Dev.sets().Devs
    { WriteDelay=p.WriteDelay; Timeout=p.TimeOut; SilentTime = p.SilentTime; RepeatCount=p.RepeatCount; EnableLog = p.EnableLog }

let rxToScn = function
        | _ when Scn.userBreaked() -> Scn.Canceled
        | Rx.Error(err)  -> Scn.Fail(err.ToString())    
        | Rx.Ok(_) | Rx.DeviceFail(_) -> Scn.Complete

type DevicePerformer () =   
    member this.Bind(r, rest) = 
        match r with         
        | Rx.Ok(_) -> rest()         
        | rx -> rxToScn rx
    member this.Return x = x
   
// отправить команду прибору в формате модбас
let write nDevice deviceCommandCode arg = 
    let addy = Dev.getAddy nDevice
    let res = MyIO.Modbus.write16val arg addy deviceCommandCode ( commSets() ) (Dev.port()) (form.Cancellation)
    Write16(deviceCommandCode,arg) |> processModbus res addy true
    res

let private writeBroadcast deviceCommandCode arg = 
    MyIO.Modbus.write16val arg 0uy deviceCommandCode ( commSets() ) (Dev.port()) (form.Cancellation) |> rxToScn


let private readByAddy addy reg enableLog =
    let res = MyIO.Modbus.read3val addy reg ( commSets() ) (Dev.port()) form.Cancellation    
    Read3(reg) |> processModbus res addy enableLog
    res

// считать float число из регистра модбас
let read nDevice reg enableLog = readByAddy (Dev.getAddy nDevice) reg enableLog

let readVar ndev (var:Var.DevVal) = read ndev (Var.getModbus3RegOfVar var) true
    

type private LoopState = EnableLoop | BreakLoop

let interrogateDev nDev =     
    
    let rec loop = function      
        | _ when Scn.userBreaked() -> Scn.Complete
        | [] -> Scn.Continue
        | reg::rest -> 
            let answer = read nDev reg false            
            match answer with
            | _ when Scn.userBreaked() -> Scn.Complete
            | Rx.DeviceFail( MyIO.Modbus.DeviceFailure.NoAnswer ) -> Scn.Complete
            | Rx.Ok(_) | Rx.DeviceFail( _ ) ->  
                loop rest
            | Rx.Error(error) -> Scn.Fail( error.ToString() )
    Var.modbus_vars
    |> List.choose( fun (reg,var) -> 
            let nd = Var.getNode Var.Grp.Interrogate (Var.Val(var))
            if nd.CheckState=Windows.Forms.CheckState.Checked then Some(reg) else None )
    |>loop 
   
let readKef nDevice (kef:Var.Kefs) = read nDevice kef.Reg  true 
let writeKef nDevice kef =  
    let b, value = Var.getKef kef nDevice |> my.tryParseFloat
    if not b then        
        log2 Scn.LogError ( sprintf "Не задано значение к-та №%d" kef.N ) ""
        Rx.Ok( OkRx.OkWrite )
    else
        write nDevice kef.Cmd value

let writeKefr ndev (kef:Var.Kefs) value = 
    let ret = write ndev kef.Cmd value
    match ret with Rx.Ok(_) ->  Var.setKef value kef ndev | _ -> ()
    ret

let loopkefs keffoo (gkefs:unit -> Var.Kefs list) nDev =
    let rec loop = function        
        | _ when Scn.userBreaked() -> [], Scn.Canceled, BreakLoop
        | kef::rest, ret, EnableLoop -> 
            match keffoo nDev kef with
            | Rx.DeviceFail( MyIO.Modbus.DeviceFailure.NoAnswer ) ->
                [], Scn.Complete, BreakLoop
            | Rx.Ok(_) | Rx.DeviceFail( _ ) -> loop (rest, Scn.Complete, EnableLoop)
            | Rx.Error(error) -> [], Scn.Fail(error.ToString()), BreakLoop
        | [], ret, EnableLoop | _, ret, BreakLoop -> [], ret, BreakLoop
    let _,ret,_ = loop (gkefs(), Scn.Complete, EnableLoop) 
    ret

let readKefs = loopkefs readKef
let writeKefs = loopkefs writeKef

let private prf = Scn.Performer()

let logDeviceResult ndev pth id v = 
    Scn.Perform.CurrentOp.log Scn.LogResult ( (Dev.cpt ndev) :: pth  ) id ( v.ToString() )

    let s = pth |>  List.mapi ( fun i arg -> if i>0 then "|"+arg else arg ) |> List.fold ( fun acc arg -> acc+arg ) ""

    Scn.Perform.CurrentOp.log Scn.LogInfo [] (sprintf "%s|%s|%s" (Dev.cpt ndev) s id) v

let logParamValueN grp prm idx ndev = 
    let vnd (nd:UI.Node) = nd.GetValue(form.columnParams).ToString()    
    let nd = Var.getNodeN grp prm 0    
    let ndGrp = nd.ParentNode 
    let v = Var.getValN grp prm idx ndev
    logDeviceResult ndev ((vnd ndGrp)::(vnd nd)::[]) ( (idx+1).ToString() ) v

let logParamValue grp prm ndev = 
    let vnd (nd:UI.Node) = nd.GetValue(form.columnParams).ToString()    
    let nd = Var.getNodeN grp prm 0    
    let ndGrp = nd.ParentNode 
    let v = Var.getVal grp prm ndev
    logDeviceResult ndev ( (vnd ndGrp)::[] ) (vnd nd) v

let setParamN grp prm idx ndev v = 
    Var.setValN v grp prm idx ndev
    //logParamValueN grp prm idx ndev

let setParam grp prm ndev v = 
    Var.setValN v grp prm 0 ndev
    //logParamValue grp prm ndev

let setKefs kefs vals ndev =     
    for (kef,value) in Seq.zip (Seq.cast kefs) (Seq.cast vals) do setParam (Var.Koefs) (Var.Kef(kef)) ndev value


let readModbusValueTask reg ndev f =     
    let rx = read ndev reg true    
    match rx with    
    | Rx.Ok( OkRx.OkRead3Float(v) ) -> f v | _ -> ()
    rxToScn rx

let fixParam grp prm idx ndev f = 
    let s = sprintf "Фиксация \"%s\"" (Var.getCaptionN grp prm idx)        
    let vnd (nd:UI.Node) = nd.GetValue(form.columnParams).ToString()    
    let nd = Var.getNodeN grp prm 0    
    let ndGrp = nd.ParentNode
    readModbusValueTask (Var.prmGetReg3 prm) ndev ( fun v ->
        setParamN grp prm idx ndev v 
        f v)

let fixParamTask grp prm idx = 
    let s = Var.getCaptionN grp prm idx
    Scn.foreach ( sprintf "Фиксировать \"%s\"" s ) ( fun nDevice -> fixParam grp prm idx nDevice (fun _ -> () ) )

let fixParamsTask (grp:Var.Grp) (prms:Var.Prm list) idx = 
    let what = 
        prms |> List.fold( fun acc prm -> 
        let acc = if acc |> String.IsNullOrEmpty then "" else acc+":"
        sprintf "%s%s" acc prm.What) ""
    let what = sprintf "Фиксировать %s-[%s], %s" grp.what what (Var.getNameOfPoint grp idx)
    Scn.foreach what <| fun nDevice -> 
        let rec loop left = 
            match left with
            | [] -> Scn.Complete
            | prm::left -> 
                match fixParam grp prm idx nDevice (fun _ -> () ) with
                | Scn.Complete -> loop left
                | els -> els
        loop prms
        

let clearGrpTask grpa =     
    Scn.foreachHidden ( fun ndev -> 
        for grp,prm,idx,_,_ in Var.grp_vars do
            if grpa=grp then Var.setValN null grp prm idx ndev
        Scn.Complete )

module Cmd =
    let setaddy (addy:byte) =  
        prf {   
                do! writeBroadcast Var.SetAddy.Code (double addy)
                System.Threading.Thread.Sleep 500
                Scn.Perform.CurrentOp.log Scn.LogLevel.LogInfo [] "Тест" "Проверка установленного адреса"
                let r = readByAddy (byte addy)  0us true
                return rxToScn r  }
    
    let send (cmd:Var.Cmd) arg n = write n cmd.Code arg |> rxToScn
    let sendv (cmd:Var.Cmd) arg = Scn.foreach cmd.What ( fun n -> send cmd (arg()) n )

    let senduser cmd  =
        let getarg = Scn.Perform.CurrentOp.getargdef
        match cmd with
        | Var.SetAddy -> 
            Scn.singleWithArg "Установка адреса" 1uy (fun () -> setaddy ( getarg (Byte.TryParse) 1uy )) 
        | _ -> Scn.foreachWithArg cmd.What 0. (fun n ->             
            write n cmd.Code ( getarg (my.tryParseFloat) 0. ) |> rxToScn )

    let sendUserCmd = 
        let r = Scn.foreach "Команда пользователя" ( fun n ->
            let sarg = match Scn.Perform.CurrentOp.arg() with 
                       | Some(s) -> s.ToString() | _-> ""
            let m = System.Text.RegularExpressions.Regex.Match(sarg, "^\\s*(\\d+)\\s+([+\\-]?[0-9]+(?:\\.[0-9]+)?)\\s*$")
            let (b1,cmd),(b2,arg) = 
                if m.Success && m.Groups.Count=3  then
                    UInt16.TryParse m.Groups.[1].Value, 
                    my.tryParseFloat m.Groups.[2].Value
                else (false,0us),(false,0.)
            if b1 && b2 && cmd>0us then
                Scn.Perform.CurrentOp.log Scn.LogInfo [] "отправка команды аользователя" (sprintf "команда %d с аргументом %g" cmd arg )
                write n cmd arg |> rxToScn
            else sprintf "Не верный формат аргумента операции: %s" sarg |> Scn.Fail )
        r.Info.Arg <- "1 1.0"
        r
                
            



    
