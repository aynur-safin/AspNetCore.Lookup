using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupExceptionTests
    {
        #region LookupException(String message)

        [Fact]
        public void LookupException_SetsMessage()
        {
            String actual = new LookupException("Test").Message;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
