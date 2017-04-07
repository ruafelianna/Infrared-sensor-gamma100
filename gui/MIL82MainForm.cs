using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reflection;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.Xml;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Diagnostics.Contracts;

namespace MIL82Gui
{
    using TreeListNode = DevExpress.XtraTreeList.Nodes.TreeListNode;
    using TreeListNodes = DevExpress.XtraTreeList.Nodes.TreeListNodes;
    using TreeListColumn = DevExpress.XtraTreeList.Columns.TreeListColumn;
    using TreeList = DevExpress.XtraTreeList.TreeList;

    using Win = ManagedWinapi.Windows;
    using WinApi = ManagedWinapi.Windows.Import;

    

    public partial class MIL82MainForm : Form
    {
        public class NodeItem
        {
            public string TreeList { get; set; }
            public int Index { get; set; }
            public bool IsMarkedOut { get; set; }
            public CheckState State { get; set; }            
        }
        public class ColumnItem
        {
            public string TreeList { get; set; }
            public int Index { get; set; }
            public bool IsMarkedOut { get; set; }
        }

        public class TreeListItems
        {
            public NodeItem[] Nodes { get; set; }
            public ColumnItem[] Columns { get; set; }
        }

        //static readonly string windowplacementFileName = "mnwn.pl";
        public static MIL82MainForm form = new MIL82MainForm();
              
        public MIL82MainForm()
        {
            InitializeComponent();
            while (treeListData.Columns.Count > 1)
                treeListData.Columns.RemoveAt(treeListData.Columns.Count - 1);            
            OnClickPneumo = null;
        }

        private void MIL82MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
           
        }

        public EventHandler OnClickPneumo { get; set; }

        private CancellationTokenSource CancellationSource { get; set; }

        public void SetCancellationTokenSource( CancellationTokenSource cts ) {CancellationSource = cts;}

        private static CancellationToken defCancelationToken = new CancellationToken();
        public CancellationToken Cancellation { 
            get {
                return this.CancellationSource == null ? defCancelationToken : CancellationSource.Token;
            } 
        }
       

        
        private void LogMemo_TextChanged(object sender, EventArgs e)
        {
            //var msg = this.LogMemo.Lines.LastOrDefault(s => !string.IsNullOrWhiteSpace(s));
            //if (string.IsNullOrWhiteSpace(msg)) return;
            //var m = Regex.Match(msg, "(?<time>(\\d+):(\\d+):(\\d+))\\|(?<what>\\w+)\\|(?<msg>[^$]+)$");
            //if (m.Success) {
            //    var s = m.Groups["what"].Value;
            //    this.lblStatusAction.ForeColor = (s == "ERROR" || s == "FATAL") ? Color.Red : Color.Navy;
            //    this.lblStatusAction.Text = m.Groups["msg"].Value;
            //}
            if (!timer1.Enabled)
                timer1.Enabled = true;

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if ( this.ActiveControl!=LogMemo ) {
                LogMemo.SelectionStart = LogMemo.Text.Length;
                LogMemo.ScrollToCaret();
                timer1.Enabled = false;
            }
        }

        private void cbPeumo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OnClickPneumo != null)
                OnClickPneumo(sender, e);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (CancellationSource != null)
                CancellationSource.Cancel();
        }

        private void btnStop_VisibleChanged(object sender, EventArgs e)
        {
            

        }

        private void buttonPause_CheckStateChanged(object sender, EventArgs e)
        {
            
        }


        private void treeListDevices_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            this.btnDelDevice.Visible = e.Node != null;
        }

        private void treeListScenary_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            var s = "";
            var node = e.Node;
            while (node != null)
            {
                s = node.GetDisplayText(this.columnScenaryOperation) + (s == "" ? "" : "\r\n\r\n") + s;
                node = node.ParentNode;
            }
            this.label1.Text = s;
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {

        }  
    }

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public class TreeLsitCellCustomDraw
    {
        public enum Kind
        {
            Error, Result, Info, Warn
        }
        public int ColumnIndex { get; set; }
        public Kind kind { get; set; }
    }

    /// <summary>
    /// TypeConverter для Enum, преобразовывающий Enum к строке с
    /// учетом атрибута Description
    /// </summary>
    public class EnumTypeConverter : EnumConverter
    {
        private Type _enumType;
        /// <summary>Инициализирует экземпляр</summary>
        /// <param name="type">тип Enum</param>
        public EnumTypeConverter(Type type)
            : base(type)
        {
            _enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
          Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture,
          object value, Type destType)
        {
            FieldInfo fi = _enumType.GetField(Enum.GetName(_enumType, value));
            DescriptionAttribute dna =
              (DescriptionAttribute)Attribute.GetCustomAttribute(
                fi, typeof(DescriptionAttribute));

            if (dna != null)
                return dna.Description;
            else
                return value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
          Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture,
          object value)
        {
            foreach (FieldInfo fi in _enumType.GetFields()) {
                DescriptionAttribute dna =
                  (DescriptionAttribute)Attribute.GetCustomAttribute(
                    fi, typeof(DescriptionAttribute));

                if ((dna != null) && ((string)value == dna.Description))
                    return Enum.Parse(_enumType, fi.Name);
            }

            return Enum.Parse(_enumType, (string)value);
        }

    }

    /// <summary>

