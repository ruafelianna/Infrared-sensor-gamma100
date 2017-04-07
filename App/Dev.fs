module Dev

open System
open System.Drawing
open System.Windows.Forms
open System.ComponentModel
open System.Collections.Generic

open MIL82Gui

type private Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type private Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes
type private Column = DevExpress.XtraTreeList.Columns.TreeListColumn

let log = NLog.LogManager.GetCurrentClassLogger()
let form = MIL82MainForm.form

let count() = form.treeListDevices.Nodes.Count

let private checkn n = if n<0 || n>=count() then failwith "Упс! n>=Dev.count()"

let isSelected n =  checkn n
                    form.treeListDevices.Nodes.[n].Checked

// серийный номер
let getSerial n =   checkn n 
                    form.treeListDevices.Nodes.[n].GetValue( form.columnSerial ).ToString()

let tryGetAddy n =     
    match form.treeListDevices.Nodes.[n].GetValue( form.columnAddy ).ToString() |> Byte.TryParse with  
    | true, v -> Some(v)
    | _ -> None

let (|TryGetAddy|_|) = tryGetAddy

let lst () = [ for n in 0..count()-1 do match n with TryGetAddy(addy) -> yield n,addy | _ -> () ]

let getAddy = function TryGetAddy(addy) -> addy | _ -> failwith("Упс! Dev.getAddy")

let cpt n = checkn n
            sprintf "№%d #%s:%d" (n+1) (getSerial n) (getAddy n)

let tryGetByAddy addy = lst() |> List.tryFind ( fun (_,v) -> addy=v )
let (|TryGetByAddy|_|) = tryGetByAddy                    

let getCaptionOfAddy addy = 
    match lst() |> List.tryFind( fun (_,v) -> addy=v ) with 
    | Some(n,addy) -> sprintf "#%s:%d" (getSerial n) addy
    | _ -> sprintf "#%d" addy


// добавить прибор с серийным номером в таблицу
let private addDevice serial addy enabled =     
    
    let col = form.treeListData.Columns.Add()
    col.Caption <- sprintf "№%d:#%s:%s" (count()+1) serial addy
    col.OptionsColumn.AllowSort <- false    
    col.Visible <- enabled
    
    let n = col.AbsoluteIndex
    if n<Data.guiSets.GrdDevColWidth.Length then col.Width <- Data.guiSets.GrdDevColWidth.[n]
    let nd = form.treeListDevices.Nodes.Add [|serial|]
    nd.SetValue( form.columnAddy, addy )
    nd.Checked <- enabled

let isValidAddy n addy = 
    if addy=0uy || addy>127uy then false else 
        [ for i in 0..count()-1 do match i with TryGetAddy(addy) when i<>n -> yield addy | _ -> () ] |> 
        List.exists ( fun addy' -> addy'=addy ) |> not
let isValidSerial n serial = 
    [ for i in 0..count()-1 do if i<>n then yield getSerial i ] |> List.exists ( fun s -> s=serial ) |> not
let isValidNewAddy = count() |> isValidAddy
let isValidNewSerial = count() |> isValidSerial

let mutable private settings = new Data.Props()

type private XElement = Xml.Linq.XElement
type private XDocument = Xml.Linq.XDocument
type private XAttribute = Xml.Linq.XAttribute

let trysave fileName =   

    let cx = MyX.celem
    my.forceDirectories fileName
    
    sprintf "Data.setFilenName %s" fileName |> log.Trace
    let cdevd n = 
        new Data.Device(
            Number=n, Serial=getSerial n, Addy=getAddy n, Enabled=isSelected n,
            Values=[|   for grp, prm, nprm, nd, _ in Var.grp_vars do
                            let v = nd.GetValue( form.treeListData.Columns.[n+1] )
                            if v<>null && v.ToString() |> String.IsNullOrWhiteSpace |> not then 
                                yield ( grp, prm, nprm, v.ToString() ) |] )
    let toSave = 
        new Data.Devices( 
            Props=settings,
            Devices = [| for n in 0..count()-1 -> cdevd n |],
            TreeListDataCustomDrawItems =  
                [| for node in UI.dataNodes ->
                    let customDraw = node.Tag :?> MIL82Gui.TreeLsitCellCustomDraw []
                    new Data.TreeListNodeCustomDraw(Items=customDraw) |] )
    
    IO.File.WriteAllBytes( fileName, FSSerialize.serializeBinary toSave )
    "сохранение: данные по приборам" |> log.Trace

let save filename =     
    sprintf "Сохранение исходных данных в файл %s..." filename |> log.Warn
    try
        trysave filename
    with exn ->
        sprintf "Ошибка при сохранении в файл %s\n%s\n%s" filename exn.Message exn.StackTrace
        |> log.Fatal

let clearDevices() = 
    let cols = form.treeListData.Columns 
    let rec loop() = 
        if cols.Count>1 then 
            cols.RemoveAt(cols.Count-1)
            loop()   
    loop()    
    form.treeListDevices.Nodes.Clear()

