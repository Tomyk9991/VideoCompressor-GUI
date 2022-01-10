using System;
using System.Windows;
using System.Windows.Input;
using VideoCompressorGUI.ContentControls;

namespace VideoCompressorGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static event Action<KeyEventArgs, IInputElement> OnKeyPressed;
        public static event Action<SizeChangedEventArgs> OnWindowSizeChanged;
        public MainWindow()
        {
            InitializeComponent();
            this.contentControl.Content = new DragAndDropControl(this.contentControl);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            IInputElement focusedControl = FocusManager.GetFocusedElement(this);
            OnKeyPressed?.Invoke(e, focusedControl);
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnWindowSizeChanged?.Invoke(e);
        }
    }
}
