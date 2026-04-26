using Domain;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComicShelfUI.Windows
{
    /// <summary>
    /// Lógica interna para AddVolumeWindow.xaml
    /// </summary>
    public partial class AddVolume : Window
    {
        public Collection? Collection { get; set; }
        public Volume Volume
        {
            set
            {
                DataContext = value;
                if (value.Number is int number)
                    VolumeNumber.Text = number.ToString();
                else
                {
                    VolumeSpecialName.Text = value.SpecialEdition;
                    chkName.IsChecked = true;
                }    
                VolumeDetails.Text = value.Details;
                Title = "Edit Volume";
                DeleteButton.Visibility = Visibility.Visible;
            }
        }
        private readonly ICollectionRepository collectionRepository;

        public AddVolume(ICollectionRepository _collectionRepository)
        {
            collectionRepository = _collectionRepository;
            InitializeComponent();
        }

        private bool NumberSelected() => VolumeNumber.IsEnabled;
        private bool NumberHasContent() => NumberSelected() && !string.IsNullOrWhiteSpace(VolumeNumber.Text);

        private bool SpecialSelected() => VolumeSpecialName.IsEnabled;
        private bool SpecialHasContent() => SpecialSelected() && !string.IsNullOrWhiteSpace(VolumeSpecialName.Text);

        private bool Save()
        {
            if (DataContext is not Volume volume) return false;

            var details = VolumeDetails.Text.Trim();
            volume.Details = !string.IsNullOrEmpty(details) ? details : null;

            if (NumberHasContent())
            {
                volume.Number = Convert.ToInt32(VolumeNumber.Text);
                volume.SpecialEdition = null;
            }
            else if (SpecialHasContent())
            {
                volume.Number = null;
                volume.SpecialEdition = VolumeSpecialName.Text.Trim();
            }
            else
            {
                MessageBox.Show("Invalid Special Name or Number selected", "Error Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (volume.CollectionId == Guid.Empty)
            {
                if (Collection is null)
                {
                    MessageBox.Show("Collection for which to save this volume is not set", "Problem Saving", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                collectionRepository.AddVolume(Collection, volume);
            }

            collectionRepository.Save();
            return true;
        }

        private void Save(object sender, RoutedEventArgs e) => Save();

        private bool InvalidText(string text) => text.Any(c => !char.IsNumber(c));

        #region Events
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (VolumeNumber.IsEnabled)
                VolumeNumber.Focus();
            else
                VolumeSpecialName.Focus();
        }

        private void ValidateText(object sender, TextCompositionEventArgs e)
        {
            e.Handled = InvalidText(e.Text);
        }

        private void ValidateText(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            var index = textBox.CaretIndex;
            var previousSize = textBox.Text.Length;
            textBox.Text = string.Concat(textBox.Text.Where(char.IsNumber));
            index -= (previousSize - textBox.Text.Length);
            textBox.CaretIndex = Math.Clamp(index, 0, textBox.Text.Length);
        }

        private void ValidateText(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.DataObject.GetDataPresent(typeof(string)) || e.DataObject.GetData(typeof(string)) is not string text || InvalidText(text))
                e.CancelCommand();
        }

        private void NextElement(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
            if (e.Key != Key.Return && e.Key != Key.Tab) return;

            e.Handled = true;
            if (sender == VolumeNumber || sender == VolumeSpecialName)
            {
                VolumeDetails.Focus();
            }
            else
            {
                if (e.Key == Key.Tab)
                {
                    if (VolumeNumber.IsEnabled) VolumeNumber.Focus();
                    else VolumeSpecialName.Focus();
                    return;
                }
                var result = MessageBox.Show("Add new Volume?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;
                if (Save()) Close();
            }
        }
        #endregion

        #region Closing
        private void ClosingEvent(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is not Volume volume) return;


            if (volume.CollectionId == Guid.Empty) //New volume
            {
                if (!NumberHasContent() && !SpecialHasContent() && string.IsNullOrWhiteSpace(VolumeDetails.Text))
                    return; //Just exit in case important things are empty
            }
            else //Update volume
            {
                if (!collectionRepository.HasChanges())
                    return; //If it doesn't have changes just exit 
            }

            var result = MessageBox.Show($"Do you wanna save the changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) e.Cancel = !Save();
            if (result == MessageBoxResult.No && volume.CollectionId != Guid.Empty) collectionRepository.DiscardChanges();
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Volume volume || volume.Collection is not { } collection) return;
            var result = MessageBox.Show("Are you sure you want to delete this volume?", "Are you sure?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            collectionRepository.RemoveVolume(collection, volume);
            collectionRepository.Save();
            Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            collectionRepository.DiscardChanges();
            DataContext = null;
            Close();
        }
        #endregion
    }
}
