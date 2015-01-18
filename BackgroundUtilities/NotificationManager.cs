using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundUtilities {
    internal class NotificationManager {

        private string lastCheckTimestamp { get; set; }

        public NotificationManager() {

        }

        public async Task<bool> GetPendingNotifications() {
            return false;
        }
    }
}
