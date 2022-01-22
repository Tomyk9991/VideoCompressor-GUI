using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoCompressorGUI.Keybindings
{
    public class ResumeHaltKeyBinding : IKeyBinding
    {
        // private DateTime lastModified = DateTime.Now;
        private Action togglePlayPause;
    
        public ResumeHaltKeyBinding(Action togglePlayPause)
        {
            this.togglePlayPause = togglePlayPause;
        }
    
        public void OnKeybinding(KeyEventArgs e, IInputElement input)
        {
            if (input is MediaElement)
            {
                //space
                if (e.Key == Key.Space /*&& DateTime.Now - lastModified > TimeSpan.FromSeconds(0.25)*/)
                {
                    // lastModified = DateTime.Now;
                    this.togglePlayPause.Invoke();
                }
            }
        }
    }
}