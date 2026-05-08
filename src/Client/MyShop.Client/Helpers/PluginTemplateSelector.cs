using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyShop.Client.Helpers
{
    public class PluginTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null) return null;

            var viewModelType = item.GetType();

            // Lookup View tương ứng
            if (DIContainer.ViewMapping.TryGetValue(viewModelType, out var viewType))
            {
                var factory = new FrameworkElementFactory(viewType);

                // QUAN TRỌNG
                factory.SetBinding(
                    FrameworkElement.DataContextProperty,
                    new Binding());

                return new DataTemplate
                {
                    DataType = viewModelType,
                    VisualTree = factory
                };
            }

            return base.SelectTemplate(item, container);
        }
    }
}
