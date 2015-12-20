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
    public class MvcLookupOfTTests
    {
        private Mock<TestLookupProxy> lookupMock;
        private Dictionary<String, String> row;
        private TestLookupProxy lookup;

        public MvcLookupOfTTests()
        {
            lookupMock = new Mock<TestLookupProxy> { CallBase = true };
            row = new Dictionary<String, String>();
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

        #region Constructor: MvcLookup()

        [Fact]
        public void MvcLookup_CallsGetColumnName()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnName", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void MvcLookup_CallsGetColumnHeader()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnHeader", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void MvcLookup_CallsGetColumnCssClass()
        {
            IEnumerable<PropertyInfo> properties = lookup.BaseAttributedProperties;
            Int32 callCount = lookup.BaseAttributedProperties.Count();

            lookupMock.Protected().Verify("GetColumnCssClass", Times.Exactly(callCount), ItExpr.Is<PropertyInfo>(match => properties.Contains(match)));
        }

        [Fact]
        public void MvcLookup_AddsColumns()
        {
            LookupColumns columns = new LookupColumns();
            foreach (PropertyInfo property in lookup.BaseAttributedProperties)
                columns.Add(new LookupColumn(lookup.BaseGetColumnName(property), lookup.BaseGetColumnHeader(property)));

            IEnumerator<LookupColumn> expected = columns.GetEnumerator();
            IEnumerator<LookupColumn> actual = lookup.Columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Name, actual.Current.Name);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
            }
        }

        #endregion

        #region Method: GetColumnName(PropertyInfo property)

        [Fact]
        public void GetColumnName_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.BaseGetColumnName(null));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnName_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Sum");

            String actual = lookup.BaseGetColumnName(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnName_NoRelation_Throws()
        {
            PropertyInfo property = typeof(NoRelationModel).GetProperty("NoRelation");

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseGetColumnName(property));

            String expected = String.Format("'{0}.{1}' does not have property named 'None'.", property.DeclaringType.Name, property.Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnName_ReturnsRelationName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("FirstRelationModel");
            String relation = property.GetCustomAttribute<LookupColumnAttribute>(false).Relation;

            String expected = String.Format("{0}.{1}", property.Name, relation);
            String actual = lookup.BaseGetColumnName(property);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: GetColumnHeader(PropertyInfo property)

        [Fact]
        public void GetColumnHeader_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.BaseGetColumnHeader(null));

            Assert.Equal("property", actual.ParamName);
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
        public void GetColumnHeader_NoRelation_Throws()
        {
            PropertyInfo property = typeof(NoRelationModel).GetProperty("NoRelation");

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseGetColumnHeader(property));

            String expected = String.Format("'{0}.{1}' does not have property named 'None'.", property.DeclaringType.Name, property.Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
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
        public void GetColumnCssClass_ReturnsEmptyString()
        {
            Assert.Empty(lookup.BaseGetColumnCssClass(null));
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
        public void GetData_DoesNotCallFilterById()
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
        public void GetData_DoesNotCallFilterByAdditionalFiltersBecauseEmpty()
        {
            lookup.CurrentFilter.AdditionalFilters.Clear();

            lookup.GetData();

            lookupMock.Protected().Verify("FilterByAdditionalFilters", Times.Never(), lookup.BaseGetModels());
        }

        [Fact]
        public void GetData_DoesNotCallFilterByAdditionalFiltersBecauseFiltersById()
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
        public void GetData_DoesNotCallFilterBySearchTermBecauseFiltersById()
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
        public void FilterById_NoIdProperty_Throws()
        {
            MvcLookupProxy<NoIdModel> lookup = new MvcLookupProxy<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));

            String expected = String.Format("'{0}' type does not have property named 'Id'.", typeof(NoIdModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_String()
        {
            lookup.CurrentFilter.Id = "9";

            IEnumerable<TestModel> expected = lookup.BaseGetModels().Where(model => model.Id == lookup.CurrentFilter.Id);
            IEnumerable<TestModel> actual = lookup.BaseFilterById(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_Number()
        {
            List<NumericIdModel> models = new List<NumericIdModel>();
            for (Int32 i = 0; i < 100; i++) models.Add(new NumericIdModel { Id = i });
            MvcLookupProxy<NumericIdModel> lookup = new MvcLookupProxy<NumericIdModel>();

            lookup.CurrentFilter.Id = "9.0";

            IEnumerable<NumericIdModel> actual = lookup.BaseFilterById(models.AsQueryable());
            IEnumerable<NumericIdModel> expected = models.Where(model => model.Id == 9);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_EnumId_Throws()
        {
            MvcLookupProxy<EnumModel> lookup = new MvcLookupProxy<EnumModel>();

            lookup.CurrentFilter.Id = IdEnum.Id.ToString();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));

            String expected = String.Format("'{0}.Id' can not be filtered by using 'Id' value, because it's not a string nor a number.", typeof(EnumModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterById_NotNumericId_Throws()
        {
            MvcLookupProxy<NonNumericIdModel> lookup = new MvcLookupProxy<NonNumericIdModel>();

            lookup.CurrentFilter.Id = "9";

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseFilterById(lookup.BaseGetModels()));

            String expected = String.Format("'{0}.Id' can not be filtered by using '9' value, because it's not a string nor a number.", typeof(NonNumericIdModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: FilterByAdditionalFilters(IQueryable<T> models)

        [Fact]
        public void FilterByAdditionalFilters_SkipsNullValues()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", null);

            IQueryable<TestModel> actual = lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            lookup.CurrentFilter.AdditionalFilters.Add("Id", "9");
            lookup.CurrentFilter.AdditionalFilters.Add("Number", 9);
            lookup.CurrentFilter.AdditionalFilters.Add("CreationDate", DateTime.Now.Date.AddDays(9));

            IEnumerable<TestModel> actual = lookup.BaseFilterByAdditionalFilters(lookup.BaseGetModels());
            IEnumerable<TestModel> expected = lookup.BaseGetModels().ToArray().Where(model => model.Id == "9" && model.Number == 9 && model.CreationDate == DateTime.Now.Date.AddDays(9));

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: FilterBySearchTerm(IQueryable<T> models)

        [Fact]
        public void FilterBySearchTerm_SkipsNullTerm()
        {
            lookup.CurrentFilter.SearchTerm = null;

            IQueryable<TestModel> expected = lookup.BaseGetModels();
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearchTerm_NoProperty_Throws()
        {
            lookup.CurrentFilter.SearchTerm = "Test";
            lookup.Columns.Add(new LookupColumn("Test", ""));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseFilterBySearchTerm(lookup.BaseGetModels()));

            String expected = String.Format("'{0}' type does not have property named 'Test'.", typeof(TestModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
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
        public void FilterBySearchTerm_NotStringProperties_DoesNotFilter()
        {
            lookup.Columns.Clear();
            lookup.CurrentFilter.SearchTerm = "1";
            lookup.Columns.Add(new LookupColumn("Number", ""));

            IQueryable<TestModel> expected = lookup.BaseGetModels();
            IQueryable<TestModel> actual = lookup.BaseFilterBySearchTerm(expected.AsQueryable());

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: Sort(IQueryable<T> models)

        [Fact]
        public void Sort_ByColumn()
        {
            lookup.CurrentFilter.SortColumn = lookup.BaseAttributedProperties.First().Name;

            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);
            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByDefaultSortColumn()
        {
            lookup.CurrentFilter.SortColumn = null;
            lookup.BaseDefaultSortColumn = lookup.BaseAttributedProperties.First().Name;

            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);
            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_NoProperty_Throws()
        {
            lookup.CurrentFilter.SortColumn = "Test";

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));

            String expected = "Lookup does not contain sort column named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_NoDefaultProperty_Throws()
        {
            lookup.BaseDefaultSortColumn = "Test";
            lookup.CurrentFilter.SortColumn = null;

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));

            String expected = "Lookup does not contain sort column named 'Test'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_ByFirstColumn()
        {
            lookup.BaseDefaultSortColumn = null;
            lookup.CurrentFilter.SortColumn = null;

            IQueryable<TestModel> actual = lookup.BaseSort(lookup.BaseGetModels());
            IQueryable<TestModel> expected = lookup.BaseGetModels().OrderBy(model => model.Number);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sort_EmptyColumns_Throws()
        {
            lookup.Columns.Clear();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseSort(lookup.BaseGetModels()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: FormLookupData(IQueryable<T> models)

        [Fact]
        public void FormLookupData_SetsFilteredRecords()
        {
            Int32 actual = lookup.BaseFormLookupData(lookup.BaseGetModels()).FilteredRecords;
            Int32 expected = lookup.BaseGetModels().Count();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_FormsColumns()
        {
            LookupColumns actual = lookup.BaseFormLookupData(lookup.BaseGetModels()).Columns;
            LookupColumns expected = lookup.Columns;

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

            List<TestModel> models = lookup.BaseGetModels().Skip(9).Take(3).ToList();
            Int32 callCount = Math.Min(lookup.CurrentFilter.RecordsPerPage, lookup.BaseGetModels().Count());

            lookupMock.Protected().Verify("AddId", Times.Exactly(callCount), ItExpr.IsAny<Dictionary<String, String>>(), ItExpr.Is<TestModel>(match => models.Contains(match)));
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
        public void AddId_NoProperty_Throws()
        {
            MvcLookupProxy<NoIdModel> lookup = new MvcLookupProxy<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseAddId(row, new NoIdModel()));

            String expected = String.Format("'{0}' type does not have property named 'Id'.", typeof(NoIdModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddId_Key()
        {
            lookup.BaseAddId(row, new TestModel());

            Assert.True(row.ContainsKey(MvcLookup.IdKey));
        }

        [Fact]
        public void AddId_Value()
        {
            TestModel model = new TestModel { Id = "Test" };

            lookup.BaseAddId(row, model);

            Assert.True(row.ContainsValue(model.Id));
        }

        [Fact]
        public void AddId_Element()
        {
            lookup.BaseAddId(row, new TestModel());

            Assert.Equal(1, row.Keys.Count);
        }

        #endregion

        #region Method: AddAutocomplete(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAutocomplete_EmptyColumns_Throws()
        {
            lookup.Columns.Clear();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseAddAutocomplete(row, new TestModel()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddAutocomplete_NoProperty_Throws()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseAddAutocomplete(row, new TestModel()));

            String expected = String.Format("'{0}' type does not have property named 'Test'.", typeof(TestModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddAutocomplete_Key()
        {
            lookup.BaseAddAutocomplete(row, new TestModel());

            Assert.Equal(MvcLookup.AcKey, row.First().Key);
        }

        [Fact]
        public void AddAutocomplete_Value()
        {
            TestModel model = new TestModel();
            PropertyInfo firstProperty = model.GetType().GetProperty(lookup.Columns.First().Name);

            lookup.BaseAddAutocomplete(row, model);

            Assert.Equal(firstProperty.GetValue(model).ToString(), row.First().Value);
        }

        [Fact]
        public void AddAutocomplete_RelationValue()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("FirstRelationModel.Value", ""));
            TestModel model = new TestModel { FirstRelationModel = new TestRelationModel { Value = "Test" } };
            PropertyInfo firstProperty = typeof(TestRelationModel).GetProperty("Value");

            lookup.BaseAddAutocomplete(row, model);

            Assert.Equal(firstProperty.GetValue(model.FirstRelationModel).ToString(), row.First().Value);
        }

        [Fact]
        public void AddAutocomplete_Element()
        {
            lookup.BaseAddAutocomplete(row, new TestModel());

            Assert.Equal(1, row.Keys.Count);
        }

        #endregion

        #region Method: AddColumns(Dictionary<String, String> row, T model)

        [Fact]
        public void AddColumns_EmptyColumns_Throws()
        {
            lookup.Columns.Clear();

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseAddColumns(null, new TestModel()));

            String expected = "Lookup should have at least one column.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddColumns_NoProperty_Throws()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            LookupException exception = Assert.Throws<LookupException>(() => lookup.BaseAddColumns(row, new TestModel()));

            String expected = String.Format("'{0}' type does not have property named 'Test'.", typeof(TestModel).Name);
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AddColumns_Keys()
        {
            lookup.BaseAddColumns(row, new TestModel());

            Assert.Equal(lookup.Columns.Keys, row.Keys);
        }

        [Fact]
        public void AddColumns_Values()
        {
            List<String> expected = new List<String>();
            TestModel model = new TestModel { FirstRelationModel = new TestRelationModel { Value = "Test" } };
            foreach (LookupColumn column in lookup.Columns)
                expected.Add(GetValue(model, column.Name));

            lookup.BaseAddColumns(row, model);

            Assert.Equal(expected, row.Values);
        }

        #endregion

        #region Method: AddAdditionalData(Dictionary<String, String> row, T model)

        [Fact]
        public void AddAdditionalData_DoesNothing()
        {
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
