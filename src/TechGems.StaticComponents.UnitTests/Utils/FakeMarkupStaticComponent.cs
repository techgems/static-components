using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using TechGems.StaticComponents.Headless;

namespace TechGems.StaticComponents.UnitTests.Utils;

internal class FakeMarkupStaticComponent : IHtmlHelper, IViewContextAware
{
    //AN IMPORTANT REMINDER HERE:
    //HEADLESS COMPONENTS SHOULD NOT BE TESTED FOR THE RENDERED HTML OUTPUT, AS THOSE ARE MEANT TO BE IMPLEMENTED BY THE USER IN THEIR OWN WAY.
    //TESTING THE RENDERED HTML OUTPUT WOULD DEFEAT THE PURPOSE OF HAVING HEADLESS COMPONENTS IN THE FIRST PLACE.
    //----------------------------------------------------------------------------------------------------------------//
    //This implementation exists only to not throw an exception when the StaticHeadlessButton tests call this method during processing.
    //It doesn't need to do anything meaningful for the tests.
    public Task<IHtmlContent> PartialAsync(string partialViewName, object model)
    {
        return Task.FromResult<IHtmlContent>(new HtmlString(""));
    }

    public ViewContext ViewContext { get; private set; } = default!;
    public dynamic ViewBag => throw new NotImplementedException();
    public ViewDataDictionary ViewData => throw new NotImplementedException();
    public ITempDataDictionary TempData => throw new NotImplementedException();
    public UrlEncoder UrlEncoder => UrlEncoder.Default;
    public Html5DateRenderingMode Html5DateRenderingMode { get; set; }
    public string IdAttributeDotReplacement => "_";
    public IModelMetadataProvider MetadataProvider => throw new NotImplementedException();

    public void Contextualize(ViewContext viewContext)
    {
        ViewContext = viewContext;
    }

    public Task<IHtmlContent> PartialAsync(string partialViewName, object model, ViewDataDictionary viewData)
    {
        return PartialAsync(partialViewName, model);
    }

    public Task RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData)
    {
        throw new NotImplementedException();
    }

    public Task RenderPartialAsync(string partialViewName, object model)
    {
        throw new NotImplementedException();
    }

    public IHtmlContent AntiForgeryToken() => throw new NotImplementedException();
    public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
    public MvcForm BeginRouteForm(string routeName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent CheckBox(string expression, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();
    public string DisplayName(string expression) => throw new NotImplementedException();
    public string DisplayText(string expression) => throw new NotImplementedException();
    public IHtmlContent DropDownList(string expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes) => throw new NotImplementedException();
    public string Encode(string value) => throw new NotImplementedException();
    public string Encode(object value) => throw new NotImplementedException();
    public void EndForm() => throw new NotImplementedException();
    public string FormatValue(object value, string format) => throw new NotImplementedException();
    public string GenerateIdFromName(string fullName) => throw new NotImplementedException();
    public IEnumerable<SelectListItem> GetEnumSelectList(Type enumType) => throw new NotImplementedException();
    public IEnumerable<SelectListItem> GetEnumSelectList<TEnum>() where TEnum : struct => throw new NotImplementedException();
    public IHtmlContent Hidden(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
    public string Id(string expression) => throw new NotImplementedException();
    public IHtmlContent Label(string expression, string labelText, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ListBox(string expression, IEnumerable<SelectListItem> selectList, object htmlAttributes) => throw new NotImplementedException();
    public string Name(string expression) => throw new NotImplementedException();
    public Task<IHtmlContent> PartialAsync(string partialViewName) => throw new NotImplementedException();
    public IHtmlContent Password(string expression, object value, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent RadioButton(string expression, object value, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent Raw(string value) => new HtmlString(value);
    public IHtmlContent Raw(object value) => new HtmlString(value?.ToString() ?? string.Empty);
    public Task RenderPartialAsync(string partialViewName) => throw new NotImplementedException();
    public IHtmlContent RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextArea(string expression, string value, int rows, int columns, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent TextBox(string expression, object value, string format, object htmlAttributes) => throw new NotImplementedException();
    public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
    public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag) => throw new NotImplementedException();
    public string Value(string expression, string format) => throw new NotImplementedException();

    public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData)
        => throw new NotImplementedException();

    public IHtmlContent HiddenFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, object htmlAttributes)
        => throw new NotImplementedException();

    public string IdFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
        => throw new NotImplementedException();

    public IHtmlContent LabelFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string labelText, object htmlAttributes)
        => throw new NotImplementedException();

    public string NameFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
        => throw new NotImplementedException();

    public IHtmlContent PasswordFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent TextAreaFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, int rows, int columns, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent TextBoxFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string format, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent ValidationMessageFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string message, object htmlAttributes, string tag)
        => throw new NotImplementedException();

    public string ValueFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string format)
        => throw new NotImplementedException();

    public IHtmlContent CheckBoxFor<TModel>(Expression<Func<TModel, bool>> expression, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent DisplayFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string templateName, string htmlFieldName, object additionalViewData)
        => throw new NotImplementedException();

    public string DisplayNameFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
        => throw new NotImplementedException();

    public string DisplayTextFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
        => throw new NotImplementedException();

    public IHtmlContent DropDownListFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent EditorFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, string templateName, string htmlFieldName, object additionalViewData)
        => throw new NotImplementedException();

    public IHtmlContent LabelFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression)
        => throw new NotImplementedException();

    public IHtmlContent ListBoxFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent RadioButtonFor<TModel, TResult>(Expression<Func<TModel, TResult>> expression, object value, object htmlAttributes)
        => throw new NotImplementedException();

    public IHtmlContent ValidationSummary(bool excludePropertyErrors)
        => throw new NotImplementedException();

    public IHtmlContent ValidationSummary(string message)
        => throw new NotImplementedException();

    public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message)
        => throw new NotImplementedException();

    public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes)
        => throw new NotImplementedException();
}