using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Shared_Code;
using Windows.UI.Xaml;
using System;

namespace _1809_UWP
{
    public sealed partial class EditCardPage : Page
    {
        public EditCardPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Card selectedCard)
            {
                this.DataContext = selectedCard;
            }
        }

        private void SaveCard_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Card selectedCard)
            {
                if (displayPicker.SelectedItem is ComboBoxItem selectedDisplayTypeItem &&
                    Enum.TryParse(selectedDisplayTypeItem.Content.ToString(), out DisplayType selectedDisplayType))
                {
                    selectedCard.DisplayType = selectedDisplayType;
                }
                CardRepository.Instance.EditCard(selectedCard);
                Frame.GoBack();
            }
        }
    }
}
