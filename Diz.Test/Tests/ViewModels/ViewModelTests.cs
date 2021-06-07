using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public interface IDummy
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
        }

        [Fact]
        void Test3()
        {
            var data = new AnnotationCollection();

            var sourceItems = data
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
            data.Add(new ByteAnnotation {Val = 222});
            data.RemoveAt(0);
            data.Add(new ByteAnnotation {Val = 199});
            data.Add(new BranchAnnotation {Point = InOutPoint.InPoint});

            sourceItems.Connect()
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

            var expectedItemsList1 = new List<Annotation>
            {
                new ByteAnnotation() {Val = 199}, new BranchAnnotation {Point = InOutPoint.InPoint}
            };
            observedItems.Should().Equal(expectedItemsList1);
            
            data.RemoveAt(0);
            expectedItemsList1.RemoveAt(0);
            observedItems.Should().Equal(expectedItemsList1);

            ((BranchAnnotation) data[0]).Point = ((BranchAnnotation) expectedItemsList1[0]).Point = InOutPoint.ReadPoint;
            observedItems.Should().Equal(expectedItemsList1);
        }
        
         /*[Fact]
        void Test4()
        {
            var sourceItems = new AnnotationCollection
            {
                new ByteAnnotation {Val = 222}
            };

            // We expose the Connect() since we are interested in a stream of changes.
            // If we have more than one subscriber, and the subscribers are known, 
            // it is recommended you look into the Reactive Extension method Publish().

            // With DynamicData you can easily manage mutable datasets,
            // even if they are extremely large. In this complex scenario 
            // a service mutates the collection, by using .Add(), .Remove(), 
            // .Clear(), .Insert(), etc. DynamicData takes care of
            // allowing you to observe all of those changes.
            sourceItems.RemoveAt(0);
            sourceItems.Add(new ByteAnnotation {Val = 199});
            sourceItems.Add(new ByteAnnotation {Val = 200});
            
            sourceItems.Connect()
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

            var expectedItemsList1 = new List<ByteAnnotation>
            {
                new() {Val = 199}, new() {Val = 200}
            };
            observedItems.Should().Equal(expectedItemsList1);
            
            sourceItems.RemoveAt(0);
            expectedItemsList1.RemoveAt(0);
            observedItems.Should().Equal(expectedItemsList1);

            observedItems[0].Val = expectedItemsList1[0].Val = 36;
            observedItems.Should().Equal(expectedItemsList1);
        }*/
    }
}