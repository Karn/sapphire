using API.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Content
{
    public class CreatePost
    {

        public static async void Text(string title, string body, string tags)
        {

            var parameterString = "type=text&body=" + body;
            if (!string.IsNullOrEmpty(title))
                parameterString += "&title=" + title;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;

            await RequestHandler.CreatePost(parameterString);
        }

        public static async void Photo(string caption, string source, object data)
        {

            var parameterString = "type=photo&";
            if (!string.IsNullOrEmpty(caption))
                parameterString += "&caption=" + source;
            if (!string.IsNullOrEmpty(source))
                parameterString += "&source=" + source;
            else if (data != null)
                parameterString += "&data=" + data.ToString();

            await RequestHandler.CreatePost(parameterString);
        }

        public static async void Quote(string quote, string source)
        {

            var parameterString = "type=quote&quote=" + quote;
            if (!string.IsNullOrEmpty(source))
                parameterString += "&source=" + source;

            await RequestHandler.CreatePost(parameterString);
        }

        public static async void Link(string title, string url, string description)
        {

            var parameterString = "type=link&url=" + url;
            if (!string.IsNullOrEmpty(title))
                parameterString += "&title=" + title;
            if (!string.IsNullOrEmpty(description))
                parameterString += "&description=" + description;

            await RequestHandler.CreatePost(parameterString);
        }


        public class Chat
        {
            string title { get; set; }
            string converstation { get; set; }
            string parameterString
            {
                get
                {
                    var x = "type=chat&conversation=" + converstation;
                    if (title != null && title != string.Empty)
                        x += "&title=" + title;
                    return x;
                }
            }
        }

        public class Audio
        {

        }

        public class Video
        {

        }
    }
}
