using NonFactors.Mvc.Lookup.Tests.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupOfTTests
    {
        private Dictionary<String, String> row;
        private TestLookup<TestModel> lookup;

        public MvcLookupOfTTests()
        {
            row = new Dictionary<String, String>();
            lookup = new TestLookup<TestModel>();

            lookup.DefaultSortColumn = null;
            lookup.CurrentFilter.SearchTerm = null;

            for (Int32 i = 0; i < 20; i++)
                lookup.Models.Add(new TestModel
                {
                    Id = i + "I",
                    Count = i + 10,
                    Value = i + "V",
                    CreationDate = new DateTime(2014, 12, 10).AddDays(i)
                });
        }

        #region AttributedProperties

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            List<PropertyInfo> actual = lookup.AttributedProperties.ToList();
            List<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<LookupColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false).Position)
                .ToList();

            Assert.Equal(expected, actual);
        }

        #endregion

        #region MvcLookup()

        [Fact]
        public void MvcLookup_AddsColumns()
        {
            LookupColumns columns = new LookupColumns();
            columns.Add(new LookupColumn("Value", null));
            columns.Add(new LookupColumn("CreationDate", "Date"));
            columns.Add(new LookupColumn("Count", "Count's value"));

            IEnumerator<LookupColumn> expected = columns.GetEnumerator();
            IEnumerator<LookupColumn> actual = lookup.Columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
            }
        }

        #endregion

        #region GetColumnKey(PropertyInfo property)

        [Fact]
        public void GetColumnKey_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.GetColumnKey(null));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnKey_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Count");

            String actual = lookup.GetColumnKey(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetColumnHeader(PropertyInfo property)

        [Fact]
        public void GetColumnHeader_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.GetColumnHeader(null));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnHeader_NoDisplayName_ReturnsNull()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Value");

            String actual = lookup.GetColumnHeader(property);

            Assert.Null(actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Count");

            String expected = property.GetCustomAttribute<DisplayAttribute>().Name;
            String actual = lookup.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetColumnCssClass(PropertyInfo property)

        [Fact]
        public void GetColumnCssClass_ReturnsNull()
        {
            Assert.Null(lookup.GetColumnCssClass(null));
        }

        #endregion

        #region GetData()

        [Fact]
        public void GetData_FiltersById()
        {
            lookup.CurrentFilter.Id = "9I";
            lookup.CurrentFilter.SearchTerm = "Term";
            lookup.CurrentFilter.AdditionalFilters.Add("Value", "5V");

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToShortDateString(), actual.Rows[0]["CreationDate"]);
            Assert.Equal("9V", actual.Rows[0][MvcLookup.AcKey]);
            Assert.Equal("9I", actual.Rows[0][MvcLookup.IdKey]);
            Assert.Equal("9V", actual.Rows[0]["Value"]);
            Assert.Equal("19", actual.Rows[0]["Count"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(1, actual.FilteredRecords);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersByAdditionalFilters()
        {
            lookup.CurrentFilter.SearchTerm = "6V";
            lookup.CurrentFilter.AdditionalFilters.Add("Count", 16);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 16).ToShortDateString(), actual.Rows[0]["CreationDate"]);
            Assert.Equal("6V", actual.Rows[0][MvcLookup.AcKey]);
            Assert.Equal("6I", actual.Rows[0][MvcLookup.IdKey]);
            Assert.Equal("6V", actual.Rows[0]["Value"]);
            Assert.Equal("16", actual.Rows[0]["Count"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(1, actual.FilteredRecords);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersBySearchTerm()
        {
            lookup.CurrentFilter.SearchTerm = "5V";

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 25).ToShortDateString(), actual.Rows[0]["CreationDate"]);
            Assert.Equal("15V", actual.Rows[0][MvcLookup.AcKey]);
            Assert.Equal("15I", actual.Rows[0][MvcLookup.IdKey]);
            Assert.Equal("15V", actual.Rows[0]["Value"]);
            Assert.Equal("25", actual.Rows[0]["Count"]);

            Assert.Equal(new DateTime(2014, 12, 15).ToShortDateString(), actual.Rows[1]["CreationDate"]);
            Assert.Equal("5V", actual.Rows[1][MvcLookup.AcKey]);
            Assert.Equal("5I", actual.Rows[1][MvcLookup.IdKey]);
            Assert.Equal("5V", actual.Rows[1]["Value"]);
            Assert.Equal("15", actual.Rows[1]["Count"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.FilteredRecords);
            Assert.Equal(2, actual.Rows.Count);
        }

        [Fact]
        public void GetData_Sorts()
        {
            lookup.CurrentFilter.SortOrder = LookupSortOrder.Asc;
            lookup.CurrentFilter.SortColumn = "Count";
            lookup.CurrentFilter.SearchTerm = "5V";

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 15).ToShortDateString(), actual.Rows[0]["CreationDate"]);
            Assert.Equal("5V", actual.Rows[0][MvcLookup.AcKey]);
            Assert.Equal("5I", actual.Rows[0][MvcLookup.IdKey]);
            Assert.Equal("5V", actual.Rows[0]["Value"]);
            Assert.Equal("15", actual.Rows[0]["Count"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToShortDateString(), actual.Rows[1]["CreationDate"]);
            Assert.Equal("15V", actual.Rows[1][MvcLookup.AcKey]);
            Assert.Equal("15I", actual.Rows[1][MvcLookup.IdKey]);
            Assert.Equal("15V", actual.Rows[1]["Value"]);
            Assert.Equal("25", actual.Rows[1]["Count"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.FilteredRecords);
            Assert.Equal(2, actual.Rows.Count);
        }

        #endregion

        #region FilterById(IQueryable<T> models)

        [Fact]
        public void FilterById_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> lookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.FilterById(null));

            String expected = $"'{typeof (NoIdModel).Name}' type does not have property named 'Id'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_String()
        {
            lookup.CurrentFilter.Id = "9I";

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Id == lookup.CurrentFilter.Id);
            IQueryable<TestModel> actual = lookup.FilterById(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_Number()
        {
            TestLookup<NumericModel> lookup = new TestLookup<NumericModel>();
            for (Int32 i = 0; i < 20; i++) lookup.Models.Add(new NumericModel { Id = i });

            lookup.CurrentFilter.Id = "9.0";

            IQueryable<NumericModel> expected = lookup.GetModels().Where(model => model.Id == 9);
            IQueryable<NumericModel> actual = lookup.FilterById(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<GuidModel>().FilterById(null));

            String expected = $"'{typeof (GuidModel).Name}.Id' can not be filtered by using '' value, because it's not a string nor a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterByAdditionalFilters(IQueryable<T> models)

        [Fact]
        public void FilterByAdditionalFilters_SkipsNullValues()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", null);

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", "9I");
            lookup.CurrentFilter.AdditionalFilters.Add("Count", 9);
            lookup.CurrentFilter.AdditionalFilters.Add("CreationDate", new DateTime(2014, 12, 15));

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels().Where(model =>
                model.Id == "9I" && model.Count == 9 && model.CreationDate == new DateTime(2014, 12, 15));

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterBySearchTerm(IQueryable<T> models)

        [Fact]
        public void FilterBySearchTerm_SkipsNullTerm()
        {
            lookup.CurrentFilter.SearchTerm = null;

            IQueryable<TestModel> actual = lookup.FilterBySearchTerm(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_NoProperty_Throws()
        {
            lookup.CurrentFilter.SearchTerm = "Test";
            lookup.Columns.Add(new LookupColumn("Test", "Test"));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.FilterBySearchTerm(lookup.GetModels()));

            String expected = $"'{typeof (TestModel).Name}' type does not have property named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_UsesContainsSearch()
        {
            lookup.CurrentFilter.SearchTerm = "1";

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Id.Contains("1"));
            IQueryable<TestModel> actual = lookup.FilterBySearchTerm(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_DoesNotFilterNonStringProperties()
        {
            lookup.Columns.Clear();
            lookup.CurrentFilter.SearchTerm = "1";
            lookup.Columns.Add(new LookupColumn("Count", ""));

            IQueryable<TestModel> actual = lookup.FilterBySearchTerm(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Sort(IQueryable<T> models)

        [Fact]
        public void Sort_ByColumn()
        {
            lookup.CurrentFilter.SortColumn = "Count";

            IQueryable<TestModel> expected = lookup.GetModels().OrderBy(model => model.Count);
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByDefaultSortColumn()
        {
            lookup.DefaultSortColumn = "Count";
            lookup.CurrentFilter.SortColumn = null;

            IQueryable<TestModel> expected = lookup.GetModels().OrderBy(model => model.Count);
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_NoColumn_Throws()
        {
            lookup.CurrentFilter.SortColumn = "Test";

            LookupException exception = Assert.Throws<LookupException>(() => lookup.Sort(lookup.GetModels()));

            String expected = "Lookup does not contain sort column named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_NoDefaultProperty_Throws()
        {
            lookup.DefaultSortColumn = "Test";
            lookup.CurrentFilter.SortColumn = null;

            LookupException exception = Assert.Throws<LookupException>(() => lookup.Sort(lookup.GetModels()));

            String expected = "Lookup does not contain sort column named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByFirstColumn()
        {
            lookup.DefaultSortColumn = null;
            lookup.CurrentFilter.SortColumn = null;

            IQueryable<TestModel> expected = lookup.GetModels().OrderBy(model => model.Value);
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_NoColumns_Throws()
        {
            lookup.Columns.Clear();
            lookup.DefaultSortColumn = null;
            lookup.CurrentFilter.SortColumn = null;

            LookupException exception = Assert.Throws<LookupException>(() => lookup.Sort(lookup.GetModels()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FormLookupData(IQueryable<T> models)

        [Fact]
        public void FormLookupData_FilteredRecords()
        {
            Int32 actual = lookup.FormLookupData(lookup.GetModels()).FilteredRecords;
            Int32 expected = lookup.GetModels().Count();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_Columns()
        {
            LookupColumns actual = lookup.FormLookupData(lookup.GetModels()).Columns;
            LookupColumns expected = lookup.Columns;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_Rows()
        {
            lookup.CurrentFilter.Page = 2;
            lookup.CurrentFilter.RecordsPerPage = 3;

            IEnumerator<Dictionary<String, String>> actual = lookup.FormLookupData(lookup.GetModels()).Rows.GetEnumerator();
            IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    [MvcLookup.IdKey] = "6I",
                    [MvcLookup.AcKey] = "6V",
                    ["Value"] = "6V",
                    ["CreationDate"] = new DateTime(2014, 12, 16).ToShortDateString(),
                    ["Count"] = "16"
                },
                new Dictionary<String, String>
                {
                    [MvcLookup.IdKey] = "7I",
                    [MvcLookup.AcKey] = "7V",
                    ["Value"] = "7V",
                    ["CreationDate"] = new DateTime(2014, 12, 17).ToShortDateString(),
                    ["Count"] = "17"
                },
                new Dictionary<String, String>
                {
                    [MvcLookup.IdKey] = "8I",
                    [MvcLookup.AcKey] = "8V",
                    ["Value"] = "8V",
                    ["CreationDate"] = new DateTime(2014, 12, 18).ToShortDateString(),
                    ["Count"] = "18"
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Values, actual.Current.Values);
                Assert.Equal(expected.Current.Keys, actual.Current.Keys);
            }
        }

        #endregion

        #region AddId(Dictionary<String, String> row, T model)

        [Fact]
        public void AddId_NoProperty_Throws()
        {
            TestLookup<NoIdModel> lookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.AddId(row, new NoIdModel()));

            String expected = $"'{typeof (NoIdModel).Name}' type does not have property named 'Id'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddId_Values()
        {
            lookup.AddId(row, new TestModel { Id = "Test" });

            KeyValuePair<String, String> actual = row.Single();

            Assert.Equal(MvcLookup.IdKey, actual.Key);
            Assert.Equal("Test", actual.Value);
        }

        #endregion

        #region AddAutocomplete(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAutocomplete_NoColumns_Throws()
        {
            lookup.Columns.Clear();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.AddAutocomplete(row, new TestModel()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddAutocomplete_NoProperty_Throws()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.AddAutocomplete(row, new TestModel()));

            String expected = $"'{typeof(TestModel).Name}' type does not have property named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddAutocomplete_Values()
        {
            lookup.AddAutocomplete(row, new TestModel { Value = "Test" });

            KeyValuePair<String, String> actual = row.Single();

            Assert.Equal(MvcLookup.AcKey, actual.Key);
            Assert.Equal("Test", actual.Value);
        }

        #endregion

        #region AddColumns(Dictionary<String, String> row, T model)

        [Fact]
        public void AddColumns_NoColumns_Throws()
        {
            lookup.Columns.Clear();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.AddColumns(null, new TestModel()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddColumns_NoProperty_Throws()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.AddColumns(row, new TestModel()));

            String expected = $"'{typeof (TestModel).Name}' type does not have property named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddColumns_Values()
        {
            lookup.AddColumns(row, new TestModel { Value = "Test", CreationDate = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", DateTime.Now.Date.ToShortDateString(), "4" }, row.Values);
            Assert.Equal(lookup.Columns.Keys, row.Keys);
        }

        #endregion

        #region AddAdditionalData(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAdditionalData_DoesNothing()
        {
            lookup.AddAdditionalData(row, new TestModel());

            Assert.Empty(row.Keys);
        }

        #endregion
    }
}
