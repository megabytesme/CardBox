using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Shared_Code;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using muxc = Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using System.Linq;

namespace _1809_UWP
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        private ObservableCollection<Card> filteredCards = new ObservableCollection<Card>();

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;

            ApplyBackdropOrAcrylic();

            NavView.ItemInvoked += NavView_ItemInvoked;
            Loaded += MainPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;

            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonPressedBackgroundColor = Colors.Transparent;

            Window.Current.SetTitleBar(AppTitleBar);
        }

        private void ApplyBackdropOrAcrylic()
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 12))
            {
                muxc.BackdropMaterial.SetApplyToRootOrPageBackground(this, true);
            }
            else
            {
                this.Background = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    TintColor = Colors.Transparent,
                    TintOpacity = 0.6,
                    FallbackColor = Colors.Gray
                };
            }
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(HomePage));
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = args.InvokedItemContainer as muxc.NavigationViewItem;
                switch (item.Tag.ToString())
                {
                    case "HomePage":
                        ContentFrame.Navigate(typeof(HomePage));
                        break;
                    case "AddCardPage":
                        ContentFrame.Navigate(typeof(AddCardPage));
                        break;
                }
            }
        }

        private void ContentFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            var backButton = (Button)FindName("BackButton");
            if (backButton != null)
            {
                backButton.Visibility = ContentFrame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            }

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = ContentFrame.CanGoBack ?
                AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Disabled;

            if (e.SourcePageType == typeof(HomePage))
            {
                SearchBox.Visibility = Visibility.Visible;
            }
            else
            {
                SearchBox.Visibility = Visibility.Collapsed;
            }
        }

        private void MainPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                e.Handled = true;
                GoBack();
            }
        }

        private void GoBack()
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        public Frame MainContentFrame => ContentFrame;

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text.ToLower();
                var filteredList = Cards.Where(card => card.CardName.ToLower().Contains(query) ||
                                                       card.CardNickname.ToLower().Contains(query)).ToList();

                filteredCards.Clear();
                foreach (var card in filteredList)
                {
                    filteredCards.Add(card);
                }

                if (ContentFrame.Content is HomePage homePage)
                {
                    homePage.FilteredCards = filteredCards;
                }
            }
        }

    }
}
