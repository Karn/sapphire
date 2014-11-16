using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Core.Utils.Misc {
    public class VariableGrid : GridView {
        protected override void PrepareContainerForItemOverride
            (Windows.UI.Xaml.DependencyObject element, object item) {

            //var tile = item as API.Content.Post;
            //if (tile != null) {
            //    var griditem = element as GridViewItem;
            //    if (griditem != null) {
            //        VariableSizedWrapGrid.SetColumnSpan(griditem, tile.ColumnSpan);
            //        VariableSizedWrapGrid.SetRowSpan(griditem, tile.RowSpan);
            //    }
            //}
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}