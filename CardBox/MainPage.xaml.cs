using System.Collections.ObjectModel;

namespace CardBox
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            SetButtonTextsAndFonts();

            // dummy cards
            Cards.Add(new Card { CardTitle = "Loyalty Card 1" });
            Cards.Add(new Card { CardTitle = "Loyalty Card 2" });
        }

        private void SetButtonTextsAndFonts()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                BackButton.Text = "\uE5C4";
                BackButton.FontFamily = "sans-serif";
                HomeButton.Text = "\uE88A";
                HomeButton.FontFamily = "sans-serif";
                AddButton.Text = "\uE145";
                AddButton.FontFamily = "sans-serif";
                SettingsButton.Text = "\uE8B8";
                SettingsButton.FontFamily = "sans-serif";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                BackButton.Text = "\uF3A5";
                BackButton.FontFamily = "system";
                HomeButton.Text = "\uF6A8";
                HomeButton.FontFamily = "system";
                AddButton.Text = "\uF59B";
                AddButton.FontFamily = "system";
                SettingsButton.Text = "\uF751";
                SettingsButton.FontFamily = "system";
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                BackButton.Text = "\uE72B";
                BackButton.FontFamily = "Segoe MDL2 Assets";
                HomeButton.Text = "\uE80F";
                HomeButton.FontFamily = "Segoe MDL2 Assets";
                AddButton.Text = "\uE710";
                AddButton.FontFamily = "Segoe MDL2 Assets";
                SettingsButton.Text = "\uE713";
                SettingsButton.FontFamily = "Segoe MDL2 Assets";
            }
        }

        private void OnBackClicked(object sender, EventArgs e)
        {

        }

        private void OnHomeClicked(object sender, EventArgs e)
        {

        }

        private void OnAddClicked(object sender, EventArgs e)
        {
            Cards.Add(new Card { CardTitle = "New Loyalty Card" });
        }

        private void OnSettingsClicked(object sender, EventArgs e)
        {

        }
    }

    public class Card
    {
        public string CardTitle { get; set; } = string.Empty;
    }
}
