using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnAttributeTests
    {
        #region LookupColumnAttribute(Int32 position)

        [Fact]
        public void LookupColumnAttribute_Position()
        {
            Assert.Equal(-5, new LookupColumnAttribute(-5).Position);
        }

        #endregion
    }
}
