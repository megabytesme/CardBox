using Shared_Code_UWP.BasePages;
using Windows.UI.Xaml.Controls;

namespace _1809_UWP
{
    public sealed partial class EditCardPage : EditCardPageBase
    {
        public EditCardPage()
        {
            this.InitializeComponent();
            MaterialHelper.ApplySystemBackdropOrAcrylic(this);
            SaveCardButton.Click += base.SaveCardButton_Click;
            InitializeBarcodeFormatPicker();
        }

        protected override ComboBox DisplayPicker => displayPicker;
    }
}