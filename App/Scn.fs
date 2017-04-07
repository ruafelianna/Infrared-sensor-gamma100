module Scn

open System
open System.ComponentModel
open System.Threading
open System.Drawing
open System.Windows.Forms

open MIL82Gui
   
let private form = MIL82MainForm.form  
let private grd = form.treeListScenary 
let private glog = NLog.LogManager.GetCurrentClassLogger()

type Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type private Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes

type Result = 
    | [<Description("Выполнено")>] Complete
    | [<Description("Продолжено")>] Continue
    | [<Description("Прервано")>] Canceled          // прервано
    | Fail of string    // ошибка
    override this.ToString() = 
        match this with 
        | Complete -> "Выполнено" 
        | Continue -> "Продолжено"
        | Canceled -> "Прервано"
        | Fail(s) -> "Ошибка|"+s

    member x.LogKind = 
        match x with 
        | Complete -> UI.LogKind.Info
        | Continue -> UI.LogKind.Info
        | Canceled -> UI.LogKind.Warn
        | Fail _ -> UI.LogKind.Error

let private isPaused = ref false

let userBreaked() =
    
    while !isPaused && (not form.Cancellation.IsCancellationRequested) do
        System.Windows.Forms.Application.DoEvents()
    form.Cancellation.IsCancellationRequested

type Performer () =   
    member this.Bind(r, rest) = 
        match r with 
        | _ when userBreaked() -> Canceled
        | Fail(err) -> Fail(err) 
        | Canceled -> Canceled
        | _ -> rest ()
    member this.Return x = x
let private prf = new Performer ()

type ActionDelegate = delegate of unit -> Result
let caction f = new ActionDelegate(f)

type ActionInfo() = 
    static let mutable cnt = 0
    do 
        cnt <- cnt + 1
    let id = cnt
    member this.Id = id
    member val public Visible = true with get, set
    member val public What = "" with get, set
    member val public Arg = null with get, set
    member val public Node:Node = null with get, set
    member val public EnableUserSkip = true with get, set
    member x.UserSkiped() = x.EnableUserSkip && x.Node<>null && x.Node.CheckState=CheckState.Unchecked

type Action = 
    | Single of ActionInfo * ActionDelegate
    | Script of ActionInfo * (Action list)
    member x.Info = match x with Single(r,_) | Script(r,_) -> r
    member x.UserSkiped = x.Info.UserSkiped

let bypassAction foo =
    let rec loop = function 
        | actn::rest -> 
            bypassAction actn
            loop rest
        | [] -> ()
    and bypassAction = function                             
        | Script(_, tsks )  ->  loop tsks
        | Single(nfo, actn) -> foo nfo actn
    bypassAction

let rec actionHasChild action child = 
    if action=child then true else
        match action with
        | Single(_) -> false
        | Script(_,actions) ->
            actions |> List.forall( fun action -> action<>child && (actionHasChild action child |> not ) ) |> not

let actionHasOneOfChildren action = List.forall( fun arg -> actionHasChild action arg |> not ) >> not
    
// выполняется ли сценарий операций
let isPerforming () = form.btnStop.Visible

type LogLevel =  
    | [<Description("Ошибка")>] LogError 
    | [<Description("Результат")>] LogResult
    | [<Description("Ход выполнения")>] LogInfo
    | [<Description("Предупреждение")>] LogWarn
    override this.ToString() = my.getUnionCaseDescription this    
    member this.Color = 
        match this with 
        | LogError -> Color.Red
        | LogResult -> Color.Navy
        | LogInfo -> Color.Black
        | LogWarn -> Color.Maroon
    member this.log : string->unit = 
        match this with
        | LogError -> glog.Error
        | LogResult | LogWarn -> glog.Warn
        | LogInfo -> glog.Info

    member this.kind =  
        match this with
        | LogError -> UI.LogKind.Error
        | LogResult -> UI.LogKind.Result
        | LogWarn -> UI.LogKind.Warn
        | LogInfo -> UI.LogKind.Info

