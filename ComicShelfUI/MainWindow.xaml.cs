using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ComicShelfUI.UserControls;
using ComicShelfUI.Windows;
using Domain;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Utils.Collections;
using Utils.DragAndDrop;

namespace ComicShelfUI;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private IServiceProvider serviceProvider;
    private ICollectionRepository collectionRepository;
    private ObservableFilteredCollection<Collection, string[]> collections;

    private bool dragAndDrop = true;
    public bool DragAndDrop {
        get => dragAndDrop;
        set {
            dragAndDrop = value;
            OnPropertyChanged();
        }
    }

    #region Properties
    private DropHandlerForCollectionList<ObservableFilteredCollection<Collection, string[]>> dropHandler = new();
    public DropHandlerForCollectionList<ObservableFilteredCollection<Collection, string[]>> DropHandler {
        get => dropHandler;
        set {
            dropHandler = value;
            OnPropertyChanged();
        }
    }

    private CustomDragHandler<Collection, ObservableFilteredCollection<Collection, string[]>>  dragHandler = new();
    public CustomDragHandler<Collection, ObservableFilteredCollection<Collection, string[]>> DragHandler {
        get => dragHandler;
        set {
            dragHandler = value;
            OnPropertyChanged();
        }
    }
    #endregion

    public MainWindow(IServiceProvider _serviceProvider, ICollectionRepository _collectionRepository)
    {
        serviceProvider = _serviceProvider;
        collectionRepository = _collectionRepository;
        collectionRepository.EnsureDbExists();
        collections = new((collection, terms) => {
            return terms.Any(t =>
                   collection
                    .Name.Contains(t, StringComparison.InvariantCultureIgnoreCase)
                || collection
                    .Tags.Any(tag => tag.Contains(t, StringComparison.InvariantCultureIgnoreCase))
            );
        });

        dropHandler.OnReorder += (previousIndex, newIndex) =>
        {
            previousIndex += 1; newIndex += 1;
            var min = Math.Min(previousIndex, newIndex);
            var max = Math.Max(previousIndex, newIndex);
            foreach (var (c, i) in collections.AsEnumerable().Where(c => c.Order >= min && c.Order <= max).Zip(Enumerable.Range(0, max-min+1)))
            {
                c.Order = min + i;
            }

            collectionRepository.Save();
        };

        InitializeComponent();
        CollectionList.ItemsSource = collections; 
        ReloadCollections();
    }

    private void ReloadCollections()
    {
        collections.ResetCollections(collectionRepository.GetAllCollections());
        CollectionList.InvalidateVisual();
        GC.Collect();
        Search.Text = "";
        FilterCollections([]);
    }

    private void FilterCollections(string[] terms)
    {
        if (terms.Length == 0) {
            DragAndDrop = true;
            collections.ClearFilter();
        }
        else
        {
            DragAndDrop = false;
            collections.Filter(terms);
            GC.Collect();
        }
    }

    private void OpenComic(Collection collection)
    {
        var inspectCollection = serviceProvider.GetRequiredService<InspectCollection>();
        inspectCollection.DataContext = collection;
        inspectCollection.ShowDialog();

        ReloadCollections();
    }

    #region Events
    private void ReloadClick(object sender, RoutedEventArgs e)
    {
        ReloadCollections();
    }

    private void AddCollection(object sender, RoutedEventArgs e)
    {
        if(serviceProvider.GetRequiredService<AddCollection>().ShowDialog() ?? false)
        {
            ReloadCollections();
        }
    }

    private void SearchKeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Return)
        {
            e.Handled = true;
            var search = Search.Text;
            var terms = search
                .Split(";")
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToArray();
            FilterCollections(terms);
        }
    }

    private void ComicClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton != System.Windows.Input.MouseButton.Left || sender is not CollectionVCard { DataContext: Collection collection })
            return;
        OpenComic(collection);
    }
    #endregion

    #region Context Menu
    private void ContextEdit(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: Collection collection })
            return;

        var addCollection = serviceProvider.GetRequiredService<AddCollection>();
        addCollection.CurrentCollection = collection;
        addCollection.ShowDialog();
        ReloadCollections();
    }

    private void ContextDelete(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: Collection collection }) return;
        //add an Active column in DB to not permanently remove maybe??
        var result = MessageBox.Show(
            $"Do you want to remove collection \"{collection.Name}\"?",
            "Are you sure?",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );
        if (result != MessageBoxResult.Yes) return;
        collectionRepository.Remove(collection);
        collectionRepository.Save();
        ReloadCollections();
    }

    private void ContextOpenComic(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { DataContext: Collection collection })
            return;

        OpenComic(collection);
    }
    #endregion
}