using GongSolutions.Wpf.DragDrop;
using System.Windows;
using System.Windows.Controls;
using Utils.Interfaces;

namespace Utils.DragAndDrop
{
    public class CustomDragHandler<T, C> : IDragSource
        where C : IRearrangeableCollection<T>
    {
        private struct PreviousState
        {
            public T Item;
            public C Collection;
            public int Index;
        }

        private PreviousState? previousState;

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return dragInfo.VisualSource is ItemsControl { ItemsSource: C collections } && collections.At(dragInfo.SourceIndex) is not null;
        }

        public void DragCancelled()
        {

            if (previousState is { } ps)
            {
                ps.Collection.InsertAt(ps.Index, ps.Item);
            }
            previousState = null;
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            previousState = null;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            previousState = null;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            if (previousState is { } ps) { return; }
            if (dragInfo.VisualSource is ItemsControl { ItemsSource: C collections } && collections.At(dragInfo.SourceIndex) is T item)
            {
                dragInfo.Data = item;
                dragInfo.Effects = DragDropEffects.Copy;
                previousState = new PreviousState
                {
                    Item = item,
                    Collection = collections,
                    Index = dragInfo.SourceIndex
                };
                collections.RemoveAt(dragInfo.SourceIndex);
            }
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }
    }
}
