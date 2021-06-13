using Diz.Core.model;
using Diz.Core.util;
using Diz.Test.Utils;
using Diz.Test.Utils.SuperFamiCheckUtil;
using FluentAssertions;
using Xunit;

namespace Diz.Test.Tests.RomInterfaceTests
{
    public static class CheckSumTests1
    {
        [FactOnlyIfFilePresent(new[] {SuperFamiCheckTool.Exe, CartNameTests.RomFileName})]
        public static void TestInternalChecksumVsExternal()
        {
            const uint expected4ByteChecksums = 0x788C8773;

            var toolResult = CheckSumTestUtils.ImportFromExternalTool(expected4ByteChecksums);
            var project = CheckSumTestUtils.ImportRomAndCalcChecksum(out var complement, out var checksum);

            TestChecksums(project, expected4ByteChecksums, toolResult, complement, checksum);

            project.Data.ComputeChecksum().Should().Be((ushort) checksum);

            // SNES docs dictate:
            // 15. Complement Check (0xFFDC, 0xFFDD)
            // 16. Check Sum (0xFFDE, 0xFFDF)

            // in the actual ROM file, it appears like this (remember: little endian for SNES)
            // complement   checksum
            // 73 87        8C 78
        }
        
        internal static void TestChecksums(Project project, uint expected4ByteChecksums, SuperFamiCheckTool.Result toolResult, int? complement, int? checksum)
        {
            project.Data.GetRomDoubleWord(0xFFDC).Should().Be((int) expected4ByteChecksums); // complement 16bit
            project.Data.RomCheckSumsFromRomBytes.Should().Be(expected4ByteChecksums);
            project.InternalCheckSum.Should().Be(expected4ByteChecksums);
            toolResult.Complement.Should().Be((uint) complement);
            toolResult.Checksum.Should().Be((uint) checksum);
        }
    }

    public static class CheckSumTests2
    {
        [FactOnlyIfFilePresent(new[] {CartNameTests.RomFileName})]
        public static void ModifyChecksum()
        {
            var project = CheckSumTestUtils.ImportRomAndCalcChecksum(out var complement, out var checksum);

            // mess with the first byte, recompute a valid checksum
            var firstByte = project.Data[0x00];
            firstByte.Should().NotBe(0);
            project.Data[0x00] = 0;
            project.Data.ComputeIsChecksumValid().Should().Be(false);
            project.Data.FixChecksum();
            project.Data.ComputeIsChecksumValid().Should().Be(true);
            
            complement = project.Data.GetRomWord(0xFFDC);
            complement.Should().NotBe(0x8773); // complement 16bit

            checksum = project.Data.GetRomWord(0xFFDE);
            checksum.Should().NotBe(0x788c); // checksum 16bit
        }
    }

    internal static class CheckSumTestUtils
    {
        internal static SuperFamiCheckTool.Result ImportFromExternalTool(uint expected4ByteChecksums)
        {
            var result = SuperFamiCheckTool.Run(CartNameTests.RomFileName);
            result.Complement.Should().Be(0x8773);
            result.Checksum.Should().Be(0x788c);
            (result.Complement + result.Checksum).Should().Be(0xFFFF);
            result.AllCheckBytes.Should().Be(expected4ByteChecksums);
            return result;
        }
        
        internal static Project ImportRomAndCalcChecksum(out int? complement, out int? checksum)
        {
            var project = ImportUtils.ImportRomAndCreateNewProject(CartNameTests.RomFileName);
            
            project.Should().NotBeNull("project should have loaded successfully");
            project.Data.GetRomByte(0xFFDC).Should().Be(0x73); // complement 1
            project.Data.GetRomByte(0xFFDD).Should().Be(0x87); // complement 2
            project.Data.GetRomByte(0xFFDE).Should().Be(0x8C); // checksum 1
            project.Data.GetRomByte(0xFFDF).Should().Be(0x78); // checksum 2

            complement = project.Data.GetRomWord(0xFFDC);
            complement.Should().Be(0x8773); // complement 16bit

            checksum = project.Data.GetRomWord(0xFFDE);
            checksum.Should().Be(0x788c); // checksum 16bit

            (complement + checksum).Should().Be(0xFFFF);
            return project;
        }
    }
}