﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Xml.Serialization;
using Diz.Core.util;
using JetBrains.Annotations;

namespace Diz.Core.model.byteSources
{
    // JUST holds the data. no graph traversal.
    public partial class ByteEntry : IParentReferenceTo<Storage<ByteEntry>>, INotifyPropertyChangedExt
    {
        // TODO: without the memory optimization of on-demand Annotation creation on the fly,
        // this can be removed and regular object initialization handles this fine now.
        public ByteEntry(AnnotationCollection annotationsToAppend)
        {
            // during initialization, append in case other object initializers have added annotations
            // via the other properties
            AppendAnnotationsFrom(annotationsToAppend, true);
        }

        [XmlIgnore]
        public bool DontSetParentOnCollectionItems
        {
            get => dontSetParentOnCollectionItems;
            init
            {
                this.SetField(ref dontSetParentOnCollectionItems, value);
                if (Annotations != null)
                    Annotations.DontSetParentOnCollectionItems = dontSetParentOnCollectionItems;
            }
        }
        private readonly bool dontSetParentOnCollectionItems;

        // future optimization: since we might have a lot of empty ByteEntry hanging around,
        // don't allocate this immediately,instead do it on-demand.
        // right now we're not doing that because it interacts badly with the serializer. 
        public AnnotationCollection Annotations
        {
            get => GetOrCreateAnnotationsList();
            [UsedImplicitly] 
            set
            {
                this.SetField(ref annotations, value);
                FixupAnnotationItems();
            }
        }
        private AnnotationCollection annotations;

        private void FixupAnnotationItems()
        {
            Annotations.DontSetParentOnCollectionItems = DontSetParentOnCollectionItems;
            if (!DontSetParentOnCollectionItems)
                Annotations.Parent = this;
        }

        // note: our thread safety isn't comprehensive in this project yet.
        // be careful with this if you're doing anything clever, especially writing.
        // TODO: instead of doing this, see if ConcurrentBag or similar classes for the container itself would work?
        public ReaderWriterLockSlim Lock => @lock ??= new ReaderWriterLockSlim();
        private ReaderWriterLockSlim @lock;
        private int parentIndex;
        private Storage<ByteEntry> parent;

        #region References to parent enclosures

        // helper
        // TODO: re-enable (get it working with serialization)
        // [XmlIgnore] public ByteSource ParentByteSource => Parent?.Parent;

        // real stuff
        [XmlIgnore]
        public int ParentIndex
        {
            get => parentIndex;
            set => this.SetField(ref parentIndex, value);
        }

        [XmlIgnore]
        public Storage<ByteEntry> Parent
        {
            get => parent;
            set => this.SetField(ref parent, value);
        }

        #endregion

        public AnnotationCollection GetOrCreateAnnotationsList()
        {
            return annotations ??= new AnnotationCollection
            {
                DontSetParentOnCollectionItems = DontSetParentOnCollectionItems,
                Parent = this
            };
        }

        protected bool AnnotationsEffectivelyEqual(ByteEntry other) => 
            AnnotationCollection.EffectivelyEqual(Annotations, other?.Annotations);

        public T GetOneAnnotation<T>() where T : Annotation => Annotations?.GetOne<T>();
        public T GetOrCreateAnnotation<T>() where T : Annotation, new() => 
            GetOrCreateAnnotationsList().GetOrCreateOne<T>();
        public void AddAnnotation(Annotation newAnnotation) => GetOrCreateAnnotationsList().Add(newAnnotation);
        public void RemoveOneAnnotationIfExists<T>() where T : Annotation => Annotations?.RemoveOneIfExists<T>();
        
        public void ReplaceAnnotationsWith(AnnotationCollection itemsToReplaceWith)
        {
            Annotations?.Clear();
            AppendAnnotationsFrom(itemsToReplaceWith);
        }
        
        private void AppendAnnotationsFrom(IReadOnlyList<Annotation> itemsToAppend, bool overrideParentCheck = false)
        {
            // this must be set so that any Annotation references we pick up here retain their original .Parent
            if (!DontSetParentOnCollectionItems && !overrideParentCheck)
                throw new InvalidOperationException("DontSetParentOnCollectionItems must be true to aggregate annotations");
            
            if (itemsToAppend == null)
                return;

            GetOrCreateAnnotationsList();
            Annotations.DontSetParentOnCollectionItems = overrideParentCheck || DontSetParentOnCollectionItems;
            Annotations.CombineWith(itemsToAppend);
            Annotations.DontSetParentOnCollectionItems = DontSetParentOnCollectionItems;
            Annotations.Parent = this;
        }

        // really, use this only when doing graph building stuff.  
        public void AppendAnnotationsFrom(ByteEntry lowerPriorityByteEntry) => 
            AppendAnnotationsFrom(lowerPriorityByteEntry?.Annotations);

        #region Equality

        protected bool Equals(ByteEntry other)
        {
            // customized code, not auto-generated.
            // we specifically ignore ParentIndex and Parent, as comparing those could get us in recursive trouble
            return AnnotationsEffectivelyEqual(other) &&
                   DontSetParentOnCollectionItems == other.DontSetParentOnCollectionItems;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ByteEntry) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Annotations, DontSetParentOnCollectionItems);
        }

        #endregion

        public ByteEntry() {}
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
