using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Migrations.Routines.Tests
{
    public class MigrateLinkedChildrenLoggingTests
    {
        [Fact]
        public void Logger_LogInformation_NoItemsFromDeletedLibrariesFound_IsCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            var dbContextFactoryMock = new Mock<object>();
            var libraryManagerMock = new Mock<object>();
            var appHostMock = new Mock<object>();
            var appPathsMock = new Mock<object>();

            var assembly = Assembly.GetExecutingAssembly();
            var migrateType = assembly.GetType("Jellyfin.Server.Migrations.Routines.MigrateLinkedChildren");
            Assert.NotNull(migrateType);

            var ctor = migrateType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(c =>
                {
                    var ps = c.GetParameters();
                    return ps.Length == 5;
                });
            Assert.NotNull(ctor);

            var instance = ctor.Invoke(new object[] {
                loggerFactoryMock.Object,
                dbContextFactoryMock.Object,
                libraryManagerMock.Object,
                appHostMock.Object,
                appPathsMock.Object
            });

            var performMethod = migrateType.GetMethod("Perform", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(performMethod);

            // Act
            performMethod.Invoke(instance, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No items from deleted libraries found.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
