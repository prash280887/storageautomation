using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Wpp.StorageAutomation.DataMovement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wpp.StorageAutomation.DataMovement.Models;
using Wpp.StorageAutomation.Security;
using Wpp.StorageAutomation.DataMovement.Repository;

namespace Wpp.StorageAutomation.UnitTests.DataMovement
{
    [TestClass()]
    public class MakeOfflineTest : ControllerBase
    {
        private readonly Mock<IBaseSecurity> baseSecurityMock;
        private readonly Mock<IProductionStoreRepository> productionStoreRepositoryMock;
        private readonly Mock<IProductionRepository> productionRepositoryMock;
        private readonly MakeOffline makeOffline;

        public MakeOfflineTest()
        {
            this.baseSecurityMock = new Mock<IBaseSecurity>();
            this.productionStoreRepositoryMock = new Mock<IProductionStoreRepository>();
            this.productionRepositoryMock = new Mock<IProductionRepository>();
            this.makeOffline = new MakeOffline(this.baseSecurityMock.Object, this.productionStoreRepositoryMock.Object, this.productionRepositoryMock.Object);
        }

        [TestMethod()]
        public async Task MakeProductionOffline_DoesnotThrowException()
        {
            // Arrange
            var prodStoreId = "testProdStoreId";
            var prodId = "testProdId";
            var requestMock = new Mock<HttpRequest>();
            var starterMock = new Mock<IDurableOrchestrationClient>();
            var logMock = new Mock<ILogger>();
            var id = "8e503c5e-19de-40e1-932d-298c4263115c";

            var userGroupResponse = new UserGroupResponse()
            {
                HasAccess = true
            };
            var expectedResponse = Ok();

            this.baseSecurityMock.Setup(x => x.ValidateUserGroupsAccess(It.IsAny<HttpRequest>(), It.IsAny<ILogger>(), It.IsAny<string>())).Returns(userGroupResponse);
            starterMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), It.IsAny<ProductionRequest>())).Returns(Task.FromResult<string>(id));
            starterMock.Setup(x => x.CreateCheckStatusResponse(It.IsAny<HttpRequest>(), It.IsAny<string>(), false)).Returns(expectedResponse);

            // Act
            var result = await makeOffline.MakeProductionOffline(requestMock.Object, prodStoreId, prodId, starterMock.Object, logMock.Object);

            // Assert
            starterMock.Verify(client => client.StartNewAsync("MakeProductionOfflineOrchestrator", It.IsAny<ProductionRequest>()), Times.Once);
            starterMock.Verify(client => client.CreateCheckStatusResponse(requestMock.Object, id, false), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public async Task MakeProductionOffline_userHasAccess_false()
        {
            // Arrange
            var prodStoreId = "testProdStoreId";
            var prodId = "testProdId";
            var requestMock = new Mock<HttpRequest>();
            var starterMock = new Mock<IDurableOrchestrationClient>();
            var logMock = new Mock<ILogger>();
            var id = "8e503c5e-19de-40e1-932d-298c4263115c";

            var userGroupResponse = new UserGroupResponse()
            {
                HasAccess = false
            };
            var expectedResponse = Ok();

            this.baseSecurityMock.Setup(x => x.ValidateUserGroupsAccess(It.IsAny<HttpRequest>(), It.IsAny<ILogger>(), It.IsAny<string>())).Returns(userGroupResponse);
            starterMock.Setup(x => x.StartNewAsync(It.IsAny<string>(), It.IsAny<ProductionRequest>())).Returns(Task.FromResult<string>(id));
            starterMock.Setup(x => x.CreateCheckStatusResponse(It.IsAny<HttpRequest>(), It.IsAny<string>(), false)).Returns(expectedResponse);

            // Act
            var result = await makeOffline.MakeProductionOffline(requestMock.Object, prodStoreId, prodId, starterMock.Object, logMock.Object);

            // Assert
            starterMock.Verify(client => client.StartNewAsync("MakeProductionOfflineOrchestrator", It.IsAny<ProductionRequest>()), Times.Never);
            starterMock.Verify(client => client.CreateCheckStatusResponse(requestMock.Object, id, false), Times.Never);
            Assert.IsNotNull(result);
        }
    }
}
