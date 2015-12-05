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
            Assert.Equal("Lookup", AbstractLookup.Prefix);
        }

        [Fact]
        public void IdKey_IsConstant()
        {
            Assert.Equal("LookupIdKey", AbstractLookup.IdKey);
        }

        [Fact]
        public void AcKey_IsConstant()
        {
            Assert.Equal("LookupAcKey", AbstractLookup.AcKey);
        }

        #endregion

        #region Constructor: AbstractLookup()

        [Fact]
        public void AbstractLookup_DefaultDialogTitle()
        {
            String expected = lookup.GetType().Name.Replace(AbstractLookup.Prefix, "");
            String actual = lookup.DialogTitle;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_DefaultLookupUrl()
        {
            Assert.Null(lookup.LookupUrl);
        }

        [Fact]
        public void AbstractLookup_NullDefaultSortColumn()
        {
            Assert.Null(lookup.DefaultSortColumn);
        }

        [Fact]
        public void AbstractLookup_DefaultDefaultRecordsPerPage()
        {
            UInt32 actual = lookup.DefaultRecordsPerPage;
            UInt32 expected = 20;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_EmptyAdditionalFilters()
        {
            Assert.Empty(lookup.AdditionalFilters);
        }

        [Fact]
        public void AbstractLookup_AscDefaultSortOrder()
        {
            LookupSortOrder actual = lookup.DefaultSortOrder;
            LookupSortOrder expected = LookupSortOrder.Asc;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AbstractLookup_EmptyColumns()
        {
            Assert.Empty(lookup.Columns);
        }

        [Fact]
        public void AbstractLookup_NotNullCurrentFilter()
        {
            Assert.NotNull(lookup.CurrentFilter);
        }

        #endregion
    }
}
