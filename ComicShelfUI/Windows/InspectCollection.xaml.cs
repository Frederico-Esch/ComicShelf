using Domain;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utils.Extensions;

namespace ComicShelfUI.Windows
{
    /// <summary>
    /// Lógica interna para InspectCollection.xaml
    /// </summary>
    public partial class InspectCollection : Window
    {
        private IServiceProvider serviceProvider;
        private ICollectionRepository collectionRepository;
        public InspectCollection(IServiceProvider _serviceProvider, ICollectionRepository _collectionRepository)
        {
            serviceProvider = _serviceProvider;
            collectionRepository = _collectionRepository;
            InitializeComponent();
        }

        private bool AskForSaving(string? msg = null)
        {

            if (DataContext is not Collection { Name: string name }) return true;
            var defaultMsg = $"Do you wanna save the changes to \"{name}\"?";

            if (collectionRepository.HasChanges())
            {
                var result = MessageBox.Show(msg ?? defaultMsg, "Save Changes?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) return false;

                if (result == MessageBoxResult.Yes)
                    collectionRepository.Save();
                else
                    collectionRepository.DiscardChanges();

            }
            return true;
        }

        private void ReloadVolumes()
        {
            if (DataContext is not Collection collection) return;

            VolumesList.ItemsSource = collection.Volumes.OrderBy(v => v.Number ?? int.MaxValue).ThenBy(v => v.SpecialEdition);
        }

        #region Events
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            ReloadVolumes();
            if (DataContext is not Collection { Name: { } name } collection) return;
            if (!string.IsNullOrEmpty(name)) Title = $"{name}";
            if (collection.Cover is { } cover && cover.Length > 0)
            {
                var bitmap = new BitmapImage();
                bitmap.CreateFrom(cover);
                Icon = bitmap;
            }
        }

        private void OnUpdateCard()
        {

            Card.ReloadOwnCount();
            Card.InvalidateVisual();
        }

        private void SaveRequest(object sender, RoutedEventArgs e)
        {
            collectionRepository.Save();
        }

        private void AddVolume(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Collection collection) return;

            if (!AskForSaving("Save before Adding a new volume? (not saving means discarding changes)")) return;
            var window = serviceProvider.GetRequiredService<AddVolume>();
            window.Collection = collection;
            window.ShowDialog();
            OnUpdateCard();
            ReloadVolumes();
        }

        private void KeyDownWindow(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                e.Handled = true;
                Close();
            }
        }
        #endregion

        #region Context Menu
        private void EditVolume(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem { DataContext: Volume volume}) return;

            if (!AskForSaving("Save before Adding a new volume? (not saving means discarding changes)")) return;
            var window = serviceProvider.GetRequiredService<AddVolume>();
            window.Volume = volume;
            window.ShowDialog();
            OnUpdateCard();
            ReloadVolumes();
        }

        private void DeleteVolume(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Collection collection || sender is not MenuItem { DataContext: Volume volume}) return;

            var result = MessageBox.Show($"Are you sure you want to delete \"{volume.Name}\"?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No) return;

            collectionRepository.RemoveVolume(collection, volume);
            OnUpdateCard();
            ReloadVolumes();
        }
        #endregion

        #region Closing
        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = !AskForSaving();

            base.OnClosing(e);
        }
        #endregion
    }
}
