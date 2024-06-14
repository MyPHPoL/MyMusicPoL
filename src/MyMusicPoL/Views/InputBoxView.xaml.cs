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
using CommunityToolkit.Mvvm.Input;

namespace mymusicpol.Views;

/// <summary>
/// Interaction logic for InputBoxView.xaml
/// </summary>
public partial class InputBoxView : Window
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

    public string? TextBody { get; private set; } = "";
    public bool Canceled { get; private set; } = false;

    //public IRelayCommand ConfirmCommand = new RelayCommand(() => ConfirmHandler());
    public InputBoxView(string labelText)
    {
        InitializeComponent();
        TextLabel.Content = labelText;
        DataContext = this;
        InputBox.Focus();
    }

    [RelayCommand]
    private void Confirm()
    {
        TextBody = InputBox.Text;
        Close();
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        TextBody = InputBox.Text;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        TextBody = null;
        Canceled = true;
    }
}
