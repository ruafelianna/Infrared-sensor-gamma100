module mainform

open System.Collections.Generic

let log = NLog.LogManager.GetCurrentClassLogger()
let form = MIL82Gui.MIL82MainForm.form

type TreeList = DevExpress.XtraTreeList.TreeList
type Node = DevExpress.XtraTreeList.Nodes.TreeListNode
type Nodes = DevExpress.XtraTreeList.Nodes.TreeListNodes
type Column = DevExpress.XtraTreeList.Columns.TreeListColumn
type CellCustomDraw = MIL82Gui.TreeLsitCellCustomDraw
type HitInfoType = DevExpress.XtraTreeList.HitInfoType

let private markedOutNodes = new HashSet<Node>()
let private marketOutColumns = new HashSet<Column>()

let initialize() = 
    form.treeList1.NodeCellStyle.AddHandler ( fun sender e ->
        let treelist = sender :?> TreeList
        System.Diagnostics.Debug.Assert(treelist<>null)
        let hitInfo = treelist.CalcHitInfo(new Point(e.X, e.Y))
        let node = hitInfo.Node
        let column = hitInfo.Column
        let isCol = hitInfo.HitInfoType=HitInfoType.Column
        let isCell = hitInfo.HitInfoType=HitInfoType.Cellif isCell {
        if markedOutNodes.Contains node
                    markedOut_.Remove(node);
                else
                    markedOut_.Add(node);
                foreach ( DevExpress.XtraTreeList.Columns.TreeListColumn col in treeList1.Columns) 
                    treeList1.RefreshCell(node, col);
            }
        )
    
