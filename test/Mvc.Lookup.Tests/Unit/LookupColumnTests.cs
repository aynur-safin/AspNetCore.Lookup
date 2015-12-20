using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnTests
    {
        #region Constructor: LookupColumn(String name, String header, String cssClass = "")

        [Fact]
        public void Add_NullName_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupColumn(null, ""));

            Assert.Equal("name", actual.ParamName);
        }

        [Fact]
        public void Add_NullHeader_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupColumn("", null));

            Assert.Equal("header", actual.ParamName);
        }

        [Fact]
        public void Add_NullCssClass_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupColumn("", "", null));

            Assert.Equal("cssClass", actual.ParamName);
        }

        [Fact]
        public void LookupColumn_SetsName()
        {
            String actual = new LookupColumn("Test", "").Name;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_SetsHeader()
        {
            String actual = new LookupColumn("", "Test").Header;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupColumn_SetsCssClass()
        {
            String actual = new LookupColumn("", "", "Test").CssClass;
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
