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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace mymusicpol.Views
{
	/// <summary>
	/// Interaction logic for PlayerView.xaml
	/// </summary>
	public partial class PlayerView : UserControl
	{
		public PlayerView()
		{
			InitializeComponent();
		}

		private VisualizerView? visualizerWindow;

		private void VisualizerClick(object snder, RoutedEventArgs e)
		{
			if (visualizerWindow is null)
			{
				visualizerWindow = new();
				visualizerWindow.Closed += (a, b) => visualizerWindow = null;
                visualizerWindow.Show();
			}
			else
			{
				visualizerWindow.Focus();
			}
		}

		//only for test purposes
		private void shuffle_Click(object sender, RoutedEventArgs e)
		{
			if (shuffle.Background.ToString() == "#FFCACFD2")
			{
				shuffle.Background = (Brush)(new BrushConverter().ConvertFrom("#d2b4de"));
			}
			else if (shuffle.Background.ToString() == "#FFD2B4DE")
			{
				shuffle.Background = shuffle.Background = (Brush)(new BrushConverter().ConvertFrom("#cacfd2"));
			}
		}

		private void repeat_Click(object sender, RoutedEventArgs e)
		{
			if (repeat.Background.ToString() == "#FFCACFD2" && repeat.Content.ToString() == "")
			{
				repeat.Background = (Brush)(new BrushConverter().ConvertFrom("#d2b4de"));
			}
			else if (repeat.Background.ToString() == "#FFD2B4DE" && repeat.Content.ToString() == "")
			{
				repeat.Content = "";
			}
			else if (repeat.Background.ToString() == "#FFD2B4DE" && repeat.Content.ToString() == "")
			{
				repeat.Content = "";
				repeat.Background = (Brush)(new BrushConverter().ConvertFrom("#cacfd2"));
			}
		}

		private void play_pause_Click(object sender, RoutedEventArgs e)
		{
			if (play_pause.Content.ToString() == "")
			{
				play_pause.Content = "";
			}
			else if (play_pause.Content.ToString() == "")
			{
				play_pause.Content = "";
			}
		}

		private void volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (volume.Value == 0)
			{
				volIcon.Content = "";
			}
			else
			{
				volIcon.Content = "";
			}
		}
	}
}
