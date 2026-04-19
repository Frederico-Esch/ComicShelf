using Domain;
using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utils.Collections;
using Utils.Interfaces;

namespace Utils.DragAndDrop
{
    public class DropHandlerForCollectionList <T> : IDropTarget
        where T : class, IRearrangeableCollection<Collection>
    {
        private Collection? hovering;

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
            var collections = (dropInfo.VisualTarget as ItemsControl)?.ItemsSource as T;
            var index = dropInfo.DragInfo.SourceIndex;
            if (dropInfo.Data is Collection collection && collection.Id == collections?.At(index)?.Id)
            {
                hovering = collection;
                collections?.RemoveAt(index);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget is ItemsControl { ItemsSource: T collections })
            {
                if (hovering is { } collection)
                    collections.InsertAt(dropInfo.InsertIndex, collection);
            }
        }
    }
}
