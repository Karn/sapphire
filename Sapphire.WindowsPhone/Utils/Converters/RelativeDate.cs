﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Sapphire.Utils.Converters {
    public sealed class RelativeDate : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            try {
                var x = GetPrettyDate(DateTime.Parse(value.ToString())).ToUpper();
                return x;
            } catch (Exception e) {
                Debug.WriteLine(e.StackTrace);
                return "";
            }
        }

        static string GetPrettyDate(DateTime d) {
            // 1.
            // Get time span elapsed since the date.
            TimeSpan s = DateTime.Now.ToLocalTime().Subtract(d);

            // 2.
            // Get total number of days elapsed.
            int dayDiff = (int)s.TotalDays;

            // 5.
            // Handle same-day times.
            if (dayDiff == 0 || dayDiff == -1) {
                return App.LocaleResources.GetString("Activity_TodayText");
            } else if (dayDiff == 1) {
                return App.LocaleResources.GetString("Activity_YesterdayText");
            } else
                return dayDiff + " " + App.LocaleResources.GetString("Activity_DaysAgoText");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
