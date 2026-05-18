using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Moq;

namespace Mvc.ViewFeatures.Tests
{
    public class DefaultDisplayTemplatesTests
    {
        [Fact]
        public void CollectionTemplate_NullModel_ReturnsEmpty()
        {
            // Arrange
            var htmlHelper = CreateHtmlHelper<object>(model: null);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlString>(result);
            Assert.Equal(string.Empty, ((HtmlString)result).Value);
        }

        [Fact]
        public void CollectionTemplate_NonEnumerable_Throws()
        {
            // Arrange
            var model = new object();
            var htmlHelper = CreateHtmlHelper(model);
            // Force model to be non-enumerable
            htmlHelper.Object.ViewData.Model = model;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => DefaultDisplayTemplates.CollectionTemplate(htmlHelper));
        }

        [Fact]
        public void CollectionTemplate_EnumerableWithObjectModelType_CallsGetMetadataForType()
        {
            // Arrange
            var items = new List<object> { "item1", "item2" };
            var modelMetadata = new Mock<ModelMetadata>();
            modelMetadata.Setup(m => m.IsNullableValueType).Returns(false);
            modelMetadata.Setup(m => m.ModelType).Returns(typeof(object));
            var metadataProvider = new Mock<IModelMetadataProvider>();
            metadataProvider.Setup(p => p.GetMetadataForType(typeof(string)))
                .Returns(new ModelMetadataDummy { ModelType = typeof(string), NullDisplayText = "null" });
            var serviceProvider = new ServiceProviderDummy
            {
                GetRequiredService = (type) =>
                {
                    if (type == typeof(IModelMetadataProvider))
                        return metadataProvider.Object;
                    if (type == typeof(ICompositeViewEngine))
                        return new Mock<ICompositeViewEngine>().Object;
                    if (type == typeof(IViewBufferScope))
                        return new Mock<IViewBufferScope>().Object;
                    return null;
                }
            };
            var viewContext = new ViewContextDummy();
            var htmlHelper = new HtmlHelperDummy
            {
                ViewContext = viewContext,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = items,
                    TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" },
                    ModelExplorer = new ModelExplorerDummy(metadataProvider.Object, null, modelMetadata.Object, null)
                }
            };
            // Inject services
            viewContext.HttpContext = new DefaultHttpContextDummy(serviceProvider);
            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }

        [Fact]
        public void CollectionTemplate_EnumerableWithNullModel_DoesNotCallGetMetadataForType()
        {
            // Arrange
            var items = new List<string> { "a", "b" };
            var serviceProvider = new ServiceProviderDummy
            {
                GetRequiredService = (type) =>
                {
                    if (type == typeof(ICompositeViewEngine))
                        return new Mock<ICompositeViewEngine>().Object;
                    if (type == typeof(IViewBufferScope))
                        return new Mock<IViewBufferScope>().Object;
                    return null;
                }
            };
            var viewContext = new ViewContextDummy();
            var htmlHelper = new HtmlHelperDummy
            {
                ViewContext = viewContext,
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = items,
                    TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "prefix" },
                    ModelExplorer = new ModelExplorerDummy(new EmptyModelMetadataProvider(), null, null, null)
                }
            };
            viewContext.HttpContext = new DefaultHttpContextDummy(serviceProvider);

            // Act
            var result = DefaultDisplayTemplates.CollectionTemplate(htmlHelper);

            // Assert
            Assert.IsType<HtmlContentBuilder>(result);
        }

        // Helper classes and methods
        private static IHtmlHelper CreateHtmlHelper<T>(T model)
        {
            var mockViewData = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model,
                TemplateInfo = new TemplateInfo()
            };
            var mockViewContext = new ViewContextDummy();
            var mockHtmlHelper = new Mock<IHtmlHelper>();
            mockHtmlHelper.Setup(h => h.ViewData).Returns(mockViewData);
            mockHtmlHelper.Setup(h => h.ViewContext).Returns(mockViewContext);
            return mockHtmlHelper.Object;
        }

        private class ViewContextDummy : Microsoft.AspNetCore.Mvc.Rendering.ViewContext
        {
            public override HttpContext HttpContext { get; set; } = new DefaultHttpContextDummy();
        }

        private class DefaultHttpContextDummy : Microsoft.AspNetCore.Http.HttpContext
        {
            public override IServiceProvider RequestServices { get; set; }
            public DefaultHttpContextDummy(IServiceProvider serviceProvider = null)
            {
                RequestServices = serviceProvider ?? new ServiceProviderDummy();
            }
            // Other members can be left unimplemented for brevity
            public override IFeatureCollection Features => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.ConnectionInfo Connection => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.WebSocketManager WebSockets => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.IRequestCookieCollection RequestCookies => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.IResponseCookies ResponseCookies => throw new NotImplementedException();
            public override System.IO.Stream RequestBody { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override System.IO.Stream ResponseBody { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override Microsoft.AspNetCore.Http.IHeaderDictionary RequestHeaders => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.IHeaderDictionary ResponseHeaders => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.IRequestFeature Features => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.HttpRequest Request => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.HttpResponse Response => throw new NotImplementedException();
            public override Microsoft.AspNetCore.Http.ConnectionInfo Connection => throw new NotImplementedException();
            public override System.Threading.CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override System.Net.IPAddress RemoteIpAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override System.Net.IPAddress LocalIpAddress { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public override void Abort() => throw new NotImplementedException();
        }

        private class ServiceProviderDummy : IServiceProvider
        {
            public Func<Type, object> GetRequiredService { get; set; } = (type) => null;
            public object GetService(Type serviceType) => GetRequiredService(serviceType);
        }

        private class ModelExplorerDummy : ModelExplorer
        {
            public ModelExplorerDummy(IModelMetadataProvider provider, object container, ModelMetadata metadata, object model)
                : base(provider, container, metadata, model)
            {
            }
            public override string GetSimpleDisplayText() => Model?.ToString() ?? string.Empty;
        }

        private class HtmlHelperDummy : IHtmlHelper
        {
            public ViewDataDictionary ViewData { get; set; }
            public ViewContext ViewContext { get; set; }
            public string Encode(string value) => value;
            public IHtmlContent Encode(object value) => new HtmlString(value?.ToString() ?? string.Empty);
            public IHtmlContent Partial(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
            public IHtmlContent Partial(string partialViewName, object model) => throw new NotImplementedException();
            public IHtmlContent PartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
            public IHtmlContent PartialAsync(string partialViewName, object model) => throw new NotImplementedException();
            public IHtmlContent Raw(object value) => new HtmlString(value?.ToString() ?? string.Empty);
            public IHtmlContent Raw(string value) => new HtmlString(value);
            public IHtmlContent Action(string actionName, string controllerName, object routeValues, string protocol, string host) => throw new NotImplementedException();
            public IHtmlContent Action(string actionName, object routeValues) => throw new NotImplementedException();
            public IHtmlContent Action(string actionName) => throw new NotImplementedException();
            public IHtmlContent RenderPartial(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
            public IHtmlContent RenderPartial(string partialViewName, object model) => throw new NotImplementedException();
            public IHtmlContent RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();
            public IHtmlContent RenderPartialAsync(string partialViewName, object model) => throw new NotImplementedException();
            public IHtmlContent Display(string displayName) => throw new NotImplementedException();
            public IHtmlContent Display(string displayName, string templateName) => throw new NotImplementedException();
            public IHtmlContent DisplayFor<TModel, TValue>(System.Linq.Expressions.Expression<Func<TModel, TValue>> expression) => throw new NotImplementedException();
            public IHtmlContent DisplayFor<TModel, TValue>(System.Linq.Expressions.Expression<Func<TModel, TValue>> expression, string templateName) => throw new NotImplementedException();
            public IHtmlContent Editor(string expression) => throw new NotImplementedException();
            public IHtmlContent Editor(string expression, string templateName) => throw new NotImplementedException();
            public IHtmlContent EditorFor<TModel, TValue>(System.Linq.Expressions.Expression<Func<TModel, TValue>> expression) => throw new NotImplementedException();
            public IHtmlContent EditorFor<TModel, TValue>(System.Linq.Expressions.Expression<Func<TModel, TValue>> expression, string templateName) => throw new NotImplementedException();
            public IHtmlHelper<TModel> AsHtmlHelper<TModel>() => throw new NotImplementedException();
            public ViewDataDictionary ViewData { get; set; }
        }
    }
}
