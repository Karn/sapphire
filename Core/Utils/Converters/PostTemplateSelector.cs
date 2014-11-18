using APIWrapper.Content.Model;
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
        public DataTemplate MailTemplate { get; set; }
        public DataTemplate AdvertTemplate { get; set; }
        public DataTemplate NoContentTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            //return base.SelectTemplateCore(item, container);
            var listItem = item as Post;
            if (listItem != null) {
                if (listItem.type.Contains("text"))
                    return TextTemplate;
                else if (listItem.type.Contains("photo"))
                    return PhotoTemplate;
                else if (listItem.type.Contains("gif"))
                    return GifTemplate;
                else if (listItem.type.Contains("photoset"))
                    return PhotoSetTemplate;
                else if (listItem.type.Contains("quote"))
                    return QuoteTemplate;
                else if (listItem.type.Contains("link"))
                    return LinkTemplate;
                else if (listItem.type.Contains("chat"))
                    return ChatTemplate;
                else if (listItem.type.Contains("audio"))
                    return AudioTemplate;
                else if (listItem.type.Contains("video"))
                    return VideoTemplate;
                else if (listItem.type.Contains("answer"))
                    return AnswerTemplate;
                else if (listItem.type.Contains("advert"))
                    return AdvertTemplate;
                else if (listItem.type.Contains("mail"))
                    return MailTemplate;
                else if (listItem.type.Contains("nocontent"))
                    return NoContentTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}