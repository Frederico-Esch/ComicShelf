using Domain;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utils.Extensions;

namespace ComicShelfUI.UserControls
{
    /// <summary>
    /// Interação lógica para CollectionVCard.xam
    /// </summary>
    public partial class CollectionVCard : UserControl
    {
        #region Dependency Properties

        public bool HasAnimation
        {
            get { return (bool)GetValue(HasAnimationProperty); }
            set { SetValue(HasAnimationProperty, value); }
        }
        public static readonly DependencyProperty HasAnimationProperty =
            DependencyProperty.Register("HasAnimation", typeof(bool), typeof(CollectionVCard), new UIPropertyMetadata(true));


        public new double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }
        public new static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(double), typeof(CollectionVCard), new UIPropertyMetadata(1.0));


        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register("BorderColor", typeof(Brush), typeof(CollectionVCard), new UIPropertyMetadata(Brushes.SlateGray));


        public new double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public new static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof(double), typeof(CollectionVCard), new PropertyMetadata(15.0));

        #endregion

        public CollectionVCard()
        {
            InitializeComponent();
        }
        
        private void LoadCover(byte[] cover)
        {
            var bitmap = new BitmapImage();
            bitmap.CreateFrom(cover);
            Cover.Source = bitmap;
        }

        public void ReloadOwnCount()
        {
            if (DataContext is not Collection collection) return;

            var ownCount = collection.Volumes.Where(v => v.IsOwned).Count();
            OwnCount.Text = ownCount.ToString();
            WantCount.Text = (collection.Volumes.Count - ownCount).ToString();
        }

        private void CollectionVCard_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not Collection collection) return;

            ReloadOwnCount();

            if (collection.Cover == null || collection.Cover.Length <= 0) return;
            LoadCover(collection.Cover);
        }

        private void CancelAnimation(object sender, System.Windows.Input.MouseEventArgs e) => e.Handled = !HasAnimation;
    }
}
