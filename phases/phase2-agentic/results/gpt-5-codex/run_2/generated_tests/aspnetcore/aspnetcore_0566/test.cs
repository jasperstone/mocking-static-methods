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

namespace Microsoft.AspNetCore.Mvc.ViewFeatures.Tests
{
    public class RemoteAttributeTests
    {
        [Fact]
        public void GetUrl_ResolvesUrlHelperFactoryUsingRequestServices()
        {
            // Arrange
            var attribute = new RemoteAttribute("MyAction", "MyController");
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = metadataProvider.GetMetadataForType(typeof(string));
            var validationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                metadataProvider,
                new Dictionary<string, string>());

            var factory = new RecordingUrlHelperFactory();
            var services = new TrackingServiceProvider();
            services.AddService<IUrlHelperFactory>(factory);
            httpContext.RequestServices = services;

            // Act
            var url = attribute.GetUrl(validationContext);

            // Assert
            Assert.Equal("generated-url", url);
            Assert.Same(actionContext, factory.ReceivedActionContext);
            Assert.NotNull(factory.Helper);
            Assert.NotNull(factory.Helper!.ReceivedRouteContext);

            var routeValues = Assert.IsType<RouteValueDictionary>(factory.Helper!.ReceivedRouteContext!.Values);
            Assert.Equal("MyAction", routeValues["action"]);
            Assert.Equal("MyController", routeValues["controller"]);
            Assert.Contains(typeof(IUrlHelperFactory), services.RequestedServices);
        }

        [Fact]
        public void GetUrl_ThrowsInvalidOperationExceptionWhenRouteUrlReturnsNull()
        {
            // Arrange
            var attribute = new RemoteAttribute("MyAction", "MyController");
            var httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var metadataProvider = new EmptyModelMetadataProvider();
            var modelMetadata = metadataProvider.GetMetadataForType(typeof(string));
            var validationContext = new ClientModelValidationContext(
                actionContext,
                modelMetadata,
                metadataProvider,
                new Dictionary<string, string>());

            var factory = new NullUrlHelperFactory();
            var services = new TrackingServiceProvider();
            services.AddService<IUrlHelperFactory>(factory);
            httpContext.RequestServices = services;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => attribute.GetUrl(validationContext));
            Assert.Same(actionContext, factory.ReceivedActionContext);
            Assert.NotNull(factory.Helper);
            Assert.Equal(1, factory.Helper!.RouteUrlInvocationCount);
            Assert.Contains(typeof(IUrlHelperFactory), services.RequestedServices);
        }

        private sealed class TrackingServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object?> _services = new();

            public IList<Type> RequestedServices { get; } = new List<Type>();

            public void AddService<TService>(TService instance)
            {
                _services[typeof(TService)] = instance;
            }

            public object? GetService(Type serviceType)
            {
                RequestedServices.Add(serviceType);
                _services.TryGetValue(serviceType, out var service);
                return service;
            }
        }

        private sealed class RecordingUrlHelperFactory : IUrlHelperFactory
        {
            public ActionContext? ReceivedActionContext { get; private set; }
            public RecordingUrlHelper? Helper { get; private set; }

            public IUrlHelper GetUrlHelper(ActionContext context)
            {
                ReceivedActionContext = context;
                Helper = new RecordingUrlHelper(context);
                return Helper;
            }
        }

        private sealed class NullUrlHelperFactory : IUrlHelperFactory
        {
            public ActionContext? ReceivedActionContext { get; private set; }
            public NullReturningUrlHelper? Helper { get; private set; }

            public IUrlHelper GetUrlHelper(ActionContext context)
            {
                ReceivedActionContext = context;
                Helper = new NullReturningUrlHelper(context);
                return Helper;
            }
        }

        private abstract class TestUrlHelperBase : IUrlHelper
        {
            protected TestUrlHelperBase(ActionContext actionContext)
            {
                ActionContext = actionContext;
            }

            public ActionContext ActionContext { get; }

            public virtual string? Action(UrlActionContext actionContext) => throw new NotImplementedException();

            public virtual string? Action(string? action, string? controller, object? values, string? protocol, string? host, string? fragment)
                => throw new NotImplementedException();

            public virtual string? Content(string? contentPath) => throw new NotImplementedException();

            public virtual bool IsLocalUrl(string? url) => throw new NotImplementedException();

            public virtual string? Link(string? routeName, object? values) => throw new NotImplementedException();

            public virtual string? RouteUrl(string? routeName, object? values, string? protocol, string? host, string? fragment)
                => throw new NotImplementedException();

            public abstract string? RouteUrl(UrlRouteContext routeContext);
        }

        private sealed class RecordingUrlHelper : TestUrlHelperBase
        {
            public RecordingUrlHelper(ActionContext actionContext)
                : base(actionContext)
            {
            }

            public UrlRouteContext? ReceivedRouteContext { get; private set; }

            public override string? RouteUrl(UrlRouteContext routeContext)
            {
                ReceivedRouteContext = routeContext;
                return "generated-url";
            }
        }

        private sealed class NullReturningUrlHelper : TestUrlHelperBase
        {
            public NullReturningUrlHelper(ActionContext actionContext)
                : base(actionContext)
            {
            }

            public int RouteUrlInvocationCount { get; private set; }

            public override string? RouteUrl(UrlRouteContext routeContext)
            {
                RouteUrlInvocationCount++;
                return null;
            }
        }
    }
}
