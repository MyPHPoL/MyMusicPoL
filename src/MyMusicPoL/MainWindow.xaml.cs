using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mymusicpol
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport(
            "uxtheme.dll",
            EntryPoint = "#135",
            SetLastError = true,
            CharSet = CharSet.Unicode
        )]
        private static extern int SetPreferredAppMode(int preferredAppMode);

        [DllImport(
            "uxtheme.dll",
            EntryPoint = "#136",
            SetLastError = true,
            CharSet = CharSet.Unicode
        )]
        private static extern void FlushMenuThemes();

        public MainWindow()
        {
            InitializeComponent();
            SetPreferredAppMode(2); // Enable dark system menu

            FlushMenuThemes();
        }

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
    }
}
