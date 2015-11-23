using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.WebEncoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public static class LookupExtensions
    {
        public static IHtmlContent AutoComplete<TModel>(this IHtmlHelper<TModel> html,
            String name, Object value, AbstractLookup model, Object htmlAttributes = null)
        {
            using (StringWriter writer = new StringWriter())
            {
                FormAutoComplete(html, model, name, htmlAttributes).WriteTo(writer, HtmlEncoder.Default);
                FormHiddenInput(html, name, value).WriteTo(writer, HtmlEncoder.Default);

                return new HtmlString(writer.ToString());
            }
        }
        public static IHtmlContent AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)
        {
            return html.AutoCompleteFor(expression, GetModelFromExpression(expression), htmlAttributes);
        }
        public static IHtmlContent AutoCompleteFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, AbstractLookup model, Object htmlAttributes = null)
        {
            String name = ExpressionHelper.GetExpressionText(expression);

            using (StringWriter writer = new StringWriter())
            {
                FormAutoComplete(html, model, name, htmlAttributes).WriteTo(writer, HtmlEncoder.Default);
                FormHiddenInputFor(html, expression).WriteTo(writer, HtmlEncoder.Default);

                return new HtmlString(writer.ToString());
            }
        }

        public static IHtmlContent Lookup<TModel>(this IHtmlHelper<TModel> html,
            String name, Object value, AbstractLookup model, Object htmlAttributes = null)
        {
            TagBuilder inputGroup = new TagBuilder("div");
            inputGroup.AddCssClass("input-group");
            inputGroup.InnerHtml.Append(html.AutoComplete(name, value, model, htmlAttributes));
            inputGroup.InnerHtml.Append(FormLookupOpenSpan());

            return inputGroup;
        }
        public static IHtmlContent LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, Object htmlAttributes = null)
        {
            return html.LookupFor(expression, GetModelFromExpression(expression), htmlAttributes);
        }
        public static IHtmlContent LookupFor<TModel, TProperty>(this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TProperty>> expression, AbstractLookup model, Object htmlAttributes = null)
        {
            TagBuilder inputGroup = new TagBuilder("div");
            inputGroup.AddCssClass("input-group");
            inputGroup.InnerHtml.Append(html.AutoCompleteFor(expression, model, htmlAttributes));
            inputGroup.InnerHtml.Append(FormLookupOpenSpan());

            return inputGroup;
        }

        private static AbstractLookup GetModelFromExpression<TModel, TProperty>(Expression<Func<TModel, TProperty>> expression)
        {
            MemberExpression exp = expression.Body as MemberExpression;
            LookupAttribute lookup = exp.Member.GetCustomAttribute<LookupAttribute>();

            if (lookup == null)
                throw new LookupException($"'{exp.Member.Name}' property does not have a '{typeof(LookupAttribute).Name}' specified.");

            return (AbstractLookup)Activator.CreateInstance(lookup.Type);
        }
        private static IHtmlContent FormAutoComplete(IHtmlHelper html, AbstractLookup model, String hiddenInput, Object htmlAttributes)
        {
            IDictionary<String, Object> attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);
            if (attributes.ContainsKey("class"))
                attributes["class"] = $"{attributes["class"]} form-control mvc-lookup-input".Trim();
            else
                attributes["class"] = "form-control mvc-lookup-input";
            attributes.Add("data-mvc-lookup-for", TagBuilder.CreateSanitizedId(hiddenInput, "_"));
            attributes.Add("data-mvc-lookup-filters", String.Join(",", model.AdditionalFilters));
            attributes.Add("data-mvc-lookup-records-per-page", model.DefaultRecordsPerPage);
            attributes.Add("data-mvc-lookup-sort-column", model.DefaultSortColumn);
            attributes.Add("data-mvc-lookup-sort-order", model.DefaultSortOrder);
            attributes.Add("data-mvc-lookup-dialog-title", model.DialogTitle);
            attributes.Add("data-mvc-lookup-url", model.LookupUrl);
            attributes.Add("data-mvc-lookup-term", "");
            attributes.Add("data-mvc-lookup-page", 0);

            return html.TextBox(hiddenInput + AbstractLookup.Prefix, null, attributes);
        }

        private static IHtmlContent FormHiddenInputFor<TModel, TProperty>(IHtmlHelper<TModel> html, Expression<Func<TModel, TProperty>> expression)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes.Add("class", "mvc-lookup-hidden-input");

            return html.HiddenFor(expression, attributes);
        }
        private static IHtmlContent FormHiddenInput(IHtmlHelper html, String name, Object value)
        {
            IDictionary<String, Object> attributes = new Dictionary<String, Object>();
            attributes.Add("class", "mvc-lookup-hidden-input");

            return html.Hidden(name, value, attributes);
        }

        private static IHtmlContent FormLookupOpenSpan()
        {
            TagBuilder outerSpan = new TagBuilder("span");
            TagBuilder innerSpan = new TagBuilder("span");

            outerSpan.AddCssClass("mvc-lookup-open-span input-group-addon");
            innerSpan.AddCssClass("mvc-lookup-open-icon glyphicon");
            outerSpan.InnerHtml.Append(innerSpan);

            return outerSpan;
        }
    }
}
