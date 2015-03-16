using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Core.Utils {
    public class AppLicenseHandler {

        private static bool TestMode = false;
        private static bool _IsTrail = true;
        public static bool IsTrial {
            get {
#if DEBUG
                if (Debugger.IsAttached)
                    return false;
#endif
                return _IsTrail;
            }
            set {
                _IsTrail = value;
            }
        }
        private static bool FromIAP = false;

        public static LicenseInformation LicenseInformation { get; private set; }

        public AppLicenseHandler() {
            if (TestMode)
                LicenseInformation = CurrentAppSimulator.LicenseInformation;
            else
                LicenseInformation = CurrentApp.LicenseInformation;

            LicenseInformation.LicenseChanged += LicenseInformation_LicenseChanged;

            LicenseInformation_LicenseChanged();
        }

        private static void LicenseInformation_LicenseChanged() {
            UpdateInAppPurchases();

            Debug.WriteLine("From IAP: {0}", FromIAP);
            if (FromIAP)
                IsTrial = false;
        }

        public static void UpdateInAppPurchases() {
            if (LicenseInformation.ProductLicenses.ContainsKey("StandardAdFree")) {
                if (LicenseInformation.ProductLicenses["StandardAdFree"].IsActive)
                    FromIAP = true;
            }
        }
        public static async Task RemoveAds() {
            try {
                //await CurrentApp.RequestAppPurchaseAsync(true);
                await CurrentApp.RequestProductPurchaseAsync("StandardAdFree");
                LicenseInformation_LicenseChanged();
            } catch (Exception) {
                // oh well
            }
        }
    }
}
