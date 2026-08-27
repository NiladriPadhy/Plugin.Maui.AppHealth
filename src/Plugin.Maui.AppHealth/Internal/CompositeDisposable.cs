namespace Plugin.Maui.AppHealth;

sealed class CompositeDisposable : IDisposable
{
	readonly List<IDisposable> _items = [];
	bool _disposed;

	public void Add(IDisposable? item)
	{
		if (item is null)
			return;

		if (_disposed)
		{
			item.Dispose();
			return;
		}

		_items.Add(item);
	}

	public void Add(Action unsubscribe)
	{
		Add(new ActionDisposable(unsubscribe));
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		foreach (var item in _items)
			item.Dispose();

		_items.Clear();
	}

	sealed class ActionDisposable(Action action) : IDisposable
	{
		int _disposed;

		public void Dispose()
		{
			if (Interlocked.Exchange(ref _disposed, 1) == 0)
				action();
		}
	}
}
