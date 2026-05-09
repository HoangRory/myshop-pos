using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MyShop.Client.Helpers
{
    public static class DataGridFocusBehavior
    {
        public static readonly DependencyProperty PendingFocusItemProperty = DependencyProperty.RegisterAttached(
            "PendingFocusItem",
            typeof(object),
            typeof(DataGridFocusBehavior),
            new PropertyMetadata(null, OnPendingFocusItemChanged));

        public static void SetPendingFocusItem(DependencyObject element, object? value) => element.SetValue(PendingFocusItemProperty, value);
        public static object? GetPendingFocusItem(DependencyObject element) => element.GetValue(PendingFocusItemProperty);

        private static void OnPendingFocusItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid || e.NewValue == null)
            {
                return;
            }

            var item = e.NewValue;
            dataGrid.Dispatcher.BeginInvoke(new Action(() => FocusRow(dataGrid, item)), DispatcherPriority.Loaded);
        }

        private static void FocusRow(DataGrid dataGrid, object item)
        {
            if (!dataGrid.Items.Contains(item))
            {
                return;
            }

            dataGrid.UpdateLayout();
            dataGrid.ScrollIntoView(item);
            dataGrid.SelectedItem = item;

            if (dataGrid.Columns.Count > 0)
            {
                dataGrid.CurrentCell = new DataGridCellInfo(item, dataGrid.Columns[0]);
            }

            dataGrid.Focus();
            dataGrid.BeginEdit();
        }
    }
}