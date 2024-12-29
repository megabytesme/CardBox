using Shared_Code;
using System.Collections.ObjectModel;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards => CardRepository.Instance.Cards;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnBackClicked(object sender, EventArgs e)
        {

        }

        private void OnHomeClicked(object sender, EventArgs e)
        {

        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {

        }
    }
}
