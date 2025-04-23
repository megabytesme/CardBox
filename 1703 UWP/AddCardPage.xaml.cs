using Windows.UI.Xaml.Controls;
using System;
using ZXing;
using Shared_Code_UWP.BasePages;

namespace _1703_UWP
{
    public sealed partial class AddCardPage : AddCardPageBase
    {
        public AddCardPage()
        {
            this.InitializeComponent();
            AddCardButton.Click += base.AddCardButton_Click;
            ScanCardButton.Click += base.ScanCardButton_Click;
            InitializeBarcodeFormatPicker();
        }

        protected override TextBox CardNameEntry => cardNameEntry;
        protected override TextBox CardNicknameEntry => cardNicknameEntry;
        protected override TextBox CardNumberEntry => cardNumberEntry;
        protected override ComboBox DisplayPicker => displayPicker;
        protected override Type GetScannerPageType() => typeof(ScannerPage);

        protected override IScannerResult GetLastScanResult()
        {
            var result = ScannerPage.LastScanResult;
            return result == null ? null : new ScannerResultAdapter(result);
        }

        protected override void ClearLastScanResult()
        {
            ScannerPage.LastScanResult = null;
        }

        private class ScannerResultAdapter : IScannerResult
        {
            private readonly ScannerPage.ScannerResult _adaptee;
            public ScannerResultAdapter(ScannerPage.ScannerResult adaptee) { _adaptee = adaptee; }
            public string Text => _adaptee?.Text;
            public BarcodeFormat Format => _adaptee != null ? _adaptee.Format : default;
        }
    }
}