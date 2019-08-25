using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupExceptionTests
    {
        [Fact]
        public void LookupException_Message()
        {
            Assert.Equal("Test", new LookupException("Test").Message);
        }
    }
}
