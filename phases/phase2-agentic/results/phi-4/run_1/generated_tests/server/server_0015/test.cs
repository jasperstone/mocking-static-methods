using System;
using System.Threading.Tasks;
using Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories;
using Bit.Core.SecretsManager.Entities;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Tests.Repositories
{
    public class SecretRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly SecretRepository _secretRepository;

        public SecretRepositoryTests()
        {
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _secretRepository = new SecretRepository(_mockScopeFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task UpdateAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var secret = new Core.SecretsManager.Entities.Secret { Id = Guid.NewGuid() };
            var accessPoliciesUpdates = new SecretAccessPoliciesUpdates();

            // Act
            await _secretRepository.UpdateAsync(secret, accessPoliciesUpdates);

            // Assert
            _mockScopeFactory.Verify(sf => sf.CreateAsyncScope(), Times.Once);
        }
    }
}
