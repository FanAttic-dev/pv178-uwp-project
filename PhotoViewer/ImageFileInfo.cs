using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace PhotoViewer
{
    /// <summary>
    /// Represents the bitmap image along with its name, properties, and location.
    /// </summary>
    public class ImageFileInfo
    {
        public StorageFile ImageFile { get; }
        public BitmapImage ImageSource { get; }
        public string ImageName { get; }
        public ImageProperties ImageProperties { get; }

        public ImageFileInfo(StorageFile imageFile, BitmapImage src, string name, ImageProperties properties)
        {
            ImageFile = imageFile;
            ImageSource = src;
            ImageName = name;
            ImageProperties = properties;
        }
    }
}
