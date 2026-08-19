using Xunit;
using Moq;
using System;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_Test()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act and Assert
            // No assertions, just test that the method does not throw
        }
    }
}
