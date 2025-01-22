using System.Text;
using System.Threading.Channels;
using LeoFlix.Api;
using LeoFlix.Api.Features.Videos;
using Microsoft.AspNetCore.Http;
using Moq;

namespace LeoFlix.Tests.Features;

public class UploadVideoTest
{
    [Fact(DisplayName = "It should upload a video")]
    public async Task Should_UploadVideo()
    {
        const string fileContent = "This is a test file";
        const string fileName = "test.txt";
        const string contentType = "text/plain";

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        ms.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        var formFile = fileMock.Object;
        var channel = Channel.CreateBounded<VideoDispatch>(10);

        Directory.CreateDirectory(Constants.VideoDirectory);

        await UploadVideo.Endpoint.Handler(new UploadVideo.Request(formFile), channel);

        var dispatchedItem = await channel.Reader.ReadAsync();
        Assert.Equal(Path.Combine(Constants.VideoDirectory, fileName), dispatchedItem.Path);

        CleanFiles();
    }

    [Fact(DisplayName = "It should not upload a empty video")]
    public async Task ShouldNot_UploadEmptyVideo()
    {
        const string fileContent = "";
        const string fileName = "test.txt";
        const string contentType = "text/plain";

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
        ms.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns(contentType);

        var formFile = fileMock.Object;
        var channel = Channel.CreateBounded<VideoDispatch>(10);

        var result = await UploadVideo.Endpoint.Handler(new UploadVideo.Request(formFile), channel);

        var readerCount = channel.Reader.Count;
        Assert.Equal(0, readerCount);
    }

    private static void CleanFiles()
    {
        var files = Directory.GetFiles(Constants.VideoDirectory);

        foreach (var file in files)
            File.Delete(file);

        Directory.Delete(Constants.VideoDirectory);
    }
}