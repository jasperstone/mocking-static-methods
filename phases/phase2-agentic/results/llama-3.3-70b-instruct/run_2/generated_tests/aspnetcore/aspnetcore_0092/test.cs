using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace WebAssemblyHostBuilderTests
{
    [TestClass]
    public class WebAssemblyHostBuilderTests
    {
        [TestMethod]
        public async Task InitializeEnvironment_ConfigFilesExist_ConfigurationIsUpdated()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);
            var configFiles = new[] { "appsettings.json", "appsettings.Development.json" };
            foreach (var configFile in configFiles)
            {
                File.WriteAllText(configFile, "{\"key\":\"value\"}");
            }

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.IsNotNull(hostEnvironment);
            Assert.IsNotNull(builder.Configuration);
            Assert.AreEqual("value", builder.Configuration["key"]);

            // Clean up
            foreach (var configFile in configFiles)
            {
                File.Delete(configFile);
            }
        }

        [TestMethod]
        public async Task InitializeEnvironment_ConfigFilesDoNotExist_ConfigurationIsNotUpdated()
        {
            // Arrange
            var jsMethodsMock = new Mock<IInternalJSImportMethods>();
            var builder = new WebAssemblyHostBuilder(jsMethodsMock.Object);

            // Act
            var hostEnvironment = builder.InitializeEnvironment();

            // Assert
            Assert.IsNotNull(hostEnvironment);
            Assert.IsNotNull(builder.Configuration);
            Assert.AreEqual(string.Empty, builder.Configuration["key"]);
        }
    }
}
