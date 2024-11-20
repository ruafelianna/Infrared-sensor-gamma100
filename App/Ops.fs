module Ops

open System
open System.Xml
open System.Xml.Linq
open System.Xml.Serialization
open System.Runtime.InteropServices


let loga = Scn.Perform.CurrentOp.log
let logax : obj -> obj -> unit = loga Scn.LogInfo [] 
let loge : obj -> obj -> unit = loga Scn.LogError  []

let tsetpoint = function
    | Var.Temperature.Nku  -> 20.
    | Var.Temperature.High -> 45.

type DivRes = DivOk of double | DivByZero
let divideCheck x y =
    match y with
    | 0. -> DivByZero
    | _ -> DivOk(x/y)

let private perform = Scn.Perform.perform
type DevPerformer () =   
    member x.Bind(r, rest) = 
        match r with 
        | DevOp.Rx.Ok(DevOp.OkRx.OkRead3Float(v)) -> rest v        
        | els -> els |> DevOp.rxToScn
    member x.Bind(r, rest) = 
        match r with         
        | DevOp.Rx.Ok(DevOp.OkRx.OkWrite) -> rest ()
        | els -> els |> DevOp.rxToScn
    member x.Bind(task, rest) = 
        let r = perform task
        match r with 
        | Scn.Canceled | _ when Scn.userBreaked() -> Scn.Canceled
        | Scn.Fail(err) -> Scn.Fail(err) 
        | _ -> rest ()
    member this.Bind((grp,var,n,ndev), rest) = 
        let sval = Var.getValN grp var n ndev
        match sval |> my.tryParseFloat with 
        | true,v -> rest v
        | _ -> 
            loge (sprintf "%s.%s" (Dev.cpt ndev) (Var.getCaptionN grp var n ) ) (sprintf "Нет значения: \"%s\"" sval)
            Scn.Complete
    member this.Bind( (x,what), rest ) = 
        match x with 
        | DivByZero ->
            loge "Деление на ноль при расчёте" what
            Scn.Complete
        | DivOk(v) -> rest v
    member x.Return y = y
    member x.Zero () = Scn.Complete
let private scn = new DevPerformer ()


module V = 
    open Var
    let conc = Val(Conc)
    let t = Val(Tk)
    let tgm = Val(SensorTGM)
    let var1 = Val(Var1)
    let var2 = Val(Var2)
    let var4 = Val(Var4)
    let var6 = Val(Var6)
    let var11 = Val(Var11)
    let utw = Val(Utw)

    let TestConcResult = Var.TestConcResult |> Var.Val

    module Kefs =
        let linIn = FR2::FR3::FR4::PGS2::PGS3::PGS4::PGS5::[]
        let linOut = K2::K3::K4::K5::B2::B3::B4::B5::[]    



let cloga id msg = 
    Scn.simpleHidden ( fun() ->
        loga Scn.LogInfo [] id (msg())
        Scn.Complete) 

[<DllImport("winmm.dll")>] extern int waveOutSetVolume(IntPtr hwo, uint32 dwVolume);




let messageBox caption message = 
    let timer = new System.Timers.Timer( Interval=1000., AutoReset=true, Enabled=true)
    timer.Elapsed.AddHandler( fun _ _ ->         
        waveOutSetVolume(nativeint(-1), UInt32.MaxValue-1u) |> ignore
        Console.Beep(); )
    my.safelyWinForm UI.form ( fun () ->  
        timer.Enabled <- true      
        Windows.Forms.MessageBox.Show( message, caption, Windows.Forms.MessageBoxButtons.OK ) |> ignore
        timer.Enabled <- false)
    Scn.Complete

let messageBoxTask msg = Scn.simpleHidden <| fun () -> messageBox "Автоматическая интерактивная настройка" msg

let switchGas (clapan:Pneumo.Clapan) = 
    messageBoxTask <| sprintf "%s и нажмите \"Ok\"" clapan.WhatDo 
        


let do_delay what limit =      
    let tm_start = ref 0
    let getElepsed() = ticks() - !tm_start


    let showOperationTime s limit = 
        let cdt ms = ms |> float |> (new DateTime()).AddMilliseconds
        let elepsed = getElepsed()
        let left = limit - elepsed
        let f v = (cdt v).ToString("HH:mm:ss")
        if left>0 then 
            let lbl = UI.form.labelScenaryDelayProgress
            my.safelyWinForm lbl.Owner <| fun() ->
                let pg = UI.form.toolStripProgressBar1
                pg.Visible <- true
                pg.Maximum <- int limit
                pg.Minimum <- 0
                pg.Value <- elepsed
                lbl.Visible <- true
                lbl.Text <- sprintf "%s %s-%s" s (f elepsed) (f left)
                lbl.ForeColor <- System.Drawing.Color.Navy   

    let setvivsibleButtonSkip vis = 
        Scn.simpleHidden <| fun () ->
            tm_start := ticks()
            my.safelyWinForm UI.form.toolStripRunStop <| fun () ->                
                let lbl = UI.form.labelScenaryDelayProgress        
                let pg = UI.form.toolStripProgressBar1
                pg.Visible <- vis
                lbl.Visible <- vis
                UI.form.labelScenaryCurrentActionStatus.Visible <- not vis
            Scn.Complete
    [   
        yield setvivsibleButtonSkip true
        yield Scn.foreachHidden  (fun n -> 
            showOperationTime what (limit())
            if getElepsed() < limit() then 
                try
                    DevOp.interrogateDev n 
                with _ -> Scn.Continue
            else Scn.Complete) 
        yield setvivsibleButtonSkip false ] 

