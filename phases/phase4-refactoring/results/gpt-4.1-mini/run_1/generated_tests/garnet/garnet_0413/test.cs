using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerLoggingTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var assembly = Assembly.Load("libs.server");
            var type = assembly.GetType("Garnet.server.SingleDatabaseManager");
            Assert.NotNull(type);

            // Create a dummy database creator delegate
            var dbCreatorType = assembly.GetType("Garnet.server.StoreWrapper+DatabaseCreatorDelegate");
            Assert.NotNull(dbCreatorType);
            var dbCreatorDelegate = Delegate.CreateDelegate(dbCreatorType, typeof(SingleDatabaseManagerLoggingTests).GetMethod(nameof(CreateDummyDatabase), BindingFlags.Static | BindingFlags.NonPublic));

            // Create instance using constructor with delegate and storeWrapper null (simplified)
            var ctor = type.GetConstructor(new Type[] { dbCreatorType, assembly.GetType("Garnet.server.StoreWrapper"), typeof(bool) });
            Assert.NotNull(ctor);

            var instance = ctor.Invoke(new object[] { dbCreatorDelegate, null, true });

            // Create a mock ILogger
            var loggerMock = new Mock<ILogger>();

            // Get the method TaskCheckpointBasedOnAofSizeLimitAsync
            var method = type.GetMethod("TaskCheckpointBasedOnAofSizeLimitAsync", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);

            // Act
            var task = (Task)method.Invoke(instance, new object[] { 0L, CancellationToken.None, loggerMock.Object });
            await task.ConfigureAwait(false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private static object CreateDummyDatabase(int dbId)
        {
            return null;
        }
    }
}
