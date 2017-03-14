using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace NonFactors.Mvc.Lookup
{
    public static class LookupExtensions
    {
        public static TagBuilder AutoComplete<TModel>(this IHtmlHelper<TModel> html,
            String name, MvcLookup model, Object value = null, Object htmlAttributes = null)
        {
            TagBuilder lookup = CreateLookup();
            lookup.AddCssClass("mvc-lookup-browseless");
            lookup.InnerHtml.AppendHtml(CreateLookupValues(html, model, name, value));
            lookup.InnerHtml.AppendHtml(CreateLookupControl(model, name, htmlAttributes));

            return lookup;
        }
        public static TagBuilder AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)
        {
            TagBuilder lookup = CreateLookup();
            lookup.AddCssClass("mvc-lookup-browseless");
            String name = ExpressionHelper.GetExpressionText(expression);
            lookup.InnerHtml.AppendHtml(CreateLookupValues(html, model, expression));
            lookup.InnerHtml.AppendHtml(CreateLookupControl(model, name, htmlAttributes));

            return lookup;
        }

        public static TagBuilder Lookup<TModel>(this IHtmlHelper<TModel> html,
            String name, MvcLookup model, Object value = null, Object htmlAttributes = null)
        {
            TagBuilder lookup = CreateLookup();
            lookup.InnerHtml.AppendHtml(CreateLookupValues(html, model, name, value));
            lookup.InnerHtml.AppendHtml(CreateLookupControl(model, name, htmlAttributes));
            lookup.InnerHtml.AppendHtml(CreateLookupBrowse(name));

            return lookup;
        }
        public static TagBuilder LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, MvcLookup model, Object htmlAttributes = null)
        {
            TagBuilder lookup = CreateLookup();
            String name = ExpressionHelper.GetExpressionText(expression);
            lookup.InnerHtml.AppendHtml(CreateLookupValues(html, model, expression));
            lookup.InnerHtml.AppendHtml(CreateLookupControl(model, name, htmlAttributes));
            lookup.InnerHtml.AppendHtml(CreateLookupBrowse(name));

            return lookup;
        }

        private static TagBuilder CreateLookup()
        {
            TagBuilder lookup = new TagBuilder("div");
            lookup.AddCssClass("mvc-lookup");

            return lookup;
        }
        private static IHtmlContent CreateLookupValues<TModel, TProperty>(IHtmlHelper<TModel> html, MvcLookup model, Expression<Func<TModel, TProperty>> expression)
        {
            Object value = ExpressionMetadataProvider.FromLambdaExpression(expression, html.ViewData, html.MetadataProvider).Model;
            String name = ExpressionHelper.GetExpressionText(expression);

            if (model.Multi)
                return CreateLookupValues(html, model, name, value);

            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "mvc-lookup-value";

            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("mvc-lookup-values");
            container.Attributes["data-for"] = name;

            container.InnerHtml.AppendHtml(html.HiddenFor(expression, attributes));

            return container;
        }
        private static IHtmlContent CreateLookupValues(IHtmlHelper html, MvcLookup model, String name, Object value)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes["class"] = "mvc-lookup-value";

            TagBuilder container = new TagBuilder("div");
            container.AddCssClass("mvc-lookup-values");
            container.Attributes["data-for"] = name;

            if (model.Multi)
            {
                IEnumerable<Object> values = (value as IEnumerable)?.Cast<Object>();
                if (values == null) return container;

                IHtmlContentBuilder inputs = new HtmlContentBuilder();
                foreach (Object val in values)
                    inputs.AppendHtml(html.Hidden(name, val, attributes));

                container.InnerHtml.AppendHtml(inputs);
            }
            else
            {
                container.InnerHtml.AppendHtml(html.Hidden(name, value, attributes));
            }

            return container;
        }

        private static IHtmlContent CreateLookupControl(MvcLookup lookup, String name, Object htmlAttributes)
        {
            IDictionary<String, Object> attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            attributes["data-filters"] = String.Join(",", lookup.AdditionalFilters);
            attributes["data-multi"] = lookup.Multi.ToString().ToLower();
            attributes["data-search"] = lookup.Filter.Search;
            attributes["data-order"] = lookup.Filter.Order;
            attributes["data-page"] = lookup.Filter.Page;
            attributes["data-rows"] = lookup.Filter.Rows;
            attributes["data-sort"] = lookup.Filter.Sort;
            attributes["data-dialog"] = lookup.Dialog;
            attributes["data-title"] = lookup.Title;
            attributes["data-url"] = lookup.Url;
            attributes["data-for"] = name;

            TagBuilder search = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            TagBuilder control = new TagBuilder("div");
            TagBuilder loader = new TagBuilder("div");

            loader.AddCssClass("mvc-lookup-control-loader");
            control.AddCssClass("mvc-lookup-control");
            search.AddCssClass("mvc-lookup-input");
            control.InnerHtml.AppendHtml(search);
            control.InnerHtml.AppendHtml(loader);
            control.MergeAttributes(attributes);

            return control;
        }
        private static IHtmlContent CreateLookupBrowse(String name)
        {
            TagBuilder browse = new TagBuilder("div");
            browse.AddCssClass("mvc-lookup-browse");
            browse.Attributes["data-for"] = name;

            return browse;
        }
    }
}
