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
    private CollectionRepository collectionRepository;
    private ObservableFilteredCollection<Collection, string[]> collections;

    private bool dragAndDrop = true;
    public bool DragAndDrop {
        get => dragAndDrop;
        set {
            dragAndDrop = value;
            OnPropertyChanged();
        }
    }

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
    public MainWindow(IServiceProvider _serviceProvider, CollectionRepository _collectionRepository)
    {
        serviceProvider = _serviceProvider;
        collectionRepository = _collectionRepository;
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
        Search.Text = "";
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

    #region Events

    private void ReloadButton_Click(object sender, RoutedEventArgs e)
    {
        ReloadCollections();
    }

    private void AddCollection_Click(object sender, RoutedEventArgs e)
    {
        if(serviceProvider.GetRequiredService<AddCollection>().ShowDialog() ?? false)
        {
            ReloadCollections();
        }
    }

    private void Search_KeyPressed(object sender, System.Windows.Input.KeyEventArgs e)
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

    #endregion

    #region Context Menu
    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is Collection collection)
        {
            var addCollection = serviceProvider.GetRequiredService<AddCollection>();
            addCollection.CurrentCollection = collection;
            addCollection.ShowDialog();
            ReloadCollections();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem menuItem && menuItem.DataContext is Collection collection)
        {
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
    }
    #endregion
}