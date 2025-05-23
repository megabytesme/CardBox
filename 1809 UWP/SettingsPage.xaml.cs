﻿using Shared_Code_UWP.BasePages;
using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using ZXing;

namespace _1809_UWP
{
    public sealed partial class SettingsPage : SettingsPageBase
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            MaterialHelper.ApplySystemBackdropOrAcrylic(this);
            ResetAppButton.Click += base.ResetAppButton_Click;
            ImportCardsButton.Click += base.ImportCardsButton_Click;
            ExportCardsButton.Click += base.ExportCardsButton_Click;
            AboutButton.Click += base.AboutButton_Click;
        }
        protected override ProgressRing LoadingProgressRing => progressRing;
        protected override Type ScannerPageType => typeof(ScannerPage);

        protected override string AppVersionString
        {
            get
            {
                try
                {
                    Package package = Package.Current;
                    PackageId packageId = package.Id;
                    PackageVersion version = packageId.Version;

                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision} (1809_UWP)";
                }
                catch (Exception ex)
                {
                    return "Unknown Version (1809_UWP)";
                }
            }
        }

        protected override AddCardPageBase.IScannerResult GetLastScanResult()
        {
            var result = ScannerPage.LastScanResult;
            return result == null ? null : new ScannerResultAdapter(result);
        }

        protected override void ClearLastScanResult()
        {
            ScannerPage.LastScanResult = null;
        }

        private class ScannerResultAdapter : AddCardPageBase.IScannerResult
        {
            private readonly ScannerPage.ScannerResult _adaptee;
            public ScannerResultAdapter(ScannerPage.ScannerResult adaptee) { _adaptee = adaptee; }
            public string Text => _adaptee?.Text;
            public BarcodeFormat Format => _adaptee != null ? _adaptee.Format : default;
        }
    }
}