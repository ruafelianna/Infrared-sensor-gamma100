module Data

open System
open System.Collections.Generic
open System.ComponentModel
open System.ComponentModel.DataAnnotations
open System.Xml
open System.Xml.Linq
open System.Xml.Serialization
open System.Drawing.Design
open System.Runtime.Serialization.Formatters.Binary

open MIL82Gui
open MyX

open ManagedWinapi.Windows
open FSSerialize

let private form = MIL82MainForm.form
let log = NLog.LogManager.GetCurrentClassLogger()
let props'to'str obj = 
    String.Join ( "; ", 
            obj.GetType().GetProperties() |> Array.map( fun prop -> 
                sprintf "%s: %s" 
                    (match prop.GetCustomAttributes(false) |> 
                        Array.tryFind( function  :? DisplayNameAttribute as atr -> true  | _ -> false ) with
                     | Some(:? DisplayNameAttribute as atr ) -> atr.DisplayName
                     | _ -> prop.Name)
                    (prop.GetValue(obj,null).ToString() ) ) )

let cminutes n =  (new System.DateTime()).AddMinutes( double(n))
let chours n =  (new System.DateTime()).AddHours( double(n))

[<TypeConverter(typeof<ExpandableObjectConverter>)>]
type CommSets () = 
    [<DisplayName("Порт")>]
    [<Description("Выбор имени используемого СОМ порта")>]
    [<TypeConverter (typeof<my.Conv.ComPortNamesConverter>) >]
    member val public PortName = "COM1" with get, set

    [<DisplayName("Таймаут, мс")>]
    [<Description("Длительность ожидания ответа от прибора в милисекундах")>]                
    member val public TimeOut = 1000 with get, set
        
    [<DisplayName("Задержка отправки, мс")>]
    [<Description("Задержка отправки запроса прибору в милисекундах")>]                
    member val public WriteDelay = 0 with get, set
        
    [<DisplayName("Время ожидания символа, мс")>]
    [<Description("Длительность ожидания символа ответа в милисекундах")>]                
    member val public SilentTime = 100 with get, set
        
    [<DisplayName("Колличество повторов запроса")>]
    [<Description("Колличество повторов запроса прибору")>]                
    member val public RepeatCount = 0 with get, set

    [<DisplayName("Показывать посылки")>]
    [<Description("Показывать посылки")>] 
    [<TypeConverter(typeof<MIL82Gui.YesNoConverter>)>]               
    member val public EnableLog = false with get, set

    override this.ToString() = ""