/// Summary description for TimeConverter.

/// </summary>

    public class TimeConverter : TypeConverter
    {

        public TimeConverter()
        {
        }

        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Type sourceType)
        {
            return sourceType == typeof(string) ? true : base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value)
        {            
            if (value is string) {
                try {
                    //DateTime ds = DateTime.ParseExact((string)value, new
                    //    string[] { "T" }, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
                    return DateTime.Parse(value as string, new DateTimeFormatInfo() { ShortTimePattern = "HH:mm:ss" });
                } catch (Exception) {
                    return new DateTime();
                }
            }
            return base.ConvertFrom(context, culture, value);
        }
        
        public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
        {   

            if(destinationType==typeof(DateTime) || destinationType==typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value, System.Type destinationType)
        {
            if(destinationType==typeof(string))
            {
                return ((DateTime)value).ToString("HH:mm:ss");
            }
            if (destinationType == typeof(DateTime))
                return (DateTime)value;

            return base.ConvertTo(context,culture,value,destinationType);
        }

        public override bool IsValid(System.ComponentModel.ITypeDescriptorContext context, object value)
        {
            if (value is string) {
                try {
                    //DateTime ds = DateTime.ParseExact((string)value, new
                    //    string[] { "T" }, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces);
                    DateTime ds = DateTime.Parse(value as string, new DateTimeFormatInfo() { ShortTimePattern = "HH:mm:ss" });
                    return true;
                } catch (Exception) {
                    return false;
                }
            }
            return false;
                        
        }
    }

    public class BooleanTypeConverter : BooleanConverter
    {
        public BooleanTypeConverter(string sfalse, string strue)
        {
            false_ = sfalse;
            true_ = strue;
        }

        readonly string false_, true_;

        public override object ConvertTo(ITypeDescriptorContext context,
          CultureInfo culture,
          object value,
          Type destType)
        {
            return (bool)value ? true_ : false_;
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
          CultureInfo culture,
          object value)
        {
            return (string)value == true_;
        }
    }

    public class VisibleBooleanTypeConverter : BooleanTypeConverter
    {
        public VisibleBooleanTypeConverter() : base ("Скрыть","Показать")
        {            
        }
    }

    public class YesNoConverter : BooleanTypeConverter
    {
        public YesNoConverter() : base("Нет", "Да")
        {
        }
    }

    public class TimePickerEditor : UITypeEditor
    {

        IWindowsFormsEditorService editorService;
        DateTimePicker picker = new DateTimePicker();
        
        public TimePickerEditor()
        {

            picker.Format = DateTimePickerFormat.Custom;
            picker.CustomFormat = "HH:mm:ss";
            picker.ShowUpDown = true;

        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {

            if (provider != null) {
                this.editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
            }

            if (this.editorService != null) {
                
                this.editorService.DropDownControl(picker);
                value = picker.Value;    
            }

            return value;

        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            if (context != null) {
                try {
                    var v = (DateTime)context.PropertyDescriptor.GetValue(context.Instance);

                    picker.Value = DateTimePicker.MinimumDateTime.AddHours(v.Hour).AddMinutes(v.Minute).AddSeconds(v.Second);
                } catch {
                    //picker.Value = DateTimePicker.MinimumDateTime;
                }
            }
            return UITypeEditorEditStyle.DropDown;
        }

    }
}
