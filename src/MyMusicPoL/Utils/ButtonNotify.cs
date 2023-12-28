using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mymusicpol.Utils;
public class ButtonNotify
{
	private string _content;
	private string _background;

	public string Content
	{
		get => _content;
		set
		{
			_content = value;
			OnPropertyChanged(nameof(Content));
		}
	}

	public string Background
	{
		get => _background;
		set
		{
			_background = value;
			OnPropertyChanged(nameof(Background));
		}
	}

	#region INotifyPropertyChanged Members
	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
	#endregion
}
