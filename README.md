# CardBox
ADD IMAGE


CardBox is an app which is designed to effortlessly present your loyalty cards on all your devices.

## Features

- **Cross-platform support:** Enjoy access to your loyalty cards on whichever device or OS you use!

## Download
Coming soon!

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
2. Drag and drop a file into the application to upload.

### Usage

1. **Add a card:** Press the ➕ icon to add a card.
2. **Fill in the details:** Fill in all the applicable card details.
3. **View your cards:** View all the cards in the main window.

## Folder Structure

- `1709 UWP`: UWP app implementation which supports devices on Windows 1709 and above (looking at you W10M) - Uses UWP WinUI. Recommended for Windows 10 Mobile users.
- `MAUI`: MAUI app implementation which supports devices on Windows 1809 and above (Buggy due to Xzing.Net.MAUI library on Windows), Android, iOS and Mac - Recommended for non-Windows platform users.
- `Shared Code`: Project which holds the code shared by all projects.

## Contributing

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a new Pull Request.

## License

This project is licensed under the CC BY-NC-SA 4.0 License - see the [LICENSE](LICENSE.md) file for details.


