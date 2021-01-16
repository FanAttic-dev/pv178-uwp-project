using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.System.UserProfile;

namespace PhotoViewer
{
    public sealed partial class DetailPage : Page
    {
        private ImageFileInfo _item;

        public DetailPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _item = e.Parameter as ImageFileInfo;

            if (_item != null)
            {
                targetImage.Source = _item.ImageSource;
            }

            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Sets the image in _item as wallpaper.
        /// 
        /// Due to access permissions, the image is first copied into the local folder
        /// and this copy serves as the wallpaper image source.
        /// </summary>
        private async Task SetAsWallpaperAsync()
        {
            if (!UserProfilePersonalizationSettings.IsSupported()) return;

            StorageFile wallpaper = await CopyPhotoToLocalAsync();
            await UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(wallpaper);
        }

        /// <summary>
        /// Sets the image in _item as lock screen background.
        ///
        /// Due to access permissions, the image is first copied into the local folder
        /// and this copy serves as the wallpaper image source.
        /// </summary>
        private async Task SetAsLockScreenAsync()
        {
            if (!UserProfilePersonalizationSettings.IsSupported()) return;

            StorageFile wallpaper = await CopyPhotoToLocalAsync();
            await UserProfilePersonalizationSettings.Current.TrySetLockScreenImageAsync(wallpaper);
        }


        /// <summary>
        /// Copies the previewed photo into the local folder to be able to work with it.
        /// </summary>
        /// <returns>The copied photo</returns>
        private async Task<StorageFile> CopyPhotoToLocalAsync()
        {
            var folder = ApplicationData.Current.LocalFolder;
            return await _item.ImageFile.CopyAsync(
                folder,
                "wallpaper" + Path.GetExtension(_item.ImageFile.Path.ToString()),
                NameCollisionOption.ReplaceExisting);
        }

        private void BackButton_OnClick()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }
    }
}
