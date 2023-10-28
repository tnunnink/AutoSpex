using FluentAssertions;
using L5Spex.Persistence.Requests.Sources;
using static L5Spex.Persistence.Tests.Testing;

namespace L5Spex.Persistence.Tests;

[TestFixture]
public class SourceTests
{
    [SetUp]
    public void Setup()
    {
        CreateDatabase();
    }
    
    [TearDown]
    public void TearDown()
    {
        DeleteDatabase();
    }

    [Test]
    public async Task AddSource_ValidSourceRequest_ShouldUpdateDatabase()
    {
        var request = new AddSource.Request("TestPath");

        var result = await SendAsync(request);

        result.IsSuccess.Should().BeTrue();
    }
}