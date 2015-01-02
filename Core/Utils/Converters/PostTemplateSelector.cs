using APIWrapper.Content.Model;
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
        public DataTemplate YoutubeTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnswerTemplate { get; set; }
        public DataTemplate MailTemplate { get; set; }
        public DataTemplate AdvertTemplate { get; set; }
        public DataTemplate NoContentTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            var listItem = item as Post;
            if (listItem != null) {
                if (listItem.type.Equals("text"))
                    return TextTemplate;
                else if (listItem.type.Equals("photoset"))
                    return PhotoSetTemplate;
                else if (listItem.type.Equals("gif"))
                    return GifTemplate;
                else if (listItem.type.Equals("photo"))
                    return PhotoTemplate;
                else if (listItem.type.Equals("quote"))
                    return QuoteTemplate;
                else if (listItem.type.Equals("link"))
                    return LinkTemplate;
                else if (listItem.type.Equals("chat"))
                    return ChatTemplate;
                else if (listItem.type.Equals("audio"))
                    return AudioTemplate;
                else if (listItem.type.Equals("youtube"))
                    return YoutubeTemplate;
                else if (listItem.type.Equals("video"))
                    return VideoTemplate;
                else if (listItem.type.Equals("answer"))
                    return AnswerTemplate;
                else if (listItem.type.Equals("advert"))
                    return AdvertTemplate;
                else if (listItem.type.Equals("mail"))
                    return MailTemplate;
                else if (listItem.type.Equals("nocontent"))
                    return NoContentTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}