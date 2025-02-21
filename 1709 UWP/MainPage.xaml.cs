using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Shared_Code;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace _1709_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public MainPage()
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