module Perform = 
    
    module CurrentOp =
        let mutable private startTime = 0
        let mutable private errors: (Action*string) list = []
        let stack = new System.Collections.Generic.List<Action>() 
        let top() = if stack.Count=0 then None else Some stack.[stack.Count-1] 
        let topAction() = if stack.Count=0 then None else stack.[stack.Count-1] |> Some
        
        let addError message = 
            if stack.Count>0 then
                errors <- ( stack.[stack.Count-1], message )::errors 

        let log (level:LogLevel) path id message = 
            if level=LogError then addError <| sprintf "%O|%O" id message
            sprintf "%O|%O" id message |> level.log
            my.safelyWinForm form <| fun () ->             
                let message = sprintf "%O: %O" id message
                let message = if message.Length<50 then message else message.Substring( 0, 49)
                form.labelScenaryCurrentActionStatus.Text <- message
                form.labelScenaryCurrentActionStatus.ForeColor <- level.Color
        
        let currentErrors() = 
            let ff acc id = 
                let s = if (acc |> String.length)=0 then "" else ", "
                sprintf "%s%s%d" acc s id
            errors 
            |> List.map( fun(act,error) -> act.Info.Id )
            |> List.toSeq |> Seq.distinct
            |> Seq.fold ff  ""

        let setResult message level =
            if message |> String.IsNullOrEmpty |> not then
                if level=LogError then addError message
                level.log message

        let fix result =             
            let currentErrors = currentErrors()
            let msg, level  =
                match result with   
                | Fail(msg) -> msg, LogError
                | Complete when currentErrors |> String.IsNullOrEmpty |> not ->  "Выполнено с ошибками: "+currentErrors, LogError
                | Complete -> "Выполнено", LogInfo 
                | Canceled -> "Прервано", LogWarn
                | Continue -> "", LogInfo
            setResult msg level                
        let push (op:Action) =            
            startTime <- ticks()            
            stack.Add op
            fix Continue 
        let pop () = 
            if stack.Count>0 then
                stack.Count-1 |> stack.RemoveAt
                startTime <- 0
        let clear () =             
            stack.Clear()
            startTime <- 0
            errors <- []
        let getStartTime() = startTime
        let arg =             
            let arg = ref null
            let getarg() = 
                my.safelyWinForm form.treeListScenary ( fun () -> 
                    arg := 
                        if stack.Count=0 then null else 
                        (stack.[stack.Count-1]).Info.Node.GetValue( form.columnScenaryParametrValue ) ) 
                Some(!arg)
            getarg               
        let getargdef  f def =
            match arg() with
            | Some(s) when s<>null -> match f <| s.ToString() with
                                      | true, v -> v
                                      | _ -> def
            | _ -> def

    let opElepsed() = ticks()-CurrentOp.getStartTime()        

    let foreach (f:int->Result) =
        let rec loop = function 
            | _ when userBreaked() ->  Canceled
            | [] -> Complete
            | ndev::rest ->
                match f ndev with
                | Complete  -> loop rest
                | Continue  -> loop ( rest @ (ndev::[]) )
                | els  ->
                    CurrentOp.log LogError [] (els.ToString()) ""
                    els
        loop [ for n in 0..Dev.count()-1 do if (Dev.isSelected n) then yield n] 
        

    let rec private loopScript = function        
        | _ when userBreaked() -> Canceled
        | actn::rest -> prf {   do! perform actn
                                return loopScript rest }
        | [] -> Complete    
    and perform actn = 
        let perform = function                 
            | _ when userBreaked() -> Canceled                
            | Script(_, tsks )  -> 
                loopScript tsks
            | Single(nfo, actn)     ->                  
                my.safelyWinForm form ( fun () ->
                    form.labelCurrentScenaryAction.Text <- sprintf "%s" nfo.What
                    form.labelCurrentScenaryAction.ForeColor <- Color.Black                     
                    form.labelCurrentScenaryActionTime.Tag <- DateTime.Now
                    form.labelCurrentScenaryActionTime.Text <- "00:00:00"
                    if nfo.Node<>null && nfo.Node.Visible then
                        nfo.Node.TreeList.SetFocusedNode nfo.Node |> ignore )        
                let rec perform () = match actn.Invoke() with Continue -> perform () | els -> els
                perform ()
                       
        let isOpStack = actn.Info.Visible
        if isOpStack then CurrentOp.push actn
        let result = 
            if actn.UserSkiped() then
                CurrentOp.log LogWarn [] actn.Info.What "Снят флажок, разрешающий выполнение операции"
                Complete
            else perform actn
        if isOpStack then 
            CurrentOp.fix result
            CurrentOp.pop ()
        result

    let private currentScenaryStartTime = ref 0
    let private timerPerform = new System.Timers.Timer( Interval=1000., AutoReset=true, Enabled=false )

    let private инициализация_элементов_управления = 
        
        
        form.btnStop.Click.AddHandler( fun _ _ ->
            CurrentOp.log LogLevel.LogInfo [] "Пользователь" "Выполнение прервано"  )

        let setControlTime ctrl = 
            let tag = my.getPropertyValue ctrl "Tag"
            if tag<>null then
                let time = tag :?> DateTime
                let dt = new DateTime(DateTime.Now.Ticks-time.Ticks)
                my.setPropertyValue ctrl "Text" (dt.ToString("HH:mm:ss"))                 

        timerPerform.Elapsed.AddHandler ( fun _ _ -> 
            if isPerforming() then
                try
                    setControlTime form.labelCurrentScenaryActionTime
                    setControlTime form.labelMainActionTime
                with _ -> () )
                

    let setScenarycontrolsVisible() =                  
        let rec loop = function
            | ctrl::rest -> 
                my.setPropertyValue ctrl "Visible" true
                loop rest
            | [] -> ()
        [   box form.labelMainAction
            box form.labelMainActionTime
            box form.labelCurrentScenaryAction
            box form.labelCurrentScenaryActionTime
            box form.labelScenaryCurrentActionStatus
            box form.toolStripSeparator3
            box form.toolStripSeparator4 ]
        |> loop        
      
    let performScenaryAsync (task:Action) =    
        инициализация_элементов_управления
        let stopToken = new System.Threading.CancellationTokenSource()
        
        form.SetCancellationTokenSource( stopToken )
        form.btnRun.Visible <- false
        form.btnStop.Visible <- true
        setScenarycontrolsVisible()

        let what = task.Info.What
        //form.Text <- sprintf "МИЛ-82|%s|Выполняется" what
        form.labelMainAction.Text <- what
        form.labelMainActionTime.Tag <- DateTime.Now
        form.labelMainActionTime.Text <- "00:00:00"
        sprintf "%s - старт" what |> glog.Warn       
        form.treeListData.Refresh()
        currentScenaryStartTime := ticks()
        isPaused := false
        
        CurrentOp.clear()
        timerPerform.Enabled <- true
                
        let showCtrls vis = 
            let rec loop = function
            | ctrl::rest -> 
                my.setPropertyValue ctrl "Enabled" vis
                loop rest
            | [] -> ()
            let box c = c :> obj            
            [   box form.btnReadKefs
                box form.btnWriteKefs
                box form.memuNewFile
                box form.menuOpenFile
                box form.btnAddDevice
                box form.btnDelDevice
                box form.menuDeletePerformLogTreeNode 
                box form.button_run_interrogate]
            |> loop 
        showCtrls false
        
        let doAfter res = 
            my.safelyWinForm form ( fun () ->
                showCtrls true
                let sset s color = 
                    form.labelCurrentScenaryAction.Text <- s
                    form.labelCurrentScenaryAction.ForeColor <- color
                    form.labelCurrentScenaryActionTime.Tag <- DateTime.Now
                    form.labelCurrentScenaryActionTime.Text <- ""
                let currentErrors = CurrentOp.currentErrors()
                let isError = currentErrors |> String.IsNullOrEmpty |> not                
                form.labelScenaryCurrentActionStatus.Visible <- false
                form.labelScenaryCurrentActionStatus.ForeColor <- Color.Black

                form.labelScenaryDelayProgress.Visible <- false
                form.toolStripProgressBar1.Visible <- false

                form.btnRun.Visible <- true
                form.btnStop.Visible <- false 


                let jrnl msg cpt color icon  = 
                   Windows.Forms.MessageBox.Show( msg, cpt, Windows.Forms.MessageBoxButtons.OK, icon ) |> ignore
                   sset msg color
                
                match res with
                | Complete | Continue when isError ->
                    jrnl
                        ( sprintf "Сценарий \"%s\" выполнен с ошибками" what ) 
                        "Выполнено с ошибками" 
                        Color.Maroon
                        MessageBoxIcon.Error
                | Complete | Continue -> 
                    jrnl
                        (sprintf "Сценарий \"%s\" выполнен" what )
                        "Выполнено" 
                        Color.Navy
                        MessageBoxIcon.None
                | Canceled ->
                    jrnl
                        (sprintf "Сценарий \"%s\" прерван пользователем." what )
                        "Прервано" 
                        Color.Maroon
                        MessageBoxIcon.Information
                | Fail(err) ->
                    jrnl
                        (sprintf "Сценарий \"%s\" прерван с ошибкой. %s" what err )
                        err 
                        Color.Red
                        MessageBoxIcon.Stop
                
                )
            Dev.port().Close()
            currentScenaryStartTime := 0
            timerPerform.Enabled <- false     

        my.casync <| 
            fun() -> 
                try
                    prf {  return perform task } |> doAfter 
                with exn ->
                    my.exn exn |> UI.log.Error 
                    exn.Message |> Fail |> doAfter
        |> Async.Start
        
        

