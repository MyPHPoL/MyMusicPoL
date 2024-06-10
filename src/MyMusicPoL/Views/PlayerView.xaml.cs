using mymusicpol.ViewModels;
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

		//private void Playlists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		//{
		//	if (DataContext is PlayerViewModel playerViewModel)
		//	{
		//		playerViewModel.ShowPlaylist();
		//	}
		//}

		private void MenuItemShow_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.ShowPlaylist();
			}
		}
		private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				var dialog = new InputBoxView("Enter new playlist name");
				dialog.ShowDialog();
				if (string.IsNullOrWhiteSpace(dialog.TextBody))
				{
					CustomMessageBox.Show("Playlist name cannot be empty", "Invalid Name");
				}
				else
				{
					if (dialog.TextBody == "Library" || dialog.TextBody == "Queue")
					{
						CustomMessageBox.Show($"Cannot change name of playlist to {dialog.TextBody}", "Invalid Name");
						return;
					}
					playerViewModel.EditPlaylist(PlaylistListBox.SelectedIndex, dialog.TextBody);
				}
			}
		}
		private void NewPlaylist_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				var dialog = new InputBoxView("Enter new playlist name");
				dialog.ShowDialog();
				if (string.IsNullOrWhiteSpace(dialog.TextBody))
				{
					CustomMessageBox.Show("Playlist name cannot be empty", "Invalid Name");
				}
				else
				{
					if (dialog.TextBody == "Library" || dialog.TextBody == "Queue")
					{
						CustomMessageBox.Show($"Cannot name playlist as {dialog.TextBody}", "Invalid Name");
						return;
					}
					var res = playerViewModel.NewPlaylist(dialog.TextBody);
					if (res is false)
					{
						CustomMessageBox.Show($"Playlist {dialog.TextBody} already exists", "Invalid Name");
					}
				}
			}
		}
		private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.DeletePlaylist(PlaylistListBox.SelectedIndex);
			}
		}
		private void MenuItemPlay_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				playerViewModel.PlayPlaylist(PlaylistListBox.SelectedIndex);
			}
		}
		private void SelectedListRemove_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.SelectedListRemove(SelectedList.SelectedIndex);
			}
		}
		private void SelectedListAddQueue_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.SelectedListAddQueue(SelectedList.SelectedIndex);
			}
		}
		private void SelectedListAddPlaylist_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				var dialog = new InputBoxView("Enter playlist name to add song");
				dialog.ShowDialog();
				if (dialog.TextBody is not null)
				{
					if (dialog.TextBody == "Library" || dialog.TextBody == "Queue")
					{
						CustomMessageBox.Show("Cannot add song to playlist " + dialog.TextBody, "Invalid Song");
						return;
					}
					playerViewModel.SelectedListAddPlaylist(SelectedList.SelectedIndex, dialog.TextBody);
				}
			}
		}
		private void SelectedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.SelectedListPlay(SelectedList.SelectedIndex);
			}
		}

		private void SelectedListExport_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				var dialog = new System.Windows.Forms.SaveFileDialog();
				dialog.Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var path = dialog.FileName;
					try
					{
						playerViewModel.SelectedListExport(path);
					}
					catch
					{
						CustomMessageBox.Show("Failed to export playlist", "Export Error");
					}
				}
			}
		}
		private void SelectedListImport_Click(object sender, RoutedEventArgs e)
		{
            if (DataContext is PlayerViewModel playerViewModel)
            {
				var dialog = new System.Windows.Forms.OpenFileDialog();
				dialog.Filter = "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
				if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					var path = dialog.FileName;
					try
					{
						playerViewModel.SelectedListImport(path);
					}
					catch
					{
						CustomMessageBox.Show("Failed to import playlist", "Export Error");
					}
				}
			}
		}

		private void fromWebPlaceholder_GotFocus(object sender, RoutedEventArgs e)
		{
			fromWebBoxPlaceholder.Visibility = Visibility.Collapsed;
			fromWebBox.Visibility = Visibility.Visible;
			fromWebBox.Focus();
		}

		private void fromWeb_LostFocus(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrWhiteSpace(fromWebBox.Text))
			{
				fromWebBoxPlaceholder.Visibility = Visibility.Visible;
				fromWebBox.Visibility = Visibility.Collapsed;
			}
		}
	}
}
