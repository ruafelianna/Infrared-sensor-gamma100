namespace MIL82Gui
{
    partial class MIL82MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                this.timer1.Enabled = false;
                this.timer1.Tick -= timer1_Tick;
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MIL82MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageDevices = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeListDevices = new DevExpress.XtraTreeList.TreeList();
            this.columnSerial = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.columnAddy = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.columnStatus = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListScenary = new DevExpress.XtraTreeList.TreeList();
            this.columnScenaryOperation = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.columnScenaryParametrValue = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.columnActionId = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.button_run_interrogate = new System.Windows.Forms.Button();
            this.btnAddDevice = new System.Windows.Forms.Button();
            this.btnDelDevice = new System.Windows.Forms.Button();
            this.tabPageData = new System.Windows.Forms.TabPage();
            this.treeListData = new DevExpress.XtraTreeList.TreeList();
            this.columnParams = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumn1 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumn2 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.treeListColumn3 = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnWriteKefs = new System.Windows.Forms.Button();
            this.btnReadKefs = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.toolStripRunStop = new System.Windows.Forms.ToolStrip();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.labelMainAction = new System.Windows.Forms.ToolStripLabel();
            this.labelMainActionTime = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.labelCurrentScenaryAction = new System.Windows.Forms.ToolStripLabel();
            this.labelCurrentScenaryActionTime = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.labelScenaryDelayProgress = new System.Windows.Forms.ToolStripLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.btnSettings = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.memuNewFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSave = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menu_report = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuConsoleVisible = new System.Windows.Forms.ToolStripMenuItem();
            this.labelScenaryCurrentActionStatus = new System.Windows.Forms.ToolStripLabel();
            this.LogMemo = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuDeletePerformLogTreeNode = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageDevices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListDevices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeListScenary)).BeginInit();
            this.panel3.SuspendLayout();
            this.tabPageData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeListData)).BeginInit();
            this.panel2.SuspendLayout();
            this.toolStripRunStop.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            this.splitContainer1.Panel1.Controls.Add(this.panel1);
            this.splitContainer1.Panel1.Controls.Add(this.toolStripRunStop);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.LogMemo);
            this.splitContainer1.Size = new System.Drawing.Size(1101, 705);
            this.splitContainer1.SplitterDistance = 552;
            this.splitContainer1.TabIndex = 1;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageDevices);
            this.tabControl1.Controls.Add(this.tabPageData);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabControl1.Location = new System.Drawing.Point(0, 39);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1091, 513);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageDevices
            // 
            this.tabPageDevices.Controls.Add(this.splitContainer2);
            this.tabPageDevices.Controls.Add(this.panel3);
            this.tabPageDevices.Location = new System.Drawing.Point(4, 30);
            this.tabPageDevices.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageDevices.Name = "tabPageDevices";
            this.tabPageDevices.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageDevices.Size = new System.Drawing.Size(1083, 479);
            this.tabPageDevices.TabIndex = 5;
            this.tabPageDevices.Text = "Датчики";
            this.tabPageDevices.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(2, 2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeListDevices);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.treeListScenary);
            this.splitContainer2.Size = new System.Drawing.Size(954, 475);
            this.splitContainer2.SplitterDistance = 92;
            this.splitContainer2.TabIndex = 6;
            // 
            // treeListDevices
            // 
            this.treeListDevices.Appearance.FocusedCell.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListDevices.Appearance.FocusedCell.Options.UseFont = true;
            this.treeListDevices.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListDevices.Appearance.HeaderPanel.Options.UseFont = true;
            this.treeListDevices.Appearance.Row.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListDevices.Appearance.Row.Options.UseFont = true;
            this.treeListDevices.Appearance.SelectedRow.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListDevices.Appearance.SelectedRow.Options.UseFont = true;
            this.treeListDevices.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.columnSerial,
            this.columnAddy,
            this.columnStatus});
            this.treeListDevices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListDevices.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListDevices.Location = new System.Drawing.Point(0, 0);
            this.treeListDevices.Margin = new System.Windows.Forms.Padding(2);
            this.treeListDevices.Name = "treeListDevices";
            this.treeListDevices.OptionsPrint.UsePrintStyles = true;
            this.treeListDevices.OptionsView.AllowHtmlDrawHeaders = true;
            this.treeListDevices.OptionsView.EnableAppearanceEvenRow = true;
            this.treeListDevices.OptionsView.EnableAppearanceOddRow = true;
            this.treeListDevices.OptionsView.ShowCheckBoxes = true;
            this.treeListDevices.Size = new System.Drawing.Size(954, 92);
            this.treeListDevices.TabIndex = 2;
            // 
            // columnSerial
            // 
            this.columnSerial.Caption = "Серийный";
            this.columnSerial.FieldName = "Серийный";
            this.columnSerial.MinWidth = 32;
            this.columnSerial.Name = "columnSerial";
            this.columnSerial.OptionsColumn.AllowMove = false;
            this.columnSerial.OptionsColumn.AllowSort = false;
            this.columnSerial.Visible = true;
            this.columnSerial.VisibleIndex = 0;
            this.columnSerial.Width = 102;
            // 
            // columnAddy
            // 
            this.columnAddy.Caption = "Адрес";
            this.columnAddy.FieldName = "234";
            this.columnAddy.Name = "columnAddy";
            this.columnAddy.Visible = true;
            this.columnAddy.VisibleIndex = 1;
            this.columnAddy.Width = 127;
            // 
            // columnStatus
            // 
            this.columnStatus.Caption = "Статус";
            this.columnStatus.FieldName = "678";
            this.columnStatus.Name = "columnStatus";
            this.columnStatus.OptionsColumn.AllowEdit = false;
            this.columnStatus.OptionsColumn.AllowMove = false;
            this.columnStatus.OptionsColumn.ReadOnly = true;
            this.columnStatus.Visible = true;
            this.columnStatus.VisibleIndex = 2;
            this.columnStatus.Width = 517;
            // 
            // treeListScenary
            // 
            this.treeListScenary.Appearance.FocusedCell.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListScenary.Appearance.FocusedCell.Options.UseFont = true;
            this.treeListScenary.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListScenary.Appearance.HeaderPanel.Options.UseFont = true;
            this.treeListScenary.Appearance.Row.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListScenary.Appearance.Row.Options.UseFont = true;
            this.treeListScenary.Appearance.SelectedRow.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListScenary.Appearance.SelectedRow.Options.UseFont = true;
            this.treeListScenary.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.columnScenaryOperation,
            this.columnScenaryParametrValue,
            this.columnActionId});
            this.treeListScenary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListScenary.Font = new System.Drawing.Font("Tahoma", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.treeListScenary.Location = new System.Drawing.Point(0, 0);
            this.treeListScenary.Margin = new System.Windows.Forms.Padding(2);
            this.treeListScenary.Name = "treeListScenary";
            this.treeListScenary.OptionsBehavior.AllowRecursiveNodeChecking = true;
            this.treeListScenary.OptionsBehavior.ImmediateEditor = false;
            this.treeListScenary.OptionsPrint.UsePrintStyles = true;
            this.treeListScenary.OptionsView.AllowHtmlDrawHeaders = true;
            this.treeListScenary.OptionsView.EnableAppearanceEvenRow = true;
            this.treeListScenary.OptionsView.EnableAppearanceOddRow = true;
            this.treeListScenary.OptionsView.ShowCheckBoxes = true;
            this.treeListScenary.Size = new System.Drawing.Size(954, 379);
            this.treeListScenary.TabIndex = 4;
            this.treeListScenary.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeListScenary_FocusedNodeChanged);
            // 
            // columnScenaryOperation
            // 
            this.columnScenaryOperation.Caption = "Операция";
            this.columnScenaryOperation.FieldName = "Операция";
            this.columnScenaryOperation.Fixed = DevExpress.XtraTreeList.Columns.FixedStyle.Left;
            this.columnScenaryOperation.MinWidth = 70;
            this.columnScenaryOperation.Name = "columnScenaryOperation";
            this.columnScenaryOperation.OptionsColumn.AllowEdit = false;
            this.columnScenaryOperation.OptionsColumn.AllowMove = false;
            this.columnScenaryOperation.OptionsColumn.AllowSort = false;
            this.columnScenaryOperation.OptionsColumn.ReadOnly = true;
            this.columnScenaryOperation.Visible = true;
            this.columnScenaryOperation.VisibleIndex = 0;
            this.columnScenaryOperation.Width = 397;
            // 
            // columnScenaryParametrValue
            // 
            this.columnScenaryParametrValue.Caption = "Значение аргумента";
            this.columnScenaryParametrValue.FieldName = "Значение аргумента";
            this.columnScenaryParametrValue.Name = "columnScenaryParametrValue";
            this.columnScenaryParametrValue.OptionsColumn.AllowMove = false;
            this.columnScenaryParametrValue.OptionsColumn.AllowSort = false;
            this.columnScenaryParametrValue.Visible = true;
            this.columnScenaryParametrValue.VisibleIndex = 1;
            // 
            // columnActionId
            // 
            this.columnActionId.Caption = "Id";
            this.columnActionId.FieldName = "Id";
            this.columnActionId.Name = "columnActionId";
            this.columnActionId.OptionsColumn.AllowEdit = false;
            this.columnActionId.OptionsColumn.AllowSort = false;
            this.columnActionId.OptionsColumn.ReadOnly = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label1);
            this.panel3.Controls.Add(this.btnRun);
            this.panel3.Controls.Add(this.button_run_interrogate);
            this.panel3.Controls.Add(this.btnAddDevice);
            this.panel3.Controls.Add(this.btnDelDevice);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(956, 2);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(125, 475);
            this.panel3.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.label1.Location = new System.Drawing.Point(4, 134);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 332);
            this.label1.TabIndex = 11;
            // 
            // btnRun
            // 
            this.btnRun.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnRun.Location = new System.Drawing.Point(4, 96);
            this.btnRun.Margin = new System.Windows.Forms.Padding(10);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(116, 28);
            this.btnRun.TabIndex = 10;
            this.btnRun.Text = "Выполнить";
            this.btnRun.UseVisualStyleBackColor = true;
            // 
            // button_run_interrogate
            // 
            this.button_run_interrogate.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.button_run_interrogate.Location = new System.Drawing.Point(4, 57);
            this.button_run_interrogate.Margin = new System.Windows.Forms.Padding(10);
            this.button_run_interrogate.Name = "button_run_interrogate";
            this.button_run_interrogate.Size = new System.Drawing.Size(116, 28);
            this.button_run_interrogate.TabIndex = 9;
            this.button_run_interrogate.Text = "Опрос";
            this.button_run_interrogate.UseVisualStyleBackColor = true;
            // 
            // btnAddDevice
            // 
            this.btnAddDevice.Image = global::MIL82Gui.Properties.Resources.plus_icon;
            this.btnAddDevice.Location = new System.Drawing.Point(4, 2);
            this.btnAddDevice.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddDevice.Name = "btnAddDevice";
            this.btnAddDevice.Size = new System.Drawing.Size(41, 45);
            this.btnAddDevice.TabIndex = 4;
            this.btnAddDevice.UseVisualStyleBackColor = true;
            // 
            // btnDelDevice
            // 
            this.btnDelDevice.Image = global::MIL82Gui.Properties.Resources.remDev;
            this.btnDelDevice.Location = new System.Drawing.Point(49, 2);
            this.btnDelDevice.Margin = new System.Windows.Forms.Padding(2);
            this.btnDelDevice.Name = "btnDelDevice";
            this.btnDelDevice.Size = new System.Drawing.Size(41, 45);
            this.btnDelDevice.TabIndex = 1;
            this.btnDelDevice.UseVisualStyleBackColor = true;
            this.btnDelDevice.Visible = false;
            // 
            // tabPageData
            // 
            this.tabPageData.AutoScroll = true;
            this.tabPageData.Controls.Add(this.treeListData);
            this.tabPageData.Controls.Add(this.panel2);
            this.tabPageData.Location = new System.Drawing.Point(4, 30);
            this.tabPageData.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageData.Name = "tabPageData";
            this.tabPageData.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageData.Size = new System.Drawing.Size(1083, 479);
            this.tabPageData.TabIndex = 8;
            this.tabPageData.Text = "Данные";
            this.tabPageData.UseVisualStyleBackColor = true;
            // 
            // treeListData
            // 
            this.treeListData.Appearance.FocusedCell.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListData.Appearance.FocusedCell.Options.UseFont = true;
            this.treeListData.Appearance.HeaderPanel.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListData.Appearance.HeaderPanel.Options.UseFont = true;
            this.treeListData.Appearance.Row.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListData.Appearance.Row.Options.UseFont = true;
            this.treeListData.Appearance.SelectedRow.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListData.Appearance.SelectedRow.Options.UseFont = true;
            this.treeListData.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.columnParams,
            this.treeListColumn1,
            this.treeListColumn2,
            this.treeListColumn3});
            this.treeListData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeListData.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.treeListData.Location = new System.Drawing.Point(2, 2);
            this.treeListData.Margin = new System.Windows.Forms.Padding(2);
            this.treeListData.Name = "treeListData";
            this.treeListData.OptionsBehavior.AllowRecursiveNodeChecking = true;
            this.treeListData.OptionsBehavior.ImmediateEditor = false;
            this.treeListData.OptionsPrint.UsePrintStyles = true;
            this.treeListData.OptionsSelection.MultiSelect = true;
            this.treeListData.OptionsSelection.UseIndicatorForSelection = true;
            this.treeListData.OptionsView.AllowHtmlDrawHeaders = true;
            this.treeListData.OptionsView.AutoWidth = false;
            this.treeListData.OptionsView.ShowCheckBoxes = true;
            this.treeListData.Size = new System.Drawing.Size(945, 475);
            this.treeListData.TabIndex = 0;
            // 
            // columnParams
            // 
            this.columnParams.Caption = "Параметры";
            this.columnParams.FieldName = "Параметры";
            this.columnParams.Fixed = DevExpress.XtraTreeList.Columns.FixedStyle.Left;
            this.columnParams.MinWidth = 86;
            this.columnParams.Name = "columnParams";
            this.columnParams.OptionsColumn.AllowEdit = false;
            this.columnParams.OptionsColumn.AllowMove = false;
            this.columnParams.OptionsColumn.AllowSort = false;
            this.columnParams.OptionsColumn.ReadOnly = true;
            this.columnParams.Visible = true;
            this.columnParams.VisibleIndex = 0;
            this.columnParams.Width = 259;
            // 
            // treeListColumn1
            // 
            this.treeListColumn1.Caption = "123";
            this.treeListColumn1.FieldName = "123";
            this.treeListColumn1.Name = "treeListColumn1";
            this.treeListColumn1.Visible = true;
            this.treeListColumn1.VisibleIndex = 1;
            this.treeListColumn1.Width = 80;
            // 
            // treeListColumn2
            // 
            this.treeListColumn2.Caption = "234";
            this.treeListColumn2.FieldName = "234";
            this.treeListColumn2.Name = "treeListColumn2";
            this.treeListColumn2.Visible = true;
            this.treeListColumn2.VisibleIndex = 2;
            // 
            // treeListColumn3
            // 
            this.treeListColumn3.Caption = "678";
            this.treeListColumn3.FieldName = "678";
            this.treeListColumn3.Name = "treeListColumn3";
            this.treeListColumn3.Visible = true;
            this.treeListColumn3.VisibleIndex = 3;
            this.treeListColumn3.Width = 360;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnWriteKefs);
            this.panel2.Controls.Add(this.btnReadKefs);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(947, 2);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(134, 475);
            this.panel2.TabIndex = 1;
            // 
            // btnWriteKefs
            // 
            this.btnWriteKefs.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnWriteKefs.Location = new System.Drawing.Point(12, 70);
            this.btnWriteKefs.Margin = new System.Windows.Forms.Padding(10);
            this.btnWriteKefs.Name = "btnWriteKefs";
            this.btnWriteKefs.Size = new System.Drawing.Size(116, 50);
            this.btnWriteKefs.TabIndex = 10;
            this.btnWriteKefs.Text = "Записать коэффициенты";
            this.btnWriteKefs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnWriteKefs.UseVisualStyleBackColor = true;
            // 
            // btnReadKefs
            // 
            this.btnReadKefs.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnReadKefs.Location = new System.Drawing.Point(12, 10);
            this.btnReadKefs.Margin = new System.Windows.Forms.Padding(10);
            this.btnReadKefs.Name = "btnReadKefs";
            this.btnReadKefs.Size = new System.Drawing.Size(116, 51);
            this.btnReadKefs.TabIndex = 9;
            this.btnReadKefs.Text = "Считать коэффициенты";
            this.btnReadKefs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnReadKefs.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(1091, 39);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(10, 513);
            this.panel1.TabIndex = 8;
            // 
            // toolStripRunStop
            // 
            this.toolStripRunStop.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripRunStop.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripRunStop.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStripRunStop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStop,
            this.labelMainAction,
            this.labelMainActionTime,
            this.toolStripSeparator3,
            this.labelCurrentScenaryAction,
            this.labelCurrentScenaryActionTime,
            this.toolStripSeparator4,
            this.labelScenaryDelayProgress,
            this.toolStripProgressBar1,
            this.btnSettings,
            this.toolStripButton1,
            this.labelScenaryCurrentActionStatus});
            this.toolStripRunStop.Location = new System.Drawing.Point(0, 0);
            this.toolStripRunStop.MaximumSize = new System.Drawing.Size(0, 45);
            this.toolStripRunStop.Name = "toolStripRunStop";
            this.toolStripRunStop.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStripRunStop.Size = new System.Drawing.Size(1101, 39);
            this.toolStripRunStop.TabIndex = 6;
            this.toolStripRunStop.Text = "toolStrip1";
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(36, 36);
            this.btnStop.Text = "Опрос";
            this.btnStop.Visible = false;
            this.btnStop.Click += new System.EventHandler(this.toolStripButton1_Click);
            this.btnStop.VisibleChanged += new System.EventHandler(this.btnStop_VisibleChanged);
            // 
            // labelMainAction
            // 
            this.labelMainAction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelMainAction.ForeColor = System.Drawing.Color.Navy;
            this.labelMainAction.Name = "labelMainAction";
            this.labelMainAction.Size = new System.Drawing.Size(16, 36);
            this.labelMainAction.Text = "...";
            this.labelMainAction.ToolTipText = "Наименование корневой операции";
            this.labelMainAction.Visible = false;
            // 
            // labelMainActionTime
            // 
            this.labelMainActionTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelMainActionTime.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelMainActionTime.Name = "labelMainActionTime";
            this.labelMainActionTime.Size = new System.Drawing.Size(16, 36);
            this.labelMainActionTime.Text = "...";
            this.labelMainActionTime.ToolTipText = "Длительность корневой операции";
            this.labelMainActionTime.Visible = false;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 39);
            this.toolStripSeparator3.Visible = false;
            // 
            // labelCurrentScenaryAction
            // 
            this.labelCurrentScenaryAction.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCurrentScenaryAction.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelCurrentScenaryAction.Name = "labelCurrentScenaryAction";
            this.labelCurrentScenaryAction.Size = new System.Drawing.Size(16, 36);
            this.labelCurrentScenaryAction.Text = "...";
            this.labelCurrentScenaryAction.ToolTipText = "Наименование текущей операции";
            this.labelCurrentScenaryAction.Visible = false;
            // 
            // labelCurrentScenaryActionTime
            // 
            this.labelCurrentScenaryActionTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelCurrentScenaryActionTime.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelCurrentScenaryActionTime.Name = "labelCurrentScenaryActionTime";
            this.labelCurrentScenaryActionTime.Size = new System.Drawing.Size(16, 36);
            this.labelCurrentScenaryActionTime.Text = "...";
            this.labelCurrentScenaryActionTime.ToolTipText = "Длительность текущей операции";
            this.labelCurrentScenaryActionTime.Visible = false;
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 39);
            this.toolStripSeparator4.Visible = false;
            // 
            // labelScenaryDelayProgress
            // 
            this.labelScenaryDelayProgress.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelScenaryDelayProgress.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelScenaryDelayProgress.Name = "labelScenaryDelayProgress";
            this.labelScenaryDelayProgress.Size = new System.Drawing.Size(16, 36);
            this.labelScenaryDelayProgress.Text = "...";
            this.labelScenaryDelayProgress.ToolTipText = "Статус текущей операции";
            this.labelScenaryDelayProgress.Visible = false;
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(200, 36);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.toolStripProgressBar1.Value = 40;
            this.toolStripProgressBar1.Visible = false;
            // 
            // btnSettings
            // 
            this.btnSettings.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.btnSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSettings.Image = ((System.Drawing.Image)(resources.GetObject("btnSettings.Image")));
            this.btnSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(36, 36);
            this.btnSettings.Text = "Настройки";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.memuNewFile,
            this.menuOpenFile,
            this.MenuSave,
            this.MenuSaveAs,
            this.toolStripMenuItem1,
            this.menu_report,
            this.toolStripMenuItem2,
            this.menuConsoleVisible});
            this.toolStripButton1.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(45, 36);
            this.toolStripButton1.Text = "Файл...";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // memuNewFile
            // 
            this.memuNewFile.Name = "memuNewFile";
            this.memuNewFile.Size = new System.Drawing.Size(193, 26);
            this.memuNewFile.Text = "Создать";
            // 
            // menuOpenFile
            // 
            this.menuOpenFile.Name = "menuOpenFile";
            this.menuOpenFile.Size = new System.Drawing.Size(193, 26);
            this.menuOpenFile.Text = "Открыть...";
            // 
            // MenuSave
            // 
            this.MenuSave.Name = "MenuSave";
            this.MenuSave.Size = new System.Drawing.Size(193, 26);
            this.MenuSave.Text = "Сохранить";
            // 
            // MenuSaveAs
            // 
            this.MenuSaveAs.Name = "MenuSaveAs";
            this.MenuSaveAs.Size = new System.Drawing.Size(193, 26);
            this.MenuSaveAs.Text = "Сохранить как...";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(190, 6);
            // 
            // menu_report
            // 
            this.menu_report.Name = "menu_report";
            this.menu_report.Size = new System.Drawing.Size(193, 26);
            this.menu_report.Text = "Отчёт для ОТК";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(190, 6);
            // 
            // menuConsoleVisible
            // 
            this.menuConsoleVisible.Name = "menuConsoleVisible";
            this.menuConsoleVisible.Size = new System.Drawing.Size(193, 26);
            this.menuConsoleVisible.Text = "Консоль";
            // 
            // labelScenaryCurrentActionStatus
            // 
            this.labelScenaryCurrentActionStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelScenaryCurrentActionStatus.ForeColor = System.Drawing.SystemColors.Desktop;
            this.labelScenaryCurrentActionStatus.Name = "labelScenaryCurrentActionStatus";
            this.labelScenaryCurrentActionStatus.Size = new System.Drawing.Size(16, 36);
            this.labelScenaryCurrentActionStatus.Text = "...";
            this.labelScenaryCurrentActionStatus.ToolTipText = "Статус текущей операции";
            this.labelScenaryCurrentActionStatus.Visible = false;
            // 
            // LogMemo
            // 
            this.LogMemo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogMemo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogMemo.Font = new System.Drawing.Font("Consolas", 10.2F);
            this.LogMemo.Location = new System.Drawing.Point(0, 0);
            this.LogMemo.Name = "LogMemo";
            this.LogMemo.Size = new System.Drawing.Size(1101, 149);
            this.LogMemo.TabIndex = 8;
            this.LogMemo.Text = "";
            this.LogMemo.TextChanged += new System.EventHandler(this.LogMemo_TextChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuDeletePerformLogTreeNode});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(68, 26);
            // 
            // menuDeletePerformLogTreeNode
            // 
            this.menuDeletePerformLogTreeNode.Name = "menuDeletePerformLogTreeNode";
            this.menuDeletePerformLogTreeNode.Size = new System.Drawing.Size(67, 22);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MIL82MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1101, 705);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MIL82MainForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MIL82MainForm_FormClosing);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageDevices.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListDevices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeListScenary)).EndInit();
            this.panel3.ResumeLayout(false);
            this.tabPageData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeListData)).EndInit();
            this.panel2.ResumeLayout(false);
            this.toolStripRunStop.ResumeLayout(false);
            this.toolStripRunStop.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        public System.Windows.Forms.ToolStrip toolStripRunStop;
        public System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.ToolStripButton btnSettings;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.TabControl tabControl1;
        public System.Windows.Forms.ToolStripMenuItem memuNewFile;
        public System.Windows.Forms.ToolStripMenuItem MenuSaveAs;
        public System.Windows.Forms.ToolStripMenuItem menuOpenFile;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        public System.Windows.Forms.ToolStripMenuItem menu_report;
        public System.Windows.Forms.TabPage tabPageData;
        public DevExpress.XtraTreeList.TreeList treeListData;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumn1;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumn2;
        private DevExpress.XtraTreeList.Columns.TreeListColumn treeListColumn3;        
        private System.Windows.Forms.Panel panel3;
        public System.Windows.Forms.Button btnAddDevice;
        public System.Windows.Forms.Button btnDelDevice;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnParams;
        public System.Windows.Forms.TabPage tabPageDevices;
        public System.Windows.Forms.ToolStripLabel labelMainAction;
        public System.Windows.Forms.ToolStripLabel labelCurrentScenaryAction;
        public System.Windows.Forms.ToolStripLabel labelScenaryCurrentActionStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        public System.Windows.Forms.ToolStripMenuItem menuDeletePerformLogTreeNode;
        public System.Windows.Forms.RichTextBox LogMemo;
        public System.Windows.Forms.ToolStripLabel labelMainActionTime;
        public System.Windows.Forms.ToolStripLabel labelCurrentScenaryActionTime;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        public System.Windows.Forms.ToolStripSeparator toolStripSeparator4;        
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        public System.Windows.Forms.ToolStripMenuItem menuConsoleVisible;
        public System.Windows.Forms.ToolStripDropDownButton toolStripButton1;
        public System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        public System.Windows.Forms.ToolStripLabel labelScenaryDelayProgress;
        public System.Windows.Forms.Button button_run_interrogate;
        private System.Windows.Forms.SplitContainer splitContainer2;
        public DevExpress.XtraTreeList.TreeList treeListDevices;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnSerial;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnAddy;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnStatus;
        public DevExpress.XtraTreeList.TreeList treeListScenary;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnScenaryOperation;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnScenaryParametrValue;
        public DevExpress.XtraTreeList.Columns.TreeListColumn columnActionId;
        public System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        public System.Windows.Forms.Button btnWriteKefs;
        public System.Windows.Forms.Button btnReadKefs;
        public System.Windows.Forms.ToolStripMenuItem MenuSave;
    }
}