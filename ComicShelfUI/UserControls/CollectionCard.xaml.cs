using System;
using System.Collections.Generic;
using System.IO;
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
using Domain;
using Utils.Extensions;

namespace ComicShelfUI.UserControls
{
    /// <summary>
    /// Interação lógica para CollectionCard.xam
    /// </summary>
    public partial class CollectionCard : UserControl
    {
        public CollectionCard()
        {
            InitializeComponent();
        }

        private void LoadCover(byte[] cover)
        {
            var bitmap = new BitmapImage();
            bitmap.CreateFrom(cover);
            Cover.Source = bitmap;
        }

        private void CollectionCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Collection collection) return;

            var ownCount = collection.Volumes.Where(v => v.IsOwned).Count();
            OwnCount.Text = ownCount.ToString();
            WantCount.Text = (collection.Volumes.Count - ownCount).ToString();

            if (collection.Cover == null || collection.Cover.Length <= 0) return;
            LoadCover(collection.Cover);
        }
    }
}
