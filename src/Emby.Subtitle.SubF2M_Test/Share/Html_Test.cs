using System;
using System.Threading;
using System.Threading.Tasks;
using Emby.Subtitle.SubF2M.Share;
using FluentAssertions;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using Moq;
using Xunit;

namespace Emby.Subtitle.SubF2M_Test.Share;

public class Html_Test
{
    [Fact]
    public void BaseRequestOptions_Return_Url()
    {
        //arrange
        var html = new Html(null, null);
        var url = "https://test.com";

        //act
        var response = html.BaseRequestOptions(url, CancellationToken.None);

        //assert
        response.Should().NotBeNull();
        response.Url.Should().Be(url);
    }

    [Fact]
    public void BaseRequestOptions_Return_AppHost()
    {
        //arrange
        var appHost = new Mock<IApplicationHost>();

        appHost.Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        var html = new Html(null, appHost.Object);
        var url = "https://test.com";

        //act
        var response = html.BaseRequestOptions(url, CancellationToken.None);

        //assert
        response.Should().NotBeNull();
        response.UserAgent.Should().Be($"Emby/1.1.1.1");
    }

    [Fact]
    public async Task GetHtmlContent_Return_Success()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();

        var responseText =
            "<html><head><title>Test</title></head><body><script aaa></script><h1>Test</h1>&#60;</body></html>";
        var responseStream = Utility.GenerateStreamFromString(responseText);

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 1,
                ContentType = "text/plain",
                Content = responseStream
            });

        var html = new Html(httpClient.Object, null);

        //act
        var response = await html.Get("https://test.com", CancellationToken.None);

        //assert
        response.Should().NotBeEmpty();
        response.Should().Be("<html><body><h1>Test</h1><</body></html>");
    }

    [Fact]
    public async Task GetHtmlContent_Return_Empty()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();

        var responseText = string.Empty;
        var responseStream = Utility.GenerateStreamFromString(responseText);

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 1,
                ContentType = "text/plain",
                Content = responseStream
            });

        var html = new Html(httpClient.Object, null);

        //act
        var response = await html.Get("https://test.com", CancellationToken.None);

        //assert
        response.Should().BeEmpty();
    }
    
}