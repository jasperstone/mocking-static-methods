using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Tests.Migrations.Routines
{
    public class ReseedFolderFlagTests
    {
        [Fact]
        public async Task PerformAsync_LogsSkipped_WhenRerunGuardFlagIsTrue()
        {
            // Arrange
            var assembly = Assembly.Load("Jellyfin.Server");
            var type = assembly.GetType("Jellyfin.Server.Migrations.Routines.ReseedFolderFlag");
            Assert.NotNull(type);

            var loggerMock = new Mock<ILogger>();
            var providerMock = new Mock<object>();
            var pathsMock = new Mock<object>();

            // Set RerunGuardFlag to true via reflection
            var rerunGuardFlagProp = type.GetProperty("RerunGuardFlag", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            rerunGuardFlagProp.SetValue(null, true);

            // Create instance via constructor
            var ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new Type[] { loggerMock.Object.GetType(), providerMock.Object.GetType(), pathsMock.Object.GetType() },
                null);

            // The constructor parameters are interfaces, so the above will fail.
            // Instead, find any constructor and invoke with mocks cast to object
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            object instance = null;
            foreach (var c in constructors)
            {
                var parameters = c.GetParameters();
                if (parameters.Length == 3)
                {
                    instance = c.Invoke(new object[] { loggerMock.Object, providerMock.Object, pathsMock.Object });
                    break;
                }
            }
            Assert.NotNull(instance);

            // Act
            var performAsyncMethod = type.GetMethod("PerformAsync", BindingFlags.Instance | BindingFlags.Public);
            var task = (Task)performAsyncMethod.Invoke(instance, new object[] { CancellationToken.None });
            await task;

            // Assert
            loggerMock.Verify(l => l.LogInformation("Migration is skipped because it does not apply."), Times.Once);
        }
    }
}
