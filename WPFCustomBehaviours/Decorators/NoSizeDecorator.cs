using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.WPFCustomBehaviours.Decorators
{
    public class NoSizeDecorator : Decorator
    {
        protected override Size MeasureOverride(Size constraint)
        {
            Child.Measure(new Size(0, 0));
            return new Size(0, 0);
        }
    }
}