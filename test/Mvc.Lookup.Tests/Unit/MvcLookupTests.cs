﻿using NSubstitute;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupTests
    {
        [Fact]
        public void MvcLookup_Defaults()
        {
            MvcLookup actual = Substitute.For<MvcLookup>();

            Assert.Empty(actual.AdditionalFilters);
            Assert.NotNull(actual.Filter);
            Assert.Empty(actual.Columns);
        }
    }
}
