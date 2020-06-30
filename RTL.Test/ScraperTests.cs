using Castle.Core.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using RTL.API.Models;
using RTL.API.Models.Parsers;
using RTL.API.Services;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTL.Test
{
    [TestFixture]
    public class ScraperTests
    {
        IConfiguration configuration;

        [SetUp]
        public void Setup()
        {
            var appSettings = @"{
            ""Shows"": {
                ""API"": {
                    ""Domain"": ""https://www.notvalid.com/"",
                    ""UpdatesAPI"": ""updates/shows"",
                    ""ShowAPI"": ""shows/{0}?embed=cast"",
                    ""MaxRequestNumber"": 1,
                    ""MaxRequestIntervalSeconds"": 0
                }
            }
            }";

            var builder = new ConfigurationBuilder();

            builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(appSettings)));

            configuration = builder.Build();
        }

        [Test]
        [TestCase("0001.GetOneShow", 1, null)]
        [TestCase("0002.GetTwoShows", 2, null)]
        [TestCase("0003.GetOneShowExisting", 1, "2019-01-01")]
        [TestCase("0004.GetOneShowExistingUpToDate", 0, "2020-01-01")]
        public void Success_ScraperTests(string testName, int callCount, string existingShowUdateTime)
        {
            Mock<HttpMessageHandler> handlerMock = MockHandler(testName);
            Mock<IShowService> showServiceMock = new Mock<IShowService>();
            Mock<ILogger<ScraperService>> loggerMock = new Mock<ILogger<ScraperService>>();

            if(!existingShowUdateTime.IsNullOrEmpty())
            {
                showServiceMock.Setup(ss => ss.GetShowByShowId(It.IsAny<int>())).Returns(new Show { LastUpdateTime = DateTime.Parse(existingShowUdateTime) });
            }
            
            var httpClient = new HttpClient(handlerMock.Object);        

            var service = new ScraperService(httpClient, showServiceMock.Object, loggerMock.Object, configuration, new ShowParser());
            service.DoWork(null);

            showServiceMock.Verify(ss => ss.Upsert(It.IsAny<Show>()), Times.Exactly(callCount));

        }

        private Mock<HttpMessageHandler> MockHandler(string testName)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage m, CancellationToken c) =>
                {
                    var result = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK
                    };
                    string content;

                    if (m.RequestUri.AbsoluteUri.Contains("updates"))
                    {
                        content = File.ReadAllText($".\\TestCases\\Scraper\\{testName}.Updates.json");
                    }
                    else
                    {
                        content = File.ReadAllText(@".\TestCases\Scraper\ShowData.json");
                    }

                    result.Content = new StringContent(content);

                    return result;
                })
                .Verifiable();
            return handlerMock;
        }
    }
}
