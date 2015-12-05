using NonFactors.Mvc.Lookup;
using System;
using Xunit;

namespace Mvc.Lookup.Tests.Unit
{
    public class LookupColumnTests
    {
        #region Constructor: LookupColumn(String key, String header, String cssClass = "")

        [Fact]
        public void Add_OnNullKeyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new LookupColumn(null, ""));
        }

        [Fact]
        public void Add_OnNullHeaderThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new LookupColumn("", null));
        }

        [Fact]
        public void Add_OnNullCssClass()
        {
            Assert.Throws<ArgumentNullException>(() => new LookupColumn("", "", null));
        }

        [Fact]
        public void LookupColumn_SetsKey()
        {
            String actual = new LookupColumn("TestKey", "").Key;
            String expected = "TestKey";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_SetsHeader()
        {
            String actual = new LookupColumn("", "TestHeader").Header;
            String expected = "TestHeader";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_SetsCssClass()
        {
            String actual = new LookupColumn("", "", "TestCss").CssClass;
            String expected = "TestCss";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
