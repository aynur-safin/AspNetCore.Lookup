using Moq;
using Moq.Protected;
using NonFactors.Mvc.Lookup.Tests.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class GenericLookupTests
    {
        private Mock<TestLookupProxy> lookupMock;
        private TestLookupProxy lookup;

        public GenericLookupTests()
        {
            lookupMock = new Mock<TestLookupProxy> { CallBase = true };
            lookup = lookupMock.Object;
        }

        #region Property: AttributedProperties

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            List<PropertyInfo> actual = lookup.BaseAttributedProperties.ToList();
            List<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<LookupColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false).Position)
                .ToList();

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Constructor: GenericLookup()

        [Fact]
        public void GenericLookup_CallsGetColumnKey()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnKey", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void GenericLookup_CallsGetColumnHeader()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnHeader", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void GenericLookup_CallsGetColumnCssClass()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnCssClass", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void GenericLookup_AddsColumns()
        {
            LookupColumns columns = new LookupColumns();
            foreach (PropertyInfo property in lookup.BaseAttributedProperties)
                columns.Add(new LookupColumn(lookup.BaseGetColumnKey(property), lookup.BaseGetColumnHeader(property)));

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

        #region Method: GetColumnKey(PropertyInfo property)

        [Fact]
        public void GetColumnKey_OnNullPropertyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => lookup.BaseGetColumnKey(null));
        }

        [Fact]
        public void GetColumnKey_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Sum");
            String actual = lookup.BaseGetColumnKey(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnKey_OnMissingRelationThrows()
        {
            PropertyInfo property = typeof(NoRelationModel).GetProperty("NoRelation");
            Assert.Throws<LookupException>(() => lookup.BaseGetColumnKey(property));
        }

        [Fact]
        public void GetColumnKey_GetsKeyWithRelation()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("FirstRelationModel");
            String expected = String.Format("{0}.{1}", property.Name, property.GetCustomAttribute<LookupColumnAttribute>(false).Relation);
            String actual = lookup.BaseGetColumnKey(property);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: GetColumnHeader(PropertyInfo property)

        [Fact]
        public void GetColumnHeader_OnNullPropertyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => lookup.BaseGetColumnHeader(null));
        }

        [Fact]
        public void GetColumnHeader_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Sum");
            String actual = lookup.BaseGetColumnHeader(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Number");
            String expected = property.GetCustomAttribute<DisplayAttribute>().Name;
            String actual = lookup.BaseGetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_OnMissingRelationThrows()
        {
            PropertyInfo property = typeof(NoRelationModel).GetProperty("NoRelation");
            Assert.Throws<LookupException>(() => lookup.BaseGetColumnHeader(property));
        }

        [Fact]
        public void GetColumnHeader_ReturnsRelationName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("SecondRelationModel");
            String expected = property.GetCustomAttribute<LookupColumnAttribute>(false).Relation;
            String actual = lookup.BaseGetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsRelationDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("FirstRelationModel");

            String expected = property.PropertyType.GetProperty("Value").GetCustomAttribute<DisplayAttribute>().Name;
            String actual = lookup.BaseGetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: GetColumnCssClass(PropertyInfo property)

        [Fact]
        public void GetColumnCssClass_AlwaysEmptyString()
        {
            Assert.Equal("", lookup.BaseGetColumnCssClass(null));
        }

        #endregion

        #region Method: GetData()

        [Fact]
        public void GetData_CallsGetModels()
        {
            lookup.GetData();

            lookupMock.Protected().Verify("GetModels", Times.Once());
        }

        [Fact]
        public void GetData_CallsFilterById()
        {
            lookup.CurrentFilter.Id = "1";

            lookup.GetData();

            lookupMock.Protected().Verify("FilterById", Times.Once(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_NotCallsFilterById()
        {
            lookup.CurrentFilter.Id = null;

            lookup.GetData();

            lookupMock.Protected().Verify("FilterById", Times.Never(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_CallsFilterByAdditionalFilters()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", "1");

            lookup.GetData();

            lookupMock.Protected().Verify("FilterByAdditionalFilters", Times.Once(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_NotCallsFilterByAdditionalFiltersBecauseEmpty()
        {
            lookup.CurrentFilter.AdditionalFilters.Clear();

            lookup.GetData();

            lookupMock.Protected().Verify("FilterByAdditionalFilters", Times.Never(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_NotCallsFilterByAdditionalFiltersBecauseFiltersById()
        {
            lookup.CurrentFilter.Id = "1";

            lookup.GetData();
            lookupMock.Protected().Verify("FilterByAdditionalFilters", Times.Never(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_CallsFilterBySearchTerm()
        {
            lookup.CurrentFilter.Id = null;

            lookup.GetData();

            lookupMock.Protected().Verify("FilterBySearchTerm", Times.Once(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_CallsFilterBySearchTermAfterAdditionalFiltered()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", "1");
            lookup.CurrentFilter.Id = null;

            lookup.GetData();

            lookupMock.Protected().Verify("FilterBySearchTerm", Times.Once(), lookup.BaseGetModels().Where(model => model.Id == "1"));
        }

        [Fact]
        public void GetData_NotCallsFilterBySearchTermBecauseFiltersById()
        {
            lookup.CurrentFilter.Id = "1";

            lookup.GetData();

            lookupMock.Protected().Verify("FilterBySearchTerm", Times.Never(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_CallsFormLookupData()
        {
            lookup.GetData();

            lookupMock.Protected().Verify("FormLookupData", Times.Once(), lookup.BaseGetModels());
        }

        #endregion

        #region Method: FilterById(IQueryable<T> models)

        [Fact]
        public void FilterById_OnMissingIdPropertyThrows()
        {
            GenericLookupProxy<NoIdModel> lookup = new GenericLookupProxy<NoIdModel>();

            Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));
        }

        [Fact]
        public void FilterById_FiltersStringId()
        {
            lookup.CurrentFilter.Id = "9";

            IEnumerable<TestModel> expected = lookup.BaseGetModels().Where(model => model.Id == lookup.CurrentFilter.Id);
            IEnumerable<TestModel> actual = lookup.BaseFilterById(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_FiltersNumericId()
        {
            List<NumericIdModel> models = new List<NumericIdModel>();
            GenericLookupProxy<NumericIdModel> lookup = new GenericLookupProxy<NumericIdModel>();
            for (Int32 i = 0; i < 100; i++)
                models.Add(new NumericIdModel { Id = i });

            Int32 id = 9;
            lookup.CurrentFilter.Id = id.ToString();

            IEnumerable<NumericIdModel> expected = models.Where(model => model.Id == id);
            IEnumerable<NumericIdModel> actual = lookup.BaseFilterById(models.AsQueryable());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_OnEnumIdPropertyThrows()
        {
            GenericLookupProxy<EnumModel> lookup = new GenericLookupProxy<EnumModel>();
            lookup.CurrentFilter.Id = IdEnum.Id.ToString();

            Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));
        }

        [Fact]
        public void FilterById_OnNonNumericIdThrows()
        {
            GenericLookupProxy<NonNumericIdModel> lookup = new GenericLookupProxy<NonNumericIdModel>();
            lookup.CurrentFilter.Id = "9";

            Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));
        }

        #endregion

        #region Method: FilterByAdditionalFilters(IQueryable<T> models)

        [Fact]
        public void FilterByAdditionalFilters_NotFiltersNulls()
        {
            IQueryable<TestModel> expected = lookup.BaseGetModels();
            lookup.CurrentFilter.AdditionalFilters.Add("Id", null);
            IQueryable<TestModel> actual = lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_OnMissingPropertyThrows()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("TestProperty", "Test");
            Assert.Throws<LookupException>(() => lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels()));
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            Int32 numberFilter = 9;
            String stringFilter = "9";
            lookup.CurrentFilter.AdditionalFilters.Add("Id", stringFilter);
            lookup.CurrentFilter.AdditionalFilters.Add("Number", numberFilter);

            IQueryable<TestModel> actual = lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels().Where(model => model.Id == stringFilter && model.Number == numberFilter);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_OnNotSupportedPropertyThrows()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("CreationDate", DateTime.Now);
            Assert.Throws<LookupException>(() => lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels()));
        }

        [Fact]
        public void FilterByAdditionalFilters_OnEnumThrows()
        {
            GenericLookupProxy<EnumModel> lookup = new GenericLookupProxy<EnumModel>();
            lookup.CurrentFilter.AdditionalFilters.Add("IdEnum", DateTime.Now);

            Assert.Throws<LookupException>(() => lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels()));
        }

        #endregion

        #region Method: FilterBySearchTerm(IQueryable<T> models)

        [Fact]
        public void FilterBySearchTerm_NotFiltersNull()
        {
            lookup.CurrentFilter.SearchTerm = null;

            IQueryable<TestModel> expected = lookup.BaseGetModels();
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_OnMissingPropertyThrows()
        {
            lookup.CurrentFilter.SearchTerm = "Test";
            lookup.Columns.Add(new LookupColumn("TestProperty", ""));

            Assert.Throws<LookupException>(() => lookup.BaseFilterBySearchTerm(lookup.BaseGetModels()));
        }

        [Fact]
        public void FilterBySearchTerm_FiltersWhiteSpace()
        {
            lookup.CurrentFilter.SearchTerm = " ";
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels().Where(model =>
                (model.Id != null && model.Id.ToLower().Contains(lookup.CurrentFilter.SearchTerm)) ||
                (model.FirstRelationModel != null && model.FirstRelationModel.Value != null && model.FirstRelationModel.Value.ToLower().Contains(lookup.CurrentFilter.SearchTerm)) ||
                (model.SecondRelationModel != null && model.SecondRelationModel.Value != null && model.SecondRelationModel.Value.ToLower().Contains(lookup.CurrentFilter.SearchTerm)));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_UsesContainsSearch()
        {
            lookup.CurrentFilter.SearchTerm = "1";
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels().Where(model =>
                (model.Id != null && model.Id.ToLower().Contains(lookup.CurrentFilter.SearchTerm)) ||
                (model.FirstRelationModel != null && model.FirstRelationModel.Value != null && model.FirstRelationModel.Value.ToLower().Contains(lookup.CurrentFilter.SearchTerm)) ||
                (model.SecondRelationModel != null && model.SecondRelationModel.Value != null && model.SecondRelationModel.Value.ToLower().Contains(lookup.CurrentFilter.SearchTerm)));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_NotFiltersNonStringProperties()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Number", ""));
            lookup.CurrentFilter.SearchTerm = "Test";

            IQueryable<TestModel> expected = lookup.BaseGetModels();
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(expected.AsQueryable());

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: Sort(IQueryable<T> models)

        [Fact]
        public void Sort_SortsBySortColumn()
        {
            lookup.CurrentFilter.SortColumn = lookup.BaseAttributedProperties.First().Name;
            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);
            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_SortsByDefaultSortColumn()
        {
            lookup.CurrentFilter.SortColumn = null;
            lookup.BaseDefaultSortColumn = lookup.BaseAttributedProperties.First().Name;
            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);
            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_MissingPropertyThrows()
        {
            lookup.CurrentFilter.SortColumn = "TestProperty";
            Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));
        }

        [Fact]
        public void Sort_MissingDefaultPropertyThrows()
        {
            lookup.BaseDefaultSortColumn = "TestProperty";
            lookup.CurrentFilter.SortColumn = null;
            Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));
        }

        [Fact]
        public void Sort_SortsByFirstColumn()
        {
            lookup.BaseDefaultSortColumn = null;
            lookup.CurrentFilter.SortColumn = null;
            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_OnEmptyColumnsThrows()
        {
            lookup.Columns.Clear();

            Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));
        }

        #endregion

        #region Method: FormLookupData(IQueryable<T> models)

        [Fact]
        public void FormLookupData_SetsFilteredRecords()
        {
            Int32 expected = lookup.BaseGetModels().Count();
            Int32 actual = lookup.BaseFormLookupData(lookup.BaseGetModels()).FilteredRecords;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_FormsColumns()
        {
            LookupColumns expected = lookup.Columns;
            LookupColumns actual = lookup.BaseFormLookupData(lookup.BaseGetModels()).Columns;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_FormsRows()
        {
            Int32 expected = lookup.CurrentFilter.RecordsPerPage;
            Int32 actual = lookup.BaseFormLookupData(lookup.BaseGetModels()).Rows.Count;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_FormsRowsUsingModels()
        {
            lookup.CurrentFilter.Page = 3;
            lookup.CurrentFilter.RecordsPerPage = 3;
            lookup.BaseFormLookupData(lookup.BaseGetModels());
            List<TestModel> expectedModels = lookup.BaseGetModels().Skip(9).Take(3).ToList();
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddId", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.Is<TestModel>(match => expectedModels.Contains(match)));
        }

        [Fact]
        public void FormLookupData_CallsAddId()
        {
            lookup.BaseFormLookupData(lookup.BaseGetModels());
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddId", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.IsAny<TestModel>());
        }

        [Fact]
        public void FormLookupData_CallsAddAutocomplete()
        {
            lookup.BaseFormLookupData(lookup.BaseGetModels());
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddAutocomplete", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.IsAny<TestModel>());
        }

        [Fact]
        public void FormLookupData_CallsAddColumns()
        {
            lookup.BaseFormLookupData(lookup.BaseGetModels());
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddColumns", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.IsAny<TestModel>());
        }

        [Fact]
        public void FormLookupData_CallsAddAdditionalData()
        {
            lookup.BaseFormLookupData(lookup.BaseGetModels());
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddAdditionalData", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.IsAny<TestModel>());
        }

        #endregion

        #region Method: AddId(Dictionary<String, String> row, T model)

        [Fact]
        public void AddId_OnMissingPropertyThrows()
        {
            Assert.Throws<LookupException>(() => new GenericLookupProxy<NoIdModel>().BaseAddId(new Dictionary<String, String>(), new NoIdModel()));
        }

        [Fact]
        public void AddId_AddsConstantKey()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddId(row, new TestModel());

            Assert.True(row.ContainsKey(AbstractLookup.IdKey));
        }

        [Fact]
        public void AddId_AddsValue()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            TestModel model = new TestModel { Id = "Test" };

            lookup.BaseAddId(row, model);

            Assert.True(row.ContainsValue(model.Id));
        }

        [Fact]
        public void AddId_AddsOneElement()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddId(row, new TestModel());

            Assert.Equal(1, row.Keys.Count);
        }

        #endregion

        #region Method: AddAutocomplete(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAutocomplete_OnEmptyColumnsThrows()
        {
            lookup.Columns.Clear();

            Assert.Throws<LookupException>(() => lookup.BaseAddAutocomplete(null, new TestModel()));
        }

        [Fact]
        public void AddAutocomplete_OnMissingPropertyThrows()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("TestProperty", ""));

            Assert.Throws<LookupException>(() => lookup.BaseAddAutocomplete(new Dictionary<String, String>(), new TestModel()));
        }

        [Fact]
        public void AddAutocomplete_AddsConstantKey()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddAutocomplete(row, new TestModel());

            Assert.Equal(AbstractLookup.AcKey, row.First().Key);
        }

        [Fact]
        public void AddAutocomplete_AddsValue()
        {
            TestModel model = new TestModel();
            Dictionary<String, String> row = new Dictionary<String, String>();
            PropertyInfo firstProperty = model.GetType().GetProperty(lookup.Columns.First().Key);
            lookup.BaseAddAutocomplete(row, model);

            Assert.Equal(firstProperty.GetValue(model).ToString(), row.First().Value);
        }

        [Fact]
        public void AddAutocomplete_AddsRelationValue()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("FirstRelationModel.Value", ""));
            TestModel model = new TestModel { FirstRelationModel = new TestRelationModel { Value = "Test" } };
            PropertyInfo firstProperty = typeof(TestRelationModel).GetProperty("Value");
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddAutocomplete(row, model);

            Assert.Equal(firstProperty.GetValue(model.FirstRelationModel).ToString(), row.First().Value);
        }

        [Fact]
        public void AddAutocomplete_AddsOneElement()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddAutocomplete(row, new TestModel());

            Assert.Equal(1, row.Keys.Count);
        }

        #endregion

        #region Method: AddColumns(Dictionary<String, String> row, T model)

        [Fact]
        public void AddColumns_OnEmptyColumnsThrows()
        {
            lookup.Columns.Clear();

            Assert.Throws<LookupException>(() => lookup.BaseAddColumns(null, new TestModel()));
        }

        [Fact]
        public void AddColumns_OnMissingPropertyThrows()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("TestProperty", ""));

            Assert.Throws<LookupException>(() => lookup.BaseAddColumns(new Dictionary<String, String>(), new TestModel()));
        }

        [Fact]
        public void AddColumns_AddsKeys()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddColumns(row, new TestModel());

            Assert.Equal(lookup.Columns.Keys, row.Keys);
        }

        [Fact]
        public void AddColumns_AddsValues()
        {
            List<String> expected = new List<String>();
            Dictionary<String, String> row = new Dictionary<String, String>();
            TestModel model = new TestModel { FirstRelationModel = new TestRelationModel { Value = "Test" } };
            foreach (LookupColumn column in lookup.Columns)
                expected.Add(GetValue(model, column.Key));

            lookup.BaseAddColumns(row, model);

            Assert.Equal(expected, row.Values);
        }

        #endregion

        #region Method: AddAdditionalData(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAdditionalData_IsEmptyMethod()
        {
            Dictionary<String, String> row = new Dictionary<String, String>();
            lookup.BaseAddAdditionalData(row, new TestModel());

            Assert.Empty(row.Keys);
        }

        #endregion

        #region Testing helpers

        private String GetValue(Object model, String fullPropertyName)
        {
            if (model == null) return "";

            Type type = model.GetType();
            String[] properties = fullPropertyName.Split('.');
            PropertyInfo property = type.GetProperty(properties[0]);

            if (properties.Length > 1)
                return GetValue(property.GetValue(model), String.Join(".", properties.Skip(1)));

            Object value = property.GetValue(model) ?? "";
            LookupColumnAttribute lookupColumn = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (lookupColumn != null && lookupColumn.Format != null)
                value = String.Format(lookupColumn.Format, value);

            return value.ToString();
        }

        #endregion
    }
}
