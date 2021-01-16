using System;
using System.Collections.Generic;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PhotoViewer
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<ImageFileInfo> FolderImages { get; } = new ObservableCollection<ImageFileInfo>();
        public ObservableCollection<ImageFileInfo> BingImages { get; } = new ObservableCollection<ImageFileInfo>();

        public event PropertyChangedEventHandler PropertyChanged;

        private const string BingXmlUrl = "https://www.bing.com/HPImageArchive.aspx?format=xml&idx=0&n=8&mkt=en-US";
        private bool _switchToMain = true;
        private readonly StorageFolder _installedLocation = Package.Current.InstalledLocation;

        private double _imgWidth;
        public double ImgWidth
        {
            get => _imgWidth;
            set
            {
                if (_imgWidth != value)
                {
                    _imgWidth = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImgWidth)));
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Load sample files from Assets
            if (FolderImages.Count == 0)
            {
                StorageFolder assetsFolder = await _installedLocation.GetFolderAsync(@"Assets\Images");
                await GetPhotosFromFolderAsync(assetsFolder, FolderImages);
            }

            // Load Bing images
            if (BingImages.Count == 0)
            {
                // Create a temporary folder for Bing images
                StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(ApplicationData.Current.LocalFolder.Path.ToString());
                StorageFolder bingFolder = await localFolder.
                    CreateFolderAsync("BingImages", CreationCollisionOption.ReplaceExisting);

                await DownloadBingImagesAsync(bingFolder);
                await GetPhotosFromFolderAsync(bingFolder, BingImages);
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Loads all JPG photos from a folder into a collection of ImageFileInfos
        /// </summary>
        /// <param name="folder">The folder to load the photos from</param>
        /// <param name="folderImages">A collection of ImageFileInfos</param>
        /// <returns></returns>
        private static async Task GetPhotosFromFolderAsync(IStorageFolder folder, ICollection<ImageFileInfo> folderImages)
        {
            folderImages.Clear();

            IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
            foreach (StorageFile file in fileList)
            {
                // Limit to only jpg files.
                if (file.ContentType == "image/jpeg")
                {
                    folderImages.Add(await LoadImageInfo(file));
                }
            }
        }

        /// <summary>
        /// Loads an image from StorageFile to ImageFileInfo
        /// </summary>
        /// <param name="file">StorageFile file</param>
        /// <returns>ImageFileInfo file</returns>
        public static async Task<ImageFileInfo> LoadImageInfo(StorageFile file)
        {
            // Open a stream for the selected file.
            using (var fileStream = await file.OpenReadAsync())
            {
                // Create a bitmap to be the image source.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);

                var properties = await file.Properties.GetImagePropertiesAsync();
                ImageFileInfo info = new ImageFileInfo(file, bitmapImage, file.DisplayName, properties);

                return info;
            }
        }

        /// <summary>
        /// Downloads Bing images into a folder
        /// </summary>
        /// <param name="folder">The folder to download the images to</param>
        /// <returns></returns>
        private async Task DownloadBingImagesAsync(StorageFolder folder)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(await GetStringFromUrlAsync(BingXmlUrl));

            foreach (XmlNode image in doc.DocumentElement.ChildNodes)
            {
                if (!image.Name.Equals("image")) continue;

                string imageUrl = "https://www.bing.com/" + image["url"].InnerText;
                string imageName = image["startdate"].InnerText + ".jpg";

                var rass = RandomAccessStreamReference.CreateFromUri(new Uri(imageUrl));
                using (var inputStream = await rass.OpenReadAsync())
                {
                    var readStream = inputStream.AsStreamForRead();
                    byte[] buffer = new byte[readStream.Length];
                    await readStream.ReadAsync(buffer, 0, buffer.Length);

                    await FileIO.WriteBytesAsync(await folder.CreateFileAsync(imageName, CreationCollisionOption.ReplaceExisting), buffer);
                }
            }
        }

        private static async Task<string> GetStringFromUrlAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Method to change the viewed images folder by a FolderPicker.
        /// </summary>
        private async void ChangeFolder()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.ViewMode = PickerViewMode.Thumbnail;
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                await GetPhotosFromFolderAsync(folder, FolderImages);
            }
            else
            {
                // Picker cancelled
            }
        }

        /// <summary>
        /// Stretch the Bing images so that they fill the window
        /// in different window sizes.
        /// </summary>
        private void DetermineBingImageSize()
        {
            if (mainPage.ActualWidth < 700)
            {
                ImgWidth = mainPage.ActualWidth - 10;
            }
            else
            {
                ImgWidth = mainPage.ActualWidth / 2.2;
            }
        }

        /// <summary>
        /// When an item is clicked, show a fullscreen preview by navigating to the DetailPage.
        /// </summary>
        private void MyPhotosGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(DetailPage), e.ClickedItem);
        }

        /// <summary>
        /// Toggle the command bar visibility when switching between pivot items.
        /// </summary>
        private void ToggleCmdBarVisibility()
        {
            if (_switchToMain)
            {
                MainCommandBar.Visibility = Visibility.Visible;
                _switchToMain = false;
            }
            else
            {
                MainCommandBar.Visibility = Visibility.Collapsed;
                DetermineBingImageSize();
                _switchToMain = true;
            }
        }
    }
}
