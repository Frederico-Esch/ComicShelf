using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Interfaces
{
    public interface IRearrangeableCollection<T>
    {
        public void RemoveAt(int index);
        public void InsertAt(int index, T value);
        public T? At(int index);
    }
}
