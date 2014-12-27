using APIWrapper.Client;
using System;
using System.Diagnostics;
using Windows.Storage;

namespace APIWrapper.Content.Model {
    public class CreatePost {

        public static async void Text(string title, string body, string tags, string additionalParameters = "") {
            try {
                var parameterString = "type=text&body=" + body;
                if (!string.IsNullOrEmpty(title))
                    parameterString += "&title=" + title;
                if (!string.IsNullOrEmpty(tags))
                    parameterString += "&tags=" + tags;
                if (!string.IsNullOrEmpty(additionalParameters))
                    parameterString += additionalParameters;

                await CreateRequest.CreatePost(parameterString);
            } catch (Exception e) {

            }
        }

        public static async void Photo(StorageFile media) {

            //var parameterString = "type=photo";
            //if (!string.IsNullOrEmpty(caption))
            //    parameterString += "&caption=" + caption;
            //if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(data))
            //    parameterString += "&source=" + source;
            //if (!string.IsNullOrEmpty(tags))
            //    parameterString += "&tags=" + tags;
            //if (!string.IsNullOrEmpty(additionalParameters))
            //    parameterString += additionalParameters;

            //string result = await RequestBuilder.PostWithMediaAPI("http://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/post", media);
            //Debug.WriteLine(result);

        }

        public static async void Quote(string quote, string source, string tags, string additionalParameters = "") {

            var parameterString = "type=quote&quote=" + quote;
            if (!string.IsNullOrEmpty(source))
                parameterString += "&source=" + source;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;
            if (!string.IsNullOrEmpty(additionalParameters))
                parameterString += additionalParameters;

            await CreateRequest.CreatePost(parameterString);
        }

        public static async void Link(string title, string url, string description, string tags, string additionalParameters = "") {

            var parameterString = "type=link&url=" + url;
            if (!string.IsNullOrEmpty(title))
                parameterString += "&title=" + title;
            if (!string.IsNullOrEmpty(description))
                parameterString += "&description=" + description;
            if (!string.IsNullOrEmpty(tags))
                parameterString += "&tags=" + tags;
            if (!string.IsNullOrEmpty(additionalParameters))
                parameterString += additionalParameters;

            await CreateRequest.CreatePost(parameterString);
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
