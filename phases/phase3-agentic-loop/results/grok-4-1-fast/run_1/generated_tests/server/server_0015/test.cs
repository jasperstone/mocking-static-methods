using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.SecretsManager.Entities;
using Bit.Core.SecretsManager.Models.Data.AccessPolicyUpdates;
using Bit.Infrastructure.EntityFramework;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests;

public class SecretRepositoryTests
{
    private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
    private readonly Mock<IAsyncDisposable> _mockAsyncScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<BitDbContext> _mockDbContext;
    private readonly Mock<IMapper> _mockMapper;
    private readonly SecretRepository _repository;

    public SecretRepositoryTests()
    {
        _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        _mockAsyncScope = new Mock<IAsyncDisposable>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockDbContext = new Mock<BitDbContext>();
        _mockMapper = new Mock<IMapper>();

        var mockScopeAsScope = _mockAsyncScope.As<IServiceScope>();
        mockScopeAsScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockAsyncScope.Setup(s => s.DisposeAsync()).Returns(ValueTask.CompletedTask);

        _mockServiceProvider.Setup(p => p.GetService(typeof(BitDbContext))).Returns(_mockDbContext.Object);

        _mockServiceScopeFactory.Setup(f => f.CreateAsyncScope()).ReturnsAsync(_mockAsyncScope.Object);

        _repository = new SecretRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task UpdateAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var secret = new Secret { Id = Guid.NewGuid() };
        _mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Secret>()))
                  .Returns(new Secret());
        _mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Secret>()))
                  .Returns(secret);

        // Act
        await _repository.UpdateAsync(secret);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CallsCreateAsyncScope()
    {
        // Arrange
        var secret = new Secret();
        _mockMapper.Setup(m => m.Map<Secret>(It.IsAny<Secret>()))
                  .Returns(new Secret());

        // Act
        await _repository.CreateAsync(secret);

        // Assert
        _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UsesAsyncScopeDispose()
    {
        // Arrange
        var secret = new Secret { Id = Guid.NewGuid() };
        _mockMapper.SetupSequence(m => m.Map<Secret>(It.IsAny<Secret>()))
                  .Returns(new Secret())
                  .Returns(new Secret());

        // Act
        await _repository.UpdateAsync(secret);

        // Assert
        _mockAsyncScope.Verify(s => s.DisposeAsync(), Times.Once);
    }
}
