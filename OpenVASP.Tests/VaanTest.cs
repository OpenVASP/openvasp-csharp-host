using OpenVASP.Messaging.Messages.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OpenVASP.Tests
{
    public class VaanTest
    {
        private readonly ITestOutputHelper testOutputHelper;

        public VaanTest(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void VaanIsCorrectTest()
        {
            string expectedVaan = "bb428798524ee3fb082809d3";
            VirtualAssetssAccountNumber vaan = VirtualAssetssAccountNumber.Create("bb428798", "524ee3fb082809");

            Assert.Equal(expectedVaan, vaan.Vaan);
        }
    }
}
