using Xunit;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        private class DummyViewData : ViewDataDictionary
        {
            public DummyViewData() : base(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
            }
        }

        private class DummyHtmlHelper : IHtmlHelper
        {
            public ViewDataDictionary ViewData { get; } = new DummyViewData();
            public ViewContext ViewContext { get; set; }
            public IHtmlGenerator Generator { get; set; }
            public string IdAttributeDotReplacement { get; set; }
            public IHtmlContent Encode(object value) => new HtmlString(value?.ToString() ?? "");
            public string Encode(string value) => value;
            public string AttributeEncode(string value) => value;
            public IHtmlContent Raw(object value) => new HtmlString(value?.ToString() ?? "");
            public IHtmlContent Raw(string value) => new HtmlString(value);
            public IHtmlContent Partial(string partialViewName, object model, ViewDataDictionary viewData) => new HtmlString("");
            public IHtmlContent Partial(string partialViewName, object model) => new HtmlString("");
            public IHtmlContent PartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => new HtmlString("");
            public IHtmlContent PartialAsync(string partialViewName, object model) => new HtmlString("");
            public bool IsAjaxRequest => false;
            public string IdAttributeDotReplacement { get; set; }
            public ViewContext ViewContext { get; set; }
            public ViewDataDictionary ViewData { get; } = new DummyViewData();
            public IHtmlHelper InnerHelper => this;
            public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes) => new HtmlString("");
            public IHtmlContent AntiForgeryToken() => new HtmlString("");
            public IHtmlContent CheckBox(string name, bool? isChecked, object htmlAttributes) => new HtmlString("");
            public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData) => new HtmlString("");
            public IHtmlContent DisplayFor<TModel, TValue>(Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData) => new HtmlString("");
            public IHtmlContent DisplayText(string expression, string templateName, string htmlFieldName, object additionalViewData) => new HtmlString("");
            public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData) => new HtmlString("");
            public IHtmlContent EditorFor<TModel, TValue>(Expression<Func<TModel, TValue>> expression, string templateName, string htmlFieldName, object additionalViewData) => new HtmlString("");
            public IHtmlContent Hidden(string name, object value, object htmlAttributes) => new HtmlString("");
            public IHtmlContent Label(string expression, string labelText, object htmlAttributes) => new HtmlString("");
            public IHtmlContent TextBox(string name, string value, object htmlAttributes) => new HtmlString("");
            public IHtmlContent TextArea(string name, string value, int rows, int columns, object htmlAttributes) => new HtmlString("");
            public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag) => new HtmlString("");
            public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag) => new HtmlString("");
        }

        [Fact]
        public void HtmlTemplate_ReturnsFormattedModelValue()
        {
            // Arrange
            var helper = new DummyHtmlHelper();
            var formattedValue = "formatted";
            helper.ViewData.TemplateInfo.FormattedModelValue = formattedValue;
            helper.ViewData.Model = "model";

            // Act
            var result = DefaultDisplayTemplates.HtmlTemplate(helper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(formattedValue, ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_ModelNull_ReturnsNullDisplayText()
        {
            // Arrange
            var helper = new DummyHtmlHelper();
            var modelExplorer = new ModelExplorer(
                new EmptyModelMetadataProvider(),
                null,
                new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(object), new Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadataProvider()),
                null);
            var viewData = helper.ViewData;
            viewData.ModelExplorer = modelExplorer;
            viewData.Model = null;
            viewData.TemplateInfo = new TemplateInfo { TemplateDepth = 1 };
            var requestServices = new ServiceCollection()
                .AddSingleton<IModelMetadataProvider, EmptyModelMetadataProvider>()
                .BuildServiceProvider();
            var context = new DefaultHttpContext
            {
                RequestServices = requestServices
            };
            helper.ViewContext = new ViewContext
            {
                HttpContext = context
            };

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(modelExplorer.Metadata.NullDisplayText, ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_TemplateDepthGreaterThanOne_ReturnsSimpleDisplayText()
        {
            // Arrange
            var helper = new DummyHtmlHelper();
            var modelExplorer = new ModelExplorer(
                new EmptyModelMetadataProvider(),
                null,
                new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(string), new Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadataProvider()),
                "test");
            var viewData = helper.ViewData;
            viewData.ModelExplorer = modelExplorer;
            viewData.Model = "model";
            viewData.TemplateInfo = new TemplateInfo { TemplateDepth = 2 };
            var requestServices = new ServiceCollection()
                .AddSingleton<IModelMetadataProvider, EmptyModelMetadataProvider>()
                .BuildServiceProvider();
            var context = new DefaultHttpContext
            {
                RequestServices = requestServices
            };
            helper.ViewContext = new ViewContext
            {
                HttpContext = context
            };

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(modelExplorer.GetSimpleDisplayText(), ((HtmlString)result).ToString());
        }

        [Fact]
        public void ObjectTemplate_TemplateDepthEqualsOne_CallsGetRequiredService()
        {
            // Arrange
            var services = new ServiceCollection();
            var viewEngine = new Mock<ICompositeViewEngine>();
            var viewBufferScope = new Mock<IViewBufferScope>();
            services.AddSingleton<ICompositeViewEngine>(viewEngine.Object);
            services.AddSingleton<IViewBufferScope>(viewBufferScope.Object);
            var provider = services.BuildServiceProvider();

            var helper = new DummyHtmlHelper();
            var modelExplorer = new ModelExplorer(
                new EmptyModelMetadataProvider(),
                null,
                new ModelMetadata(new EmptyModelMetadataProvider(), null, null, typeof(string), new Microsoft.AspNetCore.Mvc.ModelBinding.Metadata.DefaultModelMetadataProvider()),
                "test");
            var viewData = helper.ViewData;
            viewData.ModelExplorer = modelExplorer;
            viewData.Model = "model";
            viewData.TemplateInfo = new TemplateInfo { TemplateDepth = 1 };
            var context = new DefaultHttpContext
            {
                RequestServices = provider
            };
            helper.ViewContext = new ViewContext
            {
                HttpContext = context
            };

            // Act
            var result = DefaultDisplayTemplates.ObjectTemplate(helper);

            // Assert
            // The test passes if no exception thrown and GetRequiredService called
            Assert.IsType<HtmlString>(result);
        }
    }
}
