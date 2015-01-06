using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class ToPrettyDate : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            return DateTime.Parse(value.ToString()).ToLocalTime().Date.ToString("dddd, MMMM dd");
        }

        static string GetPrettyDate(DateTime d) {
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.Subtract(d);
            
            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;
            Debug.WriteLine(dayDiff);
            // 4.
            // Don't allow out of range values.
            if (dayDiff < 0 || dayDiff >= 31) {
                return null;
            }

            // 5.
            // Handle same-day times.
            if (dayDiff == 0 || dayDiff == -1) {
                return "today";
            }
            // 6.
            // Handle previous days.
            if (dayDiff == 1) {
                return "yesterday";
            }
            if (dayDiff < 7) {
                return string.Format("{0} days ago",
                dayDiff);
            }
            if (dayDiff < 31) {
                return string.Format("{0} weeks ago",
                Math.Ceiling((double)dayDiff / 7));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
