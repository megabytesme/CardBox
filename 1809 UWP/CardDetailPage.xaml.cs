using Shared_Code_UWP.BasePages;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace _1809_UWP
{
    public sealed partial class CardDetailPage : CardDetailPageBase
    {
        public CardDetailPage()
        {
            this.InitializeComponent();
            MaterialHelper.ApplySystemBackdropOrAcrylic(this);
            EditCardButton.Click += base.EditCardButton_Click;
            DeleteCardButton.Click += base.DeleteCardButton_Click;
        }

        protected override Image BarcodeImage => barcodeImage;
        protected override ProgressRing LoadingProgressRing => loadingProgressRing;
        protected override ListView LocationsListView => locationsListView;
        protected override TextBlock NoLocationsTextBlock => noLocationsTextBlock;
        protected override TextBlock LocationErrorTextBlock => locationErrorTextBlock;
        protected override Type EditCardPageType => typeof(EditCardPage);
        protected override Type MainPageType => typeof(MainPage);

        private void NavigateToLocationButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            base.NavigateToLocationButton_Click(sender, e);
        }

        private void Barcode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            base.Barcode_Tapped(sender, e);
        }
    }
}