let blow =      
    fun (gas:Pneumo.Clapan) tm ->
    let whatBlow =  gas.WhatBlow          
    let limit() = ( my.milliseconds (tm()) ) * oneTick    
    [   yield switchGas gas
        yield! do_delay whatBlow limit ] 
    |> Scn.scn whatBlow

let message_delay what message tm =
    let limit() = ( my.milliseconds (tm()) ) * oneTick    
    [   yield messageBoxTask message
        yield! do_delay what limit ] 
    |> Scn.scn what
 
let blowGas k = blow k (fun() -> Dev.sets().BlowGasTime )

// предел основной абсолютной погрешности
let basicConcErrorLimit conc = 
    let _,_,scale,errorlimit = Dev.kind()
    let errorlimit = double errorlimit
    (max conc scale.Mid)*errorlimit/100.

let concerr conc gas kefd =
    let maxd = basicConcErrorLimit conc
    let abserr = conc - (Pneumo.pgs gas) |> abs
    let relerr = abserr/maxd
    abserr, relerr, maxd, abserr<=maxd*kefd

let interrrogate =   DevOp.interrogateDev |> Scn.foreachWithoutDeviceLog "Опрос параметров" 

module Kefs = 
    open DevOp

    let fmt (kefs: Var.Kefs list) = kefs |> List.fold  ( fun acc kef->
        sprintf "%s%s%d" acc (if acc |> String.IsNullOrEmpty |> not then ", " else "") kef.N  ) ""    

    let private selected() = 
        Var.kefs |>
        List.choose( fun kef -> 
            let nd = Var.getNode Var.Koefs (Var.Kef(kef))
            if nd.CheckState=Windows.Forms.CheckState.Checked then Some(kef) else None ) 

    let readselected = readKefs selected  |> Scn.foreach "Считывание к-тов" 
    let writeselected =  writeKefs selected |> Scn.foreach "Запись к-тов"

    let private loopkefs f kefs nDev = loopkefs f (fun() -> kefs) nDev
    let write,read =    
        let foo f s = (fun kefs -> f kefs  |> Scn.foreach (sprintf "%s к-тов %s" s (fmt kefs)))
        foo (loopkefs writeKef) "Запись",
        foo (loopkefs readKef)  "Считывание"

let resetKefs kefs ndev =
    for kef in kefs do
        Var.setKef null kef ndev

let resetData grp = 
    Scn.foreachHidden( fun ndev ->
    for grpa,prm,n,_,_ in Var.grp_vars do
        if grp=grpa then 
            Var.setValN null grp prm n ndev    
    Scn.Complete)

// запись основных коэфициентов
let setMainKefs = 
    let sets = Dev.sets
    let cs v = v.ToString()
    let mainKefs = 
        [   let scale() = 
                let _,_,scale,_ =  Dev.kind()
                scale            
            yield Var.Year,         fun _ -> DateTime.Now.Year |> cs
            yield Var.Serial,       fun n -> Dev.getSerial n            
            yield Var.Gas,          fun _ -> 
                let gas,_,_,_ =  Dev.kind()
                gas.Code |> cs
            yield Var.Units,        fun _ -> 
                let _,units,_,_ =  Dev.kind()
                units.Code |> cs
            yield Var.Scale,        fun _ -> scale().Code |> cs ]
    let kefs = mainKefs |> List.map( fun (kef,_) -> kef )
    let s = sprintf "%s" (Kefs.fmt kefs)
    [   let cpt = sprintf "Установка значений к-тов %s" s
        
        yield cloga "Исполнение" Dev.formatkind
    
        yield Scn.foreach cpt ( fun n ->
            for kef, s in mainKefs do Var.setKef (s(n)) kef n
            Scn.Complete )
        yield Kefs.write kefs ] 
    |> Scn.scn "Установка коэффициентов"

