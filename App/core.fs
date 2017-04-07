module MIL82Core

open System    
open System.Windows.Forms
open System.Drawing
open System.Text.RegularExpressions
open System.Diagnostics
open System.Xml
open System.Xml.Linq
open System.IO.Ports
open System.Threading
open System.Windows.Media
open NLog

open MyX
open MIL82Gui
open Data

open ManagedWinapi.Windows
open FSSerialize

let private log = NLog.LogManager.GetCurrentClassLogger()
let private form = MIL82MainForm.form


// иниализация ГУИ и ядра
let initializeGUI() = 

    my.TreeList.addCopyPastMenu form.treeListData
    
    Dev.sets() |> ignore    

    // отслеживать отметки чекбоксов в списке приборов
    form.treeListDevices.NodeChanged.AddHandler( fun _ e ->
        if e.ChangeType=DevExpress.XtraTreeList.NodeChangeTypeEnum.CheckedState then
            let n = form.treeListDevices.Nodes.IndexOf e.Node
            form.treeListData.Columns.[1+n].Visible <- e.Node.Checked )

    let perform () =         
        if (not <| Scn.isPerforming() ) then 
            match Ops.operations |> List.tryFind( fun(action) -> action.Info.Node=form.treeListScenary.Selection.[0]) with
            | Some(action) -> 
                let dis = Ops.disableUserStartList 
                match dis |> List.tryFind( fun t -> t=action ) with
                | None when action.Info.Node.CheckState<>CheckState.Unchecked -> Scn.Perform.performScenaryAsync action
                | _ -> ()
            | _ -> ()

            
                

    form.btnRun.Click.AddHandler  ( fun _ _ ->  perform() )
    //form.treeListScenary.DoubleClick.AddHandler( fun _ _ -> if (not <| Scn.isPerforming() ) then perform() )

    form.button_run_interrogate.Click.Add( fun _ ->
        Scn.Perform.performScenaryAsync Ops.interrrogate )


    let performSome foo = 
        if Scn.isPerforming() |> not then   
            Scn.Perform.performScenaryAsync foo

    // запись - чтение кефов
    let btnKefClick = new EventHandler( fun btn _ -> 
        performSome 
            <| if btn=(form.btnReadKefs:> obj) then Ops.Kefs.readselected else Ops.Kefs.writeselected )

    form.btnReadKefs.Click.AddHandler btnKefClick   
    form.btnWriteKefs.Click.AddHandler btnKefClick
       

    Ops.scenary |> ignore

    // сброс асинхронных опрераций привыходе
    form.Closing.AddHandler( fun _ _ ->  
        if Scn.isPerforming() then
            Async.CancelDefaultToken()             
            System.Threading.Thread.Sleep(500))

    let save() =    
        Data.fileName() |> Dev.save
        Ops.save()

    let saveto filename = 
        //save()
        Data.setFilenName filename 
        save()

    // автосэйв
    let msecs() = float <| my.milliseconds( Dev.sets().AvtosaveInterval )
    let timer = new System.Timers.Timer( Interval=msecs(), AutoReset=true, Enabled=true )
    timer.Elapsed.AddHandler( fun _ _ -> 
        if Scn.isPerforming() then
            timer.Enabled <- false
            log.Info "Автосохранение"
            save()
            timer.Interval <- msecs()
            timer.Enabled <- true)

    // сохранение данных при выходе из приложения
    form.Closing.AddHandler( fun _ e ->  save() )

    // меню - сохранить
    form.MenuSaveAs.Click.AddHandler( fun _ _ ->
        let saveFileDialog = new SaveFileDialog(    Filter="Файл данных (*.dev)|*.dev" )
        if saveFileDialog.ShowDialog()=DialogResult.OK then
            saveFileDialog.FileName |> saveto  )

    form.MenuSave.Click.AddHandler( fun _ _ -> save()  )

    // меню - создать новый файл
    form.memuNewFile.Click.AddHandler( fun _ _ ->
        if Windows.Forms.MessageBox.Show( "Создать новый файл данных?", "", MessageBoxButtons.YesNo )=DialogResult.Yes then
            save()
            Dev.clearDevices()
            Data.setNewFileName() )

    // меню - открыть файл
    form.menuOpenFile.Click.AddHandler( fun _ _ ->
        let openFileDialog = new OpenFileDialog(    Filter="Файл данных (*.dev)|*.dev" )
        if openFileDialog.ShowDialog()=DialogResult.OK then
            save()
            Dev.clearDevices()
            openFileDialog.FileName |> Dev.load
            Ops.load() )

    // диалог настроек
    form.btnSettings.Click.AddHandler( fun _ _ ->
        my.simpleProperties.dialog (Dev.sets()) "Настройки" 
        timer.Stop()
        timer.Start() )

    // отрисовка ячеек таблицы журнала событий
    let nodeCellStyle (e:DevExpress.XtraTreeList.GetCustomNodeCellStyleEventArgs) = 
        let cc = e.Node.Tag :?> TreeLsitCellCustomDraw []
        if cc<>null then
            match cc |> Array.tryFind( fun i -> i.ColumnIndex=e.Column.AbsoluteIndex) with
            | Some(c) -> 
                e.Appearance.ForeColor <- UI.ccellColor c.kind
            | _ -> ()

    form.treeListData.HiddenEditor.Add ( fun e ->
        let node = form.treeListData.FocusedNode
        let column = form.treeListData.FocusedColumn        
        if node<>null && column<>null then
            let ndev = column.AbsoluteIndex-1
            let s = node.GetValue(column)
            let s = if s=null then "" else s.ToString()
            let b,value = my.tryParseFloat s
            if b then
                match Var.grp_vars |> List.tryFind( fun (_, _, _, xnode, _) -> xnode=node) with
                | Some(_, Var.Kef kef, _, _, _) ->
                    my.casync <| fun() ->
                        try DevOp.write ndev kef.Cmd value |> ignore
                        with e -> log.Error e.Message
                    |> Async.Start
                | _ -> () )
    form.treeListData.KeyDown.Add( fun e ->
        let node = form.treeListData.FocusedNode
        let column = form.treeListData.FocusedColumn        
        if e.KeyCode=Keys.Enter && node<>null && column<>null then
            let ndev = column.AbsoluteIndex-1
            let b,value = node.GetValue(column).ToString() |> my.tryParseFloat
            if b then
                match Var.grp_vars |> List.tryFind( fun (_, _, _, xnode, _) -> xnode=node) with
                | Some(_, Var.Kef kef, _, _, _) ->
                    my.casync <| fun() ->
                        try DevOp.write ndev kef.Cmd value |> ignore
                        with e -> log.Error e.Message
                    |> Async.Start
                | _ -> () )
    form.menu_report.Click.Add <| fun _ ->
        let sts = Dev.sets()
        let gas,units,scale,_ = Var.Kind.get sts.Kind
        let units = my.getUnionCaseDescription units

        let sclr (run:System.Windows.Documents.Run) = 
            run.Foreground <- new SolidColorBrush( if run.Text.Contains(">") then Colors.Red else Colors.Navy )

        for n in 0..Dev.count()-1 do
            let dlg = new OTKReport.MainWindow()
            dlg.run_serial.Text <- Dev.getSerial n
            dlg.run_kind.Text <- my.getEnumDescription sts.Kind
            dlg.run_gas.Text  <- gas.What
            dlg.run_scale.Text <- sprintf "%s %s" scale.What units
            dlg.run_pgs3.Text <- sprintf "%g %s" sts.ПГС3 units
            dlg.run_pgs4.Text <- sprintf "%g %s" sts.ПГС4 units

            let f2 s = 
                let b,v = Double.TryParse s
                if b then sprintf "%2.3f" v else s

            dlg.run_termo_conc1.Text <- Var.getValN Var.Grp.Test_Termo0 (Var.Val( Var.Conc )) 0 n |> f2
            dlg.run_termo_conc2.Text <- Var.getValN Var.Grp.Test_TermoE (Var.Val( Var.Conc )) 0 n |> f2

            dlg.run_termo_test1.Text <- Var.getValN Var.Grp.Test_Termo0 (Var.Val( Var.TestConcResult )) 0 n
            sclr dlg.run_termo_test1
            dlg.run_termo_test2.Text <- Var.getValN Var.Grp.Test_TermoE (Var.Val( Var.TestConcResult )) 0 n
            sclr dlg.run_termo_test2

            for i in 0..4 do
                dlg.Run_Date.[i].Text <- Var.getValN Var.Grp.Test_Date (Var.Val( Var.TestDate )) i n
            for i in 0..11 do
                dlg.Run_Conc.[i].Text <-  Var.getValN Var.Grp.OTKPresent (Var.Val( Var.Conc )) i n |> f2
                let r = Var.getValN Var.Grp.OTKPresent (Var.Val( Var.TestConcResult )) i n
                dlg.Run_Test.[i].Text <- r
                sclr dlg.Run_Test.[i]

            dlg.ShowDialog() |> ignore

let initialize() =  

    // заполнить заголовки таблицы данных    
    for _,_,_,node,_ in Var.grp_vars do 
        UI.fnode node
    for op in Ops.operations do
        UI.fnode op.Info.Node

    // таблица с данными датчиков    
    if System.IO.File.Exists Data.guiSets.FileName then 
        Data.guiSets.FileName |> Dev.load
    else
        Data.setNewFileName()
    
    

    Dev.initialize()    
    initializeGUI()
    Dev.sets() |> ignore
    Ops.load()