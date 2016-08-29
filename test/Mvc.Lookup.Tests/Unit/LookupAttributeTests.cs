using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupAttributeTests
    {
        #region LookupAttribute(Type type)

        [Theory]
        [InlineData(null)]
        [InlineData(typeof(Object))]
        public void LookupAttribute_NotLookup_Throws(Type type)
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new LookupAttribute(type));

            String expected = $"'{type?.Name}' type does not implement '{typeof (MvcLookup).Name}'.";
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