let tryload (fileName:string) =
    let splashScreen = new MIL82Gui.SplashScreen()
    splashScreen.label1.Text <- "Загрузка данных..."
    splashScreen.Show()
    splashScreen.Refresh()
    UI.form.Hide()
    
    my.forceDirectories fileName    
    clearDevices()
    let loaded = FSSerialize.deserializeBinaryDef ( IO.File.ReadAllBytes(fileName) ) (new Data.Devices() )

    for n,devd in loaded.Devices |> Array.mapi( fun n devd ->  n, devd) do
        addDevice devd.Serial (devd.Addy.ToString()) devd.Enabled   
        for (grp, prm, nprm,value)  in devd.Values do   
            let col = form.treeListData.Columns.[n+1]
            match Var.tryGetNode grp prm nprm with
            | Some(node) -> node.SetValue( col, value )            
            | _ -> ()            
            
    settings <- loaded.Props  

    if loaded.TreeListDataCustomDrawItems<>null && loaded.TreeListDataCustomDrawItems.Length=UI.dataNodes.Length then
        for cdn,node in Array.zip loaded.TreeListDataCustomDrawItems (UI.dataNodes |> List.toArray) do
            node.Tag <- cdn.Items

    Data.setFilenName fileName
    splashScreen.Close()
    UI.form.Show()

let load filename = 
    let tryload filename onerror = 
        try
            tryload filename
        with exn ->
            sprintf "Ошибка при открытии файла %s\n%s\n%s" filename exn.Message exn.StackTrace
            |> log.Fatal
            onerror()
    let oldFileName = Data.fileName()
    sprintf "Загрузка данных из файла %s..." filename |> log.Warn    
    tryload filename <| fun _ ->
        sprintf "Восстановление данных из файла %s..." oldFileName |> log.Warn
        tryload oldFileName <| fun _ -> ()

let sets() = settings
let kind() = Var.Kind.get settings.Kind

let formatkind() = 
    let  gas, _, scale, _ = kind()
    sprintf "%O, %O, %s" (my.getEnumDescription settings.Kind) gas.What scale.What

let port = 
    let portName() = sets().Devs.PortName
    let comPort = new MyIO.ComPort()
    fun () ->         
        if comPort.PortName<>portName() then
            comPort.Close()
            comPort.PortName <- portName()            
        comPort

let initialize() = 

    let isValidSerial n serial = 
        let serials = [for i in 0..count()-1 do 
                        if i<>n then
                            let b,v = getSerial i |> UInt32.TryParse
                            if b && v>0ul then yield v ]
        serials |> List.exists( fun a -> serial=a ) |> not

    let getValidSerial n =         
        let rec loop v = if isValidSerial n v || v>=UInt32.MaxValue-1ul then v else loop (v+1ul)
        loop 1000ul

    let isValidAddy n addy = 
        let addys = [for i in 0..count()-1 do 
                        if i<>n then 
                            let b,v = form.treeListDevices.Nodes.[i].GetValue( form.columnAddy ).ToString() |> Byte.TryParse                         
                            if b && v>0uy && v<128uy then yield v ]
        addys |> List.exists( fun a -> addy=a ) |> not

    let getValidAddy n =         
        let rec loop v = if isValidAddy n v || v>=127uy then v else loop (v+1uy)
        loop 1uy
    
    form.btnAddDevice.Click.AddHandler( fun _ e -> 
        let n = count()
        let ser, addy = getValidSerial n, getValidAddy n
        addDevice (ser.ToString()) (addy.ToString()) true )

    form.btnDelDevice.Click.AddHandler( fun _ e -> 
        let n = form.treeListDevices.Nodes.IndexOf  form.treeListDevices.FocusedNode        
        if n>=0 && n<count() &&
            System.Windows.Forms.MessageBox.Show( sprintf "Удалить прибор %s?" (cpt n), 
                "", MessageBoxButtons.YesNo )=DialogResult.Yes then 
            form.treeListData.Columns.RemoveAt(n+1)            
            form.treeListDevices.Nodes.RemoveAt n
            for i in n..count()-1 do
                form.treeListData.Columns.[i+1].Caption <- sprintf "№%d:#%s:%d" (i+1) (getSerial i) (getAddy i) )
    form.treeListDevices.FocusedNode <- null
    form.treeListDevices.FocusedNodeChanged.AddHandler( fun _ e ->                
        form.btnDelDevice.Visible <- ( e.Node<>null ) )

    form.treeListDevices.ValidatingEditor.AddHandler( fun _ e ->
        let n = form.treeListDevices.Nodes.IndexOf  form.treeListDevices.FocusedNode        
        if n>=0 && n<count() then            
            let s = if e.Value=null then "" else e.Value.ToString()
            let caddy, cserial = form.treeListDevices.FocusedColumn=form.columnAddy,form.treeListDevices.FocusedColumn=form.columnSerial
            if cserial then                
                let b,v = s |> UInt32.TryParse
                e.Valid <- b && isValidSerial n v
                if e.Valid then
                    form.treeListData.Columns.[n+1].Caption <- sprintf "№%d:#%s:%d" (n+1) s (getAddy n)
                e.ErrorText <- sprintf "Некорретный ввод серийного номера: %s" s
            elif caddy then
                let b,v = s |> Byte.TryParse
                e.Valid <- b && isValidAddy n v
                e.ErrorText <- sprintf "Некорретный ввод адреса: %s" s
                if e.Valid then
                    form.treeListData.Columns.[n+1].Caption <- sprintf "№%d:#%s:%s" (n+1) (getSerial n) s )