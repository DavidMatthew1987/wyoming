using System.Data;
using NSubstitute;
using Xunit;
using AnalyticsWebApi.Datamodel;
using Dapper;

namespace AnalyticsWebApi;

public class AnalyticsRepositoryTests
{
    [Fact]
    public async Task SaveAnalyticsData_ValidData_ReturnsSuccess()
    {
        // Arrange
        var mockConnection = Substitute.For<IDbConnection>();
        var repository = new AnalyticsRepository(""); // Pass an empty string here
        var data = new AnalyticsData { 
            Referer = "https://www.example.com",
            IpAddress = "127.0.0.1",
            DataKey = "test-key",
            Data = "test-data"
        };

        mockConnection.ExecuteAsync(Arg.Any<string>(), Arg.Any<object>()).Returns(1);

        // Act
        int result = await repository.SaveAnalyticsData(data);

        // Assert
        Assert.Equal(1, result); 
        // ... rest of your assertions
    }

}
