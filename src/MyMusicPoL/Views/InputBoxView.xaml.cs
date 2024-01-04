using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace mymusicpol.Views;
/// <summary>
/// Interaction logic for InputBoxView.xaml
/// </summary>
public partial class InputBoxView : Window
{
	public string? TextBody { get; private set; } = "";
	public InputBoxView(string labelText)
	{
		InitializeComponent();
		TextLabel.Content = labelText;
	}

	private void Confirm_Click(object sender, RoutedEventArgs e)
	{
		TextBody = InputBox.Text;
		Close();
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		TextBody = null;
	}
}
