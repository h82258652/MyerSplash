using NotificationsExtensions.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;

namespace MyerSplash.LiveTile
{
    public static class LiveTileUpdater
    {
        public static async Task UpdateImagesTileAsync(IEnumerable<string> imagesFilePath)
        {
            try
            {
                var tile = new TileBinding();
                var photosContent = new TileBindingContentPhotos();

                var maxCount = imagesFilePath.Count() >= 9 ? 9 : imagesFilePath.Count();
                for (int i = 0; i < maxCount; i++)
                {
                    var path = imagesFilePath.ElementAt(i);
                    if (string.IsNullOrEmpty(path)) continue;
                    var file = await StorageFile.GetFileFromPathAsync(path);
                    var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("temptile", CreationCollisionOption.OpenIfExists);
                    var newFile = await file.CopyAsync(folder, "tiles", NameCollisionOption.GenerateUniqueName);
                    photosContent.Images.Add(new TileImageSource(newFile.Path));
                }
                tile.Content = photosContent;

                var tileContent = new TileContent();
                tileContent.Visual = new TileVisual();
                tileContent.Visual.Branding = TileBranding.NameAndLogo;
                tileContent.Visual.TileMedium = tile;
                tileContent.Visual.TileWide = tile;
                tileContent.Visual.TileLarge = tile;

                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(new TileNotification(tileContent.GetXml()));
            }
            catch (Exception)
            {

            }
        }

        public static void CleanUpTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}
