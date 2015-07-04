using System.IO;
using Windows.Storage;

namespace Core.Content.Model.DatabaseHelpers {
    public class DatabaseConstants {

        public const string DB_NAME = "ApplicationStorage.sqlite";
        public static string DB_PATH = Path.Combine(Path.Combine(ApplicationData.Current.LocalFolder.Path, DB_NAME));

    }
}
