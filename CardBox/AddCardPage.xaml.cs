﻿using Shared_Code;

namespace CardBox
{
    public partial class AddCardPage : ContentPage
    {
        public AddCardPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public Command AddCard => new Command(OnAddCard);

        private async void OnAddCard()
        {
            string cardName = ((Entry)FindByName("cardNameEntry")).Text;
            string cardKnickname = ((Entry)FindByName("cardKnicknameEntry")).Text;
            string cardNumberText = ((Entry)FindByName("cardNumberEntry")).Text;
            Picker displayPicker = (Picker)FindByName("picker");

            if (string.IsNullOrWhiteSpace(cardName) || string.IsNullOrWhiteSpace(cardNumberText) || displayPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please fill all required fields.", "OK");
                return;
            }

            if (!int.TryParse(cardNumberText, out int cardNumber))
            {
                await DisplayAlert("Error", "Card number must be a valid integer.", "OK");
                return;
            }

            if (!Enum.TryParse(displayPicker.SelectedItem.ToString(), out Shared_Code.DisplayType selectedDisplayType))
            {
                await DisplayAlert("Error", "Invalid display type selected.", "OK");
                return;
            }

            var newCard = new Card
            {
                CardName = cardName,
                CardKnickname = cardKnickname,
                CardNumber = cardNumber,
                DisplayType = selectedDisplayType
            };

            CardRepository.Instance.AddCard(newCard);

            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
