﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Diz.Core.model
{
    public class RomBytes : IEnumerable<RomByte>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private ObservableCollection<RomByte> bytes;

        // TODO: might be able to do something more generic now that other refactorings are completed.
        //
        // This class needs to do these things that are special:
        // 1) Be handled specially by our custom XML serializer (compresses to save disk space)
        // 2) Handle Equals() by comparing each element in the list (SequenceEqual)
        private ObservableCollection<RomByte> Bytes
        {
            get => bytes;
            set
            {
                bytes = value;

                bytes.CollectionChanged += Bytes_CollectionChanged;
                foreach (var romByte in bytes)
                {
                    romByte.PropertyChanged += RomByteObjectChanged;
                }
            }
        }

        public RomByte this[int i]
        {
            get => Bytes[i];
            set => Bytes[i] = value;
        }

        public RomBytes()
        {
            Bytes = new ObservableCollection<RomByte>();
        }

        public void SetFrom(RomByte[] romBytes)
        {
            Bytes = new ObservableCollection<RomByte>(romBytes);
        }

        public int Count => Bytes.Count;
        public bool SendNotificationChangedEvents { get; set; } = true;

        public void Add(RomByte romByte)
        {
            Bytes.Add(romByte);
            romByte.SetCachedOffset(Bytes.Count - 1); // I don't love this....
        }

        public void Create(int size)
        {
            for (var i = 0; i < size; ++i)
                Add(new RomByte());
        }
        public void Clear()
        {
            Bytes.Clear();
        }

        #region Equality
        protected bool Equals(RomBytes other)
        {
            return Bytes.SequenceEqual(other.Bytes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RomBytes)obj);
        }

        public override int GetHashCode()
        {
            return (Bytes != null ? Bytes.GetHashCode() : 0);
        }
        #endregion

        #region Enumerator
        public IEnumerator<RomByte> GetEnumerator()
        {
            return Bytes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private void Bytes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (RomByte item in e.NewItems)
                    item.PropertyChanged += RomByteObjectChanged;

            if (e.OldItems != null)
                foreach (RomByte item in e.OldItems)
                    item.PropertyChanged -= RomByteObjectChanged;

            if (SendNotificationChangedEvents)
                CollectionChanged?.Invoke(sender, e);
        }

        private void RomByteObjectChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SendNotificationChangedEvents)
                PropertyChanged?.Invoke(sender, e);
        }
    }
}
