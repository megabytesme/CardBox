using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Shared_Code_UWP.Services
{
    public static class DialogService
    {
        public static async Task<ContentDialogResult> ShowContentDialogAsync(
            FrameworkElement context,
            UIElement contentElement,
            string title = null,
            string primaryButtonText = null,
            string secondaryButtonText = null,
            string closeButtonText = "Close")
        {
            if (context == null)
            {
                System.Diagnostics.Debug.WriteLine("DialogService Error: context cannot be null.");
                return ContentDialogResult.None;
            }
            if (contentElement == null)
            {
                System.Diagnostics.Debug.WriteLine("DialogService Error: contentElement cannot be null.");
                return ContentDialogResult.None;
            }

            string actualPrimaryText = string.IsNullOrEmpty(primaryButtonText) ? null : primaryButtonText;
            string actualSecondaryText = string.IsNullOrEmpty(secondaryButtonText) ? null : secondaryButtonText;
            string actualCloseText = string.IsNullOrEmpty(closeButtonText) ? "Close" : closeButtonText;
            ContentDialog dialog = new ContentDialog();
            dialog.Title = title;
            dialog.Content = contentElement;
            dialog.CloseButtonText = actualCloseText;
            if (actualPrimaryText != null) { dialog.PrimaryButtonText = actualPrimaryText; }
            if (actualSecondaryText != null) { dialog.SecondaryButtonText = actualSecondaryText; }
            dialog.DefaultButton = actualPrimaryText != null ? ContentDialogButton.Primary :
                                   actualSecondaryText != null ? ContentDialogButton.Secondary :
                                   ContentDialogButton.Close;
            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                result = await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing content dialog: {ex.GetType().FullName} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            return result;
        }

        public static async Task<ContentDialogResult> ShowImageDialogAsync(
           FrameworkElement context,
           WriteableBitmap imageSource,
           int imageWidth,
           int imageHeight,
           string title = "Image",
           string description = null,
           string primaryButtonText = null,
           string secondaryButtonText = null)
        {
            if (imageSource == null || context == null || imageWidth <= 0 || imageHeight <= 0 || imageSource.PixelWidth == 0)
            {
                System.Diagnostics.Debug.WriteLine("DialogService.ShowImageDialogAsync: Invalid parameters.");
                return ContentDialogResult.None;
            }

            Image displayImage = new Image
            {
                Source = imageSource,
                Width = imageWidth,
                Height = imageHeight,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                IsTapEnabled = true,
                IsRightTapEnabled = true
            };

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            string shareTitle = title ?? "Shared Image";

            async void PrepareAndShareData(DataRequest request)
            {
                request.Data.Properties.Title = shareTitle;
                var deferral = request.GetDeferral();
                try
                {
                    var streamRef = await GetBitmapStreamReferenceAsync(imageSource);
                    if (streamRef != null)
                    {
                        request.Data.SetBitmap(streamRef);
                        if (!string.IsNullOrEmpty(description))
                        {
                            request.Data.Properties.Description = description;
                        }
                    }
                    else
                    {
                        request.FailWithDisplayText("Error preparing image");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error preparing image for sharing: {ex.Message}");
                    request.FailWithDisplayText("Error preparing image");
                }
                finally
                {
                    deferral.Complete();
                }
            }

            async Task HandleCopyAndShare()
            {
                bool copied = await CopyImageToClipboardInternalAsync(imageSource);
                if (copied)
                {
                    dataTransferManager.DataRequested += (s, args) => PrepareAndShareData(args.Request);
                    try { DataTransferManager.ShowShareUI(); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error showing share UI: {ex.Message}"); }
                    finally { dataTransferManager.DataRequested -= (s, args) => PrepareAndShareData(args.Request); }
                }
            }


            displayImage.Tapped += async (sender, e) =>
            {
                await HandleCopyAndShare();
            };

            displayImage.RightTapped += async (sender, e) =>
            {
                e.Handled = true;
                await HandleCopyAndShare();
            };


            StackPanel imageContentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            imageContentPanel.Children.Add(displayImage);

            TextBlock shareInstruction = new TextBlock
            {
                Text = "(tap or right-click to copy and share)",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (SolidColorBrush)Application.Current.Resources["SystemControlPageTextBaseMediumBrush"]
            };
            imageContentPanel.Children.Add(shareInstruction);

            if (!string.IsNullOrEmpty(description))
            {
                TextBlock descriptionBlock = new TextBlock
                {
                    Text = description,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 0)
                };
                imageContentPanel.Children.Add(descriptionBlock);
            }

            return await ShowContentDialogAsync(
                context: context,
                contentElement: imageContentPanel,
                title: title,
                primaryButtonText: primaryButtonText,
                secondaryButtonText: secondaryButtonText,
                closeButtonText: "Close"
            );
        }

        private static async Task<RandomAccessStreamReference> GetBitmapStreamReferenceAsync(WriteableBitmap bitmap)
        {
            if (bitmap == null) return null;
            try
            {
                InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                Stream pixelStream = bitmap.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, pixels);
                await encoder.FlushAsync();
                stream.Seek(0);
                return RandomAccessStreamReference.CreateFromStream(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error encoding bitmap to stream: {ex.Message}");
                return null;
            }
        }

        private static async Task<bool> CopyImageToClipboardInternalAsync(WriteableBitmap bitmapToCopy)
        {
            if (bitmapToCopy == null) return false;
            try
            {
                var streamRef = await GetBitmapStreamReferenceAsync(bitmapToCopy);
                if (streamRef == null) return false;

                DataPackage dataPackage = new DataPackage();
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetBitmap(streamRef);
                Clipboard.SetContent(dataPackage);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying image to clipboard: {ex.Message}");
                return false;
            }
        }

        public static async Task<ContentDialogResult> ShowTextDialogAsync(
            FrameworkElement context,
            string message,
            string title = null,
            string primaryButtonText = null,
            string closeButtonText = "OK")
        {
            if (context == null || string.IsNullOrEmpty(message))
            {
                return ContentDialogResult.None;
            }

            TextBlock textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = 400
            };
            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = textBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            return await ShowContentDialogAsync(
                context: context,
                contentElement: scrollViewer,
                title: title,
                primaryButtonText: primaryButtonText,
                closeButtonText: closeButtonText
            );
        }
    }
}