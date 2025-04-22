namespace CardBox
{
    public partial class NavbarControl : ContentView
    {
        public NavbarControl()
        {
            InitializeComponent();
            SetButtonTextsAndFonts();
        }

        private async void OnAddClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddCardPage());
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private void SetButtonTextsAndFonts()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                AddButton.Text = "➕";
                SettingsButton.Text = "⚙️";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                AddButton.Text = "➕";
                SettingsButton.Text = "⚙️";
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
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
