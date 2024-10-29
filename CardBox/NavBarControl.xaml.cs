namespace CardBox
{
    public partial class NavbarControl : ContentView
    {
        public NavbarControl()
        {
            InitializeComponent();
            SetButtonTextsAndFonts();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
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
                SetButtonProps(BackButton, "\uE5C4", "sans-serif");
                SetButtonProps(HomeButton, "\uE88A", "sans-serif");
                SetButtonProps(AddButton, "\uE145", "sans-serif");
                SetButtonProps(SettingsButton, "\uE8B8", "sans-serif");
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                SetButtonProps(BackButton, "\uF3A5", "system");
                SetButtonProps(HomeButton, "\uF6A8", "system");
                SetButtonProps(AddButton, "\uF59B", "system");
                SetButtonProps(SettingsButton, "\uF751", "system");
            }
            else if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                SetButtonProps(BackButton, "\uE72B", "Segoe MDL2 Assets");
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
