using MaterialColorUtilities.Maui;

namespace CardBox
{
    public partial class App : Application
    {
        public App()
        {
            IMaterialColorService.Current.Initialize(this.Resources);
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
