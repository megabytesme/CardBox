using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System;
using muxc = Microsoft.UI.Xaml.Controls;

namespace _1809_UWP
{
    public static class MaterialHelper
    {
        public static void ApplySystemBackdropOrAcrylic(Page targetPage)
        {
            bool appliedMaterial = false;
            if (ApiInformation.IsTypePresent("Microsoft.UI.Xaml.Controls.BackdropMaterial") &&
                ApiInformation.IsMethodPresent("Microsoft.UI.Xaml.Controls.BackdropMaterial", "SetApplyToRootOrPageBackground") &&
                ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                try
                {
                    muxc.BackdropMaterial.SetApplyToRootOrPageBackground(targetPage, true);
                    appliedMaterial = true;
                    System.Diagnostics.Debug.WriteLine("Applied System Backdrop via muxc.BackdropMaterial.");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to apply muxc.BackdropMaterial: {ex.Message}. Falling back.");
                }
            }

            if (!appliedMaterial)
            {
                if (ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.AcrylicBrush"))
                {
                    var acrylicBrush = new AcrylicBrush
                    {
                        BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                        TintColor = Colors.Transparent,
                        TintOpacity = 0.6,
                        FallbackColor = Colors.Gray
                    };
                    targetPage.Background = acrylicBrush;
                    System.Diagnostics.Debug.WriteLine("Applied UWP AcrylicBrush.");
                }
                else
                {
                    targetPage.Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];
                    System.Diagnostics.Debug.WriteLine("Applied Theme Background Brush.");
                }
            }
        }
    }
}