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
        public delegate void Reordered(int previousIndex, int newIndex);
        public event Reordered? OnReorder;

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            dropInfo.Effects = System.Windows.DragDropEffects.Copy;
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.VisualTarget is ItemsControl { ItemsSource: T collections })
            {
                if (dropInfo.Data is Collection collection)
                    collections.InsertAt(dropInfo.InsertIndex, collection);
                OnReorder?.Invoke(dropInfo.DragInfo.SourceIndex, dropInfo.InsertIndex);
            }
        }
    }
}
