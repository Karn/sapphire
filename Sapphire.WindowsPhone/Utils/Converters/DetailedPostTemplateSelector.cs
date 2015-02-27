using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Sapphire.Utils.Converters
{
    public class DetailedPostTemplateSelector : DataTemplateSelector
    {

        public DataTemplate TextTemplate { get; set; }
        public DataTemplate PhotoTemplate { get; set; }
        public DataTemplate QuoteTemplate { get; set; }
        public DataTemplate LinkTemplate { get; set; }
        public DataTemplate ChatTemplate { get; set; }
        public DataTemplate AudioTemplate { get; set; }
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnswerTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            //return base.SelectTemplateCore(item, container);
            var listItem = item as string;

            Debug.WriteLine(listItem);

            if (listItem == "text")
                return TextTemplate;
            else if (listItem == "photo")
                return PhotoTemplate;
            else if (listItem == "quote")
                return QuoteTemplate;
            else if (listItem == "link")
                return LinkTemplate;
            else if (listItem == "chat")
                return ChatTemplate;
            else if (listItem == "audio")
                return AudioTemplate;
            else if (listItem == "video")
                return VideoTemplate;
            else if (listItem == "answer")
                return AnswerTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}