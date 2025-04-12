using Pulsar.Server.Controls;
using System.Drawing;
using System.Windows.Forms;

namespace Pulsar.Server.Forms
{
    partial class FrmMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                if (this.IsHandleCreated)
                {
                    components.Dispose();
                }
            }
            try
            {
                base.Dispose(disposing);
            } catch
            {
            }
            
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter1 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("CPU");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("GPU");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("RAM");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Uptime");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Antivirus");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Default Browser");
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter2 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter3 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter4 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter5 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter6 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter7 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter8 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter9 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            Pulsar.Server.Utilities.ListViewColumnSorter listViewColumnSorter10 = new Pulsar.Server.Utilities.ListViewColumnSorter();
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startupManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.taskManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteShellToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reverseProxyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.registryEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteExecuteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.localFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctxtLine = new System.Windows.Forms.ToolStripSeparator();
            this.actionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shutdownToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.standbyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.surveillanceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteDesktopToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.webcamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.audioToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hVNCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keyloggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.passwordRecoveryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kematianGrabbingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.installVirtualMonitorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.userSupportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteScriptingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMessageboxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visitWebsiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickCommandsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addCDriveExceptionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.funMethodsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bSODToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cWToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.swapMouseButtonsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideTaskBarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gdiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pixelCorruptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elevatedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elevateClientPermissionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elevateToSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deElevateFromSystemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nicknameToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uninstallToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imgFlags = new System.Windows.Forms.ImageList(this.components);
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.MainTabControl = new Pulsar.Server.Controls.NoButtonTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lstClients = new Pulsar.Server.Controls.AeroListView();
            this.hIP = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hNick = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hTag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hUserPC = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hVersion = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hCurrentWindow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hUserStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hCountry = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hOS = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hAccountType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBoxMain = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.btnQuickRemoteDesktop = new System.Windows.Forms.Button();
            this.btnQuickWebcam = new System.Windows.Forms.Button();
            this.btnQuickKeylogger = new System.Windows.Forms.Button();
            this.btnQuickFileTransfer = new System.Windows.Forms.Button();
            this.btnQuickRemoteShell = new System.Windows.Forms.Button();
            this.btnQuickFileExplorer = new System.Windows.Forms.Button();
            this.gBoxClientInfo = new System.Windows.Forms.GroupBox();
            this.clientInfoListView = new Pulsar.Server.Controls.AeroListView();
            this.Names = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Stats = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.DebugLogRichBox = new System.Windows.Forms.RichTextBox();
            this.DebugContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveSlectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.clearLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstNoti = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.NotificationContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addKeywordsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.cryptoGroupBox = new System.Windows.Forms.GroupBox();
            this.BCHTextBox = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TRXTextBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.XRPTextBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.DASHTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.SOLTextBox = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.XMRTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.LTCTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.ETHTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.BTCTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ClipperCheckbox = new System.Windows.Forms.CheckBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.lstTasks = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TasksContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addTaskToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.remoteExecuteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.shellCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kematianToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showMessageBoxToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.excludeSystemDriveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.dotNetBarTabControl1 = new Pulsar.Server.Controls.DotNetBarTabControl();
            this.Passwords = new System.Windows.Forms.TabPage();
            this.aeroListView2 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Cookies = new System.Windows.Forms.TabPage();
            this.aeroListView3 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Communication = new System.Windows.Forms.TabPage();
            this.aeroListView4 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader17 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Games = new System.Windows.Forms.TabPage();
            this.aeroListView5 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.aeroListView6 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.aeroListView1 = new Pulsar.Server.Controls.AeroListView();
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.listenToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.connectedToolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.clientsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoTasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cryptoClipperToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notificationCentreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.builderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.gBoxClientInfo.SuspendLayout();
            this.DebugContextMenuStrip.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.NotificationContextMenuStrip.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.cryptoGroupBox.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.TasksContextMenuStrip.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.dotNetBarTabControl1.SuspendLayout();
            this.Passwords.SuspendLayout();
            this.Cookies.SuspendLayout();
            this.Communication.SuspendLayout();
            this.Games.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemToolStripMenuItem,
            this.surveillanceToolStripMenuItem,
            this.userSupportToolStripMenuItem,
            this.quickCommandsToolStripMenuItem,
            this.funMethodsToolStripMenuItem,
            this.connectionToolStripMenuItem,
            this.lineToolStripMenuItem,
            this.selectAllToolStripMenuItem});
            this.contextMenuStrip.Name = "ctxtMenu";
            this.contextMenuStrip.Size = new System.Drawing.Size(180, 164);
            this.contextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_Opening);
            // 
            // systemToolStripMenuItem
            // 
            this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemInformationToolStripMenuItem,
            this.fileManagerToolStripMenuItem,
            this.startupManagerToolStripMenuItem,
            this.taskManagerToolStripMenuItem,
            this.remoteShellToolStripMenuItem,
            this.connectionsToolStripMenuItem,
            this.reverseProxyToolStripMenuItem,
            this.registryEditorToolStripMenuItem,
            this.remoteExecuteToolStripMenuItem,
            this.ctxtLine,
            this.actionsToolStripMenuItem});
            this.systemToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.cog;
            this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            this.systemToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.systemToolStripMenuItem.Text = "Administration";
            this.systemToolStripMenuItem.Click += new System.EventHandler(this.systemToolStripMenuItem_Click);
            // 
            // systemInformationToolStripMenuItem
            // 
            this.systemInformationToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("systemInformationToolStripMenuItem.Image")));
            this.systemInformationToolStripMenuItem.Name = "systemInformationToolStripMenuItem";
            this.systemInformationToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.systemInformationToolStripMenuItem.Text = "System Information";
            this.systemInformationToolStripMenuItem.Click += new System.EventHandler(this.systemInformationToolStripMenuItem_Click);
            // 
            // fileManagerToolStripMenuItem
            // 
            this.fileManagerToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("fileManagerToolStripMenuItem.Image")));
            this.fileManagerToolStripMenuItem.Name = "fileManagerToolStripMenuItem";
            this.fileManagerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.fileManagerToolStripMenuItem.Text = "File Manager";
            this.fileManagerToolStripMenuItem.Click += new System.EventHandler(this.fileManagerToolStripMenuItem_Click);
            // 
            // startupManagerToolStripMenuItem
            // 
            this.startupManagerToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.application_edit;
            this.startupManagerToolStripMenuItem.Name = "startupManagerToolStripMenuItem";
            this.startupManagerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.startupManagerToolStripMenuItem.Text = "Startup Manager";
            this.startupManagerToolStripMenuItem.Click += new System.EventHandler(this.startupManagerToolStripMenuItem_Click);
            // 
            // taskManagerToolStripMenuItem
            // 
            this.taskManagerToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.application_cascade;
            this.taskManagerToolStripMenuItem.Name = "taskManagerToolStripMenuItem";
            this.taskManagerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.taskManagerToolStripMenuItem.Text = "Task Manager";
            this.taskManagerToolStripMenuItem.Click += new System.EventHandler(this.taskManagerToolStripMenuItem_Click);
            // 
            // remoteShellToolStripMenuItem
            // 
            this.remoteShellToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("remoteShellToolStripMenuItem.Image")));
            this.remoteShellToolStripMenuItem.Name = "remoteShellToolStripMenuItem";
            this.remoteShellToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.remoteShellToolStripMenuItem.Text = "Remote Shell";
            this.remoteShellToolStripMenuItem.Click += new System.EventHandler(this.remoteShellToolStripMenuItem_Click);
            // 
            // connectionsToolStripMenuItem
            // 
            this.connectionsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.transmit_blue;
            this.connectionsToolStripMenuItem.Name = "connectionsToolStripMenuItem";
            this.connectionsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.connectionsToolStripMenuItem.Text = "TCP Connections";
            this.connectionsToolStripMenuItem.Click += new System.EventHandler(this.connectionsToolStripMenuItem_Click);
            // 
            // reverseProxyToolStripMenuItem
            // 
            this.reverseProxyToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.server_link;
            this.reverseProxyToolStripMenuItem.Name = "reverseProxyToolStripMenuItem";
            this.reverseProxyToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.reverseProxyToolStripMenuItem.Text = "Reverse Proxy";
            this.reverseProxyToolStripMenuItem.Click += new System.EventHandler(this.reverseProxyToolStripMenuItem_Click);
            // 
            // registryEditorToolStripMenuItem
            // 
            this.registryEditorToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.registry;
            this.registryEditorToolStripMenuItem.Name = "registryEditorToolStripMenuItem";
            this.registryEditorToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.registryEditorToolStripMenuItem.Text = "Registry Editor";
            this.registryEditorToolStripMenuItem.Click += new System.EventHandler(this.registryEditorToolStripMenuItem_Click);
            // 
            // remoteExecuteToolStripMenuItem
            // 
            this.remoteExecuteToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.localFileToolStripMenuItem,
            this.webFileToolStripMenuItem});
            this.remoteExecuteToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.lightning;
            this.remoteExecuteToolStripMenuItem.Name = "remoteExecuteToolStripMenuItem";
            this.remoteExecuteToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.remoteExecuteToolStripMenuItem.Text = "Remote Execute";
            // 
            // localFileToolStripMenuItem
            // 
            this.localFileToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.drive_go;
            this.localFileToolStripMenuItem.Name = "localFileToolStripMenuItem";
            this.localFileToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.localFileToolStripMenuItem.Text = "Local File...";
            this.localFileToolStripMenuItem.Click += new System.EventHandler(this.localFileToolStripMenuItem_Click);
            // 
            // webFileToolStripMenuItem
            // 
            this.webFileToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.world_go;
            this.webFileToolStripMenuItem.Name = "webFileToolStripMenuItem";
            this.webFileToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.webFileToolStripMenuItem.Text = "Web File...";
            this.webFileToolStripMenuItem.Click += new System.EventHandler(this.webFileToolStripMenuItem_Click);
            // 
            // ctxtLine
            // 
            this.ctxtLine.Name = "ctxtLine";
            this.ctxtLine.Size = new System.Drawing.Size(175, 6);
            // 
            // actionsToolStripMenuItem
            // 
            this.actionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shutdownToolStripMenuItem,
            this.restartToolStripMenuItem,
            this.standbyToolStripMenuItem});
            this.actionsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.actions;
            this.actionsToolStripMenuItem.Name = "actionsToolStripMenuItem";
            this.actionsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.actionsToolStripMenuItem.Text = "Actions";
            // 
            // shutdownToolStripMenuItem
            // 
            this.shutdownToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.shutdown;
            this.shutdownToolStripMenuItem.Name = "shutdownToolStripMenuItem";
            this.shutdownToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.shutdownToolStripMenuItem.Text = "Shutdown";
            this.shutdownToolStripMenuItem.Click += new System.EventHandler(this.shutdownToolStripMenuItem_Click);
            // 
            // restartToolStripMenuItem
            // 
            this.restartToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.restart;
            this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            this.restartToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.restartToolStripMenuItem.Text = "Restart";
            this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
            // 
            // standbyToolStripMenuItem
            // 
            this.standbyToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.standby;
            this.standbyToolStripMenuItem.Name = "standbyToolStripMenuItem";
            this.standbyToolStripMenuItem.Size = new System.Drawing.Size(128, 22);
            this.standbyToolStripMenuItem.Text = "Standby";
            this.standbyToolStripMenuItem.Click += new System.EventHandler(this.standbyToolStripMenuItem_Click);
            // 
            // surveillanceToolStripMenuItem
            // 
            this.surveillanceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.remoteDesktopToolStripMenuItem2,
            this.webcamToolStripMenuItem,
            this.audioToolStripMenuItem,
            this.hVNCToolStripMenuItem,
            this.keyloggerToolStripMenuItem,
            this.passwordRecoveryToolStripMenuItem,
            this.kematianGrabbingToolStripMenuItem,
            this.installVirtualMonitorToolStripMenuItem});
            this.surveillanceToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.monitoring;
            this.surveillanceToolStripMenuItem.Name = "surveillanceToolStripMenuItem";
            this.surveillanceToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.surveillanceToolStripMenuItem.Text = "Monitoring";
            // 
            // remoteDesktopToolStripMenuItem2
            // 
            this.remoteDesktopToolStripMenuItem2.Image = ((System.Drawing.Image)(resources.GetObject("remoteDesktopToolStripMenuItem2.Image")));
            this.remoteDesktopToolStripMenuItem2.Name = "remoteDesktopToolStripMenuItem2";
            this.remoteDesktopToolStripMenuItem2.Size = new System.Drawing.Size(188, 22);
            this.remoteDesktopToolStripMenuItem2.Text = "Remote Desktop";
            this.remoteDesktopToolStripMenuItem2.Click += new System.EventHandler(this.remoteDesktopToolStripMenuItem_Click);
            // 
            // webcamToolStripMenuItem
            // 
            this.webcamToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.webcam;
            this.webcamToolStripMenuItem.Name = "webcamToolStripMenuItem";
            this.webcamToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.webcamToolStripMenuItem.Text = "Webcam";
            this.webcamToolStripMenuItem.Click += new System.EventHandler(this.webcamToolStripMenuItem_Click);
            // 
            // audioToolStripMenuItem
            // 
            this.audioToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.sound;
            this.audioToolStripMenuItem.Name = "audioToolStripMenuItem";
            this.audioToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.audioToolStripMenuItem.Text = "Remote Microphone";
            this.audioToolStripMenuItem.Click += new System.EventHandler(this.audioToolStripMenuItem_Click);
            // 
            // hVNCToolStripMenuItem
            // 
            this.hVNCToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.ruby;
            this.hVNCToolStripMenuItem.Name = "hVNCToolStripMenuItem";
            this.hVNCToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.hVNCToolStripMenuItem.Text = "HVNC";
            this.hVNCToolStripMenuItem.Click += new System.EventHandler(this.hVNCToolStripMenuItem_Click);
            // 
            // keyloggerToolStripMenuItem
            // 
            this.keyloggerToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.keyboard_magnify;
            this.keyloggerToolStripMenuItem.Name = "keyloggerToolStripMenuItem";
            this.keyloggerToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.keyloggerToolStripMenuItem.Text = "Keylogger";
            this.keyloggerToolStripMenuItem.Click += new System.EventHandler(this.keyloggerToolStripMenuItem_Click);
            // 
            // passwordRecoveryToolStripMenuItem
            // 
            this.passwordRecoveryToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("passwordRecoveryToolStripMenuItem.Image")));
            this.passwordRecoveryToolStripMenuItem.Name = "passwordRecoveryToolStripMenuItem";
            this.passwordRecoveryToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.passwordRecoveryToolStripMenuItem.Text = "Password Recovery";
            this.passwordRecoveryToolStripMenuItem.Click += new System.EventHandler(this.passwordRecoveryToolStripMenuItem_Click);
            // 
            // kematianGrabbingToolStripMenuItem
            // 
            this.kematianGrabbingToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("kematianGrabbingToolStripMenuItem.Image")));
            this.kematianGrabbingToolStripMenuItem.Name = "kematianGrabbingToolStripMenuItem";
            this.kematianGrabbingToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.kematianGrabbingToolStripMenuItem.Text = "Kematian Grabbing";
            this.kematianGrabbingToolStripMenuItem.Click += new System.EventHandler(this.kematianGrabbingToolStripMenuItem_Click);
            // 
            // installVirtualMonitorToolStripMenuItem
            // 
            this.installVirtualMonitorToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.monitor;
            this.installVirtualMonitorToolStripMenuItem.Name = "installVirtualMonitorToolStripMenuItem";
            this.installVirtualMonitorToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.installVirtualMonitorToolStripMenuItem.Text = "Install Virtual Monitor";
            this.installVirtualMonitorToolStripMenuItem.Click += new System.EventHandler(this.installVirtualMonitorToolStripMenuItem_Click);
            // 
            // userSupportToolStripMenuItem
            // 
            this.userSupportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.remoteScriptingToolStripMenuItem,
            this.showMessageboxToolStripMenuItem,
            this.visitWebsiteToolStripMenuItem});
            this.userSupportToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.user;
            this.userSupportToolStripMenuItem.Name = "userSupportToolStripMenuItem";
            this.userSupportToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.userSupportToolStripMenuItem.Text = "User Support";
            // 
            // remoteScriptingToolStripMenuItem
            // 
            this.remoteScriptingToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.script_code;
            this.remoteScriptingToolStripMenuItem.Name = "remoteScriptingToolStripMenuItem";
            this.remoteScriptingToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.remoteScriptingToolStripMenuItem.Text = "Remote Scripting";
            this.remoteScriptingToolStripMenuItem.Click += new System.EventHandler(this.remoteScriptingToolStripMenuItem_Click);
            // 
            // showMessageboxToolStripMenuItem
            // 
            this.showMessageboxToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("showMessageboxToolStripMenuItem.Image")));
            this.showMessageboxToolStripMenuItem.Name = "showMessageboxToolStripMenuItem";
            this.showMessageboxToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.showMessageboxToolStripMenuItem.Text = "Show Messagebox";
            this.showMessageboxToolStripMenuItem.Click += new System.EventHandler(this.showMessageboxToolStripMenuItem_Click);
            // 
            // visitWebsiteToolStripMenuItem
            // 
            this.visitWebsiteToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("visitWebsiteToolStripMenuItem.Image")));
            this.visitWebsiteToolStripMenuItem.Name = "visitWebsiteToolStripMenuItem";
            this.visitWebsiteToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.visitWebsiteToolStripMenuItem.Text = "Send to Website";
            this.visitWebsiteToolStripMenuItem.Click += new System.EventHandler(this.visitWebsiteToolStripMenuItem_Click);
            // 
            // quickCommandsToolStripMenuItem
            // 
            this.quickCommandsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addCDriveExceptionToolStripMenuItem});
            this.quickCommandsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.wrench;
            this.quickCommandsToolStripMenuItem.Name = "quickCommandsToolStripMenuItem";
            this.quickCommandsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.quickCommandsToolStripMenuItem.Text = "Miscellaneous";
            // 
            // addCDriveExceptionToolStripMenuItem
            // 
            this.addCDriveExceptionToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.application_view_xp_terminal;
            this.addCDriveExceptionToolStripMenuItem.Name = "addCDriveExceptionToolStripMenuItem";
            this.addCDriveExceptionToolStripMenuItem.Size = new System.Drawing.Size(194, 22);
            this.addCDriveExceptionToolStripMenuItem.Text = "Add C: Drive Exception";
            this.addCDriveExceptionToolStripMenuItem.Click += new System.EventHandler(this.addCDriveExceptionToolStripMenuItem_Click);
            // 
            // funMethodsToolStripMenuItem
            // 
            this.funMethodsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bSODToolStripMenuItem,
            this.cWToolStripMenuItem,
            this.swapMouseButtonsToolStripMenuItem,
            this.hideTaskBarToolStripMenuItem,
            this.gdiToolStripMenuItem});
            this.funMethodsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.emoticon_evilgrin;
            this.funMethodsToolStripMenuItem.Name = "funMethodsToolStripMenuItem";
            this.funMethodsToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.funMethodsToolStripMenuItem.Text = "Fun Stuff";
            // 
            // bSODToolStripMenuItem
            // 
            this.bSODToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.nuclear;
            this.bSODToolStripMenuItem.Name = "bSODToolStripMenuItem";
            this.bSODToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.bSODToolStripMenuItem.Text = "BSOD";
            this.bSODToolStripMenuItem.Click += new System.EventHandler(this.bSODToolStripMenuItem_Click);
            // 
            // cWToolStripMenuItem
            // 
            this.cWToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.images;
            this.cWToolStripMenuItem.Name = "cWToolStripMenuItem";
            this.cWToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.cWToolStripMenuItem.Text = "Change Wallpaper";
            this.cWToolStripMenuItem.Click += new System.EventHandler(this.cWToolStripMenuItem_Click);
            // 
            // swapMouseButtonsToolStripMenuItem
            // 
            this.swapMouseButtonsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.mouse;
            this.swapMouseButtonsToolStripMenuItem.Name = "swapMouseButtonsToolStripMenuItem";
            this.swapMouseButtonsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.swapMouseButtonsToolStripMenuItem.Text = "Swap Mouse Buttons";
            this.swapMouseButtonsToolStripMenuItem.Click += new System.EventHandler(this.swapMouseButtonsToolStripMenuItem_Click);
            // 
            // hideTaskBarToolStripMenuItem
            // 
            this.hideTaskBarToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.cog;
            this.hideTaskBarToolStripMenuItem.Name = "hideTaskBarToolStripMenuItem";
            this.hideTaskBarToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.hideTaskBarToolStripMenuItem.Text = "Hide Taskbar";
            this.hideTaskBarToolStripMenuItem.Click += new System.EventHandler(this.hideTaskBarToolStripMenuItem_Click);
            // 
            // gdiToolStripMenuItem
            // 
            this.gdiToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pixelCorruptToolStripMenuItem});
            this.gdiToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.pencil;
            this.gdiToolStripMenuItem.Name = "gdiToolStripMenuItem";
            this.gdiToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.gdiToolStripMenuItem.Text = "GDI";
            // 
            // pixelCorruptToolStripMenuItem
            // 
            this.pixelCorruptToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.pencil;
            this.pixelCorruptToolStripMenuItem.Name = "pixelCorruptToolStripMenuItem";
            this.pixelCorruptToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.pixelCorruptToolStripMenuItem.Text = "Pixel Corrupt";
            // 
            // connectionToolStripMenuItem
            // 
            this.connectionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.elevatedToolStripMenuItem,
            this.nicknameToolStripMenuItem,
            this.updateToolStripMenuItem,
            this.reconnectToolStripMenuItem,
            this.disconnectToolStripMenuItem,
            this.uninstallToolStripMenuItem});
            this.connectionToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("connectionToolStripMenuItem.Image")));
            this.connectionToolStripMenuItem.Name = "connectionToolStripMenuItem";
            this.connectionToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.connectionToolStripMenuItem.Text = "Client Management";
            // 
            // elevatedToolStripMenuItem
            // 
            this.elevatedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.elevateClientPermissionsToolStripMenuItem,
            this.elevateToSystemToolStripMenuItem,
            this.deElevateFromSystemToolStripMenuItem});
            this.elevatedToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.uac_shield;
            this.elevatedToolStripMenuItem.Name = "elevatedToolStripMenuItem";
            this.elevatedToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.elevatedToolStripMenuItem.Text = "Elevated";
            // 
            // elevateClientPermissionsToolStripMenuItem
            // 
            this.elevateClientPermissionsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.uac_shield;
            this.elevateClientPermissionsToolStripMenuItem.Name = "elevateClientPermissionsToolStripMenuItem";
            this.elevateClientPermissionsToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.elevateClientPermissionsToolStripMenuItem.Text = "Elevate Client Permissions";
            this.elevateClientPermissionsToolStripMenuItem.Click += new System.EventHandler(this.elevateClientPermissionsToolStripMenuItem_Click);
            // 
            // elevateToSystemToolStripMenuItem
            // 
            this.elevateToSystemToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.uac_shield;
            this.elevateToSystemToolStripMenuItem.Name = "elevateToSystemToolStripMenuItem";
            this.elevateToSystemToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.elevateToSystemToolStripMenuItem.Text = "Elevate to System";
            this.elevateToSystemToolStripMenuItem.Click += new System.EventHandler(this.elevateToSystemToolStripMenuItem_Click);
            // 
            // deElevateFromSystemToolStripMenuItem
            // 
            this.deElevateFromSystemToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.uac_shield;
            this.deElevateFromSystemToolStripMenuItem.Name = "deElevateFromSystemToolStripMenuItem";
            this.deElevateFromSystemToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.deElevateFromSystemToolStripMenuItem.Text = "DeElevate From System";
            this.deElevateFromSystemToolStripMenuItem.Click += new System.EventHandler(this.deElevateToolStripMenuItem_Click);
            // 
            // nicknameToolStripMenuItem
            // 
            this.nicknameToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.uac_shield;
            this.nicknameToolStripMenuItem.Name = "nicknameToolStripMenuItem";
            this.nicknameToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.nicknameToolStripMenuItem.Text = "Nickname";
            this.nicknameToolStripMenuItem.Click += new System.EventHandler(this.nicknameToolStripMenuItem_Click);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("updateToolStripMenuItem.Image")));
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.updateToolStripMenuItem.Text = "Update";
            this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // reconnectToolStripMenuItem
            // 
            this.reconnectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("reconnectToolStripMenuItem.Image")));
            this.reconnectToolStripMenuItem.Name = "reconnectToolStripMenuItem";
            this.reconnectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.reconnectToolStripMenuItem.Text = "Reconnect";
            this.reconnectToolStripMenuItem.Click += new System.EventHandler(this.reconnectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("disconnectToolStripMenuItem.Image")));
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            this.disconnectToolStripMenuItem.Click += new System.EventHandler(this.disconnectToolStripMenuItem_Click);
            // 
            // uninstallToolStripMenuItem
            // 
            this.uninstallToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("uninstallToolStripMenuItem.Image")));
            this.uninstallToolStripMenuItem.Name = "uninstallToolStripMenuItem";
            this.uninstallToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.uninstallToolStripMenuItem.Text = "Uninstall";
            this.uninstallToolStripMenuItem.Click += new System.EventHandler(this.uninstallToolStripMenuItem_Click);
            // 
            // lineToolStripMenuItem
            // 
            this.lineToolStripMenuItem.Name = "lineToolStripMenuItem";
            this.lineToolStripMenuItem.Size = new System.Drawing.Size(176, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.selectAllToolStripMenuItem.Text = "Select All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.selectAllToolStripMenuItem_Click);
            // 
            // imgFlags
            // 
            this.imgFlags.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgFlags.ImageStream")));
            this.imgFlags.TransparentColor = System.Drawing.Color.Transparent;
            this.imgFlags.Images.SetKeyName(0, "ad.png");
            this.imgFlags.Images.SetKeyName(1, "ae.png");
            this.imgFlags.Images.SetKeyName(2, "af.png");
            this.imgFlags.Images.SetKeyName(3, "ag.png");
            this.imgFlags.Images.SetKeyName(4, "ai.png");
            this.imgFlags.Images.SetKeyName(5, "al.png");
            this.imgFlags.Images.SetKeyName(6, "am.png");
            this.imgFlags.Images.SetKeyName(7, "an.png");
            this.imgFlags.Images.SetKeyName(8, "ao.png");
            this.imgFlags.Images.SetKeyName(9, "ar.png");
            this.imgFlags.Images.SetKeyName(10, "as.png");
            this.imgFlags.Images.SetKeyName(11, "at.png");
            this.imgFlags.Images.SetKeyName(12, "au.png");
            this.imgFlags.Images.SetKeyName(13, "aw.png");
            this.imgFlags.Images.SetKeyName(14, "ax.png");
            this.imgFlags.Images.SetKeyName(15, "az.png");
            this.imgFlags.Images.SetKeyName(16, "ba.png");
            this.imgFlags.Images.SetKeyName(17, "bb.png");
            this.imgFlags.Images.SetKeyName(18, "bd.png");
            this.imgFlags.Images.SetKeyName(19, "be.png");
            this.imgFlags.Images.SetKeyName(20, "bf.png");
            this.imgFlags.Images.SetKeyName(21, "bg.png");
            this.imgFlags.Images.SetKeyName(22, "bh.png");
            this.imgFlags.Images.SetKeyName(23, "bi.png");
            this.imgFlags.Images.SetKeyName(24, "bj.png");
            this.imgFlags.Images.SetKeyName(25, "bm.png");
            this.imgFlags.Images.SetKeyName(26, "bn.png");
            this.imgFlags.Images.SetKeyName(27, "bo.png");
            this.imgFlags.Images.SetKeyName(28, "br.png");
            this.imgFlags.Images.SetKeyName(29, "bs.png");
            this.imgFlags.Images.SetKeyName(30, "bt.png");
            this.imgFlags.Images.SetKeyName(31, "bv.png");
            this.imgFlags.Images.SetKeyName(32, "bw.png");
            this.imgFlags.Images.SetKeyName(33, "by.png");
            this.imgFlags.Images.SetKeyName(34, "bz.png");
            this.imgFlags.Images.SetKeyName(35, "ca.png");
            this.imgFlags.Images.SetKeyName(36, "catalonia.png");
            this.imgFlags.Images.SetKeyName(37, "cc.png");
            this.imgFlags.Images.SetKeyName(38, "cd.png");
            this.imgFlags.Images.SetKeyName(39, "cf.png");
            this.imgFlags.Images.SetKeyName(40, "cg.png");
            this.imgFlags.Images.SetKeyName(41, "ch.png");
            this.imgFlags.Images.SetKeyName(42, "ci.png");
            this.imgFlags.Images.SetKeyName(43, "ck.png");
            this.imgFlags.Images.SetKeyName(44, "cl.png");
            this.imgFlags.Images.SetKeyName(45, "cm.png");
            this.imgFlags.Images.SetKeyName(46, "cn.png");
            this.imgFlags.Images.SetKeyName(47, "co.png");
            this.imgFlags.Images.SetKeyName(48, "cr.png");
            this.imgFlags.Images.SetKeyName(49, "cs.png");
            this.imgFlags.Images.SetKeyName(50, "cu.png");
            this.imgFlags.Images.SetKeyName(51, "cv.png");
            this.imgFlags.Images.SetKeyName(52, "cx.png");
            this.imgFlags.Images.SetKeyName(53, "cy.png");
            this.imgFlags.Images.SetKeyName(54, "cz.png");
            this.imgFlags.Images.SetKeyName(55, "de.png");
            this.imgFlags.Images.SetKeyName(56, "dj.png");
            this.imgFlags.Images.SetKeyName(57, "dk.png");
            this.imgFlags.Images.SetKeyName(58, "dm.png");
            this.imgFlags.Images.SetKeyName(59, "do.png");
            this.imgFlags.Images.SetKeyName(60, "dz.png");
            this.imgFlags.Images.SetKeyName(61, "ec.png");
            this.imgFlags.Images.SetKeyName(62, "ee.png");
            this.imgFlags.Images.SetKeyName(63, "eg.png");
            this.imgFlags.Images.SetKeyName(64, "eh.png");
            this.imgFlags.Images.SetKeyName(65, "england.png");
            this.imgFlags.Images.SetKeyName(66, "er.png");
            this.imgFlags.Images.SetKeyName(67, "es.png");
            this.imgFlags.Images.SetKeyName(68, "et.png");
            this.imgFlags.Images.SetKeyName(69, "europeanunion.png");
            this.imgFlags.Images.SetKeyName(70, "fam.png");
            this.imgFlags.Images.SetKeyName(71, "fi.png");
            this.imgFlags.Images.SetKeyName(72, "fj.png");
            this.imgFlags.Images.SetKeyName(73, "fk.png");
            this.imgFlags.Images.SetKeyName(74, "fm.png");
            this.imgFlags.Images.SetKeyName(75, "fo.png");
            this.imgFlags.Images.SetKeyName(76, "fr.png");
            this.imgFlags.Images.SetKeyName(77, "ga.png");
            this.imgFlags.Images.SetKeyName(78, "gb.png");
            this.imgFlags.Images.SetKeyName(79, "gd.png");
            this.imgFlags.Images.SetKeyName(80, "ge.png");
            this.imgFlags.Images.SetKeyName(81, "gf.png");
            this.imgFlags.Images.SetKeyName(82, "gh.png");
            this.imgFlags.Images.SetKeyName(83, "gi.png");
            this.imgFlags.Images.SetKeyName(84, "gl.png");
            this.imgFlags.Images.SetKeyName(85, "gm.png");
            this.imgFlags.Images.SetKeyName(86, "gn.png");
            this.imgFlags.Images.SetKeyName(87, "gp.png");
            this.imgFlags.Images.SetKeyName(88, "gq.png");
            this.imgFlags.Images.SetKeyName(89, "gr.png");
            this.imgFlags.Images.SetKeyName(90, "gs.png");
            this.imgFlags.Images.SetKeyName(91, "gt.png");
            this.imgFlags.Images.SetKeyName(92, "gu.png");
            this.imgFlags.Images.SetKeyName(93, "gw.png");
            this.imgFlags.Images.SetKeyName(94, "gy.png");
            this.imgFlags.Images.SetKeyName(95, "hk.png");
            this.imgFlags.Images.SetKeyName(96, "hm.png");
            this.imgFlags.Images.SetKeyName(97, "hn.png");
            this.imgFlags.Images.SetKeyName(98, "hr.png");
            this.imgFlags.Images.SetKeyName(99, "ht.png");
            this.imgFlags.Images.SetKeyName(100, "hu.png");
            this.imgFlags.Images.SetKeyName(101, "id.png");
            this.imgFlags.Images.SetKeyName(102, "ie.png");
            this.imgFlags.Images.SetKeyName(103, "il.png");
            this.imgFlags.Images.SetKeyName(104, "in.png");
            this.imgFlags.Images.SetKeyName(105, "io.png");
            this.imgFlags.Images.SetKeyName(106, "iq.png");
            this.imgFlags.Images.SetKeyName(107, "ir.png");
            this.imgFlags.Images.SetKeyName(108, "is.png");
            this.imgFlags.Images.SetKeyName(109, "it.png");
            this.imgFlags.Images.SetKeyName(110, "jm.png");
            this.imgFlags.Images.SetKeyName(111, "jo.png");
            this.imgFlags.Images.SetKeyName(112, "jp.png");
            this.imgFlags.Images.SetKeyName(113, "ke.png");
            this.imgFlags.Images.SetKeyName(114, "kg.png");
            this.imgFlags.Images.SetKeyName(115, "kh.png");
            this.imgFlags.Images.SetKeyName(116, "ki.png");
            this.imgFlags.Images.SetKeyName(117, "km.png");
            this.imgFlags.Images.SetKeyName(118, "kn.png");
            this.imgFlags.Images.SetKeyName(119, "kp.png");
            this.imgFlags.Images.SetKeyName(120, "kr.png");
            this.imgFlags.Images.SetKeyName(121, "kw.png");
            this.imgFlags.Images.SetKeyName(122, "ky.png");
            this.imgFlags.Images.SetKeyName(123, "kz.png");
            this.imgFlags.Images.SetKeyName(124, "la.png");
            this.imgFlags.Images.SetKeyName(125, "lb.png");
            this.imgFlags.Images.SetKeyName(126, "lc.png");
            this.imgFlags.Images.SetKeyName(127, "li.png");
            this.imgFlags.Images.SetKeyName(128, "lk.png");
            this.imgFlags.Images.SetKeyName(129, "lr.png");
            this.imgFlags.Images.SetKeyName(130, "ls.png");
            this.imgFlags.Images.SetKeyName(131, "lt.png");
            this.imgFlags.Images.SetKeyName(132, "lu.png");
            this.imgFlags.Images.SetKeyName(133, "lv.png");
            this.imgFlags.Images.SetKeyName(134, "ly.png");
            this.imgFlags.Images.SetKeyName(135, "ma.png");
            this.imgFlags.Images.SetKeyName(136, "mc.png");
            this.imgFlags.Images.SetKeyName(137, "md.png");
            this.imgFlags.Images.SetKeyName(138, "me.png");
            this.imgFlags.Images.SetKeyName(139, "mg.png");
            this.imgFlags.Images.SetKeyName(140, "mh.png");
            this.imgFlags.Images.SetKeyName(141, "mk.png");
            this.imgFlags.Images.SetKeyName(142, "ml.png");
            this.imgFlags.Images.SetKeyName(143, "mm.png");
            this.imgFlags.Images.SetKeyName(144, "mn.png");
            this.imgFlags.Images.SetKeyName(145, "mo.png");
            this.imgFlags.Images.SetKeyName(146, "mp.png");
            this.imgFlags.Images.SetKeyName(147, "mq.png");
            this.imgFlags.Images.SetKeyName(148, "mr.png");
            this.imgFlags.Images.SetKeyName(149, "ms.png");
            this.imgFlags.Images.SetKeyName(150, "mt.png");
            this.imgFlags.Images.SetKeyName(151, "mu.png");
            this.imgFlags.Images.SetKeyName(152, "mv.png");
            this.imgFlags.Images.SetKeyName(153, "mw.png");
            this.imgFlags.Images.SetKeyName(154, "mx.png");
            this.imgFlags.Images.SetKeyName(155, "my.png");
            this.imgFlags.Images.SetKeyName(156, "mz.png");
            this.imgFlags.Images.SetKeyName(157, "na.png");
            this.imgFlags.Images.SetKeyName(158, "nc.png");
            this.imgFlags.Images.SetKeyName(159, "ne.png");
            this.imgFlags.Images.SetKeyName(160, "nf.png");
            this.imgFlags.Images.SetKeyName(161, "ng.png");
            this.imgFlags.Images.SetKeyName(162, "ni.png");
            this.imgFlags.Images.SetKeyName(163, "nl.png");
            this.imgFlags.Images.SetKeyName(164, "no.png");
            this.imgFlags.Images.SetKeyName(165, "np.png");
            this.imgFlags.Images.SetKeyName(166, "nr.png");
            this.imgFlags.Images.SetKeyName(167, "nu.png");
            this.imgFlags.Images.SetKeyName(168, "nz.png");
            this.imgFlags.Images.SetKeyName(169, "om.png");
            this.imgFlags.Images.SetKeyName(170, "pa.png");
            this.imgFlags.Images.SetKeyName(171, "pe.png");
            this.imgFlags.Images.SetKeyName(172, "pf.png");
            this.imgFlags.Images.SetKeyName(173, "pg.png");
            this.imgFlags.Images.SetKeyName(174, "ph.png");
            this.imgFlags.Images.SetKeyName(175, "pk.png");
            this.imgFlags.Images.SetKeyName(176, "pl.png");
            this.imgFlags.Images.SetKeyName(177, "pm.png");
            this.imgFlags.Images.SetKeyName(178, "pn.png");
            this.imgFlags.Images.SetKeyName(179, "pr.png");
            this.imgFlags.Images.SetKeyName(180, "ps.png");
            this.imgFlags.Images.SetKeyName(181, "pt.png");
            this.imgFlags.Images.SetKeyName(182, "pw.png");
            this.imgFlags.Images.SetKeyName(183, "py.png");
            this.imgFlags.Images.SetKeyName(184, "qa.png");
            this.imgFlags.Images.SetKeyName(185, "re.png");
            this.imgFlags.Images.SetKeyName(186, "ro.png");
            this.imgFlags.Images.SetKeyName(187, "rs.png");
            this.imgFlags.Images.SetKeyName(188, "ru.png");
            this.imgFlags.Images.SetKeyName(189, "rw.png");
            this.imgFlags.Images.SetKeyName(190, "sa.png");
            this.imgFlags.Images.SetKeyName(191, "sb.png");
            this.imgFlags.Images.SetKeyName(192, "sc.png");
            this.imgFlags.Images.SetKeyName(193, "scotland.png");
            this.imgFlags.Images.SetKeyName(194, "sd.png");
            this.imgFlags.Images.SetKeyName(195, "se.png");
            this.imgFlags.Images.SetKeyName(196, "sg.png");
            this.imgFlags.Images.SetKeyName(197, "sh.png");
            this.imgFlags.Images.SetKeyName(198, "si.png");
            this.imgFlags.Images.SetKeyName(199, "sj.png");
            this.imgFlags.Images.SetKeyName(200, "sk.png");
            this.imgFlags.Images.SetKeyName(201, "sl.png");
            this.imgFlags.Images.SetKeyName(202, "sm.png");
            this.imgFlags.Images.SetKeyName(203, "sn.png");
            this.imgFlags.Images.SetKeyName(204, "so.png");
            this.imgFlags.Images.SetKeyName(205, "sr.png");
            this.imgFlags.Images.SetKeyName(206, "st.png");
            this.imgFlags.Images.SetKeyName(207, "sv.png");
            this.imgFlags.Images.SetKeyName(208, "sy.png");
            this.imgFlags.Images.SetKeyName(209, "sz.png");
            this.imgFlags.Images.SetKeyName(210, "tc.png");
            this.imgFlags.Images.SetKeyName(211, "td.png");
            this.imgFlags.Images.SetKeyName(212, "tf.png");
            this.imgFlags.Images.SetKeyName(213, "tg.png");
            this.imgFlags.Images.SetKeyName(214, "th.png");
            this.imgFlags.Images.SetKeyName(215, "tj.png");
            this.imgFlags.Images.SetKeyName(216, "tk.png");
            this.imgFlags.Images.SetKeyName(217, "tl.png");
            this.imgFlags.Images.SetKeyName(218, "tm.png");
            this.imgFlags.Images.SetKeyName(219, "tn.png");
            this.imgFlags.Images.SetKeyName(220, "to.png");
            this.imgFlags.Images.SetKeyName(221, "tr.png");
            this.imgFlags.Images.SetKeyName(222, "tt.png");
            this.imgFlags.Images.SetKeyName(223, "tv.png");
            this.imgFlags.Images.SetKeyName(224, "tw.png");
            this.imgFlags.Images.SetKeyName(225, "tz.png");
            this.imgFlags.Images.SetKeyName(226, "ua.png");
            this.imgFlags.Images.SetKeyName(227, "ug.png");
            this.imgFlags.Images.SetKeyName(228, "um.png");
            this.imgFlags.Images.SetKeyName(229, "us.png");
            this.imgFlags.Images.SetKeyName(230, "uy.png");
            this.imgFlags.Images.SetKeyName(231, "uz.png");
            this.imgFlags.Images.SetKeyName(232, "va.png");
            this.imgFlags.Images.SetKeyName(233, "vc.png");
            this.imgFlags.Images.SetKeyName(234, "ve.png");
            this.imgFlags.Images.SetKeyName(235, "vg.png");
            this.imgFlags.Images.SetKeyName(236, "vi.png");
            this.imgFlags.Images.SetKeyName(237, "vn.png");
            this.imgFlags.Images.SetKeyName(238, "vu.png");
            this.imgFlags.Images.SetKeyName(239, "wales.png");
            this.imgFlags.Images.SetKeyName(240, "wf.png");
            this.imgFlags.Images.SetKeyName(241, "ws.png");
            this.imgFlags.Images.SetKeyName(242, "ye.png");
            this.imgFlags.Images.SetKeyName(243, "yt.png");
            this.imgFlags.Images.SetKeyName(244, "za.png");
            this.imgFlags.Images.SetKeyName(245, "zm.png");
            this.imgFlags.Images.SetKeyName(246, "zw.png");
            this.imgFlags.Images.SetKeyName(247, "xy.png");
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Pulsar";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.MainTabControl, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.statusStrip, 0, 2);
            this.tableLayoutPanel.Controls.Add(this.menuStrip, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 3;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1024, 538);
            this.tableLayoutPanel.TabIndex = 6;
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.tabPage1);
            this.MainTabControl.Controls.Add(this.tabPage2);
            this.MainTabControl.Controls.Add(this.tabPage3);
            this.MainTabControl.Controls.Add(this.tabPage4);
            this.MainTabControl.Controls.Add(this.tabPage5);
            this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTabControl.Location = new System.Drawing.Point(0, 25);
            this.MainTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.MainTabControl.Name = "MainTabControl";
            this.MainTabControl.SelectedIndex = 0;
            this.MainTabControl.Size = new System.Drawing.Size(1024, 491);
            this.MainTabControl.TabIndex = 7;
            this.MainTabControl.SelectedIndexChanged += new System.EventHandler(this.MainTabControl_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lstClients);
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Controls.Add(this.splitter1);
            this.tabPage1.Controls.Add(this.DebugLogRichBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1016, 465);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "ClientTabPage";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // lstClients
            // 
            this.lstClients.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hIP,
            this.hNick,
            this.hTag,
            this.hUserPC,
            this.hVersion,
            this.hStatus,
            this.hCurrentWindow,
            this.hUserStatus,
            this.hCountry,
            this.hOS,
            this.hAccountType});
            this.lstClients.ContextMenuStrip = this.contextMenuStrip;
            this.lstClients.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstClients.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstClients.FullRowSelect = true;
            this.lstClients.HideSelection = false;
            this.lstClients.Location = new System.Drawing.Point(0, 0);
            listViewColumnSorter1.NeedNumberCompare = false;
            listViewColumnSorter1.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter1.SortColumn = 0;
            this.lstClients.LvwColumnSorter = listViewColumnSorter1;
            this.lstClients.Name = "lstClients";
            this.lstClients.ShowItemToolTips = true;
            this.lstClients.Size = new System.Drawing.Size(729, 351);
            this.lstClients.SmallImageList = this.imgFlags;
            this.lstClients.TabIndex = 1;
            this.lstClients.UseCompatibleStateImageBehavior = false;
            this.lstClients.View = System.Windows.Forms.View.Details;
            this.lstClients.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.lstClients_ColumnWidthChanged);
            this.lstClients.DrawItem += new System.Windows.Forms.DrawListViewItemEventHandler(this.lstClients_DrawItem);
            this.lstClients.SelectedIndexChanged += new System.EventHandler(this.lstClients_SelectedIndexChanged);
            this.lstClients.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.lstClients_MouseWheel);
            this.lstClients.Resize += new System.EventHandler(this.lstClients_Resize);
            // 
            // hIP
            // 
            this.hIP.Text = "IP Address";
            this.hIP.Width = 112;
            // 
            // hNick
            // 
            this.hNick.Text = "Nickname";
            this.hNick.Width = 112;
            // 
            // hTag
            // 
            this.hTag.Text = "Tag";
            // 
            // hUserPC
            // 
            this.hUserPC.Text = "User@PC";
            this.hUserPC.Width = 175;
            // 
            // hVersion
            // 
            this.hVersion.Text = "Version";
            this.hVersion.Width = 66;
            // 
            // hStatus
            // 
            this.hStatus.Text = "Status";
            this.hStatus.Width = 78;
            // 
            // hCurrentWindow
            // 
            this.hCurrentWindow.Text = "Current Window";
            this.hCurrentWindow.Width = 200;
            // 
            // hUserStatus
            // 
            this.hUserStatus.Text = "User Status";
            this.hUserStatus.Width = 72;
            // 
            // hCountry
            // 
            this.hCountry.Text = "Country";
            this.hCountry.Width = 117;
            // 
            // hOS
            // 
            this.hOS.Text = "Operating System";
            this.hOS.Width = 222;
            // 
            // hAccountType
            // 
            this.hAccountType.Text = "Account Type";
            this.hAccountType.Width = 11;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBoxMain, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.gBoxClientInfo, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(729, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 14F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 166F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(287, 351);
            this.tableLayoutPanel1.TabIndex = 32;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Client Preview";
            // 
            // pictureBoxMain
            // 
            this.pictureBoxMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxMain.Image = global::Pulsar.Server.Properties.Resources.no_previewbmp;
            this.pictureBoxMain.InitialImage = null;
            this.pictureBoxMain.Location = new System.Drawing.Point(3, 17);
            this.pictureBoxMain.Name = "pictureBoxMain";
            this.pictureBoxMain.Size = new System.Drawing.Size(281, 160);
            this.pictureBoxMain.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxMain.TabIndex = 31;
            this.pictureBoxMain.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 8;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 77F));
            this.tableLayoutPanel2.Controls.Add(this.button1, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickRemoteDesktop, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickWebcam, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickKeylogger, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickFileTransfer, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickRemoteShell, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnQuickFileExplorer, 3, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 180);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(287, 30);
            this.tableLayoutPanel2.TabIndex = 32;
            // 
            // button1
            // 
            this.button1.Image = global::Pulsar.Server.Properties.Resources.sound;
            this.button1.Location = new System.Drawing.Point(63, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(24, 24);
            this.button1.TabIndex = 9;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // btnQuickRemoteDesktop
            // 
            this.btnQuickRemoteDesktop.Image = global::Pulsar.Server.Properties.Resources.monitor;
            this.btnQuickRemoteDesktop.Location = new System.Drawing.Point(3, 3);
            this.btnQuickRemoteDesktop.Name = "btnQuickRemoteDesktop";
            this.btnQuickRemoteDesktop.Size = new System.Drawing.Size(24, 24);
            this.btnQuickRemoteDesktop.TabIndex = 4;
            this.btnQuickRemoteDesktop.UseVisualStyleBackColor = true;
            this.btnQuickRemoteDesktop.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnQuickWebcam
            // 
            this.btnQuickWebcam.Image = global::Pulsar.Server.Properties.Resources.webcam;
            this.btnQuickWebcam.Location = new System.Drawing.Point(33, 3);
            this.btnQuickWebcam.Name = "btnQuickWebcam";
            this.btnQuickWebcam.Size = new System.Drawing.Size(24, 24);
            this.btnQuickWebcam.TabIndex = 3;
            this.btnQuickWebcam.UseVisualStyleBackColor = true;
            this.btnQuickWebcam.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnQuickKeylogger
            // 
            this.btnQuickKeylogger.Image = global::Pulsar.Server.Properties.Resources.keyboard_magnify;
            this.btnQuickKeylogger.Location = new System.Drawing.Point(183, 3);
            this.btnQuickKeylogger.Name = "btnQuickKeylogger";
            this.btnQuickKeylogger.Size = new System.Drawing.Size(24, 24);
            this.btnQuickKeylogger.TabIndex = 8;
            this.btnQuickKeylogger.UseVisualStyleBackColor = true;
            this.btnQuickKeylogger.Click += new System.EventHandler(this.button6_Click);
            // 
            // btnQuickFileTransfer
            // 
            this.btnQuickFileTransfer.Image = global::Pulsar.Server.Properties.Resources.drive_go;
            this.btnQuickFileTransfer.Location = new System.Drawing.Point(153, 3);
            this.btnQuickFileTransfer.Name = "btnQuickFileTransfer";
            this.btnQuickFileTransfer.Size = new System.Drawing.Size(24, 24);
            this.btnQuickFileTransfer.TabIndex = 7;
            this.btnQuickFileTransfer.UseVisualStyleBackColor = true;
            this.btnQuickFileTransfer.Click += new System.EventHandler(this.button5_Click);
            // 
            // btnQuickRemoteShell
            // 
            this.btnQuickRemoteShell.Image = global::Pulsar.Server.Properties.Resources.terminal;
            this.btnQuickRemoteShell.Location = new System.Drawing.Point(123, 3);
            this.btnQuickRemoteShell.Name = "btnQuickRemoteShell";
            this.btnQuickRemoteShell.Size = new System.Drawing.Size(24, 24);
            this.btnQuickRemoteShell.TabIndex = 6;
            this.btnQuickRemoteShell.UseVisualStyleBackColor = true;
            this.btnQuickRemoteShell.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnQuickFileExplorer
            // 
            this.btnQuickFileExplorer.Image = global::Pulsar.Server.Properties.Resources.folder;
            this.btnQuickFileExplorer.Location = new System.Drawing.Point(93, 3);
            this.btnQuickFileExplorer.Name = "btnQuickFileExplorer";
            this.btnQuickFileExplorer.Size = new System.Drawing.Size(24, 24);
            this.btnQuickFileExplorer.TabIndex = 5;
            this.btnQuickFileExplorer.UseVisualStyleBackColor = true;
            this.btnQuickFileExplorer.Click += new System.EventHandler(this.button3_Click);
            // 
            // gBoxClientInfo
            // 
            this.gBoxClientInfo.Controls.Add(this.clientInfoListView);
            this.gBoxClientInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gBoxClientInfo.Location = new System.Drawing.Point(2, 212);
            this.gBoxClientInfo.Margin = new System.Windows.Forms.Padding(2);
            this.gBoxClientInfo.Name = "gBoxClientInfo";
            this.gBoxClientInfo.Size = new System.Drawing.Size(283, 137);
            this.gBoxClientInfo.TabIndex = 9;
            this.gBoxClientInfo.TabStop = false;
            this.gBoxClientInfo.Text = "Client Info";
            // 
            // clientInfoListView
            // 
            this.clientInfoListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Names,
            this.Stats});
            this.clientInfoListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.clientInfoListView.FullRowSelect = true;
            this.clientInfoListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.clientInfoListView.HideSelection = false;
            this.clientInfoListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6});
            this.clientInfoListView.Location = new System.Drawing.Point(3, 18);
            listViewColumnSorter2.NeedNumberCompare = false;
            listViewColumnSorter2.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter2.SortColumn = 0;
            this.clientInfoListView.LvwColumnSorter = listViewColumnSorter2;
            this.clientInfoListView.Name = "clientInfoListView";
            this.clientInfoListView.Size = new System.Drawing.Size(277, 116);
            this.clientInfoListView.TabIndex = 0;
            this.clientInfoListView.UseCompatibleStateImageBehavior = false;
            this.clientInfoListView.View = System.Windows.Forms.View.Details;
            // 
            // Names
            // 
            this.Names.Text = "Names";
            this.Names.Width = 122;
            // 
            // Stats
            // 
            this.Stats.Text = "Stats";
            this.Stats.Width = 151;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 351);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1016, 3);
            this.splitter1.TabIndex = 34;
            this.splitter1.TabStop = false;
            this.splitter1.Visible = false;
            // 
            // DebugLogRichBox
            // 
            this.DebugLogRichBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DebugLogRichBox.ContextMenuStrip = this.DebugContextMenuStrip;
            this.DebugLogRichBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.DebugLogRichBox.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.DebugLogRichBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DebugLogRichBox.ForeColor = System.Drawing.SystemColors.InfoText;
            this.DebugLogRichBox.Location = new System.Drawing.Point(0, 354);
            this.DebugLogRichBox.Name = "DebugLogRichBox";
            this.DebugLogRichBox.ReadOnly = true;
            this.DebugLogRichBox.Size = new System.Drawing.Size(1016, 111);
            this.DebugLogRichBox.TabIndex = 35;
            this.DebugLogRichBox.Text = "";
            this.DebugLogRichBox.Visible = false;
            // 
            // DebugContextMenuStrip
            // 
            this.DebugContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveLogsToolStripMenuItem,
            this.saveSlectedToolStripMenuItem,
            this.toolStripSeparator1,
            this.clearLogsToolStripMenuItem});
            this.DebugContextMenuStrip.Name = "DebugContextMenuStrip";
            this.DebugContextMenuStrip.Size = new System.Drawing.Size(146, 76);
            // 
            // saveLogsToolStripMenuItem
            // 
            this.saveLogsToolStripMenuItem.Name = "saveLogsToolStripMenuItem";
            this.saveLogsToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.saveLogsToolStripMenuItem.Text = "Save Logs";
            this.saveLogsToolStripMenuItem.Click += new System.EventHandler(this.saveLogsToolStripMenuItem_Click);
            // 
            // saveSlectedToolStripMenuItem
            // 
            this.saveSlectedToolStripMenuItem.Name = "saveSlectedToolStripMenuItem";
            this.saveSlectedToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.saveSlectedToolStripMenuItem.Text = "Save Selected";
            this.saveSlectedToolStripMenuItem.Click += new System.EventHandler(this.saveSlectedToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(142, 6);
            // 
            // clearLogsToolStripMenuItem
            // 
            this.clearLogsToolStripMenuItem.Name = "clearLogsToolStripMenuItem";
            this.clearLogsToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.clearLogsToolStripMenuItem.Text = "Clear Logs";
            this.clearLogsToolStripMenuItem.Click += new System.EventHandler(this.clearLogsToolStripMenuItem_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstNoti);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(1120, 465);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "NotiTabPage";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // lstNoti
            // 
            this.lstNoti.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader11});
            this.lstNoti.ContextMenuStrip = this.NotificationContextMenuStrip;
            this.lstNoti.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstNoti.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstNoti.FullRowSelect = true;
            this.lstNoti.HideSelection = false;
            this.lstNoti.Location = new System.Drawing.Point(0, 0);
            listViewColumnSorter3.NeedNumberCompare = false;
            listViewColumnSorter3.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter3.SortColumn = 0;
            this.lstNoti.LvwColumnSorter = listViewColumnSorter3;
            this.lstNoti.Margin = new System.Windows.Forms.Padding(0);
            this.lstNoti.Name = "lstNoti";
            this.lstNoti.ShowItemToolTips = true;
            this.lstNoti.Size = new System.Drawing.Size(1120, 465);
            this.lstNoti.SmallImageList = this.imgFlags;
            this.lstNoti.TabIndex = 2;
            this.lstNoti.UseCompatibleStateImageBehavior = false;
            this.lstNoti.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "User@PC";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Time";
            this.columnHeader2.Width = 140;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Event";
            this.columnHeader3.Width = 200;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Parameter";
            this.columnHeader11.Width = 626;
            // 
            // NotificationContextMenuStrip
            // 
            this.NotificationContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addKeywordsToolStripMenuItem,
            this.clearSelectedToolStripMenuItem});
            this.NotificationContextMenuStrip.Name = "NotificationContextMenuStrip";
            this.NotificationContextMenuStrip.Size = new System.Drawing.Size(156, 48);
            // 
            // addKeywordsToolStripMenuItem
            // 
            this.addKeywordsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.add;
            this.addKeywordsToolStripMenuItem.Name = "addKeywordsToolStripMenuItem";
            this.addKeywordsToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.addKeywordsToolStripMenuItem.Text = "Add Key-words";
            this.addKeywordsToolStripMenuItem.Click += new System.EventHandler(this.addKeywordsToolStripMenuItem_Click);
            // 
            // clearSelectedToolStripMenuItem
            // 
            this.clearSelectedToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.delete;
            this.clearSelectedToolStripMenuItem.Name = "clearSelectedToolStripMenuItem";
            this.clearSelectedToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.clearSelectedToolStripMenuItem.Text = "Clear Selected";
            this.clearSelectedToolStripMenuItem.Click += new System.EventHandler(this.clearSelectedToolStripMenuItem_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.cryptoGroupBox);
            this.tabPage3.Controls.Add(this.ClipperCheckbox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1120, 465);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "ClipperTabPage";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // cryptoGroupBox
            // 
            this.cryptoGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cryptoGroupBox.Controls.Add(this.BCHTextBox);
            this.cryptoGroupBox.Controls.Add(this.label10);
            this.cryptoGroupBox.Controls.Add(this.TRXTextBox);
            this.cryptoGroupBox.Controls.Add(this.label9);
            this.cryptoGroupBox.Controls.Add(this.XRPTextBox);
            this.cryptoGroupBox.Controls.Add(this.label8);
            this.cryptoGroupBox.Controls.Add(this.DASHTextBox);
            this.cryptoGroupBox.Controls.Add(this.label7);
            this.cryptoGroupBox.Controls.Add(this.SOLTextBox);
            this.cryptoGroupBox.Controls.Add(this.label6);
            this.cryptoGroupBox.Controls.Add(this.XMRTextBox);
            this.cryptoGroupBox.Controls.Add(this.label5);
            this.cryptoGroupBox.Controls.Add(this.LTCTextBox);
            this.cryptoGroupBox.Controls.Add(this.label4);
            this.cryptoGroupBox.Controls.Add(this.ETHTextBox);
            this.cryptoGroupBox.Controls.Add(this.label3);
            this.cryptoGroupBox.Controls.Add(this.BTCTextBox);
            this.cryptoGroupBox.Controls.Add(this.label2);
            this.cryptoGroupBox.Location = new System.Drawing.Point(8, 29);
            this.cryptoGroupBox.Name = "cryptoGroupBox";
            this.cryptoGroupBox.Size = new System.Drawing.Size(1104, 279);
            this.cryptoGroupBox.TabIndex = 1;
            this.cryptoGroupBox.TabStop = false;
            this.cryptoGroupBox.Text = "Settings";
            this.cryptoGroupBox.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // BCHTextBox
            // 
            this.BCHTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BCHTextBox.Location = new System.Drawing.Point(50, 245);
            this.BCHTextBox.Name = "BCHTextBox";
            this.BCHTextBox.Size = new System.Drawing.Size(1048, 22);
            this.BCHTextBox.TabIndex = 17;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 250);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(32, 13);
            this.label10.TabIndex = 16;
            this.label10.Text = "BCH:";
            // 
            // TRXTextBox
            // 
            this.TRXTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TRXTextBox.Location = new System.Drawing.Point(50, 217);
            this.TRXTextBox.Name = "TRXTextBox";
            this.TRXTextBox.Size = new System.Drawing.Size(1048, 22);
            this.TRXTextBox.TabIndex = 15;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 222);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(28, 13);
            this.label9.TabIndex = 14;
            this.label9.Text = "TRX:";
            // 
            // XRPTextBox
            // 
            this.XRPTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XRPTextBox.Location = new System.Drawing.Point(50, 189);
            this.XRPTextBox.Name = "XRPTextBox";
            this.XRPTextBox.Size = new System.Drawing.Size(1048, 22);
            this.XRPTextBox.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 194);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "XRP:";
            // 
            // DASHTextBox
            // 
            this.DASHTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DASHTextBox.Location = new System.Drawing.Point(50, 161);
            this.DASHTextBox.Name = "DASHTextBox";
            this.DASHTextBox.Size = new System.Drawing.Size(1048, 22);
            this.DASHTextBox.TabIndex = 11;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 166);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(39, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "DASH:";
            // 
            // SOLTextBox
            // 
            this.SOLTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SOLTextBox.Location = new System.Drawing.Point(50, 133);
            this.SOLTextBox.Name = "SOLTextBox";
            this.SOLTextBox.Size = new System.Drawing.Size(1048, 22);
            this.SOLTextBox.TabIndex = 9;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(30, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "SOL:";
            // 
            // XMRTextBox
            // 
            this.XMRTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.XMRTextBox.Location = new System.Drawing.Point(50, 105);
            this.XMRTextBox.Name = "XMRTextBox";
            this.XMRTextBox.Size = new System.Drawing.Size(1048, 22);
            this.XMRTextBox.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 110);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "XMR:";
            // 
            // LTCTextBox
            // 
            this.LTCTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LTCTextBox.Location = new System.Drawing.Point(50, 77);
            this.LTCTextBox.Name = "LTCTextBox";
            this.LTCTextBox.Size = new System.Drawing.Size(1048, 22);
            this.LTCTextBox.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 82);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(25, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "LTC:";
            // 
            // ETHTextBox
            // 
            this.ETHTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ETHTextBox.Location = new System.Drawing.Point(50, 49);
            this.ETHTextBox.Name = "ETHTextBox";
            this.ETHTextBox.Size = new System.Drawing.Size(1048, 22);
            this.ETHTextBox.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "ETH:";
            // 
            // BTCTextBox
            // 
            this.BTCTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BTCTextBox.Location = new System.Drawing.Point(50, 21);
            this.BTCTextBox.Name = "BTCTextBox";
            this.BTCTextBox.Size = new System.Drawing.Size(1048, 22);
            this.BTCTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "BTC:";
            // 
            // ClipperCheckbox
            // 
            this.ClipperCheckbox.AutoSize = true;
            this.ClipperCheckbox.Location = new System.Drawing.Point(8, 6);
            this.ClipperCheckbox.Name = "ClipperCheckbox";
            this.ClipperCheckbox.Size = new System.Drawing.Size(50, 17);
            this.ClipperCheckbox.TabIndex = 0;
            this.ClipperCheckbox.Text = "Start";
            this.ClipperCheckbox.UseVisualStyleBackColor = true;
            this.ClipperCheckbox.CheckedChanged += new System.EventHandler(this.ClipperCheckbox_CheckedChanged);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.lstTasks);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1120, 465);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "AutoTasksTabPage";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // lstTasks
            // 
            this.lstTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader8});
            this.lstTasks.ContextMenuStrip = this.TasksContextMenuStrip;
            this.lstTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTasks.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstTasks.FullRowSelect = true;
            this.lstTasks.HideSelection = false;
            this.lstTasks.Location = new System.Drawing.Point(0, 0);
            listViewColumnSorter4.NeedNumberCompare = false;
            listViewColumnSorter4.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter4.SortColumn = 0;
            this.lstTasks.LvwColumnSorter = listViewColumnSorter4;
            this.lstTasks.Margin = new System.Windows.Forms.Padding(0);
            this.lstTasks.Name = "lstTasks";
            this.lstTasks.ShowItemToolTips = true;
            this.lstTasks.Size = new System.Drawing.Size(1120, 465);
            this.lstTasks.SmallImageList = this.imgFlags;
            this.lstTasks.TabIndex = 3;
            this.lstTasks.UseCompatibleStateImageBehavior = false;
            this.lstTasks.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Task";
            this.columnHeader4.Width = 150;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Parameter 1";
            this.columnHeader5.Width = 450;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Parameter 2";
            this.columnHeader8.Width = 516;
            // 
            // TasksContextMenuStrip
            // 
            this.TasksContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addTaskToolStripMenuItem,
            this.deleteTasksToolStripMenuItem});
            this.TasksContextMenuStrip.Name = "TasksContextMenuStrip";
            this.TasksContextMenuStrip.Size = new System.Drawing.Size(139, 48);
            // 
            // addTaskToolStripMenuItem
            // 
            this.addTaskToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.remoteExecuteToolStripMenuItem1,
            this.shellCommandToolStripMenuItem,
            this.kematianToolStripMenuItem,
            this.showMessageBoxToolStripMenuItem1,
            this.excludeSystemDriveToolStripMenuItem});
            this.addTaskToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.add;
            this.addTaskToolStripMenuItem.Name = "addTaskToolStripMenuItem";
            this.addTaskToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.addTaskToolStripMenuItem.Text = "Add Task";
            // 
            // remoteExecuteToolStripMenuItem1
            // 
            this.remoteExecuteToolStripMenuItem1.Name = "remoteExecuteToolStripMenuItem1";
            this.remoteExecuteToolStripMenuItem1.Size = new System.Drawing.Size(185, 22);
            this.remoteExecuteToolStripMenuItem1.Text = "Remote Execute";
            this.remoteExecuteToolStripMenuItem1.Click += new System.EventHandler(this.remoteExecuteToolStripMenuItem1_Click);
            // 
            // shellCommandToolStripMenuItem
            // 
            this.shellCommandToolStripMenuItem.Name = "shellCommandToolStripMenuItem";
            this.shellCommandToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.shellCommandToolStripMenuItem.Text = "Shell Command";
            this.shellCommandToolStripMenuItem.Click += new System.EventHandler(this.shellCommandToolStripMenuItem_Click);
            // 
            // kematianToolStripMenuItem
            // 
            this.kematianToolStripMenuItem.Name = "kematianToolStripMenuItem";
            this.kematianToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.kematianToolStripMenuItem.Text = "Kematian Recovery";
            this.kematianToolStripMenuItem.Click += new System.EventHandler(this.kematianToolStripMenuItem_Click);
            // 
            // showMessageBoxToolStripMenuItem1
            // 
            this.showMessageBoxToolStripMenuItem1.Name = "showMessageBoxToolStripMenuItem1";
            this.showMessageBoxToolStripMenuItem1.Size = new System.Drawing.Size(185, 22);
            this.showMessageBoxToolStripMenuItem1.Text = "Show Message Box";
            this.showMessageBoxToolStripMenuItem1.Click += new System.EventHandler(this.showMessageBoxToolStripMenuItem1_Click);
            // 
            // excludeSystemDriveToolStripMenuItem
            // 
            this.excludeSystemDriveToolStripMenuItem.Name = "excludeSystemDriveToolStripMenuItem";
            this.excludeSystemDriveToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
            this.excludeSystemDriveToolStripMenuItem.Text = "Exclude System Drive";
            this.excludeSystemDriveToolStripMenuItem.Click += new System.EventHandler(this.excludeSystemDriveToolStripMenuItem_Click);
            // 
            // deleteTasksToolStripMenuItem
            // 
            this.deleteTasksToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.delete;
            this.deleteTasksToolStripMenuItem.Name = "deleteTasksToolStripMenuItem";
            this.deleteTasksToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.deleteTasksToolStripMenuItem.Text = "Delete Task&s";
            this.deleteTasksToolStripMenuItem.Click += new System.EventHandler(this.deleteTasksToolStripMenuItem_Click);
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.splitter2);
            this.tabPage5.Controls.Add(this.dotNetBarTabControl1);
            this.tabPage5.Controls.Add(this.aeroListView1);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1016, 465);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "StealerSorter";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitter2
            // 
            this.splitter2.Location = new System.Drawing.Point(156, 3);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 459);
            this.splitter2.TabIndex = 2;
            this.splitter2.TabStop = false;
            // 
            // dotNetBarTabControl1
            // 
            this.dotNetBarTabControl1.Controls.Add(this.Passwords);
            this.dotNetBarTabControl1.Controls.Add(this.Cookies);
            this.dotNetBarTabControl1.Controls.Add(this.Communication);
            this.dotNetBarTabControl1.Controls.Add(this.Games);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage6);
            this.dotNetBarTabControl1.Controls.Add(this.tabPage7);
            this.dotNetBarTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dotNetBarTabControl1.ItemSize = new System.Drawing.Size(90, 22);
            this.dotNetBarTabControl1.Location = new System.Drawing.Point(156, 3);
            this.dotNetBarTabControl1.Name = "dotNetBarTabControl1";
            this.dotNetBarTabControl1.SelectedIndex = 0;
            this.dotNetBarTabControl1.Size = new System.Drawing.Size(857, 459);
            this.dotNetBarTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.dotNetBarTabControl1.TabIndex = 1;
            // 
            // Passwords
            // 
            this.Passwords.Controls.Add(this.aeroListView2);
            this.Passwords.Location = new System.Drawing.Point(4, 26);
            this.Passwords.Name = "Passwords";
            this.Passwords.Padding = new System.Windows.Forms.Padding(3);
            this.Passwords.Size = new System.Drawing.Size(849, 429);
            this.Passwords.TabIndex = 0;
            this.Passwords.Text = "Passwords";
            this.Passwords.UseVisualStyleBackColor = true;
            // 
            // aeroListView2
            // 
            this.aeroListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader12});
            this.aeroListView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView2.FullRowSelect = true;
            this.aeroListView2.HideSelection = false;
            this.aeroListView2.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter5.NeedNumberCompare = false;
            listViewColumnSorter5.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter5.SortColumn = 0;
            this.aeroListView2.LvwColumnSorter = listViewColumnSorter5;
            this.aeroListView2.Name = "aeroListView2";
            this.aeroListView2.Size = new System.Drawing.Size(843, 423);
            this.aeroListView2.TabIndex = 0;
            this.aeroListView2.UseCompatibleStateImageBehavior = false;
            this.aeroListView2.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Username";
            this.columnHeader9.Width = 227;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Password";
            this.columnHeader10.Width = 266;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Website";
            this.columnHeader12.Width = 346;
            // 
            // Cookies
            // 
            this.Cookies.Controls.Add(this.aeroListView3);
            this.Cookies.Location = new System.Drawing.Point(4, 26);
            this.Cookies.Name = "Cookies";
            this.Cookies.Padding = new System.Windows.Forms.Padding(3);
            this.Cookies.Size = new System.Drawing.Size(953, 429);
            this.Cookies.TabIndex = 1;
            this.Cookies.Text = "Cookies";
            this.Cookies.UseVisualStyleBackColor = true;
            // 
            // aeroListView3
            // 
            this.aeroListView3.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader14,
            this.columnHeader16});
            this.aeroListView3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView3.FullRowSelect = true;
            this.aeroListView3.HideSelection = false;
            this.aeroListView3.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter6.NeedNumberCompare = false;
            listViewColumnSorter6.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter6.SortColumn = 0;
            this.aeroListView3.LvwColumnSorter = listViewColumnSorter6;
            this.aeroListView3.Name = "aeroListView3";
            this.aeroListView3.Size = new System.Drawing.Size(947, 423);
            this.aeroListView3.TabIndex = 1;
            this.aeroListView3.UseCompatibleStateImageBehavior = false;
            this.aeroListView3.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Cookie";
            this.columnHeader14.Width = 498;
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Website";
            this.columnHeader16.Width = 445;
            // 
            // Communication
            // 
            this.Communication.Controls.Add(this.aeroListView4);
            this.Communication.Location = new System.Drawing.Point(4, 26);
            this.Communication.Name = "Communication";
            this.Communication.Padding = new System.Windows.Forms.Padding(3);
            this.Communication.Size = new System.Drawing.Size(849, 429);
            this.Communication.TabIndex = 2;
            this.Communication.Text = "Communication";
            this.Communication.UseVisualStyleBackColor = true;
            // 
            // aeroListView4
            // 
            this.aeroListView4.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader15,
            this.columnHeader17});
            this.aeroListView4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView4.FullRowSelect = true;
            this.aeroListView4.HideSelection = false;
            this.aeroListView4.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter7.NeedNumberCompare = false;
            listViewColumnSorter7.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter7.SortColumn = 0;
            this.aeroListView4.LvwColumnSorter = listViewColumnSorter7;
            this.aeroListView4.Name = "aeroListView4";
            this.aeroListView4.Size = new System.Drawing.Size(843, 423);
            this.aeroListView4.TabIndex = 2;
            this.aeroListView4.UseCompatibleStateImageBehavior = false;
            this.aeroListView4.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = "Communicator";
            this.columnHeader15.Width = 140;
            // 
            // columnHeader17
            // 
            this.columnHeader17.Text = "Token";
            this.columnHeader17.Width = 699;
            // 
            // Games
            // 
            this.Games.Controls.Add(this.aeroListView5);
            this.Games.Location = new System.Drawing.Point(4, 26);
            this.Games.Name = "Games";
            this.Games.Padding = new System.Windows.Forms.Padding(3);
            this.Games.Size = new System.Drawing.Size(849, 429);
            this.Games.TabIndex = 3;
            this.Games.Text = "Games";
            this.Games.UseVisualStyleBackColor = true;
            // 
            // aeroListView5
            // 
            this.aeroListView5.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader7});
            this.aeroListView5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView5.FullRowSelect = true;
            this.aeroListView5.HideSelection = false;
            this.aeroListView5.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter8.NeedNumberCompare = false;
            listViewColumnSorter8.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter8.SortColumn = 0;
            this.aeroListView5.LvwColumnSorter = listViewColumnSorter8;
            this.aeroListView5.Name = "aeroListView5";
            this.aeroListView5.Size = new System.Drawing.Size(843, 423);
            this.aeroListView5.TabIndex = 0;
            this.aeroListView5.UseCompatibleStateImageBehavior = false;
            this.aeroListView5.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Game";
            this.columnHeader7.Width = 839;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.aeroListView6);
            this.tabPage6.Location = new System.Drawing.Point(4, 26);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(849, 429);
            this.tabPage6.TabIndex = 4;
            this.tabPage6.Text = "Wallets";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // aeroListView6
            // 
            this.aeroListView6.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader13});
            this.aeroListView6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aeroListView6.FullRowSelect = true;
            this.aeroListView6.HideSelection = false;
            this.aeroListView6.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter9.NeedNumberCompare = false;
            listViewColumnSorter9.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter9.SortColumn = 0;
            this.aeroListView6.LvwColumnSorter = listViewColumnSorter9;
            this.aeroListView6.Name = "aeroListView6";
            this.aeroListView6.Size = new System.Drawing.Size(843, 423);
            this.aeroListView6.TabIndex = 1;
            this.aeroListView6.UseCompatibleStateImageBehavior = false;
            this.aeroListView6.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = "Wallet";
            this.columnHeader13.Width = 839;
            // 
            // tabPage7
            // 
            this.tabPage7.Location = new System.Drawing.Point(4, 26);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage7.Size = new System.Drawing.Size(849, 429);
            this.tabPage7.TabIndex = 5;
            this.tabPage7.Text = "Other";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // aeroListView1
            // 
            this.aeroListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6});
            this.aeroListView1.Dock = System.Windows.Forms.DockStyle.Left;
            this.aeroListView1.FullRowSelect = true;
            this.aeroListView1.HideSelection = false;
            this.aeroListView1.Location = new System.Drawing.Point(3, 3);
            listViewColumnSorter10.NeedNumberCompare = false;
            listViewColumnSorter10.Order = System.Windows.Forms.SortOrder.None;
            listViewColumnSorter10.SortColumn = 0;
            this.aeroListView1.LvwColumnSorter = listViewColumnSorter10;
            this.aeroListView1.Name = "aeroListView1";
            this.aeroListView1.Size = new System.Drawing.Size(153, 459);
            this.aeroListView1.TabIndex = 0;
            this.aeroListView1.UseCompatibleStateImageBehavior = false;
            this.aeroListView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "User@PC";
            this.columnHeader6.Width = 149;
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.listenToolStripStatusLabel,
            this.connectedToolStripStatusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 516);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1024, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // listenToolStripStatusLabel
            // 
            this.listenToolStripStatusLabel.Image = global::Pulsar.Server.Properties.Resources.bullet_red;
            this.listenToolStripStatusLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.listenToolStripStatusLabel.ImageTransparentColor = System.Drawing.Color.White;
            this.listenToolStripStatusLabel.Name = "listenToolStripStatusLabel";
            this.listenToolStripStatusLabel.Size = new System.Drawing.Size(103, 17);
            this.listenToolStripStatusLabel.Text = "Listening: False";
            // 
            // connectedToolStripStatusLabel
            // 
            this.connectedToolStripStatusLabel.ForeColor = System.Drawing.Color.Black;
            this.connectedToolStripStatusLabel.Image = global::Pulsar.Server.Properties.Resources.status_online;
            this.connectedToolStripStatusLabel.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.connectedToolStripStatusLabel.ImageTransparentColor = System.Drawing.Color.White;
            this.connectedToolStripStatusLabel.Name = "connectedToolStripStatusLabel";
            this.connectedToolStripStatusLabel.Size = new System.Drawing.Size(93, 17);
            this.connectedToolStripStatusLabel.Text = "Connected: 0";
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.menuStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.menuStrip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.menuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clientsToolStripMenuItem,
            this.autoTasksToolStripMenuItem,
            this.cryptoClipperToolStripMenuItem,
            this.notificationCentreToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.builderToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1024, 25);
            this.menuStrip.TabIndex = 2;
            // 
            // clientsToolStripMenuItem
            // 
            this.clientsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.user;
            this.clientsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.clientsToolStripMenuItem.Name = "clientsToolStripMenuItem";
            this.clientsToolStripMenuItem.Size = new System.Drawing.Size(71, 21);
            this.clientsToolStripMenuItem.Text = "Clients";
            this.clientsToolStripMenuItem.Click += new System.EventHandler(this.clientsToolStripMenuItem_Click);
            // 
            // autoTasksToolStripMenuItem
            // 
            this.autoTasksToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.server;
            this.autoTasksToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.autoTasksToolStripMenuItem.Name = "autoTasksToolStripMenuItem";
            this.autoTasksToolStripMenuItem.Size = new System.Drawing.Size(92, 21);
            this.autoTasksToolStripMenuItem.Text = "Auto Tasks";
            this.autoTasksToolStripMenuItem.Click += new System.EventHandler(this.autoTasksToolStripMenuItem_Click);
            // 
            // cryptoClipperToolStripMenuItem
            // 
            this.cryptoClipperToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.money_dollar;
            this.cryptoClipperToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.cryptoClipperToolStripMenuItem.Name = "cryptoClipperToolStripMenuItem";
            this.cryptoClipperToolStripMenuItem.Size = new System.Drawing.Size(112, 21);
            this.cryptoClipperToolStripMenuItem.Text = "Crypto Clipper";
            this.cryptoClipperToolStripMenuItem.Click += new System.EventHandler(this.cryptoClipperToolStripMenuItem_Click);
            // 
            // notificationCentreToolStripMenuItem
            // 
            this.notificationCentreToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.bell;
            this.notificationCentreToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.notificationCentreToolStripMenuItem.Name = "notificationCentreToolStripMenuItem";
            this.notificationCentreToolStripMenuItem.Size = new System.Drawing.Size(136, 21);
            this.notificationCentreToolStripMenuItem.Text = "Notification Centre";
            this.notificationCentreToolStripMenuItem.Click += new System.EventHandler(this.notificationCentreToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
            this.aboutToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.information;
            this.aboutToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(68, 21);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
            this.settingsToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.cog;
            this.settingsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(77, 21);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // builderToolStripMenuItem
            // 
            this.builderToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.builderToolStripMenuItem.ForeColor = System.Drawing.Color.Black;
            this.builderToolStripMenuItem.Image = global::Pulsar.Server.Properties.Resources.bricks;
            this.builderToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.builderToolStripMenuItem.Name = "builderToolStripMenuItem";
            this.builderToolStripMenuItem.Size = new System.Drawing.Size(72, 21);
            this.builderToolStripMenuItem.Text = "Builder";
            this.builderToolStripMenuItem.Click += new System.EventHandler(this.builderToolStripMenuItem_Click);
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1024, 538);
            this.Controls.Add(this.tableLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(680, 415);
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pulsar Continued - Connected: 0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.contextMenuStrip.ResumeLayout(false);
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxMain)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.gBoxClientInfo.ResumeLayout(false);
            this.DebugContextMenuStrip.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.NotificationContextMenuStrip.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.cryptoGroupBox.ResumeLayout(false);
            this.cryptoGroupBox.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.TasksContextMenuStrip.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.dotNetBarTabControl1.ResumeLayout(false);
            this.Passwords.ResumeLayout(false);
            this.Cookies.ResumeLayout(false);
            this.Communication.ResumeLayout(false);
            this.Games.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem connectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ImageList imgFlags;
        private System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem uninstallToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem surveillanceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem taskManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemInformationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem passwordRecoveryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remoteShellToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator ctxtLine;
        private System.Windows.Forms.ToolStripMenuItem actionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shutdownToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem standbyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem startupManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keyloggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reverseProxyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem registryEditorToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ToolStripSeparator lineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addFakeClientsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeFakeClientsToolStripMenuItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem builderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        public System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel connectedToolStripStatusLabel;
        private System.Windows.Forms.ToolStripMenuItem connectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem userSupportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showMessageboxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visitWebsiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remoteExecuteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem localFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem remoteDesktopToolStripMenuItem2;
        private ToolStripMenuItem kematianGrabbingToolStripMenuItem;
        private ToolStripMenuItem funMethodsToolStripMenuItem;
        private ToolStripMenuItem bSODToolStripMenuItem;
        private ToolStripMenuItem cWToolStripMenuItem;
        private ToolStripMenuItem swapMouseButtonsToolStripMenuItem;
        private ToolStripMenuItem gdiToolStripMenuItem;
        private ToolStripMenuItem pixelCorruptToolStripMenuItem;
        private ToolStripMenuItem hVNCToolStripMenuItem;
        private ToolStripMenuItem webcamToolStripMenuItem;
        private ToolStripMenuItem hideTaskBarToolStripMenuItem;
        private ToolStripStatusLabel listenToolStripStatusLabel;
        private ToolStripMenuItem quickCommandsToolStripMenuItem;
        private ToolStripMenuItem addCDriveExceptionToolStripMenuItem;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private AeroListView lstNoti;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader11;
        private ContextMenuStrip NotificationContextMenuStrip;
        private ToolStripMenuItem addKeywordsToolStripMenuItem;
        private ToolStripMenuItem clearSelectedToolStripMenuItem;
        private ToolStripMenuItem clientsToolStripMenuItem;
        private ToolStripMenuItem notificationCentreToolStripMenuItem;
        private NoButtonTabControl MainTabControl;
        private TabPage tabPage3;
        private TabPage tabPage4;
        private AeroListView lstTasks;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
        private ColumnHeader columnHeader8;
        private ContextMenuStrip TasksContextMenuStrip;
        private ToolStripMenuItem addTaskToolStripMenuItem;
        private ToolStripMenuItem remoteExecuteToolStripMenuItem1;
        private ToolStripMenuItem shellCommandToolStripMenuItem;
        private ToolStripMenuItem kematianToolStripMenuItem;
        private ToolStripMenuItem showMessageBoxToolStripMenuItem1;
        private ToolStripMenuItem excludeSystemDriveToolStripMenuItem;
        private ToolStripMenuItem deleteTasksToolStripMenuItem;
        private ToolStripMenuItem autoTasksToolStripMenuItem;
        private ToolStripMenuItem cryptoClipperToolStripMenuItem;
        private GroupBox cryptoGroupBox;
        private TextBox XMRTextBox;
        private Label label5;
        private TextBox LTCTextBox;
        private Label label4;
        private TextBox ETHTextBox;
        private Label label3;
        private TextBox BTCTextBox;
        private Label label2;
        public CheckBox ClipperCheckbox;
        private TextBox SOLTextBox;
        private Label label6;
        private TextBox BCHTextBox;
        private Label label10;
        private TextBox TRXTextBox;
        private Label label9;
        private TextBox XRPTextBox;
        private Label label8;
        private TextBox DASHTextBox;
        private Label label7;
        private Splitter splitter1;
        private RichTextBox DebugLogRichBox;
        private ContextMenuStrip DebugContextMenuStrip;
        private ToolStripMenuItem saveLogsToolStripMenuItem;
        private ToolStripMenuItem saveSlectedToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem clearLogsToolStripMenuItem;
        private ToolStripMenuItem nicknameToolStripMenuItem;
        private ToolStripMenuItem remoteScriptingToolStripMenuItem;
        private ToolStripMenuItem audioToolStripMenuItem;
        private ToolStripMenuItem installVirtualMonitorToolStripMenuItem;
        private ToolStripMenuItem elevatedToolStripMenuItem;
        private ToolStripMenuItem elevateClientPermissionsToolStripMenuItem;
        private ToolStripMenuItem elevateToSystemToolStripMenuItem;
        private ToolStripMenuItem deElevateFromSystemToolStripMenuItem;
        private AeroListView lstClients;
        private ColumnHeader hIP;
        private ColumnHeader hNick;
        private ColumnHeader hTag;
        private ColumnHeader hUserPC;
        private ColumnHeader hVersion;
        private ColumnHeader hStatus;
        private ColumnHeader hCurrentWindow;
        private ColumnHeader hUserStatus;
        private ColumnHeader hCountry;
        private ColumnHeader hOS;
        private ColumnHeader hAccountType;
        private TableLayoutPanel tableLayoutPanel1;
        private Label label1;
        private PictureBox pictureBoxMain;
        private GroupBox gBoxClientInfo;
        private AeroListView clientInfoListView;
        private ColumnHeader Names;
        private ColumnHeader Stats;
        private TableLayoutPanel tableLayoutPanel2;
        private Button button1;
        private Button btnQuickRemoteDesktop;
        private Button btnQuickWebcam;
        private Button btnQuickKeylogger;
        private Button btnQuickFileTransfer;
        private Button btnQuickRemoteShell;
        private Button btnQuickFileExplorer;
        private TabPage tabPage5;
        private AeroListView aeroListView1;
        private DotNetBarTabControl dotNetBarTabControl1;
        private TabPage Passwords;
        private TabPage Cookies;
        private ColumnHeader columnHeader6;
        private Splitter splitter2;
        private AeroListView aeroListView2;
        private ColumnHeader columnHeader9;
        private ColumnHeader columnHeader10;
        private ColumnHeader columnHeader12;
        private AeroListView aeroListView3;
        private ColumnHeader columnHeader14;
        private ColumnHeader columnHeader16;
        private TabPage Communication;
        private AeroListView aeroListView4;
        private ColumnHeader columnHeader15;
        private ColumnHeader columnHeader17;
        private TabPage Games;
        private AeroListView aeroListView5;
        private ColumnHeader columnHeader7;
        private TabPage tabPage6;
        private AeroListView aeroListView6;
        private ColumnHeader columnHeader13;
        private TabPage tabPage7;
    }
}
