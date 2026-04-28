using System.Collections;
using System.Collections.Specialized;

namespace Utils.Collections;

public class ObservableHashSet<T> : INotifyCollectionChanged, IEnumerable<T>
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    private List<T> positions { get; set; } = [];
    public HashSet<T> Set { get; private init;} = [];
    public int Count => Set.Count;


    public ObservableHashSet() { }

    public bool Add(T item)
    {
        if (!Set.Add(item)) return false;

        positions.Add(item);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, positions.GetRange(positions.Count-1, 1)));
        return true;
    }

    public void Replace(IEnumerable<T> values)
    {
        positions.Clear();
        Set.Clear();

        foreach (var val in values)
        {
            if (Set.Add(val))
                positions.Add(val);
        }

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public T Remove(int index)
    {
        var item = positions[index];
        Set.Remove(item);
        positions.RemoveAt(index);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        return item;
    }


    public IEnumerator<T> GetEnumerator() => positions.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
