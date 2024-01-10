using MusicBackend.Model;
using mymusicpol.ViewModels;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace mymusicpol;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
		MusicBackend.Model.Serializer.Deserialize();
    }
	protected override void OnStartup(StartupEventArgs e)
	{
#if DEBUGNOHWACCEL
        RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
#endif
		MainWindow = new MainWindow()
		{
			DataContext = new MainViewModel()
		};
		MainWindow.Show();
		base.OnStartup(e);
	}
    protected override void OnExit(ExitEventArgs e)
    {
		MusicBackend.Model.Serializer.Serialize();
        base.OnExit(e);
    }
}
