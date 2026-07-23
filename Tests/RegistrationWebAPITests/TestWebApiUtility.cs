using RegistrationWebApp.Components.Utilities;
using Moq;
using System.Net;
using Moq.Protected;

namespace Tests.RegistrationWebAPITests
{
    // Tests for the WebApiUtility class.
    public class TestWebApiUtility
    {

        [Fact]
        public async Task TestGetCountryNameFromIPAddress()
        {
            // Arrange
            Environment.SetEnvironmentVariable("WEB_API_URL", "http://localhost/");

            // Create a mock HttpMessageHandler so we do not have to
            // hit the external API.
            Mock<HttpMessageHandler> handlerMock = new();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "{\"country\":\"United States\"}")
                });

            // Put the mock http message handler into the HttpClient
            HttpClient httpClient = new(handlerMock.Object);
            WebApiUtility webApiUtility = new(httpClient);
            string ipAddress = "8.8.8.8";

            // Act
            string countryName = await webApiUtility.GetCountryNameFromIPAddress(ipAddress);

            // Assert
            Assert.Equal("United States", countryName);
        }



    }
}