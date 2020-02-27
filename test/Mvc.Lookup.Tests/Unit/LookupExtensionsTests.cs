﻿using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
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
            html = CreateHtmlHelper();
            lookup = new TestLookup<TestModel>();

            lookup.Filter.Rows = 11;
            lookup.Dialog = "Dialog";
            lookup.Filter.Offset = 22;
            lookup.Name = "LookupName";
            lookup.Filter.Sort = "First";
            lookup.Placeholder = "Search";
            lookup.Filter.Search = "Test";
            lookup.AdditionalFilters.Clear();
            lookup.AdditionalFilters.Add("Add1");
            lookup.AdditionalFilters.Add("Add2");
            lookup.Title = "Dialog lookup title";
            lookup.Url = "http://localhost/Lookup";
            lookup.Filter.Order = LookupSortOrder.Desc;
        }

        [Fact]
        public void AutoComplete_Readonly()
        {
            lookup.ReadOnly = true;

            String actual = ToString(html.AutoComplete("Test", lookup, "Value", new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup-readonly mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"True\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoComplete_FromLookup()
        {
            String actual = ToString(html.AutoComplete("Test", lookup, "Value", new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"Value\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoComplete_FromMultiLookup()
        {
            lookup.Multi = true;

            String actual = ToString(html.AutoComplete("Test", lookup, new[] { "Value1", "Value2" }, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"True\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"Test\" type=\"hidden\" value=\"Value1\" />" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"Test\" type=\"hidden\" value=\"Value2\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_Readonly()
        {
            lookup.ReadOnly = true;

            String actual = ToString(html.AutoCompleteFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup-readonly mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"True\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_FromLookup()
        {
            String actual = ToString(html.AutoCompleteFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AutoCompleteFor_FromMultiLookup()
        {
            lookup.Multi = true;
            html.ViewData.Model.Values = new[] { "Value1's", "Value2's" };

            String actual = ToString(html.AutoCompleteFor(model => model.Values, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-browseless mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Values\" data-multi=\"True\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"Values\" type=\"hidden\" value=\"Value1&#x27;s\" />" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"Values\" type=\"hidden\" value=\"Value2&#x27;s\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Lookup_Readonly()
        {
            lookup.ReadOnly = true;

            String actual = ToString(html.Lookup("Test", lookup, 1, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-readonly mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"True\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"1\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Lookup_FromLookup()
        {
            String actual = ToString(html.Lookup("Test", lookup, 1, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"Test\" name=\"Test\" type=\"hidden\" value=\"1\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Lookup_FromMultiLookup()
        {
            lookup.Multi = true;

            String actual = ToString(html.Lookup("Test", lookup, 1, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Test\" data-multi=\"True\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Test\">" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Test\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"Test\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupFor_Readonly()
        {
            lookup.ReadOnly = true;

            String actual = ToString(html.LookupFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup-readonly mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"True\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" readonly=\"readonly\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"ParentId\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupFor_FromLookup()
        {
            String actual = ToString(html.LookupFor(model => model.ParentId, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"ParentId\" data-multi=\"False\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-value\" id=\"ParentId\" name=\"ParentId\" type=\"hidden\" value=\"Model&#x27;s parent ID\" />" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"ParentId\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"ParentId\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupFor_FromMultiLookup()
        {
            lookup.Multi = true;

            String actual = ToString(html.LookupFor(model => model.Values, lookup, new { @class = "classes", attribute = "attr" }));
            String expected =
                "<div attribute=\"attr\" class=\"mvc-lookup classes\" data-dialog=\"Dialog\" " +
                        "data-filters=\"Add1,Add2\" data-for=\"Values\" data-multi=\"True\" data-offset=\"22\" data-order=\"Desc\" " +
                        "data-readonly=\"False\" data-rows=\"11\" data-search=\"Test\" data-sort=\"First\" " +
                        "data-title=\"Dialog lookup title\" data-url=\"http://localhost/Lookup\">" +
                    "<div class=\"mvc-lookup-values\" data-for=\"Values\">" +
                    "</div>" +
                    "<div class=\"mvc-lookup-control\" data-for=\"Values\">" +
                        "<input autocomplete=\"off\" class=\"mvc-lookup-input\" id=\"LookupName\" name=\"LookupName\" placeholder=\"Search\" type=\"text\" value=\"\" />" +
                        "<div class=\"mvc-lookup-control-loader\"></div>" +
                        "<div class=\"mvc-lookup-control-error\">!</div>" +
                    "</div>" +
                    "<button class=\"mvc-lookup-browser\" data-for=\"Values\" type=\"button\">" +
                        "<span class=\"mvc-lookup-icon\"></span>" +
                    "</button>" +
                "</div>";

            Assert.Equal(expected, actual);
        }

        private IHtmlHelper<TestModel> CreateHtmlHelper()
        {
            IOptions<MvcViewOptions> options = Options.Create(new MvcViewOptions());
            IModelMetadataProvider provider = new EmptyModelMetadataProvider();

            IHtmlGenerator generator = new DefaultHtmlGenerator(
                Substitute.For<IAntiforgery>(),
                options,
                provider,
                Substitute.For<IUrlHelperFactory>(),
                HtmlEncoder.Default,
                new DefaultValidationHtmlAttributeProvider(options, provider, new ClientValidatorCache()));

            HtmlHelper<TestModel> htmlHelper = new HtmlHelper<TestModel>(
                generator,
                Substitute.For<ICompositeViewEngine>(),
                provider,
                Substitute.For<IViewBufferScope>(),
                HtmlEncoder.Default,
                UrlEncoder.Default,
                new ExpressionTextCache());

            ViewContext context = new ViewContext();
            context.HttpContext = new DefaultHttpContext();
            TestModel model = new TestModel { ParentId = "Model's parent ID" };
            context.ViewData = new ViewDataDictionary<TestModel>(context.ViewData, model);

            htmlHelper.Contextualize(context);

            return htmlHelper;
        }

        private String ToString(IHtmlContent content)
        {
            using StringWriter writer = new StringWriter();

            content.WriteTo(writer, HtmlEncoder.Default);

            return writer.ToString();
        }
    }
}
