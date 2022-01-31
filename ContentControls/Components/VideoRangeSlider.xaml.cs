using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class VideoRangeSlider : UserControl
    {
        public event Action<double> OnLowerThumbChanged;
        public event Action<double> OnUpperThumbChanged;

        public event Action<double> OnStartedMainDrag;
        public event Action<double> OnEndedMainDrag;


        public int MinimalThumbDistance => 10;
        public int MinimalTextDistance => 44;

        // Determines, if the value can be changed, when the value drag is active 
        public bool BlockValueOverrideOnDrag { get; set; }
        private bool isDraggingMainDrag = false;

        /// <summary> Percentage of the lower thumb </summary>
        public double LowerThumb { get; private set; }

        /// <summary> Percentage of the upper thumb </summary>
        public double UpperThumb { get; private set; }


        private bool ClampingLeft { get; set; }
        private bool ClampingRight { get; set; }

        private bool wasDraggingMidThumb = false;
        private double lastClickedPixelValueX = 0;


        private double _value = 0;

        public double Value
        {
            get => _value;
            set
            {
                if (!isDraggingMainDrag)
                {
                    _value = value;
                    SetValueThumb(_value);
                }
            }
        }


        // Describes the minimal and maximal values the thumbs can reach 
        private double minimalPixelValue = 0;
        private double maximalPixelValue = 0;

        private double minimalDistance = 40;
        private double maximalDistance = 0;
        private bool enabledState;

        public VideoRangeSlider()
        {
            InitializeComponent();

            MainWindow instance = (MainWindow)Application.Current.MainWindow;
            instance.OnWindowSizeChanged += (e) => { CalculateMinimalMaximalPixelValues(); };

            SetThumbTextToUndefined();
        }

        public void ResetThumbs()
        {
            Point lowerThumbPoint =
                lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
            Point upperThumbPoint =
                upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0));

            sliderParent.Margin = new Thickness(0, 0, 0, 0);
            lineParent.Margin = new Thickness(0, 0, 0, 0);
            textParent.Margin = new Thickness(0, 0, 0, 0);
            CalculatePercentages();

            this.OnLowerThumbChanged?.Invoke(this.LowerThumb);
            this.OnUpperThumbChanged?.Invoke(this.UpperThumb);
        }

        private void VideoRangeSlider_OnLoaded(object sender, RoutedEventArgs e)
        {
            CalculateMinimalMaximalPixelValues();
            CalculatePercentages();

            CheckEnabledColors();
        }

        private void CalculatePercentages()
        {
            double lowerValue = lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0)).X;
            double upperValue = upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0)).X;

            double distance = upperValue - lowerValue; // [36; 1000]
            double percentageDistance = MathHelper.Map(distance, minimalDistance, maximalDistance, 0.0d, 1.0d);

            this.LowerThumb = MathHelper.Map(lowerValue, minimalPixelValue, maximalPixelValue, 0.0d, 1.0d);
            this.UpperThumb = MathHelper.Map(upperValue, minimalPixelValue, maximalPixelValue, 0.0d, 1.0d);

            if (percentageDistance == 0 && this.ClampingLeft)
                this.UpperThumb = this.LowerThumb;

            if (percentageDistance == 0 && this.ClampingRight)
                this.LowerThumb = this.UpperThumb;


            this.LowerThumb = Math.Max(0.0d, Math.Min(this.LowerThumb, 1.0d));
            this.UpperThumb = Math.Max(0.0d, Math.Min(this.UpperThumb, 1.0d));

            this.LowerThumb = Math.Min(this.LowerThumb, this.UpperThumb);
            this.UpperThumb = Math.Max(this.LowerThumb, this.UpperThumb);
        }

        private void CalculateMinimalMaximalPixelValues()
        {
            this.minimalPixelValue =
                anchorLeft.TransformToAncestor(parent).Transform(new Point(0, 0)).X;
            this.maximalPixelValue =
                anchorRight.TransformToAncestor(parent).Transform(new Point(0, 0)).X;

            this.maximalDistance = this.maximalPixelValue - this.minimalPixelValue;
        }

        private void Thumb_LowerDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveLowerThumb(e.HorizontalChange);
        }

        private void Thumb_UpperDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveUpperThumb(e.HorizontalChange);
        }

        private bool MoveUpperThumb(double e, bool useOffset = true)
        {
            double value = useOffset ? Math.Max(0.0d, sliderParent.Margin.Right - e) : Math.Max(0.0d, e);

            Point lowerThumbPoint =
                lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
            Point upperThumbPoint =
                upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0));

            if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e > 0)
            {
                sliderParent.Margin = new Thickness(sliderParent.Margin.Left, 0, value, 0);
                lineParent.Margin = new Thickness(lineParent.Margin.Left, 0, value, 0);

                if (value < maximalPixelValue - lowerThumbPoint.X - MinimalTextDistance)
                    textParent.Margin = new Thickness(textParent.Margin.Left, 0, value, 0);
            }


            this.ClampingRight = value == 0.0d;
            CalculatePercentages();

            this.OnUpperThumbChanged?.Invoke(this.UpperThumb);

            return this.ClampingRight;
        }

        private bool MoveLowerThumb(double e, bool useOffset = true)
        {
            double value = useOffset ? Math.Max(0.0d, sliderParent.Margin.Left + e) : Math.Max(0.0d, e);

            Point lowerThumbPoint =
                lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
            Point upperThumbPoint =
                upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0));

            if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e < 0)
            {
                sliderParent.Margin = new Thickness(value, 0, sliderParent.Margin.Right, 0);
                lineParent.Margin = new Thickness(value, 0, lineParent.Margin.Right, 0);

                if (value < upperThumbPoint.X - MinimalTextDistance)
                    textParent.Margin = new Thickness(value, 0, textParent.Margin.Right, 0);
            }


            this.ClampingLeft = value == 0.0d;

            CalculatePercentages();

            this.OnLowerThumbChanged?.Invoke(this.LowerThumb);

            return this.ClampingLeft;
        }

        private void SetValueThumb(double v)
        {
            const double thickness = 5;
            double left = v * (maximalDistance - thickness);

            mainThumb.Margin = new Thickness(left + v * thickness, 0, 0, 0);
        }

        private void MainThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MainThumb_Dragged(mainThumb.Margin.Left + e.HorizontalChange);
        }

        private void MainThumb_Dragged(double newXPos)
        {
            double thickness = (minimalDistance - minimalDistance / 2) - 5;
            double value = Math.Max(0.0d, Math.Min(newXPos, this.maximalPixelValue - thickness));

            mainThumb.Margin = new Thickness(value, 0, 0, 0);

            double main = mainThumb.TransformToAncestor(parent).Transform(new Point(0, 0)).X;

            this._value = MathHelper.Map(main, minimalPixelValue + thickness, maximalPixelValue - thickness, 0.0d,
                1.0d);
            this._value = Math.Max(0.0d, Math.Min(_value, 1.0d));
        }

        private void Thumb_MidDragDelta(object sender, DragDeltaEventArgs e)
        {
            wasDraggingMidThumb = true;
            if (e.HorizontalChange < 0)
            {
                if (!MoveLowerThumb(e.HorizontalChange))
                    MoveUpperThumb(e.HorizontalChange);
            }
            else
            {
                if (!MoveUpperThumb(e.HorizontalChange))
                    MoveLowerThumb(e.HorizontalChange);
            }
        }

        private void MainThumb_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            isDraggingMainDrag = true;
            this.OnStartedMainDrag?.Invoke(this.Value);
        }


        private void MainThumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDraggingMainDrag = false;
            this.OnEndedMainDrag?.Invoke(this.Value);
        }

        private void MidThumb0_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (!wasDraggingMidThumb)
            {
                _value = lastClickedPixelValueX / this.maximalPixelValue;
                SetValueThumb(lastClickedPixelValueX / this.maximalPixelValue);

                this.OnEndedMainDrag?.Invoke(this.Value);
            }

            lastClickedPixelValueX = 0;
        }

        private void MidThumb0_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            Point lowerThumbPoint =
                lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));

            lastClickedPixelValueX = lowerThumbPoint.X + (e.HorizontalOffset + 2.5d);
        }

        private void UIElement_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            double x = e.GetPosition(anchorLeft).X;

            _value = x / this.maximalPixelValue;
            SetValueThumb(x / this.maximalPixelValue);

            this.OnEndedMainDrag?.Invoke(this.Value);
        }

        public void SetThumbTextToUndefined()
        {
            lowerThumbText.Text = "∞";
            upperThumbText.Text = "∞";
        }

        public void SetEnabledColors(bool state)
        {
            this.enabledState = state;

            if (this.IsLoaded)
                CheckEnabledColors();
        }

        private void CheckEnabledColors()
        {
            if (!enabledState)
                SetThumbTextToUndefined();

            byte offsetValue = 100;

            var yellow = new SolidColorBrush(Color.FromRgb(231, 210, 80));
            var darkerYellow = new SolidColorBrush(Color.FromRgb(
                (byte) Math.Clamp(231 - offsetValue, 0, 255), 
                (byte) Math.Clamp(210 - offsetValue, 0, 255),
                (byte) Math.Clamp(80 - offsetValue, 0, 255)
                )
            );

            var blue = new SolidColorBrush(Color.FromRgb(128, 204, 194));
            var darkerBlue = new SolidColorBrush(Color.FromRgb(
                (byte) Math.Clamp(128 - offsetValue, 0, 255), 
                (byte) Math.Clamp(204 - offsetValue, 0, 255), 
                (byte) Math.Clamp(194 - offsetValue, 0, 255)
                )
            );

            var gridBlue = new SolidColorBrush(Color.FromRgb(0, 153, 137));
            var gridDarkerBlue = new SolidColorBrush(Color.FromRgb(
                0, 
                (byte) Math.Clamp(153 - offsetValue, 0, 255), 
                (byte) Math.Clamp(137- offsetValue, 0, 255)
                )
            );

            var white = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            var gray = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));

            Rectangle rectangle = (Rectangle)mainThumb.Template.FindName("rectMainThumb", mainThumb);
            rectangle.Fill = enabledState ? yellow : darkerYellow;

            rectangle = (Rectangle) lowerThumb.Template.FindName("lowerThumbRect", lowerThumb);
            rectangle.Fill = enabledState ? yellow : darkerYellow;

            rectangle = (Rectangle)upperThumb.Template.FindName("upperThumbRect", upperThumb);
            rectangle.Fill = enabledState ? yellow : darkerYellow;
            
            rectangle = (Rectangle) lowerBall.Template.FindName("rect", lowerBall);
            rectangle.Fill = enabledState ? yellow : darkerYellow;
            
            rectangle = (Rectangle) upperBall.Template.FindName("rect", upperBall);
            rectangle.Fill = enabledState ? yellow : darkerYellow;
            
            Ellipse ellipse = (Ellipse) lowerBall.Template.FindName("ball", lowerBall);
            ellipse.Fill = enabledState ? yellow : darkerYellow;
            
            ellipse = (Ellipse) upperBall.Template.FindName("ball", upperBall);
            ellipse.Fill = enabledState ? yellow : darkerYellow;


            midThumb0.Background = enabledState ? blue : darkerBlue;
            sliderParent.BorderBrush = enabledState ? gridBlue : gridDarkerBlue;

            lowerThumbText.Foreground = enabledState ? white : gray;
            upperThumbText.Foreground = enabledState ? white : gray;
        }
    }
}