let testConcErrorScenary (grp:Var.Grp) (gas:Pneumo.Clapan) nconc ndev kefd =    
    let setTestResult v style = 
        Var.setValN v grp V.TestConcResult nconc ndev
        Var.setCellStyle grp V.TestConcResult nconc style ndev
    let what = sprintf "%s, %s, проверка погрешности %gd" (Dev.cpt(ndev)) gas.WhatPgs kefd    
    setTestResult "" UI.LogKind.Info
    let b,conc = Var.getValN grp V.conc nconc ndev |> my.tryParseFloat
    if not b then
        loge "Нет значения концентрации" ""
    else
        let abserr, relerr, maxd, ok = concerr conc gas kefd
        logax "Концентрация" conc
        logax (gas.WhatPgs) (Pneumo.pgs gas)
        logax "Абс.погр." abserr
        let relerr = sprintf "%2.0f" relerr
        logax "Отн.погр." relerr
        logax "Макс.доп.погр. по ТУ" maxd
        let sres = sprintf "%g %s %g*%g" abserr (if ok then "<" else ">") maxd kefd

        sprintf """%s

Концентрация 
    показания: %g 
    %s: %g

Погрешность
    абсолютная: %g, 
    относительная: %s, 
    максимально допустимая: %g            
                        
Результат: %s, %sУДОВЛЕТВОРИТЕЛЬНО
    
Результаты расчёта сохранены во вкладке Данные, графы Параметры, разделе Проверка показаний"""
                what conc gas.WhatPgs (Pneumo.pgs gas) abserr relerr maxd sres (if ok then "" else "НЕ ")
        |> messageBox  "Проверка погрешности" |> ignore

        let s1, s2, lk = if ok then "Да", "", UI.LogKind.Info else "Нет", "не ", UI.LogKind.Error
        logax "Удовл." s1 
        setTestResult ( sprintf "%s, %s удовл." sres s2) lk
        
    Scn.Perform.CurrentOp.pop()


let testConcTask grp nconc (gas:Pneumo.Clapan) kefd =    
    Scn.scn ( sprintf "Проверка погрешности по %s, %gd" gas.WhatPgs kefd) <| 
    [   yield DevOp.fixParamTask grp V.conc nconc
        yield Scn.foreach ( sprintf "Расчёт погрешности по %s" gas.WhatPgs) <|  fun ndev ->
            testConcErrorScenary grp gas nconc ndev kefd 
            Scn.Complete ]

let testNotMeasuredTask =    
    let cpt n = Var.getCaptionN Var.TestNotMeasured V.conc n
    let what_pgs n = (Var.getNodeN Var.TestNotMeasured V.conc n).GetDisplayText( UI.form.columnParams)
    
    let blow n =       
        let what = what_pgs n
        let limit() = ( my.milliseconds ( Dev.sets().BlowGasTime ) ) * oneTick        
        [   yield Scn.simpleHidden <| fun() ->
                messageBox ("Продувка " + what) (sprintf "Подайте неизмеряемый компонент %s согласно приложению В" what)
            yield! do_delay (what_pgs n) limit ] 
        |> Scn.scn ("Продувка " + what)
            


    Scn.scn "Проверка влияния неизмеряемых" <| 
    [   yield blowGas Pneumo.K1
        yield DevOp.fixParamTask Var.TestNotMeasured V.conc 0
        for n in 1..4 do
            yield blow n
            yield DevOp.fixParamTask Var.TestNotMeasured V.conc n 
            //yield n |> what_pgs |> sprintf "Подайте газ %s согласно приложению В" |> messageBoxTask
            
            yield Scn.foreach ( sprintf "Расчёт погрешности по %s" (cpt n) ) <|  fun ndev ->
                let setTestResult v nconc style = 
                    Var.setValN v Var.TestNotMeasured V.TestConcResult nconc ndev
                    Var.setCellStyle Var.TestNotMeasured V.TestConcResult nconc style ndev
                

                scn{
                    let! conc0 = Var.TestNotMeasured, V.conc, 0, ndev
                    logax (cpt 0) conc0
                    let! conc = Var.TestNotMeasured, V.conc, n, ndev
                    logax (cpt n) conc
                    let maxd = basicConcErrorLimit conc
                    logax "Макс.доп.погр. по ТУ" maxd
                    let _,_,scale,_ = Dev.kind()
                    let d = (conc0 - conc)/(scale.Scale * maxd)
                    logax "Погрешность" d
                    let ok = abs( d ) < maxd*0.5
                    let sres = sprintf "%g %s %g*0.5" (abs d) (if ok then "<" else ">") maxd
                    let result = 
                        if ok then 
                            logax "Удовл." "Да" 
                            setTestResult ( sprintf "%s, удовл." sres) n UI.LogKind.Info
                        else 
                            loge "Удовл." "Нет" 
                            setTestResult ( sprintf "%s, не удовл." sres) n UI.LogKind.Error 
                    sprintf """Проверка влияния неизмеряемых

Концентрация 
    показания при ПГС-ГСО №1: %g 
    показания при %s: %g

Погрешность
    максимально допустимая: %g
    относительная: %g 
                
                        
Результат: %s, %sУДОВЛЕТВОРИТЕЛЬНО""" conc0 (what_pgs n) conc maxd d sres (if ok then "" else "НЕ ")
                    |> messageBox  "Проверка погрешности" |> ignore
                    return Scn.Complete }
//                let b,conc0 = Var.getValN Var.TestNotMeasured V.conc n ndev |> my.tryParseFloat
//                if not b then loge "Нет значения концентрации" (cpt 0)  else
//                    logax "Концентрация" conc
//
//                Scn.Complete 
                ]

let sleep (n:int) = System.Threading.Thread.Sleep n

let fixKnull() = 
    Scn.foreach "Knul <- -var3" <| fun ndev ->
    scn{
        let! value = DevOp.readVar ndev Var.Var3
        do! DevOp.writeKefr ndev Var.Knul (-value)
        sleep 5000
        return Scn.Complete } 
