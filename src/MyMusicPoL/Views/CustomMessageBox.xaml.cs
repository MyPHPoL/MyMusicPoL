using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mymusicpol.Views
{
    /// <summary>
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        [DllImport("DwmApi")] //System.Runtime.InteropServices
        private static extern int DwmSetWindowAttribute(
            IntPtr hwnd,
            int attr,
            int[] attrValue,
            int attrSize
        );

        // change window topbar to dark theme
        protected override void OnSourceInitialized(EventArgs e)
        {
            if (
                DwmSetWindowAttribute(
                    new WindowInteropHelper(this).Handle,
                    19,
                    new[] { 1 },
                    4
                ) != 0
            )
                DwmSetWindowAttribute(
                    new WindowInteropHelper(this).Handle,
                    20,
                    new[] { 1 },
                    4
                );
        }

        public CustomMessageBox(string message, string title)
        {
            InitializeComponent();
            Title.Text = title;
            Message.Text = message;
        }

        public static void Show(string text, string title = "")
        {
            Application.Current.Dispatcher.Invoke(
                (Action)
                    delegate
                    {
                        new CustomMessageBox(text, title).Show();
                    }
            );
        }

        public static void ShowDialog(string text, string title = "")
        {
            Application.Current.Dispatcher.Invoke(
                (Action)
                    delegate
                    {
                        new CustomMessageBox(text, title).ShowDialog();
                    }
            );
        }

        private void Button_Click_OK(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
