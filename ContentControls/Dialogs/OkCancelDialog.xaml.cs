using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoCompressorGUI.ContentControls.Dialogs
{
    public partial class OkCancelDialog : UserControl
    {
        public event Action OnConfirm;
        public event Action OnCancel;

        public OkCancelDialog()
        {
            InitializeComponent();
        }

        private void OutsideDialog_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void InsideDialog_OnClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void Confirm_OnClick(object sender, RoutedEventArgs e)
        {
            OnConfirm?.Invoke();
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel_OnClick(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
            this.Visibility = Visibility.Collapsed;
        }
    }
}