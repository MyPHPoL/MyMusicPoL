using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicBackend.Utils;
internal class ObserverContainer<T>
{
	private List<T> _observers = new();

	public void Subscribe(T observer)
	{
		_observers.Add(observer);
	}
	public void Unsubscribe(T observer)
	{
		_observers.Remove(observer);
	}

	public void Notify(Action<T> action)
	{
		foreach (var observer in _observers)
		{
			action(observer);
		}
	}
}
