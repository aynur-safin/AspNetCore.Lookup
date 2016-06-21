using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupAttributeTests
    {
        #region LookupAttribute(Type type)

        [Fact]
        public void LookupAttribute_Null_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupAttribute(null));

            Assert.Equal("type", actual.ParamName);
        }

        [Fact]
        public void LookupAttribute_NotLookup_Throws()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new LookupAttribute(typeof(Object)));

            String expected = $"'{typeof (Object).Name}' type does not implement '{typeof (MvcLookup).Name}'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupAttribute_Type()
        {
            Type actual = new LookupAttribute(typeof(MvcLookup)).Type;
            Type expected = typeof(MvcLookup);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
