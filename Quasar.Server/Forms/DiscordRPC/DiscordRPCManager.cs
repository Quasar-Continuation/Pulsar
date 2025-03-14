using Quasar.Server.Models;
using System.Windows.Forms;

namespace Quasar.Server.Forms.DiscordRPC
{
    internal class DiscordRPCManager
    {
        private static DiscordRPC _rpcInstance;

        public static void Initialize(Form form)
        {
            if (_rpcInstance == null)
            {
                _rpcInstance = new DiscordRPC(form);
            }
            ApplyDiscordRPC(form);
        }

        public static void ApplyDiscordRPC(Form form)
        {
            bool isDiscordRPCChecked = Settings.DiscordRPC;
            if (_rpcInstance == null || _rpcInstance.Enabled != isDiscordRPCChecked)
            {
                if (_rpcInstance != null)
                {
                    _rpcInstance.Enabled = false;
                }
                _rpcInstance = new DiscordRPC(form);
                _rpcInstance.Enabled = isDiscordRPCChecked;
            }
            else
            {
                _rpcInstance.Enabled = isDiscordRPCChecked;
            }
        }

        public static void Shutdown()
        {
            if (_rpcInstance != null)
            {
                _rpcInstance.Enabled = false;
                _rpcInstance = null;
            }
        }
    }
}