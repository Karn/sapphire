using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content {
    public class User {
        public string name { get; set; }
        public int likes { get; set; }
        public int following { get; set; }
        public bool push_notifications { get; set; }
        public string default_post_format { get; set; }
        public List<Blog> blogs { get; set; }
    }
}
