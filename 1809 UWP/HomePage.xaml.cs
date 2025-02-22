using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;

namespace _1809_UWP
{
    public sealed partial class HomePage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public HomePage()
        {
            this.InitializeComponent();
            DataContext = this;
        }

        private void ViewCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Card selectedCard)
            {
                Frame.Navigate(typeof(CardDetailPage), selectedCard);
            }
        }
    }
}
