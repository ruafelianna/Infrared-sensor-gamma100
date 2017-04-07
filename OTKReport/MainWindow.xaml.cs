using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Xps.Packaging;
using System.Windows.Xps.Serialization;

using System.Globalization;
using System.IO;
using System.IO.Packaging;

namespace OTKReport
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Run[] Run_Test 
        {
            get
            {
                return new Run[12]{
                run1_test1, run1_test2, run1_test3, 
                run2_test1, run2_test2, run2_test3, 
                run3_test1, run3_test2, run3_test3, 
                run4_test1, run4_test2, run4_test3 };
            }
        }

        public Run[] Run_Conc
        {
            get
            {
                return new Run[12]{
                run1_conc1, run1_conc2, run1_conc3, 
                run2_conc1, run2_conc2, run2_conc3, 
                run3_conc1, run3_conc2, run3_conc3, 
                run4_conc1, run4_conc2, run4_conc3, };
            }
        }

        public Run[] Run_Date
        {
            get
            {
                return new Run[5] { run_termo_date, run1_date, run2_date, run3_date, run4_date };
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)this.document).DocumentPaginator;

                try
                {
                    printDialog.PrintDocument(paginator, "Печать");
                }
                catch (Exception)
                {
                    MessageBox.Show(this, "Ошибка печати. Проверьте настройки принтера.", "Ошибка печати",
                            MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "отчёт ИК датчик ГАММА-100"; // Default file name
            dlg.DefaultExt = ".xps"; // Default file extension
            dlg.Filter = "Документ отчёта (.xps)|*.xps"; // Filter files by extension
            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();
            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                string filename = dlg.FileName;
                FlowDocHelper.SaveFlowDocumentToXPS(this.document, filename);
            }
        }
    }

    public static class FlowDocHelper
    {
        public static MemoryStream FlowDocumentToXPS(FlowDocument flowDocument, int width, int height)
        {
            MemoryStream stream = new MemoryStream();
            using (Package package = Package.Open(stream, FileMode.Create, FileAccess.ReadWrite))
            {
                using (XpsDocument xpsDoc = new XpsDocument(package, CompressionOption.Maximum))
                {
                    XpsSerializationManager rsm = new XpsSerializationManager(new XpsPackagingPolicy(xpsDoc), false);
                    DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
                    paginator.PageSize = new System.Windows.Size(width, height);
                    rsm.SaveAsXaml(paginator);
                    rsm.Commit();
                }
            }
            stream.Position = 0;
            return stream;
        }

        public static void SaveFlowDocumentToXPS(FlowDocument flowDocument, string filename)
        {
            var strm = FlowDocumentToXPS(flowDocument, 768, 676);
            File.WriteAllBytes(filename, strm.ToArray());
        }
    }
}
