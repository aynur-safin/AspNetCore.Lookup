using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using NSubstitute;
using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupTagHelperTests
    {
        private LookupTagHelper helper;
        private TagHelperOutput output;
        private IUrlHelperFactory factory;

        public LookupTagHelperTests()
        {
            factory = Substitute.For<IUrlHelperFactory>();
            helper = new LookupTagHelper(Substitute.For<IHtmlGenerator>(), factory);
            helper.ViewContext = new ViewContext { HttpContext = new DefaultHttpContext() };
            output = new TagHelperOutput("div", new TagHelperAttributeList(), (useCachedResult, encoder) => null);
        }

        [Theory]
        [InlineData("/People/All", "/People/All")]
        [InlineData("~/People/All", "/People/All/Expand")]
        public void Process_Url(String url, String fullUrl)
        {
            helper.Url = url;
            IUrlHelper urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.Content(url).Returns("/People/All/Expand");
            factory.GetUrlHelper(helper.ViewContext).Returns(urlHelper);

            helper.Process(null, output);

            Object expected = fullUrl;
            Object actual = output.Attributes["data-url"].Value;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("", null, null, "mvc-lookup")]
        [InlineData("", null, true, "mvc-lookup")]
        [InlineData("", false, null, "mvc-lookup")]
        [InlineData("", false, true, "mvc-lookup")]
        [InlineData(null, null, null, "mvc-lookup")]
        [InlineData(null, null, true, "mvc-lookup")]
        [InlineData(null, false, null, "mvc-lookup")]
        [InlineData(null, false, true, "mvc-lookup")]
        [InlineData("test", null, null, "mvc-lookup test")]
        [InlineData("test", null, true, "mvc-lookup test")]
        [InlineData("test", false, null, "mvc-lookup test")]
        [InlineData("test", false, true, "mvc-lookup test")]
        [InlineData("", true, null, "mvc-lookup mvc-lookup-readonly")]
        [InlineData("", true, true, "mvc-lookup mvc-lookup-readonly")]
        [InlineData(null, true, null, "mvc-lookup mvc-lookup-readonly")]
        [InlineData(null, true, true, "mvc-lookup mvc-lookup-readonly")]
        [InlineData("", null, false, "mvc-lookup mvc-lookup-browseless")]
        [InlineData("", false, false, "mvc-lookup mvc-lookup-browseless")]
        [InlineData(null, null, false, "mvc-lookup mvc-lookup-browseless")]
        [InlineData(null, false, false, "mvc-lookup mvc-lookup-browseless")]
        [InlineData("test", true, null, "mvc-lookup mvc-lookup-readonly test")]
        [InlineData("test", true, true, "mvc-lookup mvc-lookup-readonly test")]
        [InlineData("test", null, false, "mvc-lookup mvc-lookup-browseless test")]
        [InlineData("test", false, false, "mvc-lookup mvc-lookup-browseless test")]
        [InlineData("", true, false, "mvc-lookup mvc-lookup-readonly mvc-lookup-browseless")]
        [InlineData(null, true, false, "mvc-lookup mvc-lookup-readonly mvc-lookup-browseless")]
        [InlineData("test", true, false, "mvc-lookup mvc-lookup-readonly mvc-lookup-browseless test")]
        public void Process_Classes(String classes, Boolean? isReadonly, Boolean? withBrowser, String fullClasses)
        {
            helper.Url = "/People/All";
            helper.Browser = withBrowser;
            helper.Readonly = isReadonly;

            if (classes != null)
                output.Attributes.SetAttribute("class", classes);

            helper.Process(null, output);

            Object expected = fullClasses;
            Object actual = output.Attributes["class"].Value;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Process_Attributes()
        {
            helper.Rows = 13;
            helper.Name = "Name";
            helper.Sort = "First";
            helper.Search = "Test";
            helper.Dialog = "Dialog";
            helper.AddHandler = true;
            helper.Filters = "Add1,Add2";
            helper.Placeholder = "Search";
            helper.LookupName = "LookupName";
            helper.Order = LookupSortOrder.Desc;
            helper.Title = "Dialog lookup title";
            helper.Url = "http://localhost/Lookup";

            helper.Process(null, output);

            Assert.Equal(11, output.Attributes.Count);
            Assert.Equal(13, output.Attributes["data-rows"].Value);
            Assert.Equal("First", output.Attributes["data-sort"].Value);
            Assert.Equal("Test", output.Attributes["data-search"].Value);
            Assert.Equal("mvc-lookup", output.Attributes["class"].Value);
            Assert.Equal("Dialog", output.Attributes["data-dialog"].Value);
            Assert.Equal(true, output.Attributes["data-add-handler"].Value);
            Assert.Equal("LookupName", output.Attributes["data-for"].Value);
            Assert.Equal("Add1,Add2", output.Attributes["data-filters"].Value);
            Assert.Equal(LookupSortOrder.Desc, output.Attributes["data-order"].Value);
            Assert.Equal("Dialog lookup title", output.Attributes["data-title"].Value);
            Assert.Equal("http://localhost/Lookup", output.Attributes["data-url"].Value);
        }

        [Fact]
        public void Process_Content()
        {
            helper.Multi = true;
            helper.LookupName = "LookupName";
            helper.Value = new[] { "first", "second" };

            helper.Process(null, output);

            String actual = $"{output.PreContent.GetContent()}{output.Content.GetContent()}{output.PostContent.GetContent()}";
            String expected =
                "<div class=\"mvc-lookup-values\" data-for=\"LookupName\">" +
                    "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"LookupName\" type=\"hidden\" value=\"\" />" +
                    "<input autocomplete=\"off\" class=\"mvc-lookup-value\" name=\"LookupName\" type=\"hidden\" value=\"\" />" +
                "</div>" +
                "<div class=\"mvc-lookup-control\" data-for=\"LookupName\">" +
                    "<input autocomplete=\"off\" class=\"mvc-lookup-input\" />" +
                    "<div class=\"mvc-lookup-control-loader\"></div>" +
                    "<div class=\"mvc-lookup-control-error\">!</div>" +
                "</div>" +
                "<button class=\"mvc-lookup-browser\" data-for=\"LookupName\" type=\"button\">" +
                    "<span class=\"mvc-lookup-icon\"></span>" +
                "</button>";

            Assert.Equal(expected, actual);
        }
    }
}
