
namespace API.Data {
    /// <summary>
    /// This class holds the data that is stored locally
    /// </summary>
    public class DataStoreHandler {

        public static void CreateSettings() {
            
        }

        /// <summary>
        /// Load and initialize the settings that are stored locally
        /// </summary>
        public static void LoadSettings() {

        }

        /// <summary>
        /// Save settings before exit
        /// </summary>
        public static void SaveAllSettings() {

        }

        public static void ClearSettings() {
            Config.OAuthVerifier = "";
            Config.OAuthToken = "";
            Config.OAuthTokenSecret = "";
            Config.AuthRequestToken = "";
            Config.AuthRequestTokenSecret = "";
        }
    }
}
