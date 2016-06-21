using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnTests
    {
        #region LookupColumn(String key, String header)

        [Fact]
        public void Add_NullKey_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupColumn(null, ""));

            Assert.Equal("key", actual.ParamName);
        }

        [Fact]
        public void LookupColumn_Key()
        {
            String actual = new LookupColumn("Test", "").Key;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_Header()
        {
            String actual = new LookupColumn("", "Test").Header;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
