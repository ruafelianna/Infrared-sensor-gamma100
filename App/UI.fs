module UI

open System.Collections
open System.Drawing

let log = NLog.LogManager.GetCurrentClassLogger()
let form = MIL82Gui.MIL82MainForm.form

type TreeList = DevExpress.XtraTreeList.TreeList
type Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes
type Column = DevExpress.XtraTreeList.Columns.TreeListColumn
type CellCustomDraw = MIL82Gui.TreeLsitCellCustomDraw
type HitInfoType = DevExpress.XtraTreeList.HitInfoType
type LogKind = MIL82Gui.TreeLsitCellCustomDraw.Kind  

let fnode (nd:Node) = 
    let treeList = nd.TreeList
    let cols = treeList.Columns
    if cols.Count>0 then nd.SetValue( cols.[0], nd.GetValue cols.[0]  )

let treelistNodes (treelist:TreeList) = 
    let rec loopnode (node:Node) = node::[ for node in node.Nodes do yield! loopnode node ] 
    [ for node in treelist.Nodes do yield! loopnode node ]

let dataNodes = treelistNodes form.treeListData

let ccellColor = function    
    | LogKind.Error -> Color.Red
    | LogKind.Warn -> Color.Maroon
    | LogKind.Result -> Color.Navy
    | _ -> Color.Black

let refreshDataColumn (col:Column) = 
    let treelist = col.TreeList
    let rec loop nd = 
        treelist.RefreshCell(nd, col)
        if nd.NextVisibleNode<>null then loop nd.NextVisibleNode    
    loop treelist.Nodes.FirstNode

let treelistDataNodes = 
    let treelistNodes (treelist:TreeList) = 
        let rec loopnode (node:Node) = node::[ for node in node.Nodes do yield! loopnode node ] 
        [ for node in treelist.Nodes do 
            let s = node.GetValue(form.columnParams).ToString()
            yield! loopnode node ]
    treelistNodes form.treeListData

let selectedDeviceIndex() = form.treeListDevices.Nodes.IndexOf form.treeListDevices.FocusedNode

let setTreeListCellStyle (nd:Node) (column:Column) (kind:LogKind) =     
    let customDraw = 
        match nd.Tag :?> MIL82Gui.TreeLsitCellCustomDraw [] with
        | null -> [||]
        | els -> els
    let xcdn = new MIL82Gui.TreeLsitCellCustomDraw( kind=kind, ColumnIndex = column.AbsoluteIndex )
    let customDraw =    
        match customDraw |> Array.tryFindIndex( fun cdn -> cdn.ColumnIndex=column.AbsoluteIndex ) with
        | Some(n) -> 
            customDraw.[n] <- xcdn
            customDraw
        | None -> [| yield xcdn;  yield! customDraw |]
    nd.Tag <- customDraw
    nd.TreeList.RefreshCell(nd,column)

let getTreeListCellStyle (nd:Node) (column:Column) =     
    let customDraw = 
        match nd.Tag :?> MIL82Gui.TreeLsitCellCustomDraw [] with
        | null -> [||]
        | els -> els
    match customDraw |> Array.tryFindIndex( fun cdn -> cdn.ColumnIndex=column.AbsoluteIndex ) with
    | Some(n) -> customDraw.[n].kind
    | None -> LogKind.Info





