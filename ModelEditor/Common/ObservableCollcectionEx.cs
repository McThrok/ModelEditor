using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Numerics;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace ModelEditor
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {

        public void InsertItemWithoutNotification(int index, T item)
        {
            Items.Insert(index, item);
        }

        public void AddItemWithoutNotification(T item)
        {
            InsertItemWithoutNotification(Count, item);
        }
    }
}