using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Emby.Subtitle.SubF2M.Extensions;
using FluentAssertions;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using Moq;
using Xunit;

namespace Emby.Subtitle.SubF2M_Test.Extensions;

public class HttpClientExt_Test
{
    [Fact]
    public async Task GetResponse_Return_Response()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();
        
        var responseModel = new TestResult()
        {
            Id = 1,
            Name = "Test"
        };
        var responseStream = GenerateStreamFromString(JsonSerializer.Serialize(responseModel));

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 1,
                ContentType = "text/plain",
                Content = responseStream
            });

        jsonSerializer
            .Setup(_ => _.DeserializeFromStream<It.IsAnyType>(responseStream))
            .Returns(() => JsonSerializer.Deserialize(responseStream, typeof(TestResult), new JsonSerializerOptions()));

        //act
        var result = await httpClient.Object.GetResponse<TestResult>(new HttpRequestOptions(),jsonSerializer.Object);
        
        //assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(responseModel);
    }

    [Fact]
    public async Task GetResponse_Return_Default()
    {
        //arrange
        var httpClient = new Mock<IHttpClient>();
        var jsonSerializer = new Mock<IJsonSerializer>();

        httpClient
            .Setup(_ => _.GetResponse(It.IsAny<HttpRequestOptions>()))
            .ReturnsAsync(() => new HttpResponseInfo()
            {
                ContentLength = 0,
                ContentType = "text/plain",
                Content = default
            });
        
        //act
        var result = await httpClient.Object.GetResponse<TestResult>(new HttpRequestOptions(),jsonSerializer.Object);
        
        //assert
        result.Should().BeNull();
    }    
    
    private static Stream GenerateStreamFromString(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
    
    public class TestResult
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}