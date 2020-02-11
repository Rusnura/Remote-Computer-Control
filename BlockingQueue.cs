using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteComputerControl
{
    public class BlockingQueue<T>
    {
        private List<T> synchronizedList = new List<T>();
        private object monitor = new object();

        public T get(int index) 
        {
            lock (monitor)
            {
                return synchronizedList[index];
            }
        }

        public List<T> getAll()
        {
            lock (monitor)
            {
                return synchronizedList;
            }
        }

        public void add(T element)
        {
            lock (monitor)
            {
                synchronizedList.Add(element);
            }
        }

        public void addAll(IEnumerable<T> otherCollection)
        {
            lock (monitor)
            {
                synchronizedList.AddRange(otherCollection);
            }
        }

        public void insertInto(IEnumerable<T> otherCollection, int index)
        {
            lock (monitor)
            {
                synchronizedList.InsertRange(index, otherCollection);
            }
        }

        public void remove(int index)
        {
            lock (monitor)
            {
                synchronizedList.RemoveAt(index);
            }
        }

        public void remove(T element)
        {
            lock (monitor)
            {
                synchronizedList.Remove(element);
            }
        }

        public void clearAll()
        {
            lock (monitor)
            {
                synchronizedList.Clear();
            }
        }

        public void clearAllAndPutCollection(IEnumerable<T> otherCollection)
        {
            lock (monitor)
            {
                synchronizedList.Clear();
                synchronizedList.AddRange(otherCollection);
            }
        }
    }
}
