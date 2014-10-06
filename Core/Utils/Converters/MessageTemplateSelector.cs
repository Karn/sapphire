using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Core.Utils.Converters {
    public class MessageTemplateSelector : DataTemplateSelector {

        public DataTemplate AnswerTemplate { get; set; }
        public DataTemplate PostCardTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {
            //return base.SelectTemplateCore(item, container);
                var listItem = item as API.Content.Post;

                if (listItem.type == "answer")
                    return AnswerTemplate;
                else if (listItem.type == "postcard")
                    return PostCardTemplate;         

            return base.SelectTemplateCore(item, container);
        }
    }
}