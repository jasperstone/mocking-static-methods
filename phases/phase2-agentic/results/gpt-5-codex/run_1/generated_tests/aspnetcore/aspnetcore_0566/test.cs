using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.ViewFeatures;

public class RemoteAttributeTests
{
    [Fact]
    public void GetUrl_WhenUrlHelperFactoryMissing_ThrowsWithServiceType()
    {
        // Arrange
        var serviceProvider = new RecordingServiceProvider();
        var context = CreateValidationContext(serviceProvider);
        var attribute = new TestRemoteAttribute();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => attribute.InvokeGetUrl(context));

        Assert.Equal(typeof(IUrlHelperFactory), serviceProvider.RequestedServiceType);
        Assert.Contains("Microsoft.AspNetCore.Mvc.Routing.IUrlHelperFactory", exception.Message);
    }

    [Fact]
    public void GetUrl_UsesUrlHelperFactoryAndRouteData()
    {
        // Arrange
        var urlHelper = new TestUrlHelper("https://example.com/remote");
        var factory = new TestUrlHelperFactory(urlHelper);
        var serviceProvider = new FixedServiceProvider(factory);
        var context = CreateValidationContext(serviceProvider);
        var attribute = new TestRemoteAttribute("custom-route");

        attribute.SetRouteValue("controller", "Remote");
        attribute.SetRouteValue("action", "Validate");

        // Act
        var result = attribute.InvokeGetUrl(context);

        // Assert
        Assert.Equal("https://example.com/remote", result);
        Assert.Same(context.ActionContext, factory.CapturedActionContext);
        Assert.NotNull(urlHelper.ReceivedRouteContext);
        Assert.Equal("custom-route", urlHelper.ReceivedRouteContext!.RouteName);
        Assert.Same(attribute.RouteValues, urlHelper.ReceivedRouteContext!.Values);
        Assert.Same(context.ActionContext, urlHelper.ActionContext);
    }

    private static ClientModelValidationContext CreateValidationContext(IServiceProvider services)
    {
        var metadataProvider = new EmptyModelMetadataProvider();
        var httpContext = new DefaultHttpContext
        {
            RequestServices = services,
        };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var metadata = metadataProvider.GetMetadataForType(typeof(string));
        return new ClientModelValidationContext(actionContext, metadata, metadataProvider, new Dictionary<string, string>());
    }

    private sealed class TestRemoteAttribute : RemoteAttribute
    {
        public TestRemoteAttribute(string routeName = "default-route")
            : base(routeName)
        {
        }

        public string InvokeGetUrl(ClientModelValidationContext context) => GetUrl(context);

        public RouteValueDictionary RouteValues => RouteData;

        public void SetRouteValue(string key, object? value) => RouteData[key] = value;
    }

    private sealed class RecordingServiceProvider : IServiceProvider
    {
        public Type? RequestedServiceType { get; private set; }

        public object? GetService(Type serviceType)
        {
            RequestedServiceType = serviceType;
            return null;
        }
    }

    private sealed class FixedServiceProvider : IServiceProvider
    {
        private readonly object _service;

        public FixedServiceProvider(object service)
        {
            _service = service;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IUrlHelperFactory))
            {
                return _service;
            }

            return null;
        }
    }

    private sealed class TestUrlHelperFactory : IUrlHelperFactory
    {
        public TestUrlHelperFactory(TestUrlHelper urlHelper)
        {
            UrlHelper = urlHelper;
        }

        public TestUrlHelper UrlHelper { get; }

        public ActionContext? CapturedActionContext { get; private set; }

        public IUrlHelper GetUrlHelper(ActionContext context)
        {
            CapturedActionContext = context;
            UrlHelper.SetActionContext(context);
            return UrlHelper;
        }
    }

    private sealed class TestUrlHelper : IUrlHelper
    {
        private ActionContext? _actionContext;

        public TestUrlHelper(string urlToReturn)
        {
            UrlToReturn = urlToReturn;
        }

        public string UrlToReturn { get; }

        public UrlRouteContext? ReceivedRouteContext { get; private set; }

        public ActionContext ActionContext => _actionContext ?? throw new InvalidOperationException("ActionContext was not initialized.");

        public void SetActionContext(ActionContext actionContext)
        {
            ArgumentNullException.ThrowIfNull(actionContext);
            _actionContext = actionContext;
        }

        public string? Action(UrlActionContext actionContext) => throw new NotImplementedException();

        public string? Content(string? contentPath) => throw new NotImplementedException();

        public bool IsLocalUrl(string? url) => throw new NotImplementedException();

        public string? Link(string? routeName, object? values) => throw new NotImplementedException();

        public string? RouteUrl(UrlRouteContext routeContext)
        {
            ReceivedRouteContext = routeContext;
            return UrlToReturn;
        }
    }
}
