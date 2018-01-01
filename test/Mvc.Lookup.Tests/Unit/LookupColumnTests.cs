using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnTests
    {
        #region LookupColumn(String key, String header)

        [Fact]
        public void LookupColumn_NullKey_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupColumn(null, "Test"));

            Assert.Equal("key", actual.ParamName);
        }

        [Fact]
        public void LookupColumn_Key()
        {
            String actual = new LookupColumn("Test", null).Key;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_Header()
        {
            String actual = new LookupColumn("Test", "Test").Header;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