type Props() = 

    
    [<Category("Сценарий")>] 
    [<DisplayName("Приёмопередача")>]
    [<Description("Параметры приёмопередачи ИК-датчиков")>]
    member val public Devs = new CommSets() with get, set

                         
    [<Category("Сценарий")>]        
    [<DisplayName("Исполнение")>]
    [<Description("Выбор исполнения датчиков")>]
    [<TypeConverter (typeof< EnumTypeConverter >) >]
    member val public Kind = Var.Kind.D.D0 with get, set    

    [<Category("Сценарий")>]        
    [<DisplayName("Длительность прогрева")>]
    [<Description("Длительность выдержки при установленной температуре")>] 
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public WarmingTime = cminutes 30 with get, set

    [<Category("Сценарий")>]        
    [<DisplayName("Длительность цикла техпрогона, часов")>]
    [<Description("Длительность цикла технологического прогона, часов")>] 
    member val public ДлительностьЦиклаТехпрогона = 24u with get, set

    [<Category("ПГС-ГСО")>]
    [<Description("Концентрация ПГС1")>]        
    [<DisplayName("ГСО-ПГС №1")>]    
    member val public ПГС1 = 0. with get, set

    [<Category("ПГС-ГСО")>]
    [<Description("Концентрация ПГС2")>]        
    [<DisplayName("ГСО-ПГС №2")>]    
    member val public ПГС2 = 0. with get, set

    [<Category("ПГС-ГСО")>]
    [<Description("Концентрация ПГС3")>]        
    [<DisplayName("ГСО-ПГС №3")>]    
    member val public ПГС3 = 0. with get, set

    [<Category("ПГС-ГСО")>]
    [<Description("Концентрация ПГС4")>]        
    [<DisplayName("ГСО-ПГС №4")>]    
    member val public ПГС4 = 0. with get, set

    [<Category("ПГС-ГСО")>]
    [<Description("Концентрация ПГС5")>]        
    [<DisplayName("ГСО-ПГС №5")>]    
    member val public ПГС5 = 0. with get, set
    
    [<Category("ПГС-ГСО")>]        
    [<DisplayName("Длительность продувки")>]
    [<Description("Длительность продувки газом")>] 
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public BlowGasTime = cminutes 3 with get, set

    [<Category("ПГС-ГСО")>]        
    [<DisplayName("Длительность продувки влажным газом")>]
    [<Description("Длительность продувки влажным газом")>] 
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public BlowWetGasTime = cminutes 10 with get, set

    [<Category("ПГС-ГСО")>]        
    [<DisplayName("Длительность выдувания")>]
    [<Description("Длительность продувки воздухом после газа")>] 
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public BlowAirTime = cminutes 3 with get, set

    [<Category("ПГС-ГСО")>]        
    [<DisplayName("Длительность выдержки под давлением")>]
    [<Description("Длительность выдержки под давлением")>] 
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public PressureTime = cminutes 5 with get, set

    [<Category("Дополнительно")>]   
    [<DisplayName("Интервал автосохраннения")>]        
    [<Description("Интервал автосохраннения, час:мин:с")>]    
    [<DataType(DataType.Time) >]
    [<Editor( typeof<TimePickerEditor>, typeof<UITypeEditor> )>]
    [<TypeConverter (typeof< TimeConverter >) >]   
    member val public AvtosaveInterval = cminutes 10 with get, set

type GUISettings () = 
    
    member val public Placement = new WINDOWPLACEMENT() with get, set 
       
    [<XmlArray(ElementName = "LogScnColWidth")>]    
    [<XmlArrayItem(typeof<int>)>] 
    member val public LogScnColWidth : int [] = [||] with get, set

    [<XmlArray(ElementName = "GrdDevColWidth")>]    
    [<XmlArrayItem(typeof<int>)>] 
    member val public GrdDevColWidth : int [] = [||] with get, set

    [<XmlArray(ElementName = "TreeListDevicesColWidth")>]    
    [<XmlArrayItem(typeof<int>)>] 
    member val public TreeListDevicesColWidth : int [] = [||] with get, set

    member val public FileName : string = "" with get, set

    [<XmlArray("VarsCheckState")>]    
    [<XmlArrayItem(typeof<Windows.Forms.CheckState>)>] 
    member val public VarsCheckState : Windows.Forms.CheckState [] = [||] with get, set

    member val public ConsoleVisible = true with get, set

type Device() = 
    member val public Number = 0 with get, set
    member val public Serial = "..." with get, set
    member val public Addy = 1uy with get, set
    member val public Values : (Var.Grp*Var.Prm*int*string) array = [||] with get, set
    member val public Enabled = true with get, set

type TreeListNodeCustomDraw() = 
    [<XmlArray("TreeListNodeCustomDrawItems")>]    
    [<XmlArrayItem(typeof<MIL82Gui.TreeLsitCellCustomDraw>)>] 
    member val public Items : MIL82Gui.TreeLsitCellCustomDraw array = [||] with get, set

type Devices() = 
    
    [<XmlArray("Devices")>]    
    [<XmlArrayItem(typeof<Device>)>] 
    member val public Devices : Device array = [||] with get, set

    member val public Props = new Props() with get, set

    [<XmlArray("TreeListDataCustomDraw")>]    
    [<XmlArrayItem(typeof<TreeListNodeCustomDraw>)>] 
    member val public TreeListDataCustomDrawItems : TreeListNodeCustomDraw array = [||] with get, set

