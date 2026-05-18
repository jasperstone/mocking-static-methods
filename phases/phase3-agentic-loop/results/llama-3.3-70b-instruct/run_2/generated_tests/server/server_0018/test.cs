using AutoMapper;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        [Fact]
        public async Task RestoreManyByIdAsync_ValidIds_RestoresSecrets()
        {
            // Arrange
            var serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            var mapperMock = new Mock<IMapper>();

            var serviceScopeMock = new Mock<IServiceScope>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock.Setup(sp => sp.GetService(typeof(DbContext))).Returns(new object());
            serviceScopeMock.Setup(ss => ss.ServiceProvider).Returns(serviceProviderMock.Object);

            serviceScopeFactoryMock.Setup(ssf => ssf.CreateAsyncScope()).ReturnsAsync(serviceScopeMock.Object);

            var secretRepository = new SecretRepository(serviceScopeFactoryMock.Object, mapperMock.Object);

            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

            // Act
            await secretRepository.RestoreManyByIdAsync(ids);

            // Assert
            serviceScopeFactoryMock.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
