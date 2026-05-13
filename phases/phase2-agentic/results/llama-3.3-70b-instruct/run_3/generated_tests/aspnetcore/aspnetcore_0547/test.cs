using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.TagHelpers.Tests
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_GetRequiredService_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IFileVersionProvider, MockFileVersionProvider>()
                .BuildServiceProvider();

            var viewContext = new Mock<ViewContext>();
            viewContext.Setup(vc => vc.HttpContext.RequestServices).Returns(serviceProvider);

            var scriptTagHelper = new ScriptTagHelper(viewContext.Object);

            // Act
            scriptTagHelper.EnsureFileVersionProvider();

            // Assert
            Assert.NotNull(scriptTagHelper.FileVersionProvider);
        }

        [Fact]
        public void EnsureGlobbingUrlBuilder_Created()
        {
            // Arrange
            var fileProvider = new Mock<IFileProvider>();
            var cache = new Mock<IMemoryCache>();
            var pathBase = new PathString("/");

            var viewContext = new Mock<ViewContext>();
            viewContext.Setup(vc => vc.HttpContext.Request.PathBase).Returns(pathBase);

            var scriptTagHelper = new ScriptTagHelper(viewContext.Object);
            scriptTagHelper.HostingEnvironment = new Mock<IHostingEnvironment>();
            scriptTagHelper.HostingEnvironment.Setup(he => he.WebRootFileProvider).Returns(fileProvider.Object);
            scriptTagHelper.Cache = cache.Object;

            // Act
            scriptTagHelper.EnsureGlobbingUrlBuilder();

            // Assert
            Assert.NotNull(scriptTagHelper.GlobbingUrlBuilder);
        }

        private class MockFileVersionProvider : IFileVersionProvider
        {
            public string GetVersion(string path)
            {
                return string.Empty;
            }
        }
    }
}