let fixKScale() =
    Scn.foreach "KSkale <- ПГС4/var9; PGS5 <- ПГС4" <| fun ndev ->
    scn{
        let! var9 = DevOp.readVar ndev Var.Var9                
        let a = Pneumo.pgs Pneumo.K4
        do! DevOp.writeKefr ndev Var.KSkale (if var9<>0. then a/var9 else 0.)
        do! DevOp.writeKefr ndev Var.PGS5 a
        sleep 5000
        return Scn.Complete } 

// калибровка
let adjust grp = 
    let adjust0 = 
        [   yield blowGas Pneumo.K1
            yield fixKnull()         
            yield testConcTask grp 0 Pneumo.K1 0.2]
        |> Scn.scn "Корректировка нуля"
    let adjustE = 
        [   yield blowGas Pneumo.K4
            yield fixKScale()
            yield testConcTask grp 1 Pneumo.K4 0.2]    
        |> Scn.scn "Корректировка чувствительности"
    [   yield adjust0 
        yield adjustE  ] 
    |> Scn.scn "Корректировка показаний"

let logCalcKefs ndev x y k what =
    let fseq = Seq.mapi ( fun i v -> if i>0 then sprintf "; %g" v else sprintf "%g" v )  >> Seq.fold ( fun acc s -> acc+s ) ""
    let fseq s = "[" + (fseq s) + "]"
    let s = sprintf "x=%s, y=%s -> k=%s" (fseq x) (fseq y) (fseq k)
    DevOp.logDeviceResult ndev ( "Расчёт коэффициентов"::[] ) what s

let logCantCalcKefs ndev kefs what = 
    let id = (Dev.cpt ndev) 
    let msg = sprintf "Недостаточно исходных данных для расчёта к-тов, %s, %s" what (Kefs.fmt kefs)     
    loga Scn.LogError [] id msg
    
let testMainConcScenary =     
    [   for n, ngas in Var.testMainErrorList |> List.mapi( fun i x -> i,x ) do
            let gas = Pneumo.Clapan.ByNum ngas
            yield blowGas gas
            yield testConcTask Var.IndicationTest n gas 0.2 ]
    |> Scn.scn "Проверка основной погрешности"
  
//линеаризатор
let linear =  
    [   Scn.scn "Подготовка исходных данных для расчёта линеаризатора" <|
        [   blowGas Pneumo.K1
            fixKnull() 
            blowGas Pneumo.K4
            fixKScale()
            blowGas Pneumo.K3
            Scn.foreach "FR4 <- var10; PGS4 <- ПГС3" <| fun ndev ->
                scn{
                    let! var10 = DevOp.readVar ndev Var.Var10                
                    do! DevOp.writeKefr ndev Var.PGS4 (Pneumo.pgs Pneumo.K3)              
                    do! DevOp.writeKefr ndev Var.FR4 var10                
                    return Scn.Complete } 
            blowGas Pneumo.K5
            Scn.foreach "FR3 <- var10; PGS3 <- ПГС5" <| fun ndev ->
                scn{
                    let! var10 = DevOp.readVar ndev Var.Var10                
                    do! DevOp.writeKefr ndev Var.PGS3 (Pneumo.pgs Pneumo.K5)             
                    do! DevOp.writeKefr ndev Var.FR3 var10                
                    return Scn.Complete } 
            blowGas Pneumo.K2
            Scn.foreach "FR2 <- var10; PGS2 <- ПГС2" <| fun ndev ->
                scn{
                    let! var10 = DevOp.readVar ndev Var.Var10                
                    do! DevOp.writeKefr ndev Var.PGS2 (Pneumo.pgs Pneumo.K2)             
                    do! DevOp.writeKefr ndev Var.FR2 var10                
                    return Scn.Complete } 
            Kefs.read V.Kefs.linIn ]
        Scn.foreach "Расчёт линеаризатора" <| fun ndev ->
            scn{
                let read = DevOp.readKef ndev                 
                let! (pgs5 : double) = read Var.PGS5
                let! pgs4 = read Var.PGS4
                let! pgs3 = read Var.PGS3
                let! pgs2 = read Var.PGS2
                let! fr4 = read Var.FR4
                let! fr3 = read Var.FR3
                let! fr2 = read Var.FR2

                let k5 = if pgs5=fr4 then 0. else (pgs5-pgs4)/(pgs5-fr4)
                let b5 = pgs4-k5*fr4
                let k4 = if fr4=fr3 then 0. else (pgs4-pgs3)/(fr4-fr3)
                let b4 = pgs3-k4*fr3
                let k3 = if fr3=fr2 then 0. else (pgs3-pgs2)/(fr3-fr2)
                let b3 = pgs2-k3*fr2
                let k2 = if fr2=0. then 0. else pgs2/fr2
                let b2 = 0.

                (Var.K5,k5)::(Var.K4,k4)::(Var.K3,k3)::(Var.K2,k2)::
                (Var.B5,b5)::(Var.B4,b4)::(Var.B3,b3)::(Var.B2,b2)::[]
                |> List.iter( fun(kef,value) -> Var.setKef value kef ndev )
                return Scn.Complete }
        Kefs.write V.Kefs.linOut 
        testMainConcScenary ]
    |> Scn.scn "Ввод функции преобразования" 

