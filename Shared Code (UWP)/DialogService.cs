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
using Windows.Foundation;
using Windows.UI.Xaml.Documents;

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
            dialog.RequestedTheme = context.RequestedTheme;
            dialog.Title = title;
            dialog.Content = contentElement;
            dialog.CloseButtonText = actualCloseText;
            if (actualPrimaryText != null) { dialog.PrimaryButtonText = actualPrimaryText; }
            if (actualSecondaryText != null) { dialog.SecondaryButtonText = actualSecondaryText; }
            if (actualPrimaryText != null) dialog.DefaultButton = ContentDialogButton.Primary;
            else if (actualSecondaryText != null) dialog.DefaultButton = ContentDialogButton.Secondary;
            else dialog.DefaultButton = ContentDialogButton.Close;
            ContentDialogResult result = ContentDialogResult.None;
            try
            {
                if (!context.Dispatcher.HasThreadAccess)
                {
                    await context.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        result = await dialog.ShowAsync();
                    });
                }
                else
                {
                    result = await dialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing content dialog: {ex.GetType().FullName} - {ex.Message}");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                try
                {
                    var errorDialog = new ContentDialog()
                    {
                        Title = "Dialog Error",
                        Content = $"Could not display the original dialog: {ex.Message}",
                        CloseButtonText = "OK",
                        RequestedTheme = context.RequestedTheme
                    };
                    if (!context.Dispatcher.HasThreadAccess)
                    {
                        await context.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () => await errorDialog.ShowAsync());
                    }
                    else
                    {
                        await errorDialog.ShowAsync();
                    }
                }
                catch (Exception metaEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Error showing meta-error dialog: {metaEx.GetType().FullName} - {metaEx.Message}");
                }
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
            if (imageSource == null || context == null || imageWidth <= 0 || imageHeight <= 0 || imageSource.PixelWidth <= 0 || imageSource.PixelHeight <= 0)
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
                VerticalAlignment = VerticalAlignment.Center,
                IsTapEnabled = true,
            };
            ToolTipService.SetToolTip(displayImage, "Tap to copy/share image");
            string shareTitle = title ?? "Shared Image";
            async Task HandleTapAndShare()
            {
                bool copied = await CopyImageToClipboardInternalAsync(imageSource);
                if (copied)
                {
                    DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

                    TypedEventHandler<DataTransferManager, DataRequestedEventArgs> dataRequestedHandler = null;
                    dataRequestedHandler = async (sender, args) =>
                    {
                        dataTransferManager.DataRequested -= dataRequestedHandler;

                        var request = args.Request;
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
                            else { request.FailWithDisplayText("Error preparing image"); }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error preparing image for sharing: {ex.Message}");
                            request.FailWithDisplayText("Error preparing image");
                        }
                        finally { deferral.Complete(); }
                    };

                    dataTransferManager.DataRequested += dataRequestedHandler;

                    try
                    {
                        if (!context.Dispatcher.HasThreadAccess)
                        {
                            await context.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => DataTransferManager.ShowShareUI());
                        }
                        else
                        {
                            DataTransferManager.ShowShareUI();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error showing share UI: {ex.Message}");
                        await ShowErrorDialogAsync(context, $"Could not show sharing options: {ex.Message}", "Share Error");
                        dataTransferManager.DataRequested -= dataRequestedHandler;
                    }
                }
                else
                {
                    await ShowTextDialogAsync(context, "Could not copy image to clipboard.", "Copy Failed");
                }
            }

            displayImage.Tapped += async (sender, e) => { await HandleTapAndShare(); };


            StackPanel imageContentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };

            imageContentPanel.Children.Add(displayImage);


            if (!string.IsNullOrEmpty(description))
            {
                TextBlock descriptionBlock = new TextBlock
                {
                    Text = description,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 8),
                    MaxWidth = imageWidth
                };
                imageContentPanel.Children.Add(descriptionBlock);
            }

            TextBlock shareInstruction = new TextBlock
            {
                Text = "(Tap image to copy/share)",
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
            };
            try
            {
                if (Application.Current.Resources.TryGetValue("SystemControlPageTextBaseMediumBrush", out object brush))
                {
                    shareInstruction.Foreground = brush as Brush;
                }
                else
                {
                    shareInstruction.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
                }
            }
            catch
            {
                shareInstruction.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);
            }
            imageContentPanel.Children.Add(shareInstruction);

            return await ShowContentDialogAsync(
                context: context,
                contentElement: imageContentPanel,
                title: title,
                primaryButtonText: primaryButtonText,
                secondaryButtonText: secondaryButtonText,
                closeButtonText: "Close"
            );
        }

        public static async Task ShowErrorDialogAsync(FrameworkElement context, string message, string title = "Error")
        {
            await ShowTextDialogAsync(context, message, title, closeButtonText: "OK");
        }

        public static async Task ShowWarningDialogAsync(FrameworkElement context, string message, string title = "Warning")
        {
            await ShowTextDialogAsync(context, message, title, closeButtonText: "OK");
        }

        public static async Task ShowSuccessDialogAsync(FrameworkElement context, string message, string title = "Success")
        {
            await ShowTextDialogAsync(context, message, title, closeButtonText: "OK");
        }


        public static async Task<ContentDialogResult> ShowTextDialogAsync(
            FrameworkElement context,
            string message,
            string title = null,
            string primaryButtonText = null,
            string secondaryButtonText = null,
            string closeButtonText = "OK")
        {
            if (context == null || string.IsNullOrEmpty(message))
            {
                System.Diagnostics.Debug.WriteLine("DialogService.ShowTextDialogAsync: Invalid parameters (null context or empty message).");
                return ContentDialogResult.None;
            }

            var richTextBlock = new RichTextBlock { IsTextSelectionEnabled = true };
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run { Text = message });
            richTextBlock.Blocks.Add(paragraph);

            ScrollViewer scrollViewer = new ScrollViewer
            {
                Content = richTextBlock,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                MaxHeight = 400,
                MaxWidth = 400
            };

            return await ShowContentDialogAsync(
                context: context,
                contentElement: scrollViewer,
                title: title,
                primaryButtonText: primaryButtonText,
                secondaryButtonText: secondaryButtonText,
                closeButtonText: closeButtonText
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

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    96.0,
                    96.0,
                    pixels);

                await encoder.FlushAsync();
                stream.Seek(0);
                return RandomAccessStreamReference.CreateFromStream(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error encoding bitmap to stream: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private static async Task<bool> CopyImageToClipboardInternalAsync(WriteableBitmap bitmapToCopy)
        {
            if (bitmapToCopy == null) return false;
            DataPackage dataPackage = new DataPackage();
            try
            {
                var streamRef = await GetBitmapStreamReferenceAsync(bitmapToCopy);
                if (streamRef == null) return false;

                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                dataPackage.SetBitmap(streamRef);

                Clipboard.SetContent(dataPackage);
                Clipboard.Flush();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying image to clipboard: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }
    }
}