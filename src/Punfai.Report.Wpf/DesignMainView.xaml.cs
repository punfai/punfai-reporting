using System.Windows.Controls;

namespace Punfai.Report.Wpf
{
    public partial class DesignMainView : UserControl
    {
        public DesignMainView()
        {
            InitializeComponent();
        }

        // DO NOT DELETE THIS CODE UNLESS WE NO LONGER REQUIRE ASSEMBLY A!!!
        private void DummyFunctionToMakeSureReferencesGetCopiedProperly_DO_NOT_DELETE_THIS_CODE()
        {
            // Assembly A is used by this file, and that assembly depends on assembly B,
            // but this project does not have any code that explicitly references assembly B. Therefore, when another project references
            // this project, this project's assembly and the assembly A get copied to the project's bin directory, but not
            // assembly B. So in order to get the required assembly B copied over, we add some dummy code here (that never
            // gets called) that references assembly B; this will flag VS/MSBuild to copy the required assembly B over as well.
            //var dummyType1 = typeof(Microsoft.Expression.Interactivity.Core.ExtendedVisualStateManager);
            var dummyType1 = typeof(Microsoft.Xaml.Behaviors.Core.ExtendedVisualStateManager);
        }
    }
}
