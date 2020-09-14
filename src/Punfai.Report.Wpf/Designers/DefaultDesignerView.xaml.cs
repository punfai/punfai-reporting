using Microsoft.Win32;
using System;
using System.IO;
using System.Windows.Controls;

namespace Punfai.Report.Wpf.Designers
{
    public partial class DefaultDesignerView : UserControl
    {
        public DefaultDesignerView()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var t = sender as TextBlock;
            System.Windows.Clipboard.SetText(t.Text);
        }

        private void ChooseTemplateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string filepath = dialog.FileName;
                if (filepath == null || filepath == string.Empty) return;
                FileInfo info = new FileInfo(filepath);
                if (!info.Exists)
                    throw new Exception("What? File does not exist");
                var vm = DataContext as DefaultDesignerViewModel;
                if (vm != null)
                {
                    vm.LoadTemplateCommand.Execute(filepath);
                }
            }

        }
        private void ExportTemplateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as DefaultDesignerViewModel;
            if (vm == null) return;
            Microsoft.Win32.SaveFileDialog dialog = new SaveFileDialog();
            dialog.FileName = vm.TemplateFileName == null ? vm.ReportName : vm.TemplateFileName;
            if (dialog.ShowDialog() == true)
            {
                string filepath = dialog.FileName;
                if (filepath == null || filepath == string.Empty) return;
                vm.ExportTemplateCommand.Execute(filepath);
            }

        }


    }

}
