using NonFactors.Mvc.Lookup;
using System;
using Xunit;

namespace Mvc.Lookup.Tests.Unit
{
    public class LookupExceptionTests
    {
        #region Constructor: LookupException(String message)

        [Fact]
        public void LookupException_SetsMessage()
        {
            String actual = new LookupException("T").Message;
            String expected = "T";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
