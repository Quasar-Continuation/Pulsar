using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel.Design;

namespace Pulsar.Server.Controls
{
    [DefaultProperty("Value")]
    [DefaultEvent("ValueChanged")]
    [ToolboxItem(true)]
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public class BetterProgressBar : Control
    {
        private int _value;
        private int _maximum = 100;
        private int _minimum = 0;
        private int _borderThickness = 1;
        private int _cornerRadius = 5;
        private Color _progressColor = Color.FromArgb(104, 104, 255);
        private Color _borderColor = Color.FromArgb(200, 200, 200);
        private bool _showPercentage = true;
        private Font _textFont;

        public event EventHandler ValueChanged;

        public BetterProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            _textFont = new Font("Segoe UI", 9f);
            Size = new Size(200, 20);
            BackColor = Color.White;
        }

        [Category("Appearance")]
        [Description("The current value of the progress bar")]
        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                if (value < _minimum)
                    value = _minimum;
                else if (value > _maximum)
                    value = _maximum;

                if (_value != value)
                {
                    _value = value;
                    OnValueChanged(EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The maximum value of the progress bar")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value < _minimum)
                    value = _minimum;

                if (_maximum != value)
                {
                    _maximum = value;
                    if (_value > _maximum)
                    {
                        _value = _maximum;
                        OnValueChanged(EventArgs.Empty);
                    }
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The minimum value of the progress bar")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value > _maximum)
                    value = _maximum;

                if (_minimum != value)
                {
                    _minimum = value;
                    if (_value < _minimum)
                    {
                        _value = _minimum;
                        OnValueChanged(EventArgs.Empty);
                    }
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The border thickness of the progress bar")]
        [DefaultValue(1)]
        public int BorderThickness
        {
            get => _borderThickness;
            set
            {
                if (_borderThickness != value)
                {
                    _borderThickness = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The corner radius of the progress bar")]
        [DefaultValue(5)]
        public int CornerRadius
        {
            get => _cornerRadius;
            set
            {
                if (_cornerRadius != value)
                {
                    _cornerRadius = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The color of the progress fill")]
        public Color ProgressColor
        {
            get => _progressColor;
            set
            {
                if (_progressColor != value)
                {
                    _progressColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("The color of the border")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    Invalidate();
                }
            }
        }

        [Category("Appearance")]
        [Description("Whether to show the percentage text")]
        [DefaultValue(true)]
        public bool ShowPercentage
        {
            get => _showPercentage;
            set
            {
                if (_showPercentage != value)
                {
                    _showPercentage = value;
                    Invalidate();
                }
            }
        }

        [Browsable(false)]
        public float ProgressPercentage
        {
            get
            {
                if (_maximum == _minimum)
                    return 0;
                return (float)(_value - _minimum) / (_maximum - _minimum) * 100f;
            }
        }
        
        protected virtual void OnValueChanged(EventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if (Width == 0 || Height == 0)
                return;
                
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            try
            {
                using (var backPath = GetRoundedRectangle(0, 0, Width - 1, Height - 1, _cornerRadius))
                using (var backBrush = new SolidBrush(BackColor))
                using (var borderPen = new Pen(_borderColor, _borderThickness))
                {
                    e.Graphics.FillPath(backBrush, backPath);
                    e.Graphics.DrawPath(borderPen, backPath);
                }

                int progressWidth = (int)((Width - 2 * _borderThickness) * (ProgressPercentage / 100f));
                if (progressWidth > 0)
                {
                    Rectangle progressRect = new Rectangle(
                        _borderThickness, 
                        _borderThickness, 
                        progressWidth, 
                        Height - 2 * _borderThickness);

                    int progressRadius = Math.Min(_cornerRadius - _borderThickness, _cornerRadius);
                    progressRadius = Math.Max(0, progressRadius);

                    using (var progressPath = GetRoundedRectangle(
                        progressRect.X, 
                        progressRect.Y, 
                        progressRect.Width, 
                        progressRect.Height, 
                        progressRadius))
                    using (var progressBrush = new SolidBrush(_progressColor))
                    {
                        e.Graphics.FillPath(progressBrush, progressPath);
                    }
                }

                if (_showPercentage && _textFont != null)
                {
                    string percentText = $"{(int)ProgressPercentage}%";
                    using (var textBrush = new SolidBrush(ForeColor))
                    {
                        var textSize = e.Graphics.MeasureString(percentText, _textFont);
                        float textX = (Width - textSize.Width) / 2;
                        float textY = (Height - textSize.Height) / 2;
                        e.Graphics.DrawString(percentText, _textFont, textBrush, textX, textY);
                    }
                }
            }
            catch (Exception)
            {
                if (!DesignMode)
                    throw;
            }
        }

        private GraphicsPath GetRoundedRectangle(float x, float y, float width, float height, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            if (radius <= 0)
            {
                path.AddRectangle(new RectangleF(x, y, width, height));
                return path;
            }

            radius = Math.Min(radius, Math.Min(width, height) / 2);
            float diameter = radius * 2;

            path.AddArc(x, y, diameter, diameter, 180, 90);
            path.AddLine(x + radius, y, x + width - radius, y);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            path.AddLine(x + width, y + radius, x + width, y + height - radius);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            path.AddLine(x + width - radius, y + height, x + radius, y + height);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            path.AddLine(x, y + height - radius, x, y + radius);

            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _textFont?.Dispose();
                _textFont = null;
            }
            base.Dispose(disposing);
        }
    }
} 