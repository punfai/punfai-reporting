using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;

namespace Punfai.Report.Wpf.Consumer
{
    public interface IReportPage
    {
        string PageTitle { get; }
        string[] Role { get; }
        int Order { get; }
        void Refresh();
    }

}
