using Mp.Model;
using mymusicpol.ViewModels;
using System.Configuration;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace mymusicpol;
internal class AppState
{
    public QueueModelState? qms { get; set; }
    public PlayerModelState? pms { get; set; }

    [IgnoreDataMember]
    JsonSerializerOptions jsonOptions;
    [IgnoreDataMember]
    string configPath;


    public static AppState DeserializeConfig()
    {
        var exedir = AppDomain.CurrentDomain.BaseDirectory;
        var statepath = Path.Combine(exedir, "state.json");
        var jsonOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            WriteIndented = true
        };
        AppState appState;
        try
        {
            var data = File.ReadAllText(statepath);
            appState = JsonSerializer.Deserialize<AppState>(data,jsonOptions);
            if (appState is null)
            {
                appState = new();
            }
        } catch
        {
            appState = new();
        }
        appState.jsonOptions = jsonOptions;
        appState.configPath = statepath;

        return appState;
    }

    public void SerializeConfig(
        QueueModel queueModel,
        PlayerModel playerModel )
    {
		pms = playerModel.DumpState();
		qms = queueModel.DumpState();
		var str = JsonSerializer.Serialize(this,jsonOptions);
		File.WriteAllText(configPath,str);
    }
}
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private AppState appState;
    public App()
    {
        appState = AppState.DeserializeConfig();
        if (appState.qms is not null)
        {
            QueueModel.InitWithState(appState.qms);
        }
        if (appState.pms is not null)
        {
			PlayerModel.InitWithState(appState.pms);
		}
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
        appState.SerializeConfig(QueueModel.Instance, PlayerModel.Instance);
        base.OnExit(e);
    }
}
