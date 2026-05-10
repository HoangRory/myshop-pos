using MyShop.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MyShop.Client.Controls
{
    public class AsyncImage : Image
    {
        public static readonly System.Windows.DependencyProperty ImagesProperty =
            System.Windows.DependencyProperty.Register(
                nameof(Images),
                typeof(string),
                typeof(AsyncImage),
                new System.Windows.PropertyMetadata(string.Empty, OnImagesChanged));

        public string Images
        {
            get => (string)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }

        private static void OnImagesChanged(System.Windows.DependencyObject d, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as AsyncImage;
            var imagesString = e.NewValue as string;
            ctrl?.SetFirstImageAsync(imagesString);
        }

        private void SetFirstImageAsync(string imagesString)
        {
            var first = GetFirstImageName(imagesString);
            if (string.IsNullOrWhiteSpace(first))
            {
                this.Source = null;
                return;
            }

            _ = LoadAndSetAsync(first);
        }

        private string? GetFirstImageName(string? imagesString)
        {
            if (string.IsNullOrWhiteSpace(imagesString)) return null;
            var parts = imagesString.Split(';');
            foreach (var p in parts)
            {
                var t = p?.Trim();
                if (!string.IsNullOrWhiteSpace(t)) return t;
            }
            return null;
        }

        private async Task LoadAndSetAsync(string fileName)
        {
            try
            {
                // Resolve IImageService from DI container
                var scope = MyShop.Client.DIContainer.ServiceProvider.CreateScope();
                var imageService = scope.ServiceProvider.GetService<IImageService>();
                if (imageService == null) return;

                var bytes = await imageService.DownloadImageAsync(fileName);
                if (bytes == null || bytes.Length == 0) return;

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        using var ms = new MemoryStream(bytes);
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();
                        bmp.Freeze();
                        this.Source = bmp;
                    }
                    catch
                    {
                        // ignore image decode errors
                    }
                });
            }
            catch
            {
                // swallow errors to avoid throwing on UI
            }
        }
    }
}