let single s f = Single( new ActionInfo(What=s), caction f) 
let foreachWithoutDeviceLog s f = single s (fun () -> Perform.foreach f )
let foreach s f = 
    foreachWithoutDeviceLog s <| fun ndev ->        
        let result = f ndev
        Perform.CurrentOp.fix result
        result

let singleWithArg s arg f  = Single( new ActionInfo(What=s,Arg=arg), caction f) 
let foreachWithArg s arg f  = 
    let r = foreach s f 
    r.Info.Arg <- arg
    r

let timed s getTimeLimit f = single s ( fun () -> 
    if Perform.opElepsed() < getTimeLimit() then f() else Complete ) 

let timedForeach s getTimeLimit f = foreachWithoutDeviceLog s ( fun n -> 
    if ticks()-Perform.CurrentOp.getStartTime() < getTimeLimit() then f n else Complete ) 

let scn s tsk = Script(new ActionInfo(What=s), tsk)

let simpleHidden f = Single( new ActionInfo(Visible=false), caction f )
let foreachHidden f = simpleHidden (fun () -> Perform.foreach f )

let makeScenaryTree =
 
    let (+++) (nds:Nodes) s = 
        let nd = nds.Add [|s|]
        nd.Checked <- true
        nd

    let rec makeActionsList tasks nodes acc =
        match tasks with
        | tsk::rest -> (mkActionLst tsk nodes [] ) @ ( makeActionsList rest nodes acc )
        | []        -> acc

    and mkActionLst (task:Action) nodes acc =
        if not task.Info.Visible then acc else
            let nd = (function Single(nfo,_) | Script( nfo, _ ) -> nodes +++ nfo.What ) task
            task.Info.Node <- nd
            nd.SetValue( form.columnScenaryParametrValue, task.Info.Arg )            
            nd.SetValue( form.columnActionId, task.Info.Id )
            
            match task with
            | Single(nfo, foo)     ->  (Single(nfo, foo) )::acc
            | Script( nfo, tsks )  ->  (Script(nfo, tsks) )::(makeActionsList tsks nd.Nodes []) @ acc

    fun scenary ->
        grd.Nodes.Clear()
        match scenary with 
        | Script(_,scenary) -> makeActionsList scenary grd.Nodes []
        | _ -> []

