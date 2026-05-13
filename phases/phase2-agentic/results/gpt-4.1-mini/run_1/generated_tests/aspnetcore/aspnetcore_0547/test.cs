using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Mvc.TagHelpers;

namespace Microsoft.AspNetCore.Mvc.TagHelpers
{
    public class ScriptTagHelperTests
    {
        [Fact]
        public void EnsureFileVersionProvider_GetsRequiredServiceFromRequestServices()
        {
            // Arrange
            var fileVersionProviderMock = new Mock<IFileVersionProvider>();
            var hostingEnvironmentMock = new Mock<IWebHostEnvironment>();
            var cacheProvider = new TagHelperMemoryCacheProvider();
            var htmlEncoder = HtmlEncoder.Default;
            var javaScriptEncoder = JavaScriptEncoder.Default;
            var urlHelperFactoryMock = new Mock<IUrlHelperFactory>();

            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IFileVersionProvider)))
                .Returns(fileVersionProviderMock.Object);

            var requestServicesMock = new Mock<IServiceProvider>();
            requestServicesMock
                .Setup(sp => sp.GetRequiredService<IFileVersionProvider>())
                .Returns(fileVersionProviderMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.SetupGet(c => c.RequestServices).Returns(requestServicesMock.Object);

            var viewContext = new ViewContext();
            viewContext.HttpContext = httpContextMock.Object;

            var scriptTagHelper = new ScriptTagHelper(
                hostingEnvironmentMock.Object,
                cacheProvider,
                null, // We will test the lazy loading of FileVersionProvider
                htmlEncoder,
                javaScriptEncoder,
                urlHelperFactoryMock.Object)
            {
                ViewContext = viewContext
            };

            // FileVersionProvider is initially null to test the call
            scriptTagHelper.FileVersionProvider = null;

            // Act
            // Call the private method EnsureFileVersionProvider via reflection
            var methodInfo = typeof(ScriptTagHelper).GetMethod("EnsureFileVersionProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            methodInfo.Invoke(scriptTagHelper, null);

            // Assert
            Assert.NotNull(scriptTagHelper.FileVersionProvider);
            Assert.Same(fileVersionProviderMock.Object, scriptTagHelper.FileVersionProvider);
        }
    }
}
