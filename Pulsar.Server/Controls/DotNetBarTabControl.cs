using Pulsar.Server.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// thanks to Mavamaarten~ for coding this

namespace Pulsar.Server.Controls
{
    public class TabPageEventArgs : EventArgs
    {
        public TabPage TabPage { get; }
        public TabPageEventArgs(TabPage tabPage)
        {
            TabPage = tabPage;
        }
    }
    internal class DotNetBarTabControl : TabControl
    {
        private bool _darkMode = Settings.DarkMode;
        private Dictionary<int, Rectangle> _closeButtonRects = new Dictionary<int, Rectangle>();
        private bool _showCloseButtons = true;

        public event EventHandler<TabPageEventArgs> TabClosed;

        /// <summary>
        /// Gets or sets whether close buttons are shown on tabs.
        /// </summary>
        public bool ShowCloseButtons
        {
            get { return _showCloseButtons; }
            set
            {
                if (_showCloseButtons != value)
                {
                    _showCloseButtons = value;
                    Invalidate();
                }
            }
        }
        public DotNetBarTabControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer, true);
            SizeMode = TabSizeMode.Fixed;
            SelectedIndex = 0;
            ShowCloseButtons = false;

            MouseClick += DotNetBarTabControl_MouseClick;
        }

        private void DotNetBarTabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (!_showCloseButtons)
                return;
            foreach (var kvp in _closeButtonRects)
            {
                if (kvp.Value.Contains(e.Location))
                {
                    int tabIndex = kvp.Key;
                    if (tabIndex >= 0 && tabIndex < TabCount)
                    {
                        TabPage tabPage = TabPages[tabIndex];

                        TabPages.Remove(tabPage);

                        TabClosed?.Invoke(this, new TabPageEventArgs(tabPage));

                        Invalidate();
                    }
                    break;
                }
            }
        }


        private void DrawTabText(Graphics g, Rectangle rect, string text, Font baseFont, Brush textBrush, bool isSelected)
        {

            int maxTextWidth = rect.Width - 20; 


            if (_showCloseButtons)
                maxTextWidth -= 20; 


            SizeF textSize = g.MeasureString(text, baseFont);


            Font fontToUse = baseFont;
            float scaleFactor = 1.0f;

            if (textSize.Width > maxTextWidth)
            {
                scaleFactor = maxTextWidth / textSize.Width;
                float newSize = Math.Max(baseFont.Size * scaleFactor, 7.0f); 
                fontToUse = new Font(baseFont.FontFamily, newSize, isSelected ? FontStyle.Bold : FontStyle.Regular);
            }
            else if (isSelected && baseFont.Style != FontStyle.Bold)
            {

                fontToUse = new Font(baseFont.FontFamily, baseFont.Size, FontStyle.Bold);
            }


            g.DrawString(text, fontToUse, textBrush, rect, new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            });


            if (fontToUse != baseFont)
            {
                fontToUse.Dispose();
            }
        }


        private void DrawCloseButton(Graphics g, Rectangle tabRect, int tabIndex, bool isSelected)
        {

            if (!_showCloseButtons)
                return;


            int buttonSize = 16;
            int buttonX = tabRect.Right - buttonSize - 5;
            int buttonY = tabRect.Top + (tabRect.Height - buttonSize) / 2;

            Rectangle closeRect = new Rectangle(buttonX, buttonY, buttonSize, buttonSize);


            _closeButtonRects[tabIndex] = closeRect;


            Color bgColor = _darkMode 
                ? (isSelected ? Color.FromArgb(90, 90, 90) : Color.FromArgb(60, 60, 60))
                : (isSelected ? Color.FromArgb(240, 240, 250) : Color.FromArgb(220, 220, 240));

            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                g.FillEllipse(bgBrush, closeRect);
            }


            Color xColor = _darkMode 
                ? Color.FromArgb(200, 200, 200)
                : Color.FromArgb(100, 100, 100);

            using (Pen xPen = new Pen(xColor, 1.5f))
            {

                g.DrawLine(xPen, 
                    closeRect.Left + 4, closeRect.Top + 4, 
                    closeRect.Right - 4, closeRect.Bottom - 4);

                g.DrawLine(xPen, 
                    closeRect.Left + 4, closeRect.Bottom - 4, 
                    closeRect.Right - 4, closeRect.Top + 4);
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {

            _closeButtonRects.Clear();

            Bitmap b = new Bitmap(Width, Height);
            Graphics g = Graphics.FromImage(b);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            if (_darkMode)
            {
                DrawDarkMode(g);
            }
            else
            {
                DrawLightMode(g);
            }
            e.Graphics.DrawImage(b, new Point(0, 0));
            g.Dispose();
            b.Dispose();
        }
        private void DrawDarkMode(Graphics g)
        {
            if (!DesignMode && TabCount > 0 && SelectedIndex >= 0)
                SelectedTab.BackColor = Color.FromArgb(43, 43, 43);
            g.Clear(Color.FromArgb(43, 43, 43));
            g.FillRectangle(new SolidBrush(Color.FromArgb(43, 43, 43)),
                new Rectangle(0, 0, ItemSize.Height + 4, Height));
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), new Point(ItemSize.Height + 3, 0),
                new Point(ItemSize.Height + 3, 999));
            g.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), new Point(0, Size.Height - 1),
                new Point(Width + 3, Size.Height - 1));
            for (int i = 0; i <= TabCount - 1; i++)
            {
                if (i == SelectedIndex)
                {
                    Rectangle x2 = new Rectangle(new Point(GetTabRect(i).Location.X - 2, GetTabRect(i).Location.Y - 2),
                        new Size(GetTabRect(i).Width + 3, GetTabRect(i).Height - 1));
                    g.FillRectangle(new SolidBrush(Color.FromArgb(70, 70, 70)), x2);
                    g.DrawRectangle(new Pen(Color.FromArgb(43, 43, 43)), x2);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    if (ImageList != null)
                    {
                        try
                        {
                            g.DrawImage(ImageList.Images[TabPages[i].ImageIndex],
                                new Point(x2.Location.X + 8, x2.Location.Y + 6));


                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.White, true);
                        }
                        catch (Exception)
                        {

                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.White, true);
                        }
                    }
                    else
                    {

                        DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.White, true);
                    }


                    DrawCloseButton(g, x2, i, true);
                }
                else
                {
                    Rectangle x2 = new Rectangle(new Point(GetTabRect(i).Location.X - 2, GetTabRect(i).Location.Y - 2),
                        new Size(GetTabRect(i).Width + 3, GetTabRect(i).Height - 1));
                    g.FillRectangle(new SolidBrush(Color.FromArgb(43, 43, 43)), x2);
                    g.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), new Point(x2.Right, x2.Top),
                        new Point(x2.Right, x2.Bottom));
                    if (ImageList != null)
                    {
                        try
                        {
                            g.DrawImage(ImageList.Images[TabPages[i].ImageIndex],
                                new Point(x2.Location.X + 8, x2.Location.Y + 6));


                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.LightGray, false);
                        }
                        catch (Exception)
                        {

                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.LightGray, false);
                        }
                    }
                    else
                    {

                        DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.LightGray, false);
                    }


                    DrawCloseButton(g, x2, i, false);
                }
            }
        }
        private void DrawLightMode(Graphics g)
        {
            if (!DesignMode && TabCount > 0 && SelectedIndex >= 0)
                SelectedTab.BackColor = SystemColors.Control;

            g.Clear(SystemColors.Control);
            g.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 252)),
                new Rectangle(0, 0, ItemSize.Height + 4, Height));
            g.DrawLine(new Pen(Color.FromArgb(170, 187, 204)), new Point(ItemSize.Height + 3, 0),
                new Point(ItemSize.Height + 3, 999));
            g.DrawLine(new Pen(Color.FromArgb(170, 187, 204)), new Point(0, Size.Height - 1),
                new Point(Width + 3, Size.Height - 1));

            for (int i = 0; i <= TabCount - 1; i++)
            {
                if (i == SelectedIndex)
                {
                    Rectangle x2 = new Rectangle(new Point(GetTabRect(i).Location.X - 2, GetTabRect(i).Location.Y - 2),
                        new Size(GetTabRect(i).Width + 3, GetTabRect(i).Height - 1));
                    ColorBlend myBlend = new ColorBlend();
                    myBlend.Colors = new Color[] { Color.FromArgb(232, 232, 240), Color.FromArgb(232, 232, 240), Color.FromArgb(232, 232, 240) };
                    myBlend.Positions = new float[] { 0f, 0.5f, 1f };
                    LinearGradientBrush lgBrush = new LinearGradientBrush(x2, Color.Black, Color.Black, 90f);
                    lgBrush.InterpolationColors = myBlend;
                    g.FillRectangle(lgBrush, x2);
                    g.DrawRectangle(new Pen(Color.FromArgb(170, 187, 204)), x2);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    Point[] p =
                    {
                        new Point(ItemSize.Height - 3, GetTabRect(i).Location.Y + 20),
                        new Point(ItemSize.Height + 4, GetTabRect(i).Location.Y + 14),
                        new Point(ItemSize.Height + 4, GetTabRect(i).Location.Y + 27)
                    };
                    g.FillPolygon(SystemBrushes.Control, p);
                    g.DrawPolygon(new Pen(Color.FromArgb(170, 187, 204)), p);
                    if (ImageList != null)
                    {
                        try
                        {
                            g.DrawImage(ImageList.Images[TabPages[i].ImageIndex],
                                new Point(x2.Location.X + 8, x2.Location.Y + 6));


                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.Black, true);
                        }
                        catch (Exception)
                        {

                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.Black, true);
                        }
                    }
                    else
                    {

                        DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.Black, true);
                    }
                    g.DrawLine(new Pen(Color.FromArgb(200, 200, 250)), new Point(x2.Location.X - 1, x2.Location.Y - 1),
                        new Point(x2.Location.X, x2.Location.Y));
                    g.DrawLine(new Pen(Color.FromArgb(200, 200, 250)), new Point(x2.Location.X - 1, x2.Bottom - 1),
                        new Point(x2.Location.X, x2.Bottom));


                    DrawCloseButton(g, x2, i, true);
                }
                else
                {
                    Rectangle x2 = new Rectangle(new Point(GetTabRect(i).Location.X - 2, GetTabRect(i).Location.Y - 2),
                        new Size(GetTabRect(i).Width + 3, GetTabRect(i).Height - 1));
                    g.FillRectangle(new SolidBrush(Color.FromArgb(246, 248, 252)), x2);
                    g.DrawLine(new Pen(Color.FromArgb(170, 187, 204)), new Point(x2.Right, x2.Top),
                        new Point(x2.Right, x2.Bottom));

                    if (ImageList != null)
                    {
                        try
                        {
                            g.DrawImage(ImageList.Images[TabPages[i].ImageIndex],
                                new Point(x2.Location.X + 8, x2.Location.Y + 6));


                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.DimGray, false);
                        }
                        catch (Exception)
                        {

                            DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.DimGray, false);
                        }
                    }
                    else
                    {

                        DrawTabText(g, x2, TabPages[i].Text, Font, Brushes.DimGray, false);
                    }


                    DrawCloseButton(g, x2, i, false);
                }
            }
        }
    }
}
