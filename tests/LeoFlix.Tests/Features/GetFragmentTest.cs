using System.Text;
using LeoFlix.Api.Features.Videos;
using LeoFlix.CrossCutting.Storage;
using Moq;

namespace LeoFlix.Tests.Features;

public class GetFragmentTest
{
    [Fact(DisplayName = "It should return the video fragment if it exists")]
    public async Task Should_ReturnVideoFragment_IfExists()
    {
        // Arrange
        const string videoName = "test-video";
        const string fragmentName = "fragment1";
        const string fileContent = "This is the video fragment content.";

        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

        var storageServiceMock = new Mock<IStorageService>();
        storageServiceMock
            .Setup(s => s.Download(Path.Combine(videoName, fragmentName)))
            .ReturnsAsync(memoryStream);

        // Act
        var result = await GetFragment.Endpoint.Handler(storageServiceMock.Object, videoName, fragmentName);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.FileStreamHttpResult>(result);
    }

    [Fact(DisplayName = "It should return NotFound if the video fragment does not exist")]
    public async Task Should_ReturnNotFound_IfFragmentDoesNotExist()
    {
        // Arrange
        const string videoName = "test-video";
        const string fragmentName = "non-existent-fragment";

        var storageServiceMock = new Mock<IStorageService>();
        storageServiceMock
            .Setup(s => s.Download(Path.Combine(videoName, fragmentName)))
            .ReturnsAsync((Stream)null!);

        // Act
        var result = await GetFragment.Endpoint.Handler(storageServiceMock.Object, videoName, fragmentName);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.NotFound<string>>(result);
    }
}