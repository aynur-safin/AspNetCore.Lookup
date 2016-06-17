using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;
using Moq;
using NonFactors.Mvc.Lookup.Tests.Objects;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupExtensionsTests
    {
        private IHtmlHelper<TestModel> html;
        private TestLookupProxy lookup;
        private TestModel testModel;

        public LookupExtensionsTests()
        {
            lookup = new TestLookupProxy();
            testModel = new TestModel();
            html = MockHtmlHelper();
        }

        #region AutoComplete<TModel>(this IHtmlHelper<TModel> html, String name, Object value, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void AutoComplete_CreatesAutocompleteAndHiddenInput()
        {
            CreatesAutocompleteAndHiddenInput("TestId", html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsIdAttribute()
        {
            AddsIdAttribute("TestId", html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.AutoComplete("TestId", "", lookup, new { @class = "TestClass" }));
        }

        [Fact]
        public void AutoComplete_AddsHiddenInputAttribute()
        {
            AddsHiddenInputAttribute("TestId", html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsFiltersAttribute()
        {
            lookup.AdditionalFilters.Add("Test1");
            lookup.AdditionalFilters.Add("Test2");

            AddsFiltersAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsTermAttribute()
        {
            AddsTermAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsPageAttribute()
        {
            AddsPageAttribute(html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsIdForHiddenInput()
        {
            AddsIdForHiddenInput("TestId", html.AutoComplete("TestId", "", lookup));
        }

        [Fact]
        public void AutoComplete_AddsValueForHiddenInput()
        {
            AddsValueForHiddenInput("TestValue", html.AutoComplete("TestId", "TestValue", lookup));
        }

        [Fact]
        public void AutoComplete_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.AutoComplete("Id", "", lookup));
        }

        #endregion

        #region AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)

        [Fact]
        public void AutoCompleteFor_WithoutModel_MissingAttributeThrows()
        {
            Assert.Throws<LookupException>(() =>
                CreatesAutocompleteAndHiddenInputFromExpression(
                    model => model.Id,
                    html.AutoCompleteFor(model => model.Id)));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_CreatesAutocompleteAndHiddenInputFromExpression()
        {
            CreatesAutocompleteAndHiddenInputFromExpression(model => model.ParentId, html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsIdAttributeFromExpression()
        {
            AddsIdAttributeFromExpression(model => model.ParentId, html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.AutoCompleteFor(model => model.ParentId, new { @class = "TestClass" }));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsHiddenInputAttributeFromExpression()
        {
            AddsHiddenInputAttributeFromExpression(model => model.ParentId, html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsFiltersAttribute()
        {
            AddsFiltersAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsTermAttribute()
        {
            AddsTermAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsPageAttribute()
        {
            AddsPageAttribute(html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsIdForHiddenInputFromExpression()
        {
            AddsIdForHiddenInputFromExpression(model => model.ParentId, html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsValueForHiddenInput()
        {
            testModel.ParentId = "TestValue";

            AddsValueForHiddenInput(testModel.ParentId, html.AutoCompleteFor(model => model.ParentId));
        }

        [Fact]
        public void AutoCompleteFor_WithoutModel_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.AutoCompleteFor(model => model.ParentId));
        }

        #endregion

        #region AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void AutoCompleteFor_CreatesAutocompleteAndHiddenInputFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            CreatesAutocompleteAndHiddenInputFromExpression(expression, html.AutoCompleteFor(expression, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsIdAttributeFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsIdAttributeFromExpression(expression, html.AutoCompleteFor(expression, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.AutoCompleteFor(model => model.Id, lookup, new { @class = "TestClass" }));
        }

        [Fact]
        public void AutoCompleteFor_AddsHiddenInputAttributeFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsHiddenInputAttributeFromExpression(expression, html.AutoCompleteFor(expression, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsFiltersAttribute()
        {
            lookup.AdditionalFilters.Add("Test1");
            lookup.AdditionalFilters.Add("Test2");

            AddsFiltersAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsTermAttribute()
        {
            AddsTermAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsPageAttribute()
        {
            AddsPageAttribute(html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsIdForHiddenInputFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsIdForHiddenInputFromExpression(expression, html.AutoCompleteFor(expression, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsValueForHiddenInput()
        {
            testModel.Id = "TestValue";

            AddsValueForHiddenInput(testModel.Id, html.AutoCompleteFor(model => model.Id, lookup));
        }

        [Fact]
        public void AutoCompleteFor_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.AutoCompleteFor(model => model.Id, lookup));
        }

        #endregion

        #region Lookup<TModel>(this IHtmlHelper<TModel> html, String name, Object value, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void Lookup_WrapsAutocompleteInInputGroup()
        {
            WrapsAutocompleteInInputGroup(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_CreatesAutocompleteAndHiddenInput()
        {
            CreatesAutocompleteAndHiddenInput("TestId", html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsIdAttribute()
        {
            AddsIdAttribute("TestId", html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.Lookup("TestId", "", lookup, new { @class = "TestClass" }));
        }

        [Fact]
        public void Lookup_AddsHiddenInputAttribute()
        {
            AddsHiddenInputAttribute("TestId", html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsFiltersAttribute()
        {
            lookup.AdditionalFilters.Add("Test1");
            lookup.AdditionalFilters.Add("Test2");

            AddsFiltersAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsTermAttribute()
        {
            AddsTermAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsPageAttribute()
        {
            AddsPageAttribute(html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsIdForHiddenInput()
        {
            AddsIdForHiddenInput("TestId", html.Lookup("TestId", "", lookup));
        }

        [Fact]
        public void Lookup_AddsValueForHiddenInput()
        {
            AddsValueForHiddenInput("TestValue", html.Lookup("TestId", "TestValue", lookup));
        }

        [Fact]
        public void Lookup_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.Lookup("Id", "", lookup));
        }

        [Fact]
        public void Lookup_CreatesOpenSpan()
        {
            CreatesOpenSpan(html.Lookup("TestId", "", lookup));
        }

        #endregion

        #region LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)

        [Fact]
        public void LookupFor_WithoutModel_WrapsAutocompleteInInputGroup()
        {
            WrapsAutocompleteInInputGroup(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_MissingAttributeThrows()
        {
            Assert.Throws<LookupException>(() =>
                CreatesAutocompleteAndHiddenInputFromExpression(
                    model => model.Id,
                    html.LookupFor(model => model.Id)));
        }

        [Fact]
        public void LookupFor_WithoutModel_CreatesAutocompleteAndHiddenInputFromExpression()
        {
            CreatesAutocompleteAndHiddenInputFromExpression(model => model.ParentId, html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsIdAttributeFromExpression()
        {
            AddsIdAttributeFromExpression(model => model.ParentId, html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.LookupFor(model => model.ParentId, new { @class = "TestClass" }));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsHiddenInputAttributeFromExpression()
        {
            AddsHiddenInputAttributeFromExpression(model => model.ParentId, html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsFiltersAttribute()
        {
            AddsFiltersAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsTermAttribute()
        {
            AddsTermAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsPageAttribute()
        {
            AddsPageAttribute(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsIdForHiddenInputFromExpression()
        {
            AddsIdForHiddenInputFromExpression(model => model.ParentId, html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsValueForHiddenInput()
        {
            testModel.ParentId = "TestValue";

            AddsValueForHiddenInput(testModel.ParentId, html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_WithoutModel_CreatesOpenSpan()
        {
            CreatesOpenSpan(html.LookupFor(model => model.ParentId));
        }

        #endregion

        #region LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void LookupFor_WrapsAutocompleteInInputGroup()
        {
            WrapsAutocompleteInInputGroup(html.LookupFor(model => model.ParentId));
        }

        [Fact]
        public void LookupFor_CreatesAutocompleteAndHiddenInputFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            CreatesAutocompleteAndHiddenInputFromExpression(expression, html.LookupFor(expression, lookup));
        }

        [Fact]
        public void LookupFor_AddsIdAttributeFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsIdAttributeFromExpression(expression, html.LookupFor(expression, lookup));
        }

        [Fact]
        public void LookupFor_AddsLookupClasses()
        {
            AddsLookupClassesForLookupInput(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsSpecifiedClass()
        {
            AddsSpecifiedClass("TestClass", html.LookupFor(model => model.Id, lookup, new { @class = "TestClass" }));
        }

        [Fact]
        public void LookupFor_AddsHiddenInputAttributeFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsHiddenInputAttributeFromExpression(expression, html.LookupFor(expression, lookup));
        }

        [Fact]
        public void LookupFor_AddsFiltersAttribute()
        {
            lookup.AdditionalFilters.Add("Test1");
            lookup.AdditionalFilters.Add("Test2");

            AddsFiltersAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsRecordsPerPageAttribute()
        {
            AddsRecordsPerPageAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsSortColumnAttribute()
        {
            AddsSortColumnAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsSortOrderAttribute()
        {
            AddsSortOrderAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsDialogTitleAttribute()
        {
            AddsDialogTitleAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsUrlAttribute()
        {
            AddsUrlAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsTermAttribute()
        {
            AddsTermAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsPageAttribute()
        {
            AddsPageAttribute(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsIdForHiddenInputFromExpression()
        {
            Expression<Func<TestModel, String>> expression = model => model.Relation.Value;

            AddsIdForHiddenInputFromExpression(expression, html.LookupFor(expression, lookup));
        }

        [Fact]
        public void LookupFor_AddsValueForHiddenInput()
        {
            testModel.Id = "TestValue";

            AddsValueForHiddenInput(testModel.Id, html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_AddsLookupClassesForHiddenInput()
        {
            AddsLookupClassesForHiddenInput(html.LookupFor(model => model.Id, lookup));
        }

        [Fact]
        public void LookupFor_CreatesOpenSpan()
        {
            CreatesOpenSpan(html.LookupFor(model => model.ParentId, lookup));
        }

        #endregion

        #region Test helpers

        private IHtmlHelper<TestModel> MockHtmlHelper()
        {
            Mock<IOptions<MvcViewOptions>> optionsMock = new Mock<IOptions<MvcViewOptions>>();
            optionsMock.SetupGet(mock => mock.Value).Returns(new MvcViewOptions());
            IModelMetadataProvider provider = new EmptyModelMetadataProvider();

            IHtmlGenerator generator = new DefaultHtmlGenerator(
                Mock.Of<IAntiforgery>(),
                optionsMock.Object,
                provider,
                Mock.Of<IUrlHelperFactory>(),
                HtmlEncoder.Default,
                new ClientValidatorCache());

            HtmlHelper<TestModel> htmlHelper = new HtmlHelper<TestModel>(
                generator,
                Mock.Of<ICompositeViewEngine>(),
                provider,
                Mock.Of<IViewBufferScope>(),
                HtmlEncoder.Default,
                UrlEncoder.Default,
                new ExpressionTextCache());

            ViewContext context = new ViewContext();
            context.ViewData = new ViewDataDictionary<TestModel>(context.ViewData, testModel);

            htmlHelper.Contextualize(context);

            return htmlHelper;
        }

        private void CreatesAutocompleteAndHiddenInput(String id, Object actual)
        {
            String pattern = String.Format(@"<input(.*) id=""{0}{1}""(.*) /><input(.*) id=""{0}""(.*) />", id, MvcLookup.Prefix);

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsIdAttribute(String id, Object actual)
        {
            String pattern = $@"<input(.*) id=""{id}{MvcLookup.Prefix}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsLookupClassesForLookupInput(Object actual)
        {
            String pattern = @"<input(.*) class=""(.*)(form-control mvc-lookup-input|mvc-lookup-input form-control)(.*)""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsSpecifiedClass(String classAttribute, Object actual)
        {
            String pattern = $@"<input(.*) class=""(.*){classAttribute}(.*)""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsFiltersAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-filters=""{String.Join(",", lookup.AdditionalFilters)}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsRecordsPerPageAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-records-per-page=""{lookup.DefaultRecordsPerPage}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsSortColumnAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-sort-column=""{lookup.DefaultSortColumn}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsSortOrderAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-sort-order=""{lookup.DefaultSortOrder}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsDialogTitleAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-dialog-title=""{lookup.DialogTitle}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsHiddenInputAttribute(String id, Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-for=""{id}""(.*) />";
            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsUrlAttribute(Object actual)
        {
            String pattern = $@"<input(.*) data-mvc-lookup-url=""{lookup.Url}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsTermAttribute(Object actual)
        {
            String pattern = @"<input(.*) data-mvc-lookup-term=""""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsPageAttribute(Object actual)
        {
            String pattern = @"<input(.*) data-mvc-lookup-page=""0""(.*) />";
            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsIdForHiddenInput(String id, Object actual)
        {
            String pattern = $@"<input(.*) id=""{id}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsValueForHiddenInput(String value, Object actual)
        {
            String pattern = $@"/><input(.*) value=""{value}""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void AddsLookupClassesForHiddenInput(Object actual)
        {
            String pattern = @"<input(.*) class=""mvc-lookup-hidden-input""(.*) />";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }

        private void CreatesAutocompleteAndHiddenInputFromExpression(Expression<Func<TestModel, String>> expression, Object actual)
        {
            String expressionId = TagBuilder.CreateSanitizedId(ExpressionHelper.GetExpressionText(expression), html.IdAttributeDotReplacement);
            CreatesAutocompleteAndHiddenInput(expressionId, actual);
        }
        private void AddsIdAttributeFromExpression(Expression<Func<TestModel, String>> expression, Object actual)
        {
            String expressionId = TagBuilder.CreateSanitizedId(ExpressionHelper.GetExpressionText(expression), html.IdAttributeDotReplacement);
            AddsIdAttribute(expressionId, actual);
        }
        private void AddsHiddenInputAttributeFromExpression(Expression<Func<TestModel, String>> expression, Object actual)
        {
            String expressionId = TagBuilder.CreateSanitizedId(ExpressionHelper.GetExpressionText(expression), html.IdAttributeDotReplacement);
            AddsHiddenInputAttribute(expressionId, actual);
        }
        private void AddsIdForHiddenInputFromExpression(Expression<Func<TestModel, String>> expression, Object actual)
        {
            String expressionId = TagBuilder.CreateSanitizedId(ExpressionHelper.GetExpressionText(expression), html.IdAttributeDotReplacement);
            AddsIdForHiddenInput(expressionId, actual);
        }

        private void WrapsAutocompleteInInputGroup(Object actual)
        {
            String pattern = @"<div class=""input-group"">(.*)</div>";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }
        private void CreatesOpenSpan(Object actual)
        {
            String pattern = @"<span class=""mvc-lookup-open-span input-group-addon""><span class=""mvc-lookup-open-icon glyphicon""></span></span>";

            Assert.True(Regex.IsMatch(ToString(actual), pattern));
        }

        private String ToString(Object obj)
        {
            using (StringWriter writer = new StringWriter())
            {
                TagBuilder builder = obj as TagBuilder;
                if (builder != null)
                {
                    builder.WriteTo(writer, HtmlEncoder.Default);
                    obj = writer;
                }

                return obj.ToString();
            }
        }

        #endregion
    }
}
