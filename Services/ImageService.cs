using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AIAnywhere.Services
{
    public static class ImageService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Downloads an image from a URL and returns it as a BitmapImage for WPF display
        /// </summary>
        public static async Task<BitmapImage?> DownloadImageAsync(string imageUrl)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                return ConvertBytesToBitmapImage(imageBytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads an image from a URL and returns it as raw bytes
        /// </summary>
        public static async Task<byte[]?> DownloadImageBytesAsync(string imageUrl)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(imageUrl);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts byte array to BitmapImage for WPF display
        /// </summary>
        public static BitmapImage? ConvertBytesToBitmapImage(byte[] imageBytes)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                using (var stream = new MemoryStream(imageBytes))
                {
                    bitmapImage.BeginInit();
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.StreamSource = stream;
                    bitmapImage.EndInit();
                }
                bitmapImage.Freeze(); // Make it thread-safe
                return bitmapImage;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Copies an image to the Windows clipboard
        /// </summary>
        public static void CopyImageToClipboard(BitmapImage bitmapImage)
        {
            try
            {
                Clipboard.SetImage(bitmapImage);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Copies an image from URL to the Windows clipboard
        /// </summary>
        public static async Task<bool> CopyImageFromUrlToClipboardAsync(string imageUrl)
        {
            try
            {
                var bitmapImage = await DownloadImageAsync(imageUrl);
                if (bitmapImage != null)
                {
                    CopyImageToClipboard(bitmapImage);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts BitmapImage to System.Drawing.Bitmap for advanced operations
        /// </summary>
        public static Bitmap? ConvertBitmapImageToBitmap(BitmapImage bitmapImage)
        {
            try
            {
                using (var outStream = new MemoryStream())
                {
                    var enc = new BmpBitmapEncoder();
                    enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                    enc.Save(outStream);
                    return new Bitmap(outStream);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
