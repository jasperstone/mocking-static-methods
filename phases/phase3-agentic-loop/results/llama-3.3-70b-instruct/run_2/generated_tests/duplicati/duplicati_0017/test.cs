using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Duplicati.Tests
{
    public class ConnectionTests
    {
        [Fact]
        public void SignalSettingsChanged_GetRequiredService()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var connection = new Duplicati.Library.RestAPI.Database.Connection(serviceProviderMock.Object);

            // Act
            connection.SignalSettingsChanged();

            // Assert
            // No assertion, just test if it compiles and runs without errors
        }
    }
}
