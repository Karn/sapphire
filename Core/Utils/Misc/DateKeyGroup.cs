using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Utils {
    public class DateKeyGroup<T> : List<T> {
        public string Key { get; private set; }

        public DateKeyGroup(string key) {
            Key = key;
        }
    }
}