let signalPhaseK51  =    
    let kef = Var.Phase_Start   
    let dlg = new MIL82Gui.FormK31()
    let rec actn ndev =            
        scn {
            let! kefValue = DevOp.readKef ndev kef
            my.safelyWinForm UI.form <| ( fun () ->
                dlg.label2.Text <- Dev.cpt ndev
                dlg.label2.Text <- sprintf "Kоэффициент 31 = %g" kefValue 
                dlg.ShowDialog() |> ignore)
            if dlg.Selection=0. then return Scn.Complete else            
                let kefValue = kefValue + dlg.Selection
                do! DevOp.writeKefr ndev kef kefValue
                return actn ndev }
    Scn.foreach "Настройка фазы сигнала (коэффициент 31)" <| actn

let testGrpIndication (grp:Var.Grp) (gas:Pneumo.Clapan) kefd =    
    
    Scn.foreach (sprintf "Проверка показаний, %s, %s, ±%gY" grp.what gas.WhatPgs kefd) <| fun ndev ->
    let setTestResult v style = 
        Var.setValN v grp V.TestConcResult 0 ndev
        Var.setCellStyle grp V.TestConcResult 0 style ndev
    scn{
        let! conc   = Var.Press, V.conc,  0, ndev        
        let! concx  = Var.Press, V.conc,  1, ndev
        let dconc   = conc-concx        
        let errorLimit = basicConcErrorLimit <| Pneumo.pgs Pneumo.K4
        let okconc = conc-concx |> abs < kefd*errorLimit
        let logx,style,s1 = 
            if okconc then 
                logax, UI.LogKind.Info ,"<"
            else 
                loge, UI.LogKind.Error, ">"
        let result = sprintf "конц.0-конц.1 = %g-%g = %g %s ±%g*%g" conc concx dconc s1 errorLimit kefd
        Var.setValN result grp V.TestConcResult 0 ndev
        Var.setCellStyle grp V.TestConcResult 0 style ndev
        logx result  ""
        return Scn.Complete }

let docalc2 (x:Var.Kefs) (y:Var.Kefs) (grp:Var.Grp) (var1:Var.DevVal) (var2:Var.DevVal) =
    let uni = my.unionToString
    [   yield Scn.foreach ( sprintf "Расчёт коэффициентов %s, %s по данным %s, %s группы %s" (uni x) (uni y) (uni var1) (uni var2) (uni grp) ) <| fun ndev ->
            Var.setKef "" x ndev
            Var.setKef "" y ndev
            scn{
                let! v1     = grp, var1 |> Var.Val,  0, ndev
                let! v2     = grp, var2 |> Var.Val, 0, ndev                
                let! v1x    = grp, var1 |> Var.Val,  1, ndev
                let! v2x    = grp, var2 |> Var.Val,  1, ndev
                logax "Исходные данные" (sprintf "%s(0)=%g, %s(1)=%g, %s(0)=%g, %s(1)=%g" (uni var1) v1 (uni var1) v1x (uni var2) v2 (uni var2) v2x )                    
                let formulaY = sprintf "%s=[1-%s(0)/%s(1)]/[(%s(0)-%s(1))]" (uni y) (uni var1) (uni var1) (uni var2) (uni var2)
                let a = v2
                let! b = divideCheck v1 v1x, sprintf "%s(0)/%s(1)" (uni var1) (uni var1)
                let c = v2x
                let! valueY = divideCheck (1.-b) (a-c), formulaY
                let valueX = 1. - a*valueY
                logax ( sprintf "%s=1-%s(0)*%s" (uni x) (uni var2) (uni y) ) valueX
                logax formulaY y
                Var.setKef valueX x ndev
                Var.setKef valueY y ndev
                return Scn.Complete } 
        yield x::y::[] |> Kefs.write ]


let docalc3 (k : Var.Kefs list)  (grp:Var.Grp) (var1:Var.DevVal) (var2:Var.DevVal) =
    let uni = my.unionToString
    let log = NLog.LogManager.GetCurrentClassLogger()
    let ff s = 
        let ff = Seq.mapi ( fun i v -> if i>0 then sprintf "; %g" v else sprintf "%g" v )  >> Seq.fold ( fun acc s -> acc+s ) ""
        "[" + (ff s) + "]"
    let round6 (x:float) = System.Math.Round(x,6)
    let what = sprintf "Расчёт коэффициентов %s, %s, %s" (uni k.[0]) (uni k.[1]) (uni k.[2]) 
    [   yield Scn.foreach ( sprintf "%s по данным %s, %s группы %s" 
                                what  (uni var1) (uni var2) (uni grp) ) <| fun ndev ->
            Var.setKef "" k.[0] ndev
            Var.setKef "" k.[1] ndev
            Var.setKef "" k.[2] ndev
            scn{
                let! x0    = grp, var1 |> Var.Val,  2, ndev
                let! x1    = grp, var1 |> Var.Val,  0, ndev
                let! x2    = grp, var1 |> Var.Val,  1, ndev

                let! y0    = grp, var2 |> Var.Val,  2, ndev
                let! y1    = grp, var2 |> Var.Val,  0, ndev
                let! y2    = grp, var2 |> Var.Val,  1, ndev
                                
                let x = [| x0; x1; x2 |]
                let y = [| y1 - y0; 0; y1 - y2 |]
                let kv = 
                    mynumeric.GaussInterpolation.calculate(x, y)
                    |> Array.map round6 |> Array.toList
                sprintf "%s X=%s, Y=%s -> %s" what (ff x) (ff y) (ff kv)
                |> log.Info

                List.zip k kv
                |> List.iter( fun (k,v) ->
                    Var.setKef v k ndev )
                return Scn.Complete } 
        yield Kefs.write k ]

