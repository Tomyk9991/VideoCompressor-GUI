using System;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class VideoRangeSlider : UserControl
{
    public event Action OnLowerThumbChanged;
    public event Action OnUpperThumbChanged;
    
    
    public int MinimalThumbDistance => 18;

    /// <summary> Percentage of the lower thumb </summary>
    public double LowerThumb { get; private set; }
    /// <summary> Percentage of the upper thumb </summary>
    public double UpperThumb { get; private set; }

    private bool initialized = false;

    private bool ClampingLeft { get; set; }
    private bool ClampingRight { get; set; }


    private double _value = 0;
    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            SetValueThumb(_value);
        }
    }
    
    
    // Describes the minimal and maximal values the thumbs can reach 
    private double minimalPixelValue = 0;
    private double maximalPixelValue = 0;

    private double minimalDistance = 36;
    private double maximalDistance = 0;

    public VideoRangeSlider()
    {
        InitializeComponent();
        MainWindow.OnWindowSizeChanged += (e) =>
        {
            CalculateMinimalMaximalPixelValues();
        };
    }
    
    private void VideoRangeSlider_OnLoaded(object sender, RoutedEventArgs e)
    {
        CalculateMinimalMaximalPixelValues();
        CalculatePercentages();
    }

    private void CalculatePercentages()
    {
        double lowerValue = lowerThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
        double upperValue = upperThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
        
        double distance = upperValue - lowerValue; // [36; 1000]
        double percentageDistance = MathHelper.Map(distance, minimalDistance, maximalDistance, 0.0d, 1.0d);
        
        this.LowerThumb = MathHelper.Map(lowerValue, minimalPixelValue, maximalPixelValue - minimalDistance, 0.0d, 1.0d);
        this.UpperThumb = MathHelper.Map(upperValue, minimalPixelValue + minimalDistance, maximalPixelValue, 0.0d, 1.0d);

        if (percentageDistance == 0 && this.ClampingLeft)
            this.UpperThumb = this.LowerThumb;

        if (percentageDistance == 0 && this.ClampingRight)
            this.LowerThumb = this.UpperThumb;


        this.LowerThumb = Math.Max(0.0d, Math.Min(this.LowerThumb, 1.0d));
        this.UpperThumb = Math.Max(0.0d, Math.Min(this.UpperThumb, 1.0d));

        this.LowerThumb = Math.Min(this.LowerThumb, this.UpperThumb);
        this.UpperThumb = Math.Max(this.LowerThumb, this.UpperThumb);

        this.textboxLowerPercentage.Text = this.LowerThumb.ToString("F");
        this.textboxUpperPercentage.Text = this.UpperThumb.ToString("F");
    }

    private void CalculateMinimalMaximalPixelValues()
    {
        this.minimalPixelValue =
            anchorLeft.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
        this.maximalPixelValue =
            anchorRight.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0)).X;
        
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

    private bool MoveUpperThumb(double e)
    {
        double value = Math.Max(0.0d, sliderParent.Margin.Right - e);

        Point lowerThumbPoint =
            lowerThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
        Point upperThumbPoint =
            upperThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));

        if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e > 0)
        {
            sliderParent.Margin = new Thickness(sliderParent.Margin.Left, 0, value, 0);
            lineParent.Margin = new Thickness(lineParent.Margin.Left, 0, value, 0);
        }

        this.ClampingRight = value == 0.0d;

        CalculatePercentages();
        
        return this.ClampingRight;
    }

    private bool MoveLowerThumb(double e)
    {
        double value = Math.Max(0.0d, sliderParent.Margin.Left + e);

        Point lowerThumbPoint =
            lowerThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
        Point upperThumbPoint =
            upperThumb.TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
        
        if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e < 0)
        {
            sliderParent.Margin = new Thickness(value, 0, sliderParent.Margin.Right, 0);
            lineParent.Margin = new Thickness(value, 0, lineParent.Margin.Right, 0);
        }

        this.ClampingLeft = value == 0.0d;
        
        CalculatePercentages();
        
        return this.ClampingLeft;
    }
    
    private void SetValueThumb(double v)
    {
        double left = v * (this.maximalPixelValue - (minimalDistance - minimalDistance / 2) - 5);
        mainThumb.Margin = new Thickness(left, 0, 0, 0);
    }

    private void MainThumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        double value = Math.Max(0.0d, Math.Min(mainThumb.Margin.Left + e.HorizontalChange, this.maximalPixelValue - (minimalDistance - minimalDistance / 2) - 5));
        mainThumb.Margin = new Thickness(value, 0, 0, 0);
    }

    private void Thumb_MidDragDelta(object sender, DragDeltaEventArgs e)
    {
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
}