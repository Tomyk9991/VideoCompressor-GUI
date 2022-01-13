using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using VideoCompressorGUI.Utils;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class VideoRangeSlider : UserControl
{
    public event Action<double> OnLowerThumbChanged;
    public event Action<double> OnUpperThumbChanged;
    
    public event Action<double> OnStartedMainDrag;
    public event Action<double> OnEndedMainDrag;
    
    
    public int MinimalThumbDistance => 20;
    
    // Determines, if the value can be changed, when the value drag is active 
    public bool BlockValueOverrideOnDrag { get; set; }
    private bool isDraggingMainDrag = false;

    /// <summary> Percentage of the lower thumb </summary>
    public double LowerThumb { get; private set; }
    /// <summary> Percentage of the upper thumb </summary>
    public double UpperThumb { get; private set; }

    private bool initialized = false;

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

    public VideoRangeSlider()
    {
        InitializeComponent();
        MainWindow instance = (MainWindow) Application.Current.MainWindow;
        instance.OnWindowSizeChanged += (e) =>
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
        double lowerValue = lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0)).X;
        double upperValue = upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0)).X;
        
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

    private bool MoveUpperThumb(double e)
    {
        double value = Math.Max(0.0d, sliderParent.Margin.Right - e);

        Point lowerThumbPoint =
            lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
        Point upperThumbPoint =
            upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0));

        if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e > 0)
        {
            sliderParent.Margin = new Thickness(sliderParent.Margin.Left, 0, value, 0);
            lineParent.Margin = new Thickness(lineParent.Margin.Left, 0, value, 0);
        }


        this.ClampingRight = value == 0.0d;

        ValidateDistanceForTextSpan(upperThumbPoint.X - lowerThumbPoint.X);
        CalculatePercentages();

        this.OnUpperThumbChanged?.Invoke(this.UpperThumb);
        
        return this.ClampingRight;
    }

    private bool MoveLowerThumb(double e)
    {
        double value = Math.Max(0.0d, sliderParent.Margin.Left + e);

        Point lowerThumbPoint =
            lowerThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
        Point upperThumbPoint =
            upperThumb.TransformToAncestor(parent).Transform(new Point(0, 0));
        
        if (lowerThumbPoint.X < (upperThumbPoint.X - MinimalThumbDistance) || e < 0)
        {
            sliderParent.Margin = new Thickness(value, 0, sliderParent.Margin.Right, 0);
            lineParent.Margin = new Thickness(value, 0, lineParent.Margin.Right, 0);
        }
        

        this.ClampingLeft = value == 0.0d;
        
        ValidateDistanceForTextSpan(upperThumbPoint.X - lowerThumbPoint.X);
        CalculatePercentages();
        
        this.OnLowerThumbChanged?.Invoke(this.LowerThumb);
        
        return this.ClampingLeft;
    }

    private void ValidateDistanceForTextSpan(double distance)
    {
        if (distance < this.minimalDistance)
        {
            this.spanTextBlock.Visibility = Visibility.Collapsed;
            this.textColumnDefinition.Width = new GridLength(0);
        }
        else
        {
            this.spanTextBlock.Visibility = Visibility.Visible;
            this.textColumnDefinition.Width = new GridLength(30);
        }
    }
    
    private void SetValueThumb(double v)
    {
        double thickness = (minimalDistance - minimalDistance / 2) - 5;
        
        double availableMargin = maximalPixelValue - minimalPixelValue;
        double left = v * (availableMargin - thickness);
        
        mainThumb.Margin = new Thickness(left, 0, 0, 0);
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
        
        this._value = MathHelper.Map(main, minimalPixelValue + thickness, maximalPixelValue - thickness, 0.0d, 1.0d);
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
        Console.WriteLine("ended");
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
        Console.WriteLine("started");
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
}