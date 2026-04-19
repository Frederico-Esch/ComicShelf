using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Utils.Interfaces;

namespace Utils.Collections;

public class ObservableFilteredCollection<T, U> : INotifyCollectionChanged, IEnumerable<T>, IRearrangeableCollection<T>
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    private ReadOnlyCollection<T> filtered;
    private List<T> items;
    private Func<T, U, bool> filter;

    public IEnumerator<T> GetEnumerator() => filtered.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ObservableFilteredCollection(Func<T, U, bool> filterFunction)
    {
        items = [];
        filtered = items.AsReadOnly();
        filter = filterFunction;
    }

    public void ResetCollections(List<T> newCollections)
    {
        items = newCollections;
        filtered = items.AsReadOnly();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void Filter(U filterParams)
    {

        //filteredCollections = collections.Where(c =>
        //    terms.Any(t =>
        //        c.Name.Contains(t, StringComparison.InvariantCultureIgnoreCase) || c.Tags.Any(tag => tag.Contains(t, StringComparison.InvariantCultureIgnoreCase))
        //    )
        //).ToList().AsReadOnly();
        filtered = items.Where(i => filter(i, filterParams)).ToList().AsReadOnly();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public void ClearFilter()
    {
        filtered = items.AsReadOnly();
    }

    public void RemoveAt(int index)
    {
        if (items.ElementAtOrDefault(index) is T item)
        {
            items.RemoveAt(index);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }
    }

    public void InsertAt(int index, T value)
    {
        items.Insert(index, value);
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
    }

    public T? At(int index) => items.ElementAtOrDefault(index);
}
