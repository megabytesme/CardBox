# CardBox
![Screenshot 2025-02-24 211528](https://github.com/user-attachments/assets/7bb321d0-d03e-4fe8-9f25-5a44fe72acf2)

CardBox is an opensource cross-platform app which is designed to effortlessly present your loyalty cards on all your devices.

## Features

- **Cross-platform support:** Enjoy access to your loyalty cards on whichever device or OS you use!

## Download
<a href="https://apps.microsoft.com/detail/9NRC7F9MD2JW"><img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>

## Build Guide

### Prerequisites

- Windows 10 or later
- Visual Studio 2019 or later

### Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/megabytesme/CardBox.git
    ```
2. Open the project in Visual Studio.
3. Build the solution.

### Running the Application

1. Start the application by pressing `F5` or by selecting `Debug > Start Debugging`.

### Usage

1. **Add a card:** Press the ➕ icon to add a card.
2. **Fill in the details:** Fill in all the applicable card details.
3. **View your cards:** View all the cards in the main window.

## Folder Structure

- `1703 UWP`: UWP app implementation which supports devices on Windows 1703 and above (looking at you W10M) - Uses UWP WinUI. Recommended for Windows 10 Mobile users.
- `1809 UWP`: UWP app implementation which supports devices on Windows 1809 and above (Includes 10X!) - Uses WinUI 2. Recommended for all Windows device users.
- `MAUI`: MAUI app implementation which supports devices on Windows 1809 and above (Buggy due to Xzing.Net.MAUI library on Windows), Android, iOS and Mac - Recommended for non-Windows platform users. Currently using Material You design principles.
- `Shared Code`: Project which holds the code shared by all projects.
- `App Assets`: Folder which holds the assets used by all projects when publishing to App Stores.

## Contributing

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

## License

This project is licensed under the CC BY-NC-SA 4.0 License - see the [LICENSE](LICENSE.md) file for details.


