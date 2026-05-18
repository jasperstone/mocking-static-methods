using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bit.Core.Enums;
using Bit.Infrastructure.EntityFramework.SecretsManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Language;
using Xunit;

namespace Bit.Commercial.Infrastructure.EntityFramework.SecretsManager.Repositories.Tests
{
    public class ProjectRepositoryTests
    {
        private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private readonly Mock<IMapper> _mockMapper;
        private readonly ProjectRepository _repository;

        public ProjectRepositoryTests()
        {
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockMapper = new Mock<IMapper>();
            _repository = new ProjectRepository(_mockServiceScopeFactory.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task DeleteManyByIdAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var mockScope = new Mock<IServiceScope>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .Returns(mockScope.Object);

            // Act
            await _repository.DeleteManyByIdAsync(ids);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task AccessToProjectsAsync_CallsCreateAsyncScope()
        {
            // Arrange
            var projectIds = new List<Guid> { Guid.NewGuid() };
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;
            var mockScope = new Mock<IServiceScope>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .Returns(mockScope.Object);

            // Act
            await _repository.AccessToProjectsAsync(projectIds, userId, accessType);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        }

        [Fact]
        public async Task GetProjectCountByOrganizationIdAsync_WithAccessCheck_CallsCreateAsyncScope()
        {
            // Arrange
            var organizationId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var accessType = AccessClientType.User;
            var mockScope = new Mock<IServiceScope>();

            _mockServiceScopeFactory
                .Setup(f => f.CreateAsyncScope())
                .Returns(mockScope.Object);

            // Act
            await _repository.GetProjectCountByOrganizationIdAsync(organizationId, userId, accessType);

            // Assert
            _mockServiceScopeFactory.Verify(f => f.CreateAsyncScope(), Times.Once);
        }
    }
}