let docalc4 (k : Var.Kefs list) (grp:Var.Grp) (grp0:Var.Grp) (varx:Var.DevVal) (vary0:Var.DevVal) (vary:Var.DevVal) =
    let uni = my.unionToString
    let log = NLog.LogManager.GetCurrentClassLogger()
    let ff s = 
        let ff = Seq.mapi ( fun i v -> if i>0 then sprintf "; %g" v else sprintf "%g" v )  >> Seq.fold ( fun acc s -> acc+s ) ""
        "[" + (ff s) + "]"
    let round6 (x:float) = System.Math.Round(x,6)
    let what = sprintf "Расчёт коэффициентов %s, %s, %s" (uni k.[0]) (uni k.[1]) (uni k.[2]) 
    [   yield Scn.foreach ( sprintf "%s по данным %s, %s, %s группы %s" 
                                what  (uni varx) (uni vary0) (uni vary) (uni grp) ) <| fun ndev ->
            Var.setKef "" k.[0] ndev
            Var.setKef "" k.[1] ndev
            Var.setKef "" k.[2] ndev
            scn{
                let! x0    = grp, varx |> Var.Val,  2, ndev
                let! x1    = grp, varx |> Var.Val,  0, ndev
                let! x2    = grp, varx |> Var.Val,  1, ndev

                let! y00    = grp, vary0 |> Var.Val,  2, ndev
                let! y01    = grp, vary0 |> Var.Val,  0, ndev
                let! y02    = grp, vary0 |> Var.Val,  1, ndev

                let! y0    = grp0, vary |> Var.Val,  2, ndev
                let! y1    = grp0, vary |> Var.Val,  0, ndev
                let! y2    = grp0, vary |> Var.Val,  1, ndev

                let y0 = y0 - y00
                let y1 = y1 - y01
                let y2 = y2 - y02
                                
                let x = [| x0; x1; x2 |]
                let y = [| y1 / y0 ; 1.; y1 / y2 |]
                let kv = 
                    mynumeric.GaussInterpolation.calculate(x, y)
                    |> Array.map round6 |> Array.toList
                sprintf "%s X=%s, Y=%s -> %s" what (ff x) (ff y) (ff kv)
                |> log.Info

                List.zip k kv
                |> List.iter( fun (k,v) ->
                    Var.setKef v k ndev )
                return Scn.Complete } 
        yield Kefs.write k ]

let pressure = 
    let fixpoint npt =     
        let prms = Var.Var6::Var.Var11::Var.Conc::[] |> List.map Var.Prm.Val
        DevOp.fixParamsTask Var.Press prms npt

    
    Scn.scn "Настройка по давлению" <| 
    [   yield adjust Var.AdjustBeforPressure
        yield blowGas Pneumo.K4
        yield fixpoint 0
        yield messageBoxTask <|
            "Установите с помощью вентилей точной регулировки по манометру"+
            "избыточное давление 8±1 кПа, поддерживая постоянный расход 0,9±0,1 л/мин"
        yield! do_delay "выдержка под давлением" (fun () -> my.milliseconds ( Dev.sets().PressureTime )  )
        yield fixpoint 1
        yield! docalc2 Var.Kefs.D0 Var.Kefs.D1 Var.Press Var.Var6 Var.Var11
        yield testGrpIndication Var.Press Pneumo.K3 0.4 ]

let docalc1 (x:Var.Kefs) (y:Var.Kefs) grp (var1:Var.DevVal) (var2:Var.DevVal) =
    let uni = my.unionToString    
    [   yield Scn.foreach ( sprintf "Расчёт коэффициентов %s, %s по данным %s, %s группы %s" (uni x) (uni y) (uni var1) (uni var2) (uni grp) ) <| fun ndev ->
            Var.setKef "" x ndev
            Var.setKef "" y ndev
            scn{       
                let! v1     = grp, var1 |> Var.Val, 0, ndev
                let! v1x    = grp, var1 |> Var.Val, 1, ndev         
                let! v2     = grp, var2 |> Var.Val, 0, ndev
                let! v2x    = grp, var2 |> Var.Val, 1, ndev
                logax "Исходные данные" (sprintf "%s(0)=%g, %s(1)=%g, %s(0)=%g, %s(1)=%g" (uni var1) v1 (uni var1) v1x (uni var2) v2 (uni var2) v2x )
                let formulaY = sprintf "%s=[%s(0)-%s(1)]/[%s(1)-%s(0)]" (uni y) (uni var1)(uni var1)(uni var2)(uni var2)
                let a = v2
                let b = v1-v1x
                let c = v2x
                let! valueY = divideCheck b (c-a), formulaY
                let valueX = - a*valueY
                logax (sprintf "%s=-%s(0)*%s" (uni x) (uni var2) (uni y) ) valueX
                logax formulaY valueY
                Var.setKef valueX x ndev
                Var.setKef valueY y ndev
                return Scn.Complete } 
        yield x::y::[] |> Kefs.write]



