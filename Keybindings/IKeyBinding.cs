using System.Windows;
using System.Windows.Input;

namespace VideoCompressorGUI.Keybindings
{
    public interface IKeyBinding
    {
        void OnKeybinding(KeyEventArgs e, IInputElement inputElement);
    }

    public interface IMouseBinding
    {
        void OnMouseClick(MouseButtonEventArgs e, IInputElement inputElement);
    }
}