using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using Windows.UI;
using muxc = Microsoft.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using System.Linq;
using System;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using System.ComponentModel;

namespace _1809_UWP
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;
        private ObservableCollection<Card> filteredCards = new ObservableCollection<Card>();

        public Frame MainContentFrame => ContentFrame;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            MaterialHelper.ApplySystemBackdropOrAcrylic(this);
            SetupTitleBar();

            Loaded += MainPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigated += (s, e) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentFrame.CanGoBack)));
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavView_Navigate("HomePage", null);

            var homeItem = NavView.MenuItems.OfType<muxc.NavigationViewItem>().FirstOrDefault(i => i.Tag?.ToString() == "HomePage");
            if (homeItem != null)
            {
                NavView.SelectedItem = homeItem;
            }
            SearchBox.Visibility = (ContentFrame.CurrentSourcePageType == typeof(HomePage)) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetupTitleBar()
        {
            var appViewTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appViewTitleBar.ButtonBackgroundColor = Colors.Transparent;
            appViewTitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
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
                var tagToSelect = e.SourcePageType?.Name;
                if (tagToSelect != null)
                {
                    NavView.SelectedItem = NavView.MenuItems
                                                .OfType<muxc.NavigationViewItem>()
                                                .FirstOrDefault(item => item.Tag?.ToString() == tagToSelect);
                }
                else
                {
                    NavView.SelectedItem = null;
                }
            }

            SearchBox.Visibility = (e.SourcePageType == typeof(HomePage)) ? Visibility.Visible : Visibility.Collapsed;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ContentFrame.CanGoBack)));
        }

        private void NavView_Navigate(string navItemTag, NavigationTransitionInfo transitionInfo)
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
                object parameter = (pageType == typeof(HomePage)) ? new ObservableCollection<Card>(this.Cards) : null;
                ContentFrame.Navigate(pageType, parameter, transitionInfo);
            }
            else if (pageType == typeof(HomePage) && ContentFrame.CurrentSourcePageType == typeof(HomePage))
            {
                ContentFrame.Navigate(pageType, new ObservableCollection<Card>(this.Cards), new SuppressNavigationTransitionInfo());
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
                var query = sender.Text?.ToLowerInvariant() ?? "";

                var filteredList = Cards.Where(card => (card.CardName?.ToLowerInvariant() ?? "").Contains(query) ||
                                                       (card.CardNickname?.ToLowerInvariant() ?? "").Contains(query)).ToList();
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
            else if (string.IsNullOrEmpty(sender.Text) && (args.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange || args.Reason == AutoSuggestionBoxTextChangeReason.SuggestionChosen))
            {
                if (ContentFrame.Content is HomePage homePage)
                {
                    filteredCards = new ObservableCollection<Card>(this.Cards);
                    homePage.FilteredCards = filteredCards;
                }
            }
        }
    }
}