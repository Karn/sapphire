using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils {
	public class TagFormatter {

		public static string Format(string s) {
			var tags = s.Split(',');
			var converted = "";
			foreach (var tag in tags) {
				converted += string.Format("#{0}, ", tag.Trim('#', ',', ' '));
			}
            return converted.TrimEnd(' ');
		}
	}
}
