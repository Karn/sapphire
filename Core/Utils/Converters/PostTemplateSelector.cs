using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Core.Utils.Converters {
    public class PostTemplateSelector : DataTemplateSelector {
        public DataTemplate TextTemplate { get; set; }
        public DataTemplate PhotoTemplate { get; set; }
        public DataTemplate GifTemplate { get; set; }
        public DataTemplate PhotoSetTemplate { get; set; }
        public DataTemplate QuoteTemplate { get; set; }
        public DataTemplate LinkTemplate { get; set; }
        public DataTemplate ChatTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnswerTemplate { get; set; }
        public DataTemplate AdvertTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            //return base.SelectTemplateCore(item, container);
                var listItem = item as API.Content.Post;
                if (listItem.type == "text")
                    return TextTemplate;
                else if (listItem.type == "photo")
                    return PhotoTemplate;
                else if (listItem.type == "gif") {
                    return GifTemplate;
                } else if (listItem.type == "photoset")
                    return PhotoSetTemplate;
                else if (listItem.type == "quote")
                    return QuoteTemplate;
                else if (listItem.type == "link")
                    return LinkTemplate;
                else if (listItem.type == "chat")
                    return ChatTemplate;
                else if (listItem.type == "audio")
                    return AudioTemplate;
                else if (listItem.type == "video")
                    return VideoTemplate;
                else if (listItem.type == "answer")
                    return AnswerTemplate;
                else if (listItem.type == "advert")
                    return AdvertTemplate;     

            return base.SelectTemplateCore(item, container);
        }
    }
}