type TreeList = DevExpress.XtraTreeList.TreeList

let setvisConsole vis = 
    form.splitContainer1.Panel2Collapsed <- vis |> not
    form.menuConsoleVisible.Checked <- vis

// изменяемые пользователем настройки приложения
let guiSets =
    let configXmlFileName = 
        IO.Path.ChangeExtension( Windows.Forms.Application.ExecutablePath, ".userconfig" )
    let ui = 
        ref (   try 
                    use stream = new IO.MemoryStream(IO.File.ReadAllBytes configXmlFileName)
                    let binFormatter = new BinaryFormatter()
                    binFormatter.Deserialize(stream) :?> GUISettings 
                with _ ->  new GUISettings()  )
    let apply (ui:GUISettings) = 
        let setColWidth (tl:TreeList) = Array.iteri( fun i v -> if i<tl.Columns.Count then tl.Columns.[i].Width <- v )
        let wndpl = ref ui.Placement
        Import.SetWindowPlacement( form.Handle, wndpl ) |> ignore
        setColWidth form.treeListData ui.GrdDevColWidth
        setColWidth form.treeListDevices ui.TreeListDevicesColWidth
        // выбранность элементов таблицы данных    
        
        let sts = ui.VarsCheckState |> Array.toList
        if List.length sts = List.length UI.treelistDataNodes then            
            for st, nd in (List.zip sts UI.treelistDataNodes) do
                nd.CheckState <- st
        ui.ConsoleVisible |> setvisConsole
        
    let ofForm() = 
        let getColWidth (tl:TreeList) = [| for i in 0..tl.Columns.Count-1 -> tl.Columns.[i].Width |]
        let wndpl = ref ( new WINDOWPLACEMENT() )
        Import.GetWindowPlacement( form.Handle, wndpl ) |> ignore  
          
        (!ui).Placement <- !wndpl
        (!ui).GrdDevColWidth <- getColWidth form.treeListData
        (!ui).TreeListDevicesColWidth <- getColWidth form.treeListDevices       
        
        // выбранность элементов таблицы данных    
        (!ui).VarsCheckState  <- [| for nd in UI.treelistDataNodes -> nd.CheckState |]       
        (!ui).ConsoleVisible <- form.menuConsoleVisible.Checked
 
    form.toolStripButton1.DropDownOpening.AddHandler ( fun _ _ ->
        form.menuConsoleVisible.Checked <- (!ui).ConsoleVisible
        )
    form.menuConsoleVisible.Click.AddHandler( fun _ _ ->          
        (!ui).ConsoleVisible <- (!ui).ConsoleVisible |> not
        (!ui).ConsoleVisible |> setvisConsole)
        
    form.Closed.AddHandler ( fun _ _ ->
        ofForm()
        let binFormatter = new BinaryFormatter()
        use stream = new IO.MemoryStream()
        try
            binFormatter.Serialize(stream, !ui )
        with exn ->
            failwith exn.Message
        IO.File.WriteAllBytes ( configXmlFileName, stream.ToArray() ) )

    apply !ui
    !ui

let fileName() = guiSets.FileName

let dataPath = sprintf @"%s\Данные\" (my.getExePath)

let setFilenName fileName = 
    my.safelyWinForm form ( fun() ->
        let relPath = my.relativePath dataPath fileName
        log.Info( relPath )
        form.Text <- "Версия 1.03. Настройка ИК датчиков ГАММА-100. "+my.relativePath dataPath fileName
        guiSets.FileName <- fileName )

let setNewFileName() = 
    let nw = System.DateTime.Now
    sprintf @"%s%d\%d\%d\партия.%d.%d.%d.%d.%d.dev" dataPath (nw.Year) (nw.Month) (nw.Day) 
        (nw.Year-2000) (nw.Month) (nw.Day) (nw.Hour) (nw.Minute) |> setFilenName