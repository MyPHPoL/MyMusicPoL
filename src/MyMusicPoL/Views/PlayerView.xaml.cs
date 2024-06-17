using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using mymusicpol.ViewModels;
using Forms = System.Windows.Forms;
using mymusicpol.Views.Languages;
using System.Globalization;

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
            Loaded += (s, e) =>
            {
                window = Window.GetWindow(this);
                LoadIcon();
                window.Closing += (s, e) =>
                {
                    notifyIcon?.Dispose();
                }; 
            };
        }

        private void NotifyIconMinimize(object sender, RoutedEventArgs e)
        {
            window.Hide();
            window.ShowInTaskbar = false; 
        }

        private VisualizerView? visualizerWindow;
        private Forms.NotifyIcon notifyIcon;
        private Window window;
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void LoadIcon()
        {
            notifyIcon = new()
            {
                Icon = Icon.ExtractAssociatedIcon(
                    Process.GetCurrentProcess().MainModule!.FileName
                ),
                Visible = false,
                Text = "MyMusicPoL",
            };
            notifyIcon.MouseClick += (s, e) =>
            {
                if (e.Button == Forms.MouseButtons.Left)
                {
                    window.Topmost = true;
                    window.Show();
                    window.Topmost = false;
                    window.ShowInTaskbar = true;
                }
                else if (e.Button == Forms.MouseButtons.Right)
                {
                    var menu = (ContextMenu)FindResource("NotifyIconMenu");
                    menu.IsOpen = true;

                    IntPtr handle = ((HwndSource)PresentationSource.FromVisual(menu)).Handle;
                    SetForegroundWindow(handle);
                }
            };
            notifyIcon.Visible = true;
        }

        private void NotifyIconShowWindow(object sender, RoutedEventArgs e)
        {
            window.ShowActivated = true;
            window.Show();
            window.ShowInTaskbar = true;
        }

        private void NotifyIconPlayPause(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.PlayPauseButton.Execute(null);
            }
        }

        private void NotifyIconQuit(object sender, RoutedEventArgs e)
        {
            notifyIcon.Dispose();
            Application.Current.Shutdown();
        }

        private void VisualizerClick(object sender, RoutedEventArgs e)
        {
            OpenVisualizer();
        }

        private void OpenVisualizer()
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
                var dialog = new InputBoxView(Languages.Resources.ibvPlaylistName);
                dialog.ShowDialog();
                if (dialog.Canceled) return;
                if (string.IsNullOrWhiteSpace(dialog.TextBody))
                {
                    CustomMessageBox.Show(
                        Languages.Resources.cmbCannotEmpty,
                        Languages.Resources.cmbErrorInvName
                    );
                }
                else
                {
                    try
                    {
                        playerViewModel.EditPlaylist(
                            PlaylistListBox.SelectedIndex,
                            dialog.TextBody
                        );
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(ex.Message, Languages.Resources.cmbErrorInvName);
                    }
                }
            }
        }

        private void NewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlayerViewModel playerViewModel)
            {
                var dialog = new InputBoxView(Languages.Resources.ibvPlaylistName);
                dialog.ShowDialog();
                if (dialog.Canceled) return;
                if (string.IsNullOrWhiteSpace(dialog.TextBody))
                {
                    CustomMessageBox.Show(
                        Languages.Resources.cmbCannotEmpty,
                        Languages.Resources.cmbErrorInvName
                    );
                }
                else
                {
                    try
                    {
                        playerViewModel.NewPlaylist(dialog.TextBody);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBox.Show(ex.Message, Languages.Resources.cmbErrorInvName);
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

        private void SelectedListAddQueue_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (DataContext is PlayerViewModel playerViewModel)
            {
                playerViewModel.SelectedListAddQueue(
                    SelectedList.SelectedIndex
                );
            }
        }

        private void SelectedListAddPlaylist_Click(
            object sender,
            RoutedEventArgs e
        )
        {
            if (DataContext is PlayerViewModel playerViewModel)
            {
                var dialog = new InputBoxView(
                    Languages.Resources.ibvAddSong
                );
                dialog.ShowDialog();
                if (dialog.TextBody is not null)
                {
                    playerViewModel.SelectedListAddPlaylist(
                        SelectedList.SelectedIndex,
                        dialog.TextBody
                    );
                }
            }
        }

        private void SelectedList_MouseDoubleClick(
            object sender,
            MouseButtonEventArgs e
        )
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
                dialog.Filter =
                    "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var path = dialog.FileName;
                    try
                    {
                        playerViewModel.SelectedListExport(path);
                    }
                    catch
                    {
                        CustomMessageBox.Show(
                            Languages.Resources.cmbExportFail,
                            Languages.Resources.cmbErrorExport
                        );
                    }
                }
            }
        }

        private void SelectedListImport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PlayerViewModel playerViewModel)
            {
                var dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter =
                    "JSON files (*.json)|*.json|XML files (*.xml)|*.xml";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var path = dialog.FileName;
                    try
                    {
                        playerViewModel.SelectedListImport(path);
                    }
                    catch
                    {
                        CustomMessageBox.Show(
                            Languages.Resources.cmbImportFail,
                            Languages.Resources.cmbErrorImport
                        );
                    }
                }
            }
        }

        private void fromWebPlaceholder_GotFocus(
            object sender,
            RoutedEventArgs e
        )
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
