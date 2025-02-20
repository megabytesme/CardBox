namespace CardBox
{
    public partial class NavbarControl : ContentView
    {
        public NavbarControl()
        {
            InitializeComponent();
            SetButtonTextsAndFonts();
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//AddCardPage"); 
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//SettingsPage");
        }

        private void SetButtonTextsAndFonts()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                HomeButton.Text = "🏠";
                AddButton.Text = "➕";
                SettingsButton.Text = "⚙️";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                HomeButton.Text = "🏠";
                AddButton.Text = "➕";
                SettingsButton.Text = "⚙️";
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                SetButtonProps(HomeButton, "\uE80F", "Segoe MDL2 Assets");
                SetButtonProps(AddButton, "\uE710", "Segoe MDL2 Assets");
                SetButtonProps(SettingsButton, "\uE713", "Segoe MDL2 Assets");
            }
        }


        private void SetButtonProps(Button button, string text, string fontFamily)
        {
            button.Text = text;
            button.FontFamily = fontFamily;
        }
    }
}
