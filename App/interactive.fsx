#r "DevExpress.Data.v12.1"
#r "DevExpress.Utils.v12.1"
#r "DevExpress.XtraEditors.v12.1"
#r "DevExpress.XtraTreeList.v12.1"
#r "mscorlib"
#r "FSharp.Core"
#r "System"
#r "System.Core"
#r "System.ComponentModel.DataAnnotations"
#r "System.Drawing"
#r "System.Numerics"
#r "System.Runtime.Serialization"
#r "System.Windows.Forms"
#r "System.Xml"
#r "System.Xml.Linq"
#r "System.Xml.Serialization"
#r "System.Windows.Forms.DataVisualization.dll"
#r "d:\\Projects\\F#\\mil82build\\mil82build\\bin\\Debug\\UI.dll"
#r "d:\\Projects\\F#\\mil82build\\mil82build\\bin\\Debug\\mynumeric.dll"
#r "NLog"



open System
open System.IO
open MIL82Gui
#load "utls.fs"
Environment.CurrentDirectory <- @"d:\\Projects\\F#\\mil82build\\mil82build\\bin\\Debug\\"

let form = MIL82MainForm.form
form.Show()
let log = let cfgFileName = @"\NLog.config"    
          let path1 = my.getExePath + cfgFileName
          let path2 = __SOURCE_DIRECTORY__ + cfgFileName
          if (not <| File.Exists path1) && ( File.Exists path2 ) then File.Copy( path2, path1)
          NLog.LogManager.GetCurrentClassLogger()
#load "MyX.fs"
#load "FSSerialize.fs"
#load "MyIO.fs"
#load "UI.fs"
#load "Var.fs"
#load "Data.fs"
#load "tick.fs"
#load "Dev.fs"
#load "Scn.fs"
#load "Devop.fs"
#load "Termo.fs"
#load "Pneumo.fs"
#load "Ops.fs"
#load "Core.fs"

MIL82Core.initialize()