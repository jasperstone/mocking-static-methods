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
            var serviceProviderMock = new Mock<IServiceProvider>();
            var fileVersionProviderMock = new Mock<IFileVersionProvider>();
            serviceProviderMock.Setup(p => p.GetRequiredService<IFileVersionProvider>()).Returns(fileVersionProviderMock.Object);

            var scriptTagHelper = new ScriptTagHelper(
                Mock.Of<IWebHostEnvironment>(),
                Mock.Of<TagHelperMemoryCacheProvider>(),
                null,
                Mock.Of<HtmlEncoder>(),
                Mock.Of<JavaScriptEncoder>(),
                Mock.Of<IUrlHelperFactory>());

            scriptTagHelper.ViewContext = new ViewContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProviderMock.Object
                }
            };

            // Act
            scriptTagHelper.EnsureFileVersionProvider();

            // Assert
            serviceProviderMock.Verify(p => p.GetRequiredService<IFileVersionProvider>(), Times.Once);
        }
    }
}
