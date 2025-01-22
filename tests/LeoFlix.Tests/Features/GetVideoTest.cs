using System.Text;
using LeoFlix.Api;
using LeoFlix.Api.Features.Videos;
using LeoFlix.CrossCutting.Storage;
using Moq;

namespace LeoFlix.Tests.Features;

public class GetVideoTest
{
    [Fact(DisplayName = "It should return the video file if it exists")]
    public async Task Should_ReturnVideoFile_IfExists()
    {
        // Arrange
        const string videoName = "test-video";
        const string fileContent = "This is the video file content.";
        const string contentType = "application/vnd.apple.mpegurl";

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var storageServiceMock = new Mock<IStorageService>();
        storageServiceMock
            .Setup(s => s.Download(Path.Combine(videoName, Constants.OutputFileName)))
            .ReturnsAsync(memoryStream);

        // Act
        var result = await GetVideo.Endpoint.Handler(storageServiceMock.Object, videoName);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.FileStreamHttpResult>(result);
    }

    [Fact(DisplayName = "It should return NotFound if the video does not exist")]
    public async Task Should_ReturnNotFound_IfVideoDoesNotExist()
    {
        // Arrange
        const string videoName = "non-existent-video";

        var storageServiceMock = new Mock<IStorageService>();
        storageServiceMock
            .Setup(s => s.Download(Path.Combine(videoName, Constants.OutputFileName)))
            .ReturnsAsync((Stream)null!);

        // Act
        var result = await GetVideo.Endpoint.Handler(storageServiceMock.Object, "name");

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound<string>>(result);
    }
}