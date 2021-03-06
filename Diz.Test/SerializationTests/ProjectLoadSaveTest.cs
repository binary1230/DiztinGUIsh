﻿using System.Diagnostics;
using System.Linq;
using Diz.Core.model;
using Diz.Core.serialization;
using Diz.Core.serialization.xml_serializer;
using Diz.Core.util;
using Diz.Test.Tests.RomInterfaceTests;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Diz.Test.SerializationTests
{
    public class LoadSaveTest
    {
        [Fact(Skip="This actually works, just takes too long right now. perf tune it.")]
        private void FullSerializeAndDeserialize()
        {
            // use the sample data to fake a project
            var srcProject = BuildSampleProject2();
            
            var expectedTitle = SampleRomData.GetSampleUtf8CartridgeTitle();
            
            srcProject.Data.SnesAddressSpace.GetAnnotationCountInAllChildren<Comment>().Should().BeGreaterThan(0);
            srcProject.Data.SnesAddressSpace.GetAnnotationCountInAllChildren<Label>().Should().BeGreaterThan(0);

            // extract the bytes that would normally be in the SMC file (they only exist in code for this sample data)
            var romFileBytes = srcProject.Data.RomByteSource.Bytes.Select(e=>e?.Byte ?? 0).ToList().ToList();

            // save it to create an output byte stream, we'd normally write this to the disk
            var serializer = new ProjectXmlSerializer();
            var outputBytes = serializer.Save(srcProject);

            // now do the reverse and load our output back as the input
            var (deserializedProject, warning) = serializer.Load(outputBytes);
            
            // final step, the loading process doesn't save the actual SMC file bytes, so we do it ourselves here
            deserializedProject.Project.Data.RomByteSource.SetBytesFrom(romFileBytes);

            // now we can do a full compare between the original project, and the project which has been cycled through
            // serialization and deserialization
            Assert.True(warning == "");
            Assert.True(srcProject.Equals(deserializedProject.Project));
            
            deserializedProject.Project.Data.SnesAddressSpace.GetAnnotationCountInAllChildren<Comment>().Should().BeGreaterThan(0);
            deserializedProject.Project.Data.SnesAddressSpace.GetAnnotationCountInAllChildren<Label>().Should().BeGreaterThan(0);
            
            CartNameTests.TestRomCartTitle(deserializedProject.Project, expectedTitle);
        }

        public static Project BuildSampleProject2()
        {
            var project2 = new Project
            {
                Data = SampleRomData.CreateSampleData().Data,
            };
            
            project2.CacheVerificationInfo();
            
            return project2;
        }

        private readonly ITestOutputHelper output;

        public LoadSaveTest(ITestOutputHelper output)
        {
            this.output = output;
        }
        
        public static Project OpenProject(string openFile)
        {
            var projectFileManager = new ProjectFileManager();
            var (project, warning) = projectFileManager.Open(openFile);

            Assert.Equal("", warning);
            Assert.True(project.Data.RomByteSource.Bytes.Count >= 0x1000 * 64);
            
            return project;
        }

        [Fact(Skip = "Performance Test")]
        private void OpenFilePerformanceTest()
        {
            var s = Stopwatch.StartNew();

            var openFile = "INSERT YOUR FILE HERE BEFORE RUNNING THIS TEST.dizraw";
            var project = OpenProject(openFile);

            s.Stop();

            output.WriteLine($"runtime: {s.ElapsedMilliseconds:N0}, #bytes={project.Data.GetRomSize()}");
        }


        [Fact(Skip = "Performance Test")]
        private void SaveFilePerformanceTest()
        {
            var openFile = "INSERT YOUR FILE HERE BEFORE RUNNING THIS TEST.dizraw";
            var project = OpenProject(openFile);
            
            var s = Stopwatch.StartNew();

            var data = new ProjectXmlSerializer().Save(project);

            s.Stop();
            
            Assert.True(data.Length != 0);

            output.WriteLine($"runtime: {s.ElapsedMilliseconds:N0}");
        }
    }
}
