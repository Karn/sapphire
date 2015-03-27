using System;
using System.Collections.Generic;
using System.Text;

namespace Sapphire.Utils {
	public class LocalizedStrings {
		public string this[string key] {
			get {
				return App.LocaleResources.GetString(key);
			}
		}
	}
}