let humidity = 
    let fixpoint npt =     
        let prms = Var.Var1::Var.SensorTGM::Var.Conc::[] |> List.map Var.Prm.Val        
        DevOp.fixParamsTask Var.Humidity prms npt
    
    Scn.scn "Настройка по влаге" <| 
    [   yield adjust Var.AdjustBeforHumidity
        yield message_delay 
                "Продуть сухой газ" 
                "Подайте \"сухой\" газ" 
                (fun() -> Dev.sets().BlowGasTime )
        yield fixpoint 0
        yield message_delay 
                "Продуть влажный газ" 
                "Установите стенд ЭН8800-4415 в режим 30%. Установите расход ГСО-ПГС№1 0,9±0,1 л/мин" 
                (fun() -> Dev.sets().BlowWetGasTime )
        yield fixpoint 1
        yield! docalc1 Var.Kefs.W0 Var.Kefs.W1 Var.Humidity Var.Var1 Var.SensorTGM        
        yield testGrpIndication Var.Humidity Pneumo.K1 0.5 ]

let termo = 
    let fix0 grp npt =     
        let prms = Var.Var1::Var.Tk::Var.Up::Var.Utw::[] |> List.map Var.Prm.Val        
        DevOp.fixParamsTask grp prms npt
    let fixE grp npt =     
        let prms = Var.Var4::Var.Tk::[] |> List.map Var.Prm.Val        
        DevOp.fixParamsTask grp prms npt
    Scn.scn "Настройка по температуре" <| 
    [   Scn.scn "Н.к.у." [  messageBoxTask "Установите нормальные климатические условия в термокамере и продуйте газ 1"
                            adjust Var.AdjustBeforTermo
                            blowGas Pneumo.K1
                            fix0 Var.Termo0 0
                            blowGas Pneumo.K4 
                            fixE Var.TermoE 0 ]
        Scn.scn "Пониженная температура" <|
            [   messageBoxTask "Установите пониженную температуру в термокамере и продуйте газ 1"
                blowGas Pneumo.K1
                fix0 Var.Termo0 2
                blowGas Pneumo.K4 
                fixE Var.TermoE 2 ]

        Scn.scn "Повышенная температура" <|   
            [   yield message_delay "Продуть газ 1" "Установите повышенную температуру в термокамере и продуйте газ 1" (fun() -> Dev.sets().BlowGasTime )
                yield fix0 Var.Termo0 1
                yield! docalc1 Var.Kefs.A0 Var.Kefs.A1 Var.Termo0 Var.Var1 Var.Tk
                yield! docalc1 Var.Kefs.C0 Var.Kefs.C1 Var.Termo0 Var.Up Var.Tk
                yield Scn.foreach "Расчёт коэффициента Ktw" <| fun ndev ->
                    Var.setKef "" Var.Kefs.Ktw ndev
                    scn{                
                        let! tk0    = Var.Termo0, V.t, 0, ndev
                        let! tk1    = Var.Termo0, V.t, 1, ndev
                        let! utw0   = Var.Termo0, V.utw, 0, ndev
                        let! utw1   = Var.Termo0, V.utw, 1, ndev
                        logax "Исходные данные" (sprintf "Tk0=%g, Tk1=%g, Utw0=%g, Utw1=%g" tk0 tk1 utw0 utw1)
                        let formula = "Ktw=(Tk1-Tk0)/(Utw1-Utw0)"                
                        let! ktw = divideCheck (tk1-tk0) (utw1-utw0), formula
                        logax (sprintf "Ktw=%s" formula )  ktw
                        Var.setKef ktw Var.Kefs.Ktw ndev
                        return Scn.Complete } 
                yield Var.Ktw::[] |> Kefs.write
                yield Scn.simpleHidden <| fun () ->
                    sleep 5000
                    Scn.Complete
                yield testConcTask Var.Test_Termo0 0 Pneumo.K1 0.2
                yield blowGas Pneumo.K4
                yield fixE Var.TermoE 1

                yield! docalc2 Var.Kefs.B0_T Var.Kefs.B1_T Var.TermoE Var.Var4 Var.Tk

                yield Scn.scn "Расчёт по трём точкам" 
                    [   yield! docalc3 [Var.Kefs.A0; Var.Kefs.A1; Var.Kefs.A2] Var.Termo0 Var.Tk Var.Var1
                        yield! docalc4 [Var.Kefs.B0_T; Var.Kefs.B1_T; Var.Kefs.B2_T] Var.TermoE Var.Termo0 Var.Tk Var.Var4 Var.Var1  ]

                yield testConcTask Var.Test_TermoE 0 Pneumo.K4 0.2 
                yield Scn.foreachHidden <| fun ndev ->
                    Var.setValN (DateTime.Now.ToString("dd MMMM yyyy HH:mm")) Var.Grp.Test_Date (Var.Val( Var.TestDate )) 0 ndev
                    Scn.Complete
                
                ] ]

