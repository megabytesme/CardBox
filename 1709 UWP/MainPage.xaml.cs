﻿using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Shared_Code;
using Windows.UI;
using Windows.UI.Core;

namespace _1709_UWP
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;

            ApplyAcrylic();

            NavView.ItemInvoked += NavView_ItemInvoked;
            Loaded += MainPage_Loaded;
            ContentFrame.Navigated += ContentFrame_Navigated;

            SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void ApplyAcrylic()
        {
            this.Background = new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                TintColor = Colors.Transparent,
                TintOpacity = 0.6,
                FallbackColor = Colors.Gray
            };
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(HomePage));
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var item = args.InvokedItemContainer as NavigationViewItem;
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
                 AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
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
    }
}
