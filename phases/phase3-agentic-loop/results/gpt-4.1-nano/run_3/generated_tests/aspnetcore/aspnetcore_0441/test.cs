using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;
using System;

namespace ControllerBaseTests
{
    public class ControllerBaseMock : ControllerBase
    {
        public ControllerContext ControllerContextMock { get; set; }
        public ControllerBaseMock()
        {
            ControllerContextMock = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            ControllerContext = ControllerContextMock;
        }
    }

    public class ControllerBaseUnitTests
    {
        private ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IUrlHelperFactory, DefaultUrlHelperFactory>();
            services.AddTransient<IObjectModelValidator, DefaultObjectValidator>();
            services.AddTransient<IModelBinderFactory, DefaultModelBinderFactory>();
            services.AddTransient<IModelMetadataProvider, DefaultModelMetadataProvider>();
            services.AddTransient<ProblemDetailsFactory, DefaultProblemDetailsFactory>();
            return services.BuildServiceProvider();
        }

        [Fact]
        public void ModelBinderFactory_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var controller = new ControllerBaseMock();
            var serviceProvider = CreateServiceProvider();
            controller.HttpContext.RequestServices = serviceProvider;

            // Act
            var factory = controller.ModelBinderFactory;

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void Url_Should_Call_GetRequiredService_And_GetUrlHelper_When_Null()
        {
            // Arrange
            var controller = new ControllerBaseMock();
            var serviceProvider = CreateServiceProvider();
            controller.HttpContext.RequestServices = serviceProvider;
            var urlHelperFactory = serviceProvider.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = new MockUrlHelper();

            // Setup factory to return our mock url helper
            var mockFactory = new MockUrlHelperFactory(urlHelper);
            controller.HttpContext.RequestServices = new ServiceCollection()
                .AddSingleton<IUrlHelperFactory>(mockFactory)
                .BuildServiceProvider();

            // Act
            var url = controller.Url;

            // Assert
            Assert.NotNull(url);
            Assert.IsType<MockUrlHelper>(url);
        }

        [Fact]
        public void ObjectValidator_Should_Call_GetRequiredService_When_Null()
        {
            // Arrange
            var controller = new ControllerBaseMock();
            var serviceProvider = CreateServiceProvider();
            controller.HttpContext.RequestServices = serviceProvider;

            // Act
            var validator = controller.ObjectValidator;

            // Assert
            Assert.NotNull(validator);
        }
    }

    // Mock implementations
    public class MockUrlHelper : IUrlHelper
    {
        public string ActionContext => throw new NotImplementedException();
        public string Content(string contentPath) => throw new NotImplementedException();
        public bool IsLocalUrl(string url) => false;
        public string Link(string routeName, object values) => "";
        public string RouteUrl(UrlRouteContext routeContext) => "";
        public string EncodeUrl(string url) => url;
        public string Decode(string encodedUrl) => encodedUrl;
        public string GetPathByAction(string action, string controller, object values, string protocol, string host, string fragment) => "";
        public string GetUriByAction(string action, string controller, object values, string protocol, string host, string fragment) => "";
        public string GetPathByRoute(string routeName, object values, string protocol, string host, string fragment) => "";
        public string GetUriByRoute(string routeName, object values, string protocol, string host, string fragment) => "";
        public string GetEncodedUrl(string url) => url;
        public string GetDecodedUrl(string url) => url;
        public string GetPathByAction(string action, string controller, object values, string protocol, string host, string fragment, bool encode) => "";
        public string GetUriByAction(string action, string controller, object values, string protocol, string host, string fragment, bool encode) => "";
        public string GetPathByRoute(string routeName, object values, string protocol, string host, string fragment, bool encode) => "";
        public string GetUriByRoute(string routeName, object values, string protocol, string host, string fragment, bool encode) => "";
        public IUrlHelper GetUrlHelper(ControllerContext context) => this;
    }

    public class MockUrlHelperFactory : IUrlHelperFactory
    {
        private readonly IUrlHelper _urlHelper;
        public MockUrlHelperFactory(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }
        public IUrlHelper GetUrlHelper(ControllerContext context) => _urlHelper;
    }

    public class DefaultObjectValidator : IObjectModelValidator
    {
        public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model)
        {
            // no-op
        }
    }

    public class DefaultModelBinderFactory : IModelBinderFactory
    {
        public IModelBinder CreateBinder(ModelBinderFactoryContext context) => null;
    }

    public class DefaultModelMetadataProvider : IModelMetadataProvider
    {
        // Implementation omitted
    }

    public class DefaultProblemDetailsFactory : ProblemDetailsFactory
    {
        // Implementation omitted
    }
}
