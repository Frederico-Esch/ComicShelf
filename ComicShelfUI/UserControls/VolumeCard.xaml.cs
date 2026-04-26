using Domain;
using Microsoft.VisualBasic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComicShelfUI.UserControls
{
    /// <summary>
    /// Interação lógica para VolumeCard.xam
    /// </summary>
    public partial class VolumeCard : UserControl
    {
        private readonly SolidColorBrush owned = new SolidColorBrush(Color.FromArgb(26, 0, 255, 0));
        private readonly SolidColorBrush wanted = new SolidColorBrush(Color.FromArgb(26, 255, 0, 0));

        public delegate void UpdateCard();
        public event UpdateCard? OnUpdateCard;

        public VolumeCard()
        {
            InitializeComponent();
        }

        private void ChangeBackgroundColor(bool isOwned, bool isUpdate = true)
        {
            BackgroundGrid.Background = isOwned ? owned : wanted;
            if (isUpdate) OnUpdateCard?.Invoke();
        }

        private void CheckClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Volume volume)
                return;
            e.Handled = true;

            //volume.IsOwned = !volume.IsOwned;
            ChangeBackgroundColor(volume.IsOwned);
        }

        private void CardLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Volume volume)
                return;
            
            ChangeBackgroundColor(volume.IsOwned, false);
            Sep.Visibility = volume.HasDetail ? Visibility.Visible : Visibility.Hidden;
        }

        private void ClickedTwice(object sender, MouseButtonEventArgs e)
        {
            IsOwnCheckBox.IsChecked = !IsOwnCheckBox.IsChecked;
        }
    }
}
