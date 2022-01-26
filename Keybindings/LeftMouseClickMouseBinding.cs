using System;
using System.Windows;
using System.Windows.Input;
using Unosquare.FFME;

namespace VideoCompressorGUI.Keybindings
{
    public class LeftMouseClickMouseBinding : IMouseBinding
    {
        private Action t;
        private MediaElement element;

        public LeftMouseClickMouseBinding(Action t, MediaElement element)
        {
            this.t = t;
            this.element = element;
        }

        public void OnMouseClick(MouseButtonEventArgs e, IInputElement input)
        {
            if (element != null)
            {
                Point mousePoint = e.GetPosition(element);
                bool mouseOver = mousePoint.X > 0.0d && mousePoint.X <= element.ActualWidth &&
                                 mousePoint.Y > 0 && mousePoint.Y <= (element.ActualHeight - 35.0d);
                
                if (Mouse.LeftButton == MouseButtonState.Released && mouseOver)
                {
                    t?.Invoke();
                }
            }
            
        }
    }
}