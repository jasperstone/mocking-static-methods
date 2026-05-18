using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc.Routing.Tests
{
    public class DynamicControllerEndpointMatcherPolicyTests
    {
        [Fact]
        public async Task ApplyAsync_WithTransformerMetadata_ShouldCallGetRequiredService()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            var mockCandidateSet = new Mock<CandidateSet>();
            var mockEndpoint = new Mock<Endpoint>();
            var mockMetadata = new Mock<DynamicControllerRouteValueTransformerMetadata>(typeof(DynamicRouteValueTransformer), null);

            mockHttpContext.Setup(h => h.RequestServices).Returns(mockRequestServices.Object);
            mockRequestServices.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(mockTransformer.Object);
            mockCandidateSet.Setup(c => c.Count).Returns(1);
            mockCandidateSet.Setup(c => c.IsValidCandidate(0)).Returns(true);
            mockCandidateSet.Setup(c => c[0]).Returns(new Candidate(mockEndpoint.Object, new RouteValueDictionary()));
            mockEndpoint.Setup(e => e.Metadata.GetMetadata<DynamicControllerRouteValueTransformerMetadata>()).Returns(mockMetadata.Object);

            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(mockHttpContext.Object, mockCandidateSet.Object);

            // Assert
            mockRequestServices.Verify(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer)), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_WithTransformerMetadata_ShouldSetTransformerState()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            var mockCandidateSet = new Mock<CandidateSet>();
            var mockEndpoint = new Mock<Endpoint>();
            var mockMetadata = new Mock<DynamicControllerRouteValueTransformerMetadata>(typeof(DynamicRouteValueTransformer), null);

            mockHttpContext.Setup(h => h.RequestServices).Returns(mockRequestServices.Object);
            mockRequestServices.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(mockTransformer.Object);
            mockCandidateSet.Setup(c => c.Count).Returns(1);
            mockCandidateSet.Setup(c => c.IsValidCandidate(0)).Returns(true);
            mockCandidateSet.Setup(c => c[0]).Returns(new Candidate(mockEndpoint.Object, new RouteValueDictionary()));
            mockEndpoint.Setup(e => e.Metadata.GetMetadata<DynamicControllerRouteValueTransformerMetadata>()).Returns(mockMetadata.Object);

            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(mockHttpContext.Object, mockCandidateSet.Object);

            // Assert
            mockTransformer.VerifySet(t => t.State = null);
        }

        [Fact]
        public async Task ApplyAsync_WithTransformerMetadata_ShouldCallTransformAsync()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            var mockCandidateSet = new Mock<CandidateSet>();
            var mockEndpoint = new Mock<Endpoint>();
            var mockMetadata = new Mock<DynamicControllerRouteValueTransformerMetadata>(typeof(DynamicRouteValueTransformer), null);

            mockHttpContext.Setup(h => h.RequestServices).Returns(mockRequestServices.Object);
            mockRequestServices.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(mockTransformer.Object);
            mockCandidateSet.Setup(c => c.Count).Returns(1);
            mockCandidateSet.Setup(c => c.IsValidCandidate(0)).Returns(true);
            mockCandidateSet.Setup(c => c[0]).Returns(new Candidate(mockEndpoint.Object, new RouteValueDictionary()));
            mockEndpoint.Setup(e => e.Metadata.GetMetadata<DynamicControllerRouteValueTransformerMetadata>()).Returns(mockMetadata.Object);

            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(mockHttpContext.Object, mockCandidateSet.Object);

            // Assert
            mockTransformer.Verify(t => t.TransformAsync(mockHttpContext.Object, It.IsAny<RouteValueDictionary>()), Times.Once);
        }

        [Fact]
        public async Task ApplyAsync_WithTransformerMetadata_ShouldCallFilterAsync()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequestServices = new Mock<IServiceProvider>();
            var mockTransformer = new Mock<DynamicRouteValueTransformer>();
            var mockCandidateSet = new Mock<CandidateSet>();
            var mockEndpoint = new Mock<Endpoint>();
            var mockMetadata = new Mock<DynamicControllerRouteValueTransformerMetadata>(typeof(DynamicRouteValueTransformer), null);

            mockHttpContext.Setup(h => h.RequestServices).Returns(mockRequestServices.Object);
            mockRequestServices.Setup(s => s.GetRequiredService(typeof(DynamicRouteValueTransformer))).Returns(mockTransformer.Object);
            mockCandidateSet.Setup(c => c.Count).Returns(1);
            mockCandidateSet.Setup(c => c.IsValidCandidate(0)).Returns(true);
            mockCandidateSet.Setup(c => c[0]).Returns(new Candidate(mockEndpoint.Object, new RouteValueDictionary()));
            mockEndpoint.Setup(e => e.Metadata.GetMetadata<DynamicControllerRouteValueTransformerMetadata>()).Returns(mockMetadata.Object);

            var policy = new DynamicControllerEndpointMatcherPolicy(new DynamicControllerEndpointSelectorCache(), new EndpointMetadataComparer());

            // Act
            await policy.ApplyAsync(mockHttpContext.Object, mockCandidateSet.Object);

            // Assert
            mockTransformer.Verify(t => t.FilterAsync(mockHttpContext.Object, It.IsAny<RouteValueDictionary>(), It.IsAny<IReadOnlyList<Endpoint>>()), Times.Once);
        }
    }
}
