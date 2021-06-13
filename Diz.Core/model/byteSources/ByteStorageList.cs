using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Diz.Core.model.byteSources
{
    /// <summary>
    /// Simple version of byte storage that stores everything as an actual list
    /// Use for linear filled data (like Roms), don't use for mostly empty large storage (like SNES Address space)
    /// address spaces (24bits of addressable bytes x HUGE data = slowwwww) 
    /// </summary>
    public class StorageList<T> : Storage<T>
        where 
        T : IParentReferenceTo<Storage<T>>, new()
    {
        public override int Count => Items.Count;

        private ObservableCollection<T> Items
        {
            get
            {
                if (items != null)
                    return items;
                
                items = new ObservableCollection<T>();
                items.CollectionChanged += BytesOnCollectionChanged;

                return items;
            }
        }

        private ObservableCollection<T> items;

        [UsedImplicitly] public StorageList() : base(0) { }
        
        public StorageList(int emptyCreateSize) : base(emptyCreateSize) { }
        
        public StorageList(IReadOnlyCollection<T> inBytes) : base(inBytes) { }
        
        public override IDisposable Subscribe(IObserver<T> observer) => 
            Items.Subscribe(observer);
        
        private void BytesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        protected override void InitEmptyContainer(int capacity)
        {
            Items.Clear();
        }

        protected override void FillEmptyContainerWithBytesFrom(IReadOnlyCollection<T> inBytes)
        {
            ImportBytes(inBytes);
        }
        
        protected override void FillEmptyContainerWithBlankBytes(int numEntries)
        {
            for (var i = 0; i < numEntries; ++i) 
                Add(new T());
        }
        
        public override int IndexOf(T item) => Items.IndexOf(item);
        public override void Insert(int index, T item)
        {
            var existing = index < Items.Count ? Items[index] : default;
            
            Items[index] = item;
            SetParentInfoFor(item, index);
            
            if (existing != null)
                ClearParentInfoFor(item);
        }

        public override void RemoveAt(int index)
        {
            var existing = index < Items.Count ? Items[index] : default;
            
            Items.RemoveAt(index);
            
            if (existing != null)
                OnRemoved(existing);
        }

        public override T this[int index]
        {
            get => Items[index];
            set => Insert(index, value);
        }
        
        public override void Clear()
        {
            OnPreClear();
            Items.Clear();
        }

        public override bool Contains(T item) => Items.Contains(item);
        public override void CopyTo(T[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        public override bool Remove(T val)
        {
            if (!Items.Remove(val))
                return false;

            OnRemoved(val);
            return true;
        }
        
        protected override void UpdateAllParentInfo(bool shouldUnsetAll = false)
        {
            for (var i = 0; i < Items.Count; ++i)
            {
                UpdateParentInfoFor(Items[i], shouldUnsetAll, i);
            }
        }

        public override void CopyTo(Array array, int index) => Items.CopyTo((T[]) array, index);

        public override void Add(T byteOffset)
        {
            Debug.Assert(Items != null);
            
            var newIndex = Count; // will be true once we add it 
            SetParentInfoFor(byteOffset, newIndex);

            Items.Add(byteOffset);
        }
        
        public override IEnumerator<T> GetGaplessEnumerator() => Items.GetEnumerator();
        
        // NOTE: in this implementation, all bytes at all addresses always exist, so,
        // this will never return null or have gaps in the sequence.
        //
        // other implementations can differ.
        public override IEnumerator<T> GetNativeEnumerator() => GetGaplessEnumerator();
        public override ReadOnlyObservableCollection<T> ToObservable() => new(Items);
    }
}