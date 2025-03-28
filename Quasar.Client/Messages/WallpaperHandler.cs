using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Messages.FunStuff;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Quasar.Common.Messages.other;

namespace Quasar.Client.Messages
{
public class WallpaperHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoChangeWallpaper;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            if (message is DoChangeWallpaper changeWallpaperMessage)
            {
                SetWallpaper(changeWallpaperMessage.ImageData, changeWallpaperMessage.ImageFormat);
            }
        }

        private void SetWallpaper(byte[] imageData, string imageFormat)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), $"wallpaper.{imageFormat}");
            File.WriteAllBytes(tempPath, imageData);

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDCHANGE = 0x02;
    }
}
