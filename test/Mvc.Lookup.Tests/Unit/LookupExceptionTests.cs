using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupExceptionTests
    {
        #region LookupException(String message)

        [Fact]
        public void LookupException_Message()
        {
            Assert.Equal("Test", new LookupException("Test").Message);
        }

        #endregion
    }
}