let presentOTK =     
    [   for nday in 0..3 do
            let blow = blow Pneumo.Off <| fun _ -> Dev.sets().ДлительностьЦиклаТехпрогона |> int |> Data.chours
            blow.Info.What <- sprintf "Цикл техпрогона №%d" (nday+1)

            yield Scn.scn (sprintf "День %d" (nday+1) ) <| 
                [   yield blow
                    let n = nday * 3
                    for gas,ngas in (Pneumo.K1,0)::(Pneumo.K3,1)::(Pneumo.K1,2)::[] do
                        yield blowGas gas
                        yield testConcTask Var.OTKPresent (n+ngas) gas 1. 
                        yield Scn.foreachHidden <| fun ndev ->
                            Var.setValN (DateTime.Now.ToString("dd MMMM yyyy HH:mm")) Var.Grp.Test_Date (Var.Val( Var.TestDate )) (nday+1) ndev
                            Scn.Complete
                        ] ]
    |> Scn.scn "Сдача ОТК"



let commands =              DevOp.Cmd.sendUserCmd :: (Var.Cmd.cases |> List.map DevOp.Cmd.senduser) |> Scn.scn "Отправка команды"
let kefs =                  Kefs.writeselected::Kefs.readselected::[]  |>  Scn.scn "Коэффициенты"

let manual =                interrrogate :: kefs :: commands :: [] |> Scn.scn "Ручное управление"

let automation =     
    [   messageBoxTask "Подключите датчики к СОМ-порту и подайте на датчики питание"
        setMainKefs
        signalPhaseK51
        linear
        pressure
        humidity
        termo 
        testNotMeasuredTask ] |>
    Scn.scn "Настройка ИК датчика ГАММА-100" 
let scenary =
    manual |> Scn.bypassAction ( fun nfo dlgt -> nfo.EnableUserSkip <- false )
    manual :: (adjust Var.Adjust) :: automation :: presentOTK :: [] |>
    Scn.scn "Сценарий" 
let disableUserStartList = manual :: kefs :: commands :: scenary :: []

type private Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type private Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes


type ScenaryActionInfo() =     
    member val public ChechedState = Windows.Forms.CheckState.Checked with get, set    
    member val public Arg = "" with get, set

type ScenaryActionInfos() = 
    [<XmlArray("ScenaryActionInfos")>]    
    [<XmlArrayItem(typeof<ScenaryActionInfo>)>] 
    member val public Nfos : ScenaryActionInfo array = [||] with get, set

let operations = Scn.makeScenaryTree scenary

let makeFileNameFrom fileName = 
    sprintf "%s\\%s.scenary" (IO.Path.GetDirectoryName fileName) (IO.Path.GetFileNameWithoutExtension fileName ) 
    
let dafaultFilename() = Data.fileName() |> makeFileNameFrom

let saveTo filename =    
    try
        sprintf "Сохранение настроек операций в файл %s..." filename |> UI.log.Warn
        let nfos = new ScenaryActionInfos()
        nfos.Nfos <- 
            operations 
            |> List.map( fun actn -> 
                let nfo = actn.Info
                let s = nfo.Node.GetValue( UI.form.columnScenaryParametrValue)
                let s = if s<>null && nfo.Arg<>null then (s.ToString()) else ""
                new ScenaryActionInfo(  ChechedState=nfo.Node.CheckState, Arg=s ) )
            |> List.toArray
        my.forceDirectories filename
        IO.File.WriteAllBytes( filename, FSSerialize.serializeBinary nfos )
    with exn -> 
        sprintf "%s %s" exn.Message exn.StackTrace |> UI.log.Fatal
     
let loadFrom filename =
    sprintf "Загрузка настроек операций из файла %s..." filename |> UI.log.Warn
    if filename |> IO.File.Exists |> not  then 
        sprintf "Файл %s не найден" filename |> UI.log.Fatal
    else
        try
            let nfos =  IO.File.ReadAllBytes filename |> FSSerialize.deserializeBinary< ScenaryActionInfos >
            for nfo,actn in operations |> List.toArray |> Array.zip nfos.Nfos do
                actn.Info.Node.CheckState <- nfo.ChechedState            
                if actn.Info.Arg<>null && nfo.Arg |> String.IsNullOrEmpty |> not then
                    actn.Info.Node.SetValue( UI.form.columnScenaryParametrValue, nfo.Arg )
        with exn -> 
            sprintf "%s %s" exn.Message exn.StackTrace |> UI.log.Fatal

let save() = dafaultFilename() |> saveTo 
let load() = dafaultFilename() |> loadFrom

