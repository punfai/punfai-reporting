using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace Punfai.Report.Wpf
{
    public class EnumStateBehavior : Behavior<FrameworkElement>
    {
        [Category("Common")]
        public object EnumProperty
        {
            get { return (object)GetValue(EnumPropertyProperty); }
            set { SetValue(EnumPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EnumProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnumPropertyProperty =
            DependencyProperty.Register("EnumProperty", typeof(object), typeof(EnumStateBehavior), new UIPropertyMetadata(null, EnumPropertyChanged));

        static void EnumPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;
            EnumStateBehavior eb = sender as EnumStateBehavior;
            if (eb.AssociatedObject == null) return;

            if (e.NewValue == null)
            {
                return;
            }

            VisualStateManager.GoToElementState(eb.AssociatedObject, e.NewValue.ToString(), true);
        }

    }
}
