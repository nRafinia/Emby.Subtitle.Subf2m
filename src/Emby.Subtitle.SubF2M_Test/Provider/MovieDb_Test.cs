using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Emby.Subtitle.SubF2M.Models;
using Emby.Subtitle.SubF2M.Providers;
using FluentAssertions;
using MediaBrowser.Common;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using Moq;
using Xunit;

namespace Emby.Subtitle.SubF2M_Test.Provider;

public class MovieDb_Test
{
    [Fact]
    public async Task MovieDb_Return_Success()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();
        var applicationHost = new Mock<IApplicationHost>();

        var responseModel = new MovieInformation()
        {
            Id = 123,
            Imdb_Id = "123",
            Title = "Test",
            release_date = DateTime.Now,
            Original_Title = "Test"
        };
        var responseStream = Utility.GenerateStreamFromString(JsonSerializer.Serialize(responseModel));

        jsonSerializer
            .Setup(_ => _.DeserializeFromStream<It.IsAnyType>(responseStream))
            .Returns(() => JsonSerializer.Deserialize(responseStream, typeof(MovieInformation),
                new JsonSerializerOptions()));

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 1,
                ContentType = "application/json",
                Content = responseStream
            });

        applicationHost
            .Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        var movieDb = new MovieDb(jsonSerializer.Object, httpClient.Object, applicationHost.Object);

        //act
        var response = await movieDb.GetMovieInfo("123", CancellationToken.None);

        //assert
        response.Should().NotBeNull();
        response.Id.Should().Be(123);
        response.Imdb_Id.Should().Be("123");
    }

    [Fact]
    public async Task MovieDb_Return_NotFound()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var applicationHost = new Mock<IApplicationHost>();

        applicationHost
            .Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 0,
                ContentType = "application/json",
                Content = default
            });

        var movieDb = new MovieDb(null, httpClient.Object, applicationHost.Object);

        //act
        var response = await movieDb.GetMovieInfo("123", CancellationToken.None);

        //assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetTvInfo_Return_Success()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();
        var applicationHost = new Mock<IApplicationHost>();

        var responseTvModel = new TvInformation()
        {
            Id = 123,
            Name = "Test"
        };

        var responseFindModel = new FindMovie()
        {
            tv_results = new List<TvEpisodeResult>()
            {
                new TvEpisodeResult()
                {
                    id = 123,
                    show_id = 123
                }
            },
            tv_episode_results = new List<TvEpisodeResult>()
            {
                new TvEpisodeResult()
                {
                    id = 1234,
                    show_id = 1234
                }
            }
        };
        var responseTvStream = Utility.GenerateStreamFromString(JsonSerializer.Serialize(responseTvModel));
        var responseFindStream = Utility.GenerateStreamFromString(JsonSerializer.Serialize(responseFindModel));

        jsonSerializer
            .Setup(_ => _.DeserializeFromStream<FindMovie>(It.IsAny<Stream>()))
            .Returns(() => JsonSerializer.Deserialize<FindMovie>(responseFindStream));
        jsonSerializer
            .Setup(_ => _.DeserializeFromStream<TvInformation>(It.IsAny<Stream>()))
            .Returns(() => JsonSerializer.Deserialize<TvInformation>(responseTvStream));

        var httpSteps = 0;
        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() =>
            {
                httpSteps++;
                return new HttpResponseInfo()
                {
                    ContentLength = 1,
                    ContentType = "application/json",
                    Content = httpSteps == 0 ? responseFindStream : responseTvStream
                };
            });

        applicationHost
            .Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        var movieDb = new MovieDb(jsonSerializer.Object, httpClient.Object, applicationHost.Object);

        //act
        var response = await movieDb.GetTvInfo("tt123", CancellationToken.None);

        //assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task SearchMovie_Return_NotFound_1()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();
        var applicationHost = new Mock<IApplicationHost>();

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 0,
                ContentType = "application/json",
                Content = null
            });

        applicationHost
            .Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        var movieDb = new MovieDb(jsonSerializer.Object, httpClient.Object, applicationHost.Object);

        //act
        var response = await movieDb.GetTvInfo("tt123", CancellationToken.None);

        //assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task SearchMovie_Return_NotFound_2()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();
        var applicationHost = new Mock<IApplicationHost>();

        var responseModel = new FindMovie()
        {
            tv_results = new List<TvEpisodeResult>(),
            tv_episode_results = new List<TvEpisodeResult>()
        };
        var responseStream = Utility.GenerateStreamFromString(JsonSerializer.Serialize(responseModel));

        jsonSerializer
            .Setup(_ => _.DeserializeFromStream<It.IsAnyType>(responseStream))
            .Returns(() => JsonSerializer.Deserialize(responseStream, typeof(FindMovie),
                new JsonSerializerOptions()));

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 1,
                ContentType = "application/json",
                Content = responseStream
            });

        applicationHost
            .Setup(_ => _.ApplicationVersion)
            .Returns(() => new Version("1.1.1.1"));

        var movieDb = new MovieDb(jsonSerializer.Object, httpClient.Object, applicationHost.Object);

        //act
        var response = await movieDb.GetTvInfo("tt123", CancellationToken.None);

        //assert
        response.Should().BeNull();
    }
}