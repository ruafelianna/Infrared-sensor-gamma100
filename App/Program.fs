open MIL82Gui
open NLog
open MIL82Core
open System.Windows.Forms

let initialize() = 
    try     
        MIL82Core.initialize ()
        None
    with
    | _ as exn ->
        Some( exn |> my.exn |> sprintf "Ошибка при инициализации, %s" )

[<EntryPoint>]
[<System.STAThreadAttribute>]
let main argv =   
    let splashScreen = new MIL82Gui.SplashScreen()
    splashScreen.Show()
    splashScreen.Refresh()
    let form = MIL82MainForm.form
    form.Show()
    let log = LogManager.GetCurrentClassLogger()
    let fail = initialize()

    Var.saveScalesFS() 

    splashScreen.Close()
    match fail with
    | Some(fail) ->
        fail |> log.Fatal
        form.Hide()
        MessageBox.Show( fail, "ИК-датчик. Ошибка инициализации", MessageBoxButtons.OK, MessageBoxIcon.Stop ) |> ignore
    | _ -> 
        System.Windows.Forms.Application.Run(form) |> ignore
    0 // возвращение целочисленного кода выхода
