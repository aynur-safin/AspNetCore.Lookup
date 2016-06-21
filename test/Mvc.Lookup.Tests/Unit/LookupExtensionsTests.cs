using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Options;
using NonFactors.Mvc.Lookup.Tests.Objects;
using NSubstitute;
using System;
using System.IO;
using System.Text.Encodings.Web;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupExtensionsTests
    {
        private TestLookup<TestModel> lookup;
        private IHtmlHelper<TestModel> html;

        public LookupExtensionsTests()
        {
            html = MockHtmlHelper();
            lookup = new TestLookup<TestModel>();

            lookup.AdditionalFilters.Clear();
            lookup.DefaultRecordsPerPage = 11;
            lookup.DefaultSortColumn = "First";
            lookup.DialogTitle = "Dialog title";
            lookup.AdditionalFilters.Add("Add1");
            lookup.AdditionalFilters.Add("Add2");
            lookup.Url = "http://localhost/Lookup";
            lookup.CurrentFilter.SearchTerm = "Terminal";
            lookup.DefaultSortOrder = LookupSortOrder.Desc;
        }

        #region AutoComplete<TModel>(this IHtmlHelper<TModel> html, String name, Object value, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void AutoComplete_FromModel()
        {
            String actual = ToString(html.AutoComplete("Test", "Value", lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Dialog title\" " +
                    "data-mvc-lookup-filters=\"Add1,Add2\" data-mvc-lookup-for=\"Test\" data-mvc-lookup-page=\"0\" " +
                    "data-mvc-lookup-records-per-page=\"11\" data-mvc-lookup-sort-column=\"First\" data-mvc-lookup-sort-order=\"Desc\" " +
                    "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Lookup\" id=\"TestLookup\" name=\"TestLookup\" type=\"text\" value=\"\" />" +
                "<input class=\"mvc-lookup-hidden-input\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)

        [Fact]
        public void AutoCompleteFor_NoModel_Throws()
        {
            Exception actual = Assert.Throws<LookupException>(() => html.AutoCompleteFor(model => model.Id));

            Assert.Equal("'Id' property does not have a 'LookupAttribute' specified.", actual.Message);
        }

        [Fact]
        public void AutoCompleteFor_Expression()
        {
            String actual = ToString(html.AutoCompleteFor(model => model.ParentId, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Test title\" " +
                    "data-mvc-lookup-filters=\"Test1,Test2\" data-mvc-lookup-for=\"ParentId\" data-mvc-lookup-page=\"0\" " +
                    "data-mvc-lookup-records-per-page=\"7\" data-mvc-lookup-sort-column=\"SortCol\" data-mvc-lookup-sort-order=\"Asc\" " +
                    "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Test\" id=\"ParentIdLookup\" name=\"ParentIdLookup\" type=\"text\" value=\"\" />" +
                "<input class=\"mvc-lookup-hidden-input\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void AutoCompleteFor_FromModelExpression()
        {
            String actual = ToString(html.AutoCompleteFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Dialog title\" " +
                    "data-mvc-lookup-filters=\"Add1,Add2\" data-mvc-lookup-for=\"ParentId\" data-mvc-lookup-page=\"0\" " +
                    "data-mvc-lookup-records-per-page=\"11\" data-mvc-lookup-sort-column=\"First\" data-mvc-lookup-sort-order=\"Desc\" " +
                    "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Lookup\" id=\"ParentIdLookup\" name=\"ParentIdLookup\" type=\"text\" value=\"\" />" +
                "<input class=\"mvc-lookup-hidden-input\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Lookup<TModel>(this IHtmlHelper<TModel> html, String name, Object value, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void Lookup_FromModel()
        {
            String actual = ToString(html.Lookup("Test", "Value", lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div class=\"input-group\">" +
                    "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Dialog title\" " +
                        "data-mvc-lookup-filters=\"Add1,Add2\" data-mvc-lookup-for=\"Test\" data-mvc-lookup-page=\"0\" " +
                        "data-mvc-lookup-records-per-page=\"11\" data-mvc-lookup-sort-column=\"First\" data-mvc-lookup-sort-order=\"Desc\" " +
                        "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Lookup\" id=\"TestLookup\" name=\"TestLookup\" type=\"text\" value=\"\" />" +
                    "<input class=\"mvc-lookup-hidden-input\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />" +
                    "<span class=\"mvc-lookup-open-span input-group-addon\">" +
                        "<span class=\"mvc-lookup-open-icon glyphicon\"></span>" +
                    "</span>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)

        [Fact]
        public void LookupFor_NoModel_Throws()
        {
            Exception actual = Assert.Throws<LookupException>(() => html.LookupFor(model => model.Id));

            Assert.Equal("'Id' property does not have a 'LookupAttribute' specified.", actual.Message);
        }

        [Fact]
        public void LookupFor_Expression()
        {
            String actual = ToString(html.LookupFor(model => model.ParentId, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div class=\"input-group\">" +
                    "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Test title\" " +
                    "data-mvc-lookup-filters=\"Test1,Test2\" data-mvc-lookup-for=\"ParentId\" data-mvc-lookup-page=\"0\" " +
                    "data-mvc-lookup-records-per-page=\"7\" data-mvc-lookup-sort-column=\"SortCol\" data-mvc-lookup-sort-order=\"Asc\" " +
                    "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Test\" id=\"ParentIdLookup\" name=\"ParentIdLookup\" type=\"text\" value=\"\" />" +
                    "<input class=\"mvc-lookup-hidden-input\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "<span class=\"mvc-lookup-open-span input-group-addon\">" +
                        "<span class=\"mvc-lookup-open-icon glyphicon\"></span>" +
                    "</span>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)

        [Fact]
        public void LookupFor_ExpressionWithModel()
        {
            String actual = ToString(html.LookupFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div class=\"input-group\">" +
                    "<input attribute=\"attr\" class=\"classes form-control mvc-lookup-input\" data-mvc-lookup-dialog-title=\"Dialog title\" " +
                    "data-mvc-lookup-filters=\"Add1,Add2\" data-mvc-lookup-for=\"ParentId\" data-mvc-lookup-page=\"0\" " +
                    "data-mvc-lookup-records-per-page=\"11\" data-mvc-lookup-sort-column=\"First\" data-mvc-lookup-sort-order=\"Desc\" " +
                    "data-mvc-lookup-term=\"\" data-mvc-lookup-url=\"http://localhost/Lookup\" id=\"ParentIdLookup\" name=\"ParentIdLookup\" type=\"text\" value=\"\" />" +
                    "<input class=\"mvc-lookup-hidden-input\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "<span class=\"mvc-lookup-open-span input-group-addon\">" +
                        "<span class=\"mvc-lookup-open-icon glyphicon\"></span>" +
                    "</span>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Test helpers

        private IHtmlHelper<TestModel> MockHtmlHelper()
        {
            IOptions<MvcViewOptions> options = Substitute.For<IOptions<MvcViewOptions>>();
            IModelMetadataProvider provider = new EmptyModelMetadataProvider();
            options.Value.Returns(new MvcViewOptions());

            IHtmlGenerator generator = new DefaultHtmlGenerator(
                Substitute.For<IAntiforgery>(),
                options,
                provider,
                Substitute.For<IUrlHelperFactory>(),
                HtmlEncoder.Default,
                new ClientValidatorCache());

            HtmlHelper<TestModel> htmlHelper = new HtmlHelper<TestModel>(
                generator,
                Substitute.For<ICompositeViewEngine>(),
                provider,
                Substitute.For<IViewBufferScope>(),
                HtmlEncoder.Default,
                UrlEncoder.Default,
                new ExpressionTextCache());

            TestModel model = new TestModel();
            model.ParentId = "Model's parent ID";
            ViewContext context = new ViewContext();
            context.ViewData = new ViewDataDictionary<TestModel>(context.ViewData, model);

            htmlHelper.Contextualize(context);

            return htmlHelper;
        }

        private String ToString(IHtmlContent html)
        {
            using (StringWriter writer = new StringWriter())
            {
                html.WriteTo(writer, HtmlEncoder.Default);

                return writer.ToString();
            }
        }

        #endregion
    }
}
