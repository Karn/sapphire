using API.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content {
    public class CreatePost {

        public static async void Text(string title, string body, string tags) {
            try {
                var parameterString = "type=text&body=" + body;
                if (!string.IsNullOrEmpty(title))
                    parameterString += "&title=" + title;
                if (!string.IsNullOrEmpty(tags))
                    parameterString += "&tags=" + tags;

                await RequestHandler.CreatePost(parameterString);
            } catch (Exception e) {

            }
        }

        public static async void Photo(string caption, string source, string data, string tags) {

            var parameterString = "type=photo";
            if (!string.IsNullOrEmpty(caption))
                parameterString += "&caption=" + caption;
            if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(data))
                parameterString += "&source=" + source;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;

            await RequestHandler.CreateMediaPost(parameterString, data);
        }

        public static async void Quote(string quote, string source, string tags) {

            var parameterString = "type=quote&quote=" + quote;
            if (!string.IsNullOrEmpty(source))
                parameterString += "&source=" + source;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;

            await RequestHandler.CreatePost(parameterString);
        }

        public static async void Link(string title, string url, string description, string tags) {

            var parameterString = "type=link&url=" + url;
            if (!string.IsNullOrEmpty(title))
                parameterString += "&title=" + title;
            if (!string.IsNullOrEmpty(description))
                parameterString += "&description=" + description;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;

            await RequestHandler.CreatePost(parameterString);
        }


        public class Chat {
            string title { get; set; }
            string converstation { get; set; }
            string parameterString {
                get {
                    var x = "type=chat&conversation=" + converstation;
                    if (title != null && title != string.Empty)
                        x += "&title=" + title;
                    return x;
                }
            }
        }

        public class Audio {

        }

        public class Video {

        }
    }
}
