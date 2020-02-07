using NonFactors.Mvc.Lookup.Tests.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupOfTTests
    {
        private TestLookup<TestModel> lookup;

        public MvcLookupOfTTests()
        {
            lookup = new TestLookup<TestModel>();
            lookup.Filter.Rows = 20;

            for (Int32 i = 0; i < 200; i++)
                lookup.Models.Add(new TestModel
                {
                    Id = i + "I",
                    Count = i + 10,
                    ParentId = "1000",
                    Value = $"{i}Value{i + 1}",
                    NextValue = $"{i + 1}Value",
                    Date = new DateTime(2014, 12, 10).AddDays(i)
                });
        }

        [Fact]
        public void GetId_NoProperty()
        {
            Assert.Empty(new TestLookup<NoIdModel>().GetId(new NoIdModel()));
        }

        [Fact]
        public void GetId_Value()
        {
            String actual = lookup.GetId(new TestModel { Id = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetLabel_NoColumns()
        {
            lookup.Columns.Clear();

            Assert.Empty(lookup.GetLabel(new TestModel()));
        }

        [Fact]
        public void GetLabel_Value()
        {
            String actual = lookup.GetLabel(new TestModel { Value = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            IEnumerable<PropertyInfo> actual = lookup.AttributedProperties;
            IEnumerable<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<LookupColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false)!.Position);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MvcLookup_AddsColumns()
        {
            List<LookupColumn> columns = new List<LookupColumn>
            {
                new LookupColumn(nameof(TestModel.Value), "") { Hidden = false, Filterable = true },
                new LookupColumn(nameof(TestModel.NextValue), "") { Hidden = false, Filterable = true },
                new LookupColumn(nameof(TestModel.Date), "Date") { Hidden = false, Filterable = true },
                new LookupColumn(nameof(TestModel.Count), "Counter") { Hidden = false, Filterable = false }
            };

            using IEnumerator<LookupColumn> expected = columns.GetEnumerator();
            using IEnumerator<LookupColumn> actual = lookup.Columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.Hidden, actual.Current.Hidden);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
                Assert.Equal(expected.Current.Filterable, actual.Current.Filterable);
                Assert.Equal(expected.Current.FilterCase, actual.Current.FilterCase);
                Assert.Equal(expected.Current.FilterPredicate, actual.Current.FilterPredicate);
            }
        }

        [Fact]
        public void GetData_FiltersByIds()
        {
            lookup.Filter.Ids.Add("9I");
            lookup.Filter.Ids.Add("15I");
            lookup.Filter.Search = "Term";
            lookup.Filter.Selected.Add("17I");
            lookup.Filter.Sort = nameof(TestModel.Count);
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Value), "5Value");

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("9Value10", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("19", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("9I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("9Value10", actual.Rows[0]["Label"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1][nameof(TestModel.Date)]);
            Assert.Equal("15Value16", actual.Rows[1][nameof(TestModel.Value)]);
            Assert.Equal("25", actual.Rows[1][nameof(TestModel.Count)]);
            Assert.Equal("15I", actual.Rows[1][nameof(TestModel.Id)]);
            Assert.Equal("15Value16", actual.Rows[1]["Label"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_FiltersByCheckIds()
        {
            lookup.Filter.CheckIds.Add("9I");
            lookup.Filter.CheckIds.Add("15I");
            lookup.Filter.Sort = nameof(TestModel.Count);
            lookup.Filter.AdditionalFilters.Add("ParentId", "1000");

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("9Value10", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("19", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("9I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("9Value10", actual.Rows[0]["Label"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1][nameof(TestModel.Date)]);
            Assert.Equal("15Value16", actual.Rows[1][nameof(TestModel.Value)]);
            Assert.Equal("25", actual.Rows[1][nameof(TestModel.Count)]);
            Assert.Equal("15I", actual.Rows[1][nameof(TestModel.Id)]);
            Assert.Equal("15Value16", actual.Rows[1]["Label"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_FiltersByNotSelected()
        {
            lookup.Filter.Search = "133Value";
            lookup.Filter.Selected.Add("15I");
            lookup.Filter.Selected.Add("125I");
            lookup.Filter.Sort = nameof(TestModel.Count);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Selected[0][nameof(TestModel.Date)]);
            Assert.Equal("15Value16", actual.Selected[0][nameof(TestModel.Value)]);
            Assert.Equal("25", actual.Selected[0][nameof(TestModel.Count)]);
            Assert.Equal("15I", actual.Selected[0][nameof(TestModel.Id)]);
            Assert.Equal("15Value16", actual.Selected[0]["Label"]);

            Assert.Equal(new DateTime(2015, 4, 14).ToString("d"), actual.Selected[1][nameof(TestModel.Date)]);
            Assert.Equal("125Value126", actual.Selected[1][nameof(TestModel.Value)]);
            Assert.Equal("135", actual.Selected[1][nameof(TestModel.Count)]);
            Assert.Equal("125I", actual.Selected[1][nameof(TestModel.Id)]);
            Assert.Equal("125Value126", actual.Selected[1]["Label"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("133Value134", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("143", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("133I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("133Value134", actual.Rows[0]["Label"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.Selected.Count);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersByAdditionalFilters()
        {
            lookup.Filter.Search = "6Value";
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Count), 16);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 16).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("6Value7", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("16", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("6I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("6Value7", actual.Rows[0]["Label"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Empty(actual.Selected);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersBySearch()
        {
            lookup.Filter.Search = "33Value";
            lookup.Filter.Sort = nameof(TestModel.Count);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2015, 1, 12).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("33Value34", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("43", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("33I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("33Value34", actual.Rows[0]["Label"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[1][nameof(TestModel.Date)]);
            Assert.Equal("133Value134", actual.Rows[1][nameof(TestModel.Value)]);
            Assert.Equal("143", actual.Rows[1][nameof(TestModel.Count)]);
            Assert.Equal("133I", actual.Rows[1][nameof(TestModel.Id)]);
            Assert.Equal("133Value134", actual.Rows[1]["Label"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
        }

        [Fact]
        public void GetData_Sorts()
        {
            lookup.Filter.Search = "55Value";
            lookup.Filter.Order = LookupSortOrder.Asc;
            lookup.Filter.Sort = nameof(TestModel.Count);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2015, 2, 3).ToString("d"), actual.Rows[0][nameof(TestModel.Date)]);
            Assert.Equal("55Value56", actual.Rows[0][nameof(TestModel.Value)]);
            Assert.Equal("65", actual.Rows[0][nameof(TestModel.Count)]);
            Assert.Equal("55I", actual.Rows[0][nameof(TestModel.Id)]);
            Assert.Equal("55Value56", actual.Rows[0]["Label"]);

            Assert.Equal(new DateTime(2015, 5, 14).ToString("d"), actual.Rows[1][nameof(TestModel.Date)]);
            Assert.Equal("155Value156", actual.Rows[1][nameof(TestModel.Value)]);
            Assert.Equal("165", actual.Rows[1][nameof(TestModel.Count)]);
            Assert.Equal("155I", actual.Rows[1][nameof(TestModel.Id)]);
            Assert.Equal("155Value156", actual.Rows[1]["Label"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void FilterBySearch_SkipsEmpty(String search)
        {
            lookup.Filter.Search = search;

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotExistingProperties()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Test", "Test"));

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_UsesCaseSensitiveSearch()
        {
            lookup.Filter.Search = "1va";
            lookup.FilterCase = LookupFilterCase.Original;

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value!.Contains("1va"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(LookupFilterCase.Lower)]
        [InlineData(LookupFilterCase.Upper)]
        public void FilterBySearch_UsesCaseInsensitiveSearch(LookupFilterCase filterCase)
        {
            lookup.Filter.Search = "1Va";
            lookup.FilterCase = filterCase;

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value!.ToLower().Contains("1va"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotFilterableColumns()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn(nameof(TestModel.Value), "") { Filterable = false });

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_EqualsTerm()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "20Value";
            lookup.FilterPredicate = LookupFilterPredicate.Equals;
            lookup.Columns.Add(new LookupColumn(nameof(TestModel.Value), "") { Filterable = true });

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value == "20Value");
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_ContainsTerm()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "0Va";
            lookup.FilterPredicate = LookupFilterPredicate.Contains;
            lookup.Columns.Add(new LookupColumn(nameof(TestModel.Value), "") { Filterable = true });

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value!.Contains("0Va"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_EndsWithTerm()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "a14";
            lookup.FilterPredicate = LookupFilterPredicate.EndsWith;
            lookup.Columns.Add(new LookupColumn(nameof(TestModel.Value), "") { Filterable = true });

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value!.EndsWith("a14"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_StartsWithTerm()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "20";
            lookup.FilterPredicate = LookupFilterPredicate.StartsWith;
            lookup.Columns.Add(new LookupColumn(nameof(TestModel.Value), "") { Filterable = true });

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Value!.StartsWith("20"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_SkipsNullValues()
        {
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Id), null);

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Id), "9I");
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Count), new[] { 19, 30 });
            lookup.Filter.AdditionalFilters.Add(nameof(TestModel.Date), new DateTime(2014, 12, 19));

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels().Where(model =>
                model.Id == "9I" && model.Count == 19 && model.Date == new DateTime(2014, 12, 19));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_Guids()
        {
            TestLookup<GuidModel> testLookup = new TestLookup<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testLookup.Models[4].Id.ToString(), testLookup.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testLookup.GetModels().Where(model => ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testLookup.FilterByIds(testLookup.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByIds(Array.Empty<ObjectModel>().AsQueryable(), new String[0]));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type is not filterable.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByNotIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByNotIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => !ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_Guids()
        {
            TestLookup<GuidModel> testLookup = new TestLookup<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testLookup.Models[4].Id.ToString(), testLookup.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testLookup.GetModels().Where(model => !ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testLookup.FilterByNotIds(testLookup.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByNotIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => !new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByNotIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type is not filterable.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByCheckIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByCheckIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByCheckIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByCheckIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type is not filterable.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterBySelected(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterBySelected(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterBySelected(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterBySelected(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type is not filterable.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByColumn()
        {
            lookup.Filter.Sort = nameof(TestModel.Count);

            IQueryable<TestModel> expected = lookup.GetModels().OrderBy(model => model.Count);
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortColumns(String column)
        {
            lookup.Columns.Clear();
            lookup.Filter.Sort = column;

            IQueryable<TestModel> expected = lookup.GetModels();
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortOrderedColumns(String column)
        {
            lookup.Columns.Clear();
            lookup.Filter.Sort = column;

            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels().OrderByDescending(model => model.Id).Where(model => true));
            IQueryable<TestModel> expected = lookup.GetModels().OrderByDescending(model => model.Id);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1, -1, 0, 1)]
        [InlineData(-1, 0, 0, 1)]
        [InlineData(-1, 1, 0, 1)]
        [InlineData(-1, 100, 0, 100)]
        [InlineData(-1, 101, 0, 100)]
        [InlineData(0, -1, 0, 1)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 100, 0, 100)]
        [InlineData(0, 101, 0, 100)]
        [InlineData(50, -1, 50, 1)]
        [InlineData(50, 0, 50, 1)]
        [InlineData(50, 1, 50, 1)]
        [InlineData(50, 100, 50, 100)]
        [InlineData(50, 101, 50, 100)]
        public void Page_Rows(Int32 offset, Int32 rows, Int32 expectedOffset, Int32 expectedRows)
        {
            lookup.Filter.Rows = rows;
            lookup.Filter.Offset = offset;

            IQueryable<TestModel> expected = lookup.GetModels().Skip(expectedOffset).Take(expectedRows);
            IQueryable<TestModel> actual = lookup.Page(lookup.GetModels());

            Assert.Equal(expectedOffset, lookup.Filter.Offset);
            Assert.Equal(expectedRows, lookup.Filter.Rows);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_Columns()
        {
            Object actual = lookup.FormLookupData(lookup.GetModels(), new[] { new TestModel() }.AsQueryable(), lookup.GetModels()).Columns;
            Object expected = lookup.Columns;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void FormLookupData_Selected()
        {
            IQueryable<TestModel> notSelected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> selected = lookup.GetModels().Skip(6).Take(3);

            using IEnumerator<Dictionary<String, String>> actual = lookup.FormLookupData(lookup.GetModels(), selected, notSelected).Selected.GetEnumerator();
            using IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    ["Label"] = "6Value7",
                    [nameof(TestModel.Id)] = "6I",
                    [nameof(TestModel.Count)] = "16",
                    [nameof(TestModel.Value)] = "6Value7",
                    [nameof(TestModel.NextValue)] = "7Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 16).ToString("d")
                },
                new Dictionary<String, String>
                {
                    ["Label"] = "7Value8",
                    [nameof(TestModel.Id)] = "7I",
                    [nameof(TestModel.Count)] = "17",
                    [nameof(TestModel.Value)] = "7Value8",
                    [nameof(TestModel.NextValue)] = "8Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 17).ToString("d")
                },
                new Dictionary<String, String>
                {
                    ["Label"] = "8Value9",
                    [nameof(TestModel.Id)] = "8I",
                    [nameof(TestModel.Count)] = "18",
                    [nameof(TestModel.Value)] = "8Value9",
                    [nameof(TestModel.NextValue)] = "9Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 18).ToString("d")
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
                Assert.Equal(expected.Current, actual.Current);
        }

        [Fact]
        public void FormLookupData_Rows()
        {
            IQueryable<TestModel> selected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> notSelected = lookup.GetModels().Skip(6).Take(3);

            using IEnumerator<Dictionary<String, String>> actual = lookup.FormLookupData(lookup.GetModels(), selected, notSelected).Rows.GetEnumerator();
            using IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    ["Label"] = "6Value7",
                    [nameof(TestModel.Id)] = "6I",
                    [nameof(TestModel.Count)] = "16",
                    [nameof(TestModel.Value)] = "6Value7",
                    [nameof(TestModel.NextValue)] = "7Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 16).ToString("d")
                },
                new Dictionary<String, String>
                {
                    ["Label"] = "7Value8",
                    [nameof(TestModel.Id)] = "7I",
                    [nameof(TestModel.Count)] = "17",
                    [nameof(TestModel.Value)] = "7Value8",
                    [nameof(TestModel.NextValue)] = "8Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 17).ToString("d")
                },
                new Dictionary<String, String>
                {
                    ["Label"] = "8Value9",
                    [nameof(TestModel.Id)] = "8I",
                    [nameof(TestModel.Count)] = "18",
                    [nameof(TestModel.Value)] = "8Value9",
                    [nameof(TestModel.NextValue)] = "9Value",
                    [nameof(TestModel.Date)] = new DateTime(2014, 12, 18).ToString("d")
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
                Assert.Equal(expected.Current, actual.Current);
        }

        [Fact]
        public void FormData_EmptyValues()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", "Label", nameof(TestModel.Id) }, row.Keys);
            Assert.Equal(new[] { null, "", "1" }, row.Values);
        }

        [Fact]
        public void FormData_Values()
        {
            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", NextValue = "Next", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", "Next", DateTime.Now.Date.ToString("d"), "4", "Test", "1" }, row.Values);
            Assert.Equal(new[] { nameof(TestModel.Value), nameof(TestModel.NextValue), nameof(TestModel.Date), nameof(TestModel.Count), "Label", nameof(TestModel.Id) }, row.Keys);
        }

        [Fact]
        public void FormData_OverridenValues()
        {
            lookup.GetId = (model) => $"Test {model.Id}";
            lookup.GetLabel = (model) => $"Test label {model.Id}";
            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", NextValue = "Next", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", "Next", DateTime.Now.Date.ToString("d"), "4", "Test label 1", "Test 1" }, row.Values);
            Assert.Equal(new[] { nameof(TestModel.Value), nameof(TestModel.NextValue), nameof(TestModel.Date), nameof(TestModel.Count), "Label", nameof(TestModel.Id) }, row.Keys);
        }
    }
}
