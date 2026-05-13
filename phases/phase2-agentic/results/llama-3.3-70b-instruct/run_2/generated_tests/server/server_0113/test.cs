using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Bit.Core;
using Bit.Core.Auth.Identity;
using Bit.Core.Auth.Models.Business.Tokenables;
using Moq;

namespace Bit.SharedWeb.Utilities.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddTokenizers_GetRequiredService_Called()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddDataProtection()
                .AddLogging()
                .BuildServiceProvider();

            var loggerMock = new Mock<ILogger<DataProtectorTokenFactory<EmergencyAccessInviteTokenable>>>();
            serviceProvider.GetService<ILoggerFactory>().AddProvider(new MockLoggerProvider(loggerMock.Object));

            // Act
            var services = new ServiceCollection();
            services.AddTokenizers();
            var serviceProvider2 = services.BuildServiceProvider();

            // Assert
            loggerMock.Verify(l => l.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<ITimestamp>(), It.IsAny<Exception>(), It.IsAny<Func<ITimestamp, Exception, string>>()), Times.Once);
        }
    }

    public class MockLoggerProvider : ILoggerProvider
    {
        private readonly ILogger _logger;

        public MockLoggerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
        }
    }
}
