using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Shared_Code;

namespace _1709_UWP
{
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;

            // Navigate to HomePage by default
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(typeof(HomePage));
        }

        public Frame MainContentFrame => ContentFrame;
    }
}
