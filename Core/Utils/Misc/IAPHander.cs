using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils {
    public class IAPHander {
        public static bool ShowAds { get; set; }
        public static void UpdateInAppPurchases() {
            ShowAds = true;
            var allLicenses = Windows.ApplicationModel.Store.
                CurrentApp.LicenseInformation.ProductLicenses;
            if (allLicenses.ContainsKey("StandardAdFree")) {
                var license = allLicenses["StandardAdFree"];
                if (license.IsActive) {
                    ShowAds = false; 
                }
            }
        }
        public static async Task RemoveAds() {
            try {
                await Windows.ApplicationModel.Store.CurrentApp
                    .RequestProductPurchaseAsync("StandardAdFree");
                UpdateInAppPurchases();
            } catch (Exception) {
                // oh well
            }
        }
    }
}
