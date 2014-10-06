using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Core.Utils.Converters {
    public class ActivityTemplateSelector : DataTemplateSelector {

        public DataTemplate ReblogTemplate { get; set; }
        public DataTemplate LikeTemplate { get; set; }
        public DataTemplate FollowTemplate { get; set; }
        public DataTemplate UserMentionTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) {

            var listItem = item as API.Content.Activity.Notification;
            if (listItem.type == "reblog")
                return ReblogTemplate;
            else if (listItem.type == "like")
                return LikeTemplate;
            else if (listItem.type == "follower")
                return FollowTemplate;
            else if (listItem.type == "user_mention")
                return UserMentionTemplate;

            return base.SelectTemplateCore(item, container);
        }
    }
}