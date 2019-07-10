using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignColors.ColorManipulation;

namespace MaterialDesignThemes.Wpf
{
    [TemplatePart(Name = HueSliderPartName, Type = typeof(Slider))]
    [TemplatePart(Name = SaturationBrightnessPickerPartName, Type = typeof(Canvas))]
    [TemplatePart(Name = SaturationBrightnessPickerThumbPartName, Type = typeof(Thumb))]
    public class ColorPicker : Control
    {
        public const string HueSliderPartName = "PART_HueSlider";
        public const string SaturationBrightnessPickerPartName = "PART_SaturationBrightnessPicker";
        public const string SaturationBrightnessPickerThumbPartName = "PART_SaturationBrightnessPickerThumb";

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        private Thumb _saturationBrightnessThumb;
        private Canvas _saturationBrightnessCanvas;
        private Slider _hueSlider;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Color), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ColorPropertyChangedCallback));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        private static void ColorPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            if (colorPicker._inCallback)
            {
                return;
            }
            
            colorPicker._inCallback = true;
            var color = (Color) e.NewValue;
            colorPicker.SetCurrentValue(HsbProperty, color.ToHsb());
            colorPicker.SetCurrentValue(AlphaProperty, color.A);
            SetAlphaSliderBackgroundBrush(colorPicker, color);
            colorPicker._inCallback = false;
        }

        internal static readonly DependencyProperty HsbProperty = DependencyProperty.Register(nameof(Hsb), typeof(Hsb), typeof(ColorPicker),
            new FrameworkPropertyMetadata(default(Hsb), FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HsbPropertyChangedCallback));

        internal Hsb Hsb
        {
            get => (Hsb)GetValue(HsbProperty);
            set => SetValue(HsbProperty, value);
        }

        private bool _inCallback;
        private static void HsbPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            if (colorPicker._inCallback)
            {
                return;
            }

            colorPicker._inCallback = true;

            var color = default(Color);
            if (e.NewValue is Hsb hsb)
            {
                color = hsb.ToColor();
            }
            colorPicker.SetCurrentValue(ColorProperty, color);
            SetAlphaSliderBackgroundBrush(colorPicker, color);

            colorPicker._inCallback = false;
        }

        public static readonly DependencyProperty HueSliderPositionProperty = DependencyProperty.Register(
            nameof(HueSliderPosition), typeof(Dock), typeof(ColorPicker), new PropertyMetadata(Dock.Right));

        public Dock HueSliderPosition
        {
            get => (Dock)GetValue(HueSliderPositionProperty);
            set => SetValue(HueSliderPositionProperty, value);
        }

        public static readonly DependencyProperty AlphaSliderPositionProperty = DependencyProperty.Register(
            nameof(AlphaSliderPosition), typeof(Dock), typeof(ColorPicker), new PropertyMetadata(Dock.Right));

        public Dock AlphaSliderPosition
        {
            get => (Dock)GetValue(AlphaSliderPositionProperty);
            set => SetValue(AlphaSliderPositionProperty, value);
        }

        #region AlphaSliderBackgroundBrush Property
        public static readonly DependencyProperty AlphaSliderBackgroundBrushProperty = DependencyProperty.Register("AlphaSliderBackgroundBrush", typeof(LinearGradientBrush), typeof(ColorPicker));

        public LinearGradientBrush AlphaSliderBackgroundBrush
        {
            get => (LinearGradientBrush)GetValue(AlphaSliderBackgroundBrushProperty);
            set => SetValue(AlphaSliderBackgroundBrushProperty, value);
        }
        #endregion

        #region Alpha Property
        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register(nameof(Alpha), typeof(byte), typeof(ColorPicker), new FrameworkPropertyMetadata((byte)255, AlphaPropertyChangedCallback));

        public byte Alpha
        {
            get => (byte)GetValue(AlphaProperty);
            set => SetValue(AlphaProperty, value);
        }

        private static void AlphaPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            if (colorPicker._inCallback) return;
            if (!(e.NewValue is byte newValue)) return;
            UpdateAlpha(colorPicker, newValue);
        }

        #endregion

        #region ShowAlphaSlider Property
        public static readonly DependencyProperty ShowAlphaSliderProperty =
            DependencyProperty.Register(nameof(ShowAlphaSlider), typeof(bool), typeof(ColorPicker), new FrameworkPropertyMetadata(false, ShowAlphaSliderPropertyChangedCallback));

        public bool ShowAlphaSlider
        {
            get => (bool)GetValue(ShowAlphaSliderProperty);
            set => SetValue(ShowAlphaSliderProperty, value);
        }

        private static void ShowAlphaSliderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var colorPicker = (ColorPicker)d;
            //if (colorPicker._inCallback) return;
            //if (!(e.NewValue is byte newValue)) return;
            //UpdateAlpha(colorPicker, newValue);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            if (_saturationBrightnessCanvas != null)
            {
                _saturationBrightnessCanvas.MouseDown -= SaturationBrightnessCanvasMouseDown;
                _saturationBrightnessCanvas.MouseMove -= SaturationBrightnessCanvasMouseMove;
                _saturationBrightnessCanvas.MouseUp -= SaturationBrightnessCanvasMouseUp;
            }
            _saturationBrightnessCanvas = GetTemplateChild(SaturationBrightnessPickerPartName) as Canvas;
            if (_saturationBrightnessCanvas != null)
            {
                _saturationBrightnessCanvas.MouseDown += SaturationBrightnessCanvasMouseDown;
                _saturationBrightnessCanvas.MouseMove += SaturationBrightnessCanvasMouseMove;
                _saturationBrightnessCanvas.MouseUp += SaturationBrightnessCanvasMouseUp;
            }

            if (_saturationBrightnessThumb != null) _saturationBrightnessThumb.DragDelta -= SaturationBrightnessThumbDragDelta;
            _saturationBrightnessThumb = GetTemplateChild(SaturationBrightnessPickerThumbPartName) as Thumb;
            if (_saturationBrightnessThumb != null) _saturationBrightnessThumb.DragDelta += SaturationBrightnessThumbDragDelta;

            if (_hueSlider != null)
            {
                _hueSlider.ValueChanged -= HueSliderOnValueChanged;
            }
            _hueSlider = GetTemplateChild(HueSliderPartName) as Slider;
            if (_hueSlider != null)
            {
                _hueSlider.ValueChanged += HueSliderOnValueChanged;
            }
            
            //if (AlphaSliderBackgroundBrush == null)
            //{
            //    // Set for horizontal display
            //    //AlphaSliderBackgroundBrush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 2.5) };

            //    // Set for vertical display
            //    AlphaSliderBackgroundBrush = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1) };


            //    var color = Colors.Black;
            //    color.A = 0;
            //    AlphaSliderBackgroundBrush.GradientStops.Add(new GradientStop(color, 0.0));
            //    color.A = 255;
            //    AlphaSliderBackgroundBrush.GradientStops.Add(new GradientStop(color, 1.0));
            //}

            base.OnApplyTemplate();
        }

        private void HueSliderOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Hsb is Hsb hsb)
            {
                Hsb = new Hsb(e.NewValue, hsb.Saturation, hsb.Brightness);
            }
        }

        private void SaturationBrightnessCanvasMouseDown(object sender, MouseButtonEventArgs e)
        {
            _saturationBrightnessThumb.CaptureMouse();
        }

        private void SaturationBrightnessCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var position = e.GetPosition(_saturationBrightnessCanvas);
                ApplyThumbPosition(position.X, position.Y);
            }
        }

        private void SaturationBrightnessCanvasMouseUp(object sender, MouseButtonEventArgs e)
        {
            _saturationBrightnessThumb.ReleaseMouseCapture();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var result = base.ArrangeOverride(arrangeBounds);
            SetThumbLeft();
            SetThumbTop();
            return result;
        }

        private void SaturationBrightnessThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            var thumb = (UIElement)e.Source;

            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;
            ApplyThumbPosition(left, top);
        }

        private void ApplyThumbPosition(double left, double top)
        {
            if (left < 0) left = 0;
            if (top < 0) top = 0;

            if (left > _saturationBrightnessCanvas.ActualWidth) left = _saturationBrightnessCanvas.ActualWidth;
            if (top > _saturationBrightnessCanvas.ActualHeight) top = _saturationBrightnessCanvas.ActualHeight;

            var saturation = 1 / (_saturationBrightnessCanvas.ActualWidth / left);
            var brightness = 1 - (top / _saturationBrightnessCanvas.ActualHeight);

            SetCurrentValue(HsbProperty, new Hsb(Hsb.Hue, saturation, brightness));
        }

        private void SetThumbLeft()
        {
            if (_saturationBrightnessCanvas == null) return;
            var left = (_saturationBrightnessCanvas.ActualWidth) / (1 / Hsb.Saturation);
            Canvas.SetLeft(_saturationBrightnessThumb, left);
        }

        private void SetThumbTop()
        {
            if (_saturationBrightnessCanvas == null) return;
            var top = ((1 - Hsb.Brightness) * _saturationBrightnessCanvas.ActualHeight);
            Canvas.SetTop(_saturationBrightnessThumb, top);
        }

        private static void UpdateAlpha(ColorPicker colorPicker, byte newValue)
        {
            colorPicker.SetCurrentValue(ColorProperty, AddTransparency(newValue, colorPicker.Color));
        }

        private static Color AddTransparency(byte alpha, Color color)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static void SetAlphaSliderBackgroundBrush(ColorPicker colorPicker, Color color)
        {
            if (colorPicker.AlphaSliderBackgroundBrush == null)
            {
                // Set for horizontal display
                //AlphaSliderBackgroundBrush = new LinearGradientBrush { StartPoint = new Point(0, 0.5), EndPoint = new Point(1, 2.5) };

                // Set for vertical display
                colorPicker.AlphaSliderBackgroundBrush = new LinearGradientBrush { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1) };

                color.A = 0;
                colorPicker.AlphaSliderBackgroundBrush.GradientStops.Add(new GradientStop(color, 0.0));
                color.A = 255;
                colorPicker.AlphaSliderBackgroundBrush.GradientStops.Add(new GradientStop(color, 1.0));
            }

            var alphaBrush = colorPicker.AlphaSliderBackgroundBrush;
            if (alphaBrush.IsFrozen)
            {
                alphaBrush = alphaBrush.Clone();
            }

            color.A = 0;
            alphaBrush.GradientStops[0].Color = color;
            color.A = 255;
            alphaBrush.GradientStops[1].Color = color;
            colorPicker.SetCurrentValue(AlphaSliderBackgroundBrushProperty, alphaBrush);
        }
    }
}
