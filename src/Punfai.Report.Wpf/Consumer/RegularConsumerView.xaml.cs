using System.Windows.Controls;

namespace Punfai.Report.Wpf.Consumer
{
    public partial class RegularConsumerView : UserControl
    {
        public RegularConsumerView()
        {
            InitializeComponent();
        }

        //private void GenerateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    if (dialog.ShowDialog() == true)
        //    {
        //        string filepath = dialog.FileName;
        //        if (filepath == null || filepath == string.Empty) return;
        //        FileInfo info = new FileInfo(filepath);
        //        if (!info.Exists)
        //            throw new Exception("What? File does not exist");
        //        var vm = DataContext as RegularConsumerViewModel;
        //        if (vm != null)
        //        {
        //            vm.GenerateCommand.Execute(filepath);
        //        }
        //    }
        //}

    }

}
