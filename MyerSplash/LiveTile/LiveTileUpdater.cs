using JP.Utils.Debug;
using NotificationsExtensions.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    photosContent.Images.Add(new TileImageSource(path));
                }
                tile.Content = photosContent;

                Debug.WriteLine("Photos size is " + photosContent.Images.Count);

                var tileContent = new TileContent();
                tileContent.Visual = new TileVisual();
                tileContent.Visual.Branding = TileBranding.NameAndLogo;
                tileContent.Visual.TileMedium = tile;
                tileContent.Visual.TileWide = tile;
                tileContent.Visual.TileLarge = tile;

                await ClearAllTileFile();

                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
                TileUpdateManager.CreateTileUpdaterForApplication().Update(
                    new TileNotification(tileContent.GetXml()));
            }
            catch (Exception e)
            {
                var task = ExceptionHelper.WriteRecordAsync(e, nameof(LiveTileUpdater), nameof(UpdateImagesTileAsync));
            }
        }

        public async static Task ClearAllTileFile()
        {
            var folder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("temptile", CreationCollisionOption.OpenIfExists);
            var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery);
            files.ToList().ForEach(async f => await f.DeleteAsync(StorageDeleteOption.PermanentDelete));
        }

        public static void CleanUpTile()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }
    }
}
