using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Shared_Code;
using Windows.Foundation.Metadata;
using Windows.UI;
using muxc = Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using System.Linq;
using System;
using Windows.UI.Xaml.Navigation;

namespace _1809_UWP
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;
        private ObservableCollection<Card> filteredCards = new ObservableCollection<Card>();

        public Frame MainContentFrame => ContentFrame;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            ApplyBackdropOrAcrylic();
            SetupTitleBar();

            Loaded += MainPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavView_Navigate("HomePage", null);

            var homeItem = NavView.MenuItems.OfType<muxc.NavigationViewItem>().FirstOrDefault(i => i.Tag?.ToString() == "HomePage");
            if (homeItem != null)
            {
                NavView.SelectedItem = homeItem;
            }
        }

        private void SetupTitleBar()
        {
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonPressedBackgroundColor = Colors.Transparent;

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

        private void NavView_ItemInvoked(muxc.NavigationView sender, muxc.NavigationViewItemInvokedEventArgs args)
        {
            string tag;
            if (args.IsSettingsInvoked)
            {
                tag = "SettingsPage";
            }
            else
            {
                var invokedItem = args.InvokedItemContainer as muxc.NavigationViewItem;
                tag = invokedItem?.Tag?.ToString();
            }

            if (!string.IsNullOrEmpty(tag))
            {
                NavView_Navigate(tag, args.RecommendedNavigationTransitionInfo);
            }
        }

        private void NavView_BackRequested(muxc.NavigationView sender, muxc.NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem;
            }
            else
            {
                var tagToSelect = e.SourcePageType.Name;
                NavView.SelectedItem = NavView.MenuItems
                                            .OfType<muxc.NavigationViewItem>()
                                            .FirstOrDefault(item => item.Tag?.ToString() == tagToSelect);
            }

            SearchBox.Visibility = (e.SourcePageType == typeof(HomePage)) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void NavView_Navigate(string navItemTag, Windows.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
        {
            Type pageType = null;
            switch (navItemTag)
            {
                case "HomePage":
                    pageType = typeof(HomePage);
                    break;
                case "AddCardPage":
                    pageType = typeof(AddCardPage);
                    break;
                case "SettingsPage":
                    pageType = typeof(SettingsPage);
                    break;
            }

            if (pageType != null && ContentFrame.CurrentSourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType, null, transitionInfo);
            }
        }

        private bool TryGoBack()
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                return true;
            }
            return false;
        }

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
