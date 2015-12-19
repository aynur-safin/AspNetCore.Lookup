using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupAttributeTests
    {
        #region Constructor: LookupAttribute(Type type)

        [Fact]
        public void LookupAttribute_NullType_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => new LookupAttribute(null));

            Assert.Equal("type", actual.ParamName);
        }

        [Fact]
        public void LookupAttribute_NotLookupType_Throws()
        {
            ArgumentException exception = Assert.Throws<ArgumentException>(() => new LookupAttribute(typeof(Object)));

            String expected = String.Format("'{0}' type does not implement '{1}'.",
                typeof(Object).Name, typeof(MvcLookup).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupAttribute_SetsType()
        {
            Type actual = new LookupAttribute(typeof(MvcLookup)).Type;
            Type expected = typeof(MvcLookup);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
