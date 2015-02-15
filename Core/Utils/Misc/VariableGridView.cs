using APIWrapper.Content.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Core.Utils.Misc {
	public class VariableGridView : GridView {
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
			var model = (Photo)item;
			try {
				Debug.WriteLine(model.ColSpan);
				element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, model.ColSpan);
				element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
			} catch {
				element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 6);
				element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
			} finally {
				base.PrepareContainerForItemOverride(element, item);
			}
		}

		// refresh the variablesizedwrapgrid layout
		public void Update() {
			if (!(this.ItemsPanelRoot is VariableSizedWrapGrid))
				throw new ArgumentException("ItemsPanel is not VariableSizedWrapGrid");

			foreach (var container in this.ItemsPanelRoot.Children.Cast<GridViewItem>()) {
				dynamic data = container.Content;
				VariableSizedWrapGrid.SetRowSpan(container, data.RowSpan);
				VariableSizedWrapGrid.SetColumnSpan(container, data.ColSpan);
			}

			this.ItemsPanelRoot.InvalidateMeasure();
		}
	}

}
