using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;
using Windows.UI.Core;
using System.Linq;
using System;
using Windows.UI.Xaml.Navigation;

namespace _1703_UWP
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        private ObservableCollection<Card> filteredCards;

        public MainPage()
        {
            this.InitializeComponent();

            filteredCards = new ObservableCollection<Card>(this.Cards);
            Loaded += MainPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;
            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.Content == null)
            {
                ContentFrame.Navigate(typeof(HomePage), filteredCards);
            }
            UpdateBackButtonVisibility();
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.Tag is string tag)
            {
                switch (tag)
                {
                    case "HomePage":
                        if (ContentFrame.CurrentSourcePageType != typeof(HomePage))
                        {
                            ContentFrame.Navigate(typeof(HomePage), new ObservableCollection<Card>(this.Cards));
                        }
                        break;
                }
            }
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdateBackButtonVisibility();

            bool isHomePage = e.SourcePageType == typeof(HomePage);
            SearchBox.Visibility = isHomePage ? Visibility.Visible : Visibility.Collapsed;

            if (!isHomePage)
            {
                SearchBox.Text = string.Empty;
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

        private void UpdateBackButtonVisibility()
        {
            try
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                   ContentFrame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting back button visibility: {ex.Message}");
            }
        }

        public Frame MainContentFrame => ContentFrame;

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput || args.Reason == AutoSuggestionBoxTextChangeReason.ProgrammaticChange)
            {
                string query = sender.Text?.ToLowerInvariant() ?? "";

                var filteredListSource = Cards.Where(card =>
                                                    (card.CardName?.ToLowerInvariant() ?? "").Contains(query) ||
                                                    (card.CardNickname?.ToLowerInvariant() ?? "").Contains(query))
                                               .ToList();

                filteredCards.Clear();
                foreach (var card in filteredListSource)
                {
                    filteredCards.Add(card);
                }

                if (ContentFrame.Content is HomePage currentPage && ContentFrame.CurrentSourcePageType == typeof(HomePage))
                {
                    currentPage.FilteredCards = filteredCards;
                }
                else if (ContentFrame.CurrentSourcePageType != typeof(HomePage) && !string.IsNullOrEmpty(query))
                {
                    ContentFrame.Navigate(typeof(HomePage), filteredCards);
                }
                else if (string.IsNullOrEmpty(query) && ContentFrame.CurrentSourcePageType != typeof(HomePage))
                {
                    ContentFrame.Navigate(typeof(HomePage), new ObservableCollection<Card>(this.Cards));
                }
            }
        }
    }
}