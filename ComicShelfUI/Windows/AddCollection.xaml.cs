using Domain;
using Microsoft.Win32;
using Repositories;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Utils.Collections;
using Utils.Extensions;

namespace ComicShelfUI.Windows;

public partial class AddCollection : Window
{
    private ObservableHashSet<string> Tags { get; set; } = [];
    private byte[]? Cover { get; set; } = [];
    private BitmapImage? CoverImage { get; set; }
    private ICollectionRepository collectionRepository;
    private Collection? currentCollection;
    public Collection? CurrentCollection {
        get => currentCollection;
        set
        {
            if (value is not { } collection)
            {
                DeleteCollectionButton.Visibility = Visibility.Collapsed;
                currentCollection = value;
            }
            else
            {
                DeleteCollectionButton.Visibility = Visibility.Visible;
                currentCollection = collection;
                NameTextBox.Text = collection.Name;
                Cover = collection.Cover;
                if (Cover is { } cover && cover.Length > 0)
                {
                    CoverImage = new BitmapImage();
                    CoverImage.CreateFrom(cover);
                    CoverButton.Source = CoverImage;
                }
                Tags.Replace(collection.Tags);
                Title = $"Edit \"{collection.Name}\"";
            }
        }
    }

    private bool HasName => NameTextBox.Text.Trim() is string name && string.IsNullOrWhiteSpace(name);

    public AddCollection(ICollectionRepository _collectionRepository)
    {
        collectionRepository = _collectionRepository;

        InitializeComponent();
        TagList.ItemsSource = Tags;
        CurrentCollection = null;
    }

    private void AddTag(object sender, RoutedEventArgs e)
    {
        if (TagTextBox.Text.Trim() is { } newTag && newTag.Length > 0) {
            if(Tags.Add(newTag))
            {
                TagTextBox.Text = string.Empty;
            }
        }
    }

    private void DeleteTag(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (TagList.SelectedIndex > -1)
        {
            var tag = Tags.Remove(TagList.SelectedIndex);
            TagList.SelectedIndex = -1;
            TagTextBox.Text = tag;
        }
    }

    private void TextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)
        {
            if(sender == TagTextBox)
            {
                AddTag(0, new RoutedEventArgs());
            }
            else
            {
                TagTextBox.Focus();
            }
        }
    }

    private void ChangeCover(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Image Files (.jpg, .jpeg, .png)|*.png;*.PNG;*.jpg;*.JPG;*.jpeg;*.JPEG;*.webp;*.WEBP";

            if (openFileDialog.ShowDialog() ?? false)
            {
                using var fileStream = openFileDialog.OpenFile();
                using var memStream = new MemoryStream();
                fileStream.CopyTo(memStream);
                Cover = memStream.ToArray();

                CoverImage = new BitmapImage();
                CoverImage.CreateFrom(Cover);

                CoverButton.Source = CoverImage;
            }
        }
        else
        {
            CoverButton.Source = (BitmapImage)Resources["GenericCover"];
            Cover = null;
            CoverImage = null;
        }
    }

    private void Save()
    {
        if (NameTextBox.Text.Trim() is string name && !string.IsNullOrWhiteSpace(name))
        {
            if (currentCollection is { } collection)
            {
                collection.Name = name;
                collection.Tags = Tags.Set;
                collection.Cover = Cover;
            }
            else
            {
                collectionRepository.Add(new Collection(name, Tags.Set, Cover));
            }
            collectionRepository.Save();
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Collection name can't be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Save(object sender, RoutedEventArgs e) => Save();

    private void Delete(object sender, RoutedEventArgs e)
    {
        if (currentCollection is { } collection)
        {
            var result = MessageBox.Show(
                $"Do you want to remove collection \"{collection.Name}\"?",
                "Are you sure?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );
            if (result != MessageBoxResult.Yes) return;
            collectionRepository.Remove(collection);
            collectionRepository.Save();
            DialogResult = true;
            Close();
        }
    }

    private void KeyDownWindow(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var askToSave = HasName || Tags.Count > 0 || (Cover?.Length ?? 0) > 0 || currentCollection is not null;

        if (askToSave && !(DialogResult ?? false))
        {
            var result = MessageBox.Show("Do you want to exit without saving?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                collectionRepository.DiscardChanges();
            else
                e.Cancel = true;
        }
    }
}
