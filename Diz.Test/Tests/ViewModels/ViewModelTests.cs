using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Diz.Core.model;
using Diz.Core.model.byteSources;
using Diz.ViewModels;
using DynamicData;
using DynamicData.Binding;
using FluentAssertions;
using Moq;
using ReactiveUI;
using Xunit;

namespace Diz.Test.Tests.ViewModels
{
    public class ViewModelTests
    {
       /* public interface IDummy
        {
            void DummyMethod(object observable);
            void DummyMethod<TRet>(TRet obj);
        }

        [Fact]
        public void TestByteEntriesViewModelTwoCopies()
        {
            var vm1 = new ByteEntriesViewModel();
            var vm2 = new ByteEntriesViewModel();
            vm1.ByteEntries.Count.Should().BeGreaterThan(1);
            vm2.ByteEntries.Count.Should().BeGreaterThan(1);

            var byteAnnotation1 = vm1.ByteEntries[0].ByteEntry.ByteEntry.Annotations.GetOne<ByteAnnotation>();
            byteAnnotation1.Should().NotBeNull();
            
            byteAnnotation1.Val = 77;
            
            var byteAnnotation2 = vm1.ByteEntries[0].ByteEntry.ByteEntry.Annotations.GetOne<ByteAnnotation>();
            byteAnnotation2.Val.Should().Be(77);

            var m = new Mock<IDummy>();
            
            m.Setup(x => x
                .DummyMethod(
                    It.IsAny<ObservableCollection<ByteEntryDetailsViewModel>>())
            );

            vm1.WhenAnyValue(x => x.ByteEntries)
                .Subscribe(m.Object.DummyMethod);

            byteAnnotation1.Val = 22;

            m.Verify(
                mock => mock.DummyMethod(null),
                Times.Once
            );
        }
        
        [Fact]
        public void TestByteEntriesViewModelTwoCopies2()
        {
            // TODO: use DynamicData for this then.
            
            var byteAnnotation = new ByteAnnotation();
            
            var m = new Mock<IDummy>();
            
            m.Setup(x => x
                .DummyMethod(
                    It.IsAny<ObservableCollection<ByteEntryDetailsViewModel>>())
            );

            byteAnnotation.WhenAnyValue(x => x.Val)
                .Subscribe(m.Object.DummyMethod);

            byteAnnotation.Val = 22;

            m.Verify(
                mock => mock.DummyMethod(null),
                Times.Once
            );
        }*/

        [Fact]
        void Test3()
        {
            var sourceAnnotationCollection = new AnnotationCollection();

            var observableAnnotations = sourceAnnotationCollection
                .ToObservableChangeSet()
                .AsObservableList();

            // We expose the Connect() since we are interested in a stream of changes.
            // If we have more than one subscriber, and the subscribers are known, 
            // it is recommended you look into the Reactive Extension method Publish().
            
            // With DynamicData you can easily manage mutable datasets,
            // even if they are extremely large. In this complex scenario 
            // a service mutates the collection, by using .Add(), .Remove(), 
            // .Clear(), .Insert(), etc. DynamicData takes care of
            // allowing you to observe all of those changes.
            sourceAnnotationCollection.Add(new ByteAnnotation {Val = 222});
            sourceAnnotationCollection.RemoveAt(0);
            sourceAnnotationCollection.Add(new ByteAnnotation {Val = 199});
            sourceAnnotationCollection.Add(new BranchAnnotation {Point = InOutPoint.InPoint});

            observableAnnotations.Connect()
                // Transform in DynamicData works like Select in
                // LINQ, it observes changes in one collection, and
                // projects it's elements to another collection.
                // .Transform(x => !x)
                // Filter is basically the same as .Where() operator
                // from LINQ. See all operators in DynamicData docs.
                // .Filter(x => x)
                // Ensure the updates arrive on the UI thread.
                .ObserveOn(RxApp.MainThreadScheduler)
                // We .Bind() and now our mutable Items collection 
                // contains the new items and the GUI gets refreshed.
                .Bind(out var observedItems)
                .Subscribe();

            var expected = new List<Annotation>
            {
                new ByteAnnotation {Val = 199}, 
                new BranchAnnotation {Point = InOutPoint.InPoint}
            };
            observedItems.Should().Equal(expected);
            
            sourceAnnotationCollection.RemoveAt(0);
            expected.RemoveAt(0);
            observedItems.Should().Equal(expected);

            ((BranchAnnotation) sourceAnnotationCollection[0]).Point = ((BranchAnnotation) expected[0]).Point = InOutPoint.ReadPoint;
            observedItems.Should().Equal(expected);
        }

        private static ByteEntry CreateSampleEntry() => new()
        {
            Byte = 0xCA, MFlag = true, DataBank = 0x80, DirectPage = 0x2100,
            Annotations = {new Label {Name = "SomeLabel"}, new Comment {Text = "This is a comment"}}
        };

        private static StorageList<ByteEntry> CreateSampleByteList()
        {
            var sample2 = CreateSampleEntry();
            sample2.Annotations.Add(new BranchAnnotation {Point = InOutPoint.OutPoint});
            sample2.Annotations.Add(new MarkAnnotation {TypeFlag = FlagType.Opcode});

            return new StorageList<ByteEntry>(new List<ByteEntry>
                {CreateSampleEntry(), sample2});
        }

        [Fact]
        void Test5()
        {
            AnnotationCollection MakeAnnotations()
            {
                return new()
                {
                    new BranchAnnotation {Point = InOutPoint.OutPoint},
                    new MarkAnnotation {TypeFlag = FlagType.Opcode},
                };
            }

            var underlyingByteStorage = new ObservableCollection<ByteEntry>();
            
            underlyingByteStorage.Add(new ByteEntry {
                Annotations = MakeAnnotations()
            });
            underlyingByteStorage.Add(CreateSampleEntry());


            var expectedByteStorage = new List<ByteEntry>()
            {
                new()
                {
                    Annotations = MakeAnnotations()
                }   
            };
            expectedByteStorage.Add(CreateSampleEntry());

            var observable = underlyingByteStorage
                .ToObservableChangeSet()
                .AsObservableList();
            
            observable.Connect()
                // Transform in DynamicData works like Select in
                // LINQ, it observes changes in one collection, and
                // projects it's elements to another collection.
                // .Transform(x => !x)
                // Filter is basically the same as .Where() operator
                // from LINQ. See all operators in DynamicData docs.
                // .Filter(x => x)
                // Ensure the updates arrive on the UI thread.
                .ObserveOn(RxApp.MainThreadScheduler)
                // We .Bind() and now our mutable Items collection 
                // contains the new items and the GUI gets refreshed.
                .Bind(out var observedItems)
                .Subscribe(set => Console.WriteLine(set.ToString()));
            
            observedItems.Should().Equal(expectedByteStorage);

            var called = false;
            underlyingByteStorage.CollectionChanged += (sender, args) => { called = true; };

            expectedByteStorage.RemoveAt(0);
            underlyingByteStorage.RemoveAt(0);

            called.Should().Be(true);

            observedItems.Should().ContainSingle();
            observedItems.Should().Equal(expectedByteStorage);
        }
    }
}