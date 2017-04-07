module Dev

open System
open System.Drawing

open MIL82Gui

let log = NLog.LogManager.GetCurrentClassLogger()
let form = MIL82MainForm.form
let devGrd = form.treeList1

let count() = form.treeList1.Columns.Count-1

let isSelected n = not <| form.IsMarkedOutColumn (n+1)

// получить список выбранных устройств
let getSelected() = [ for n in 0..count() do if isSelected n then yield n ]



