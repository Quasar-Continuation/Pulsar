using Quasar.Server.Models;
using System.Drawing;
using System.Windows.Forms;

namespace Quasar.Server.Controls
{
    public class Line : Control
    {
        public enum Alignment
        {
            Horizontal,
            Vertical
        }

        public Alignment LineAlignment { get; set; }

        public Line()
        {
            this.TabStop = false;
            this.BackColor = GetBackgroundColor();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawLine(new Pen(new SolidBrush(Color.LightGray)), new Point(5, 5),
                LineAlignment == Alignment.Horizontal ? new Point(500, 5) : new Point(5, 500));
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (var brush = new SolidBrush(GetBackgroundColor()))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private Color GetBackgroundColor()
        {
            if (Settings.DarkMode)
            {
                return Color.FromArgb(43, 43, 43);
            }
            else
            {
                return this.BackColor;
            }
        }
    }
}