using Moq;
using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class AbstractLookupTests
    {
        private AbstractLookup lookup;

        public AbstractLookupTests()
        {
            lookup = new Mock<AbstractLookup>().Object;
        }

        #region Constants

        [Fact]
        public void Prefix_IsConstant()
        {
            Assert.True(typeof(AbstractLookup).GetField("Prefix").IsLiteral);
            Assert.Equal("Lookup", AbstractLookup.Prefix);
        }

        [Fact]
        public void IdKey_IsConstant()
        {
            Assert.True(typeof(AbstractLookup).GetField("IdKey").IsLiteral);
            Assert.Equal("LookupIdKey", AbstractLookup.IdKey);
        }

        [Fact]
        public void AcKey_IsConstant()
        {
            Assert.True(typeof(AbstractLookup).GetField("AcKey").IsLiteral);
            Assert.Equal("LookupAcKey", AbstractLookup.AcKey);
        }

        #endregion

        #region Constructor: AbstractLookup()

        [Fact]
        public void AbstractLookup_SetsDialogTitle()
        {
            String expected = lookup.GetType().Name.Replace(AbstractLookup.Prefix, "");
            String actual = lookup.DialogTitle;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_SetsLookupUrl()
        {
            Assert.Null(lookup.LookupUrl);
        }

        [Fact]
        public void AbstractLookup_SetsDefaultSortColumn()
        {
            Assert.Null(lookup.DefaultSortColumn);
        }

        [Fact]
        public void AbstractLookup_SetsDefaultRecordsPerPage()
        {
            UInt32 actual = lookup.DefaultRecordsPerPage;
            UInt32 expected = 20;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_SetsAdditionalFilters()
        {
            Assert.Empty(lookup.AdditionalFilters);
        }

        [Fact]
        public void AbstractLookup_SetsDefaultSortOrder()
        {
            LookupSortOrder actual = lookup.DefaultSortOrder;
            LookupSortOrder expected = LookupSortOrder.Asc;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_SetsColumns()
        {
            Assert.Empty(lookup.Columns);
        }

        [Fact]
        public void AbstractLookup_SetsCurrentFilter()
        {
            Assert.NotNull(lookup.CurrentFilter);
        }

        #endregion
    }
}
