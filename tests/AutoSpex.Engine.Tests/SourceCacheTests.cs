using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceCacheTests
{
    [Test]
    public void Local_WhenCalled_ShouldBeExpected()
    {
        var cache = SourceCache.Local;

        cache.Should().NotBeNull();
    }

    [Test]
    public async Task GetOrAdd_MarkupSource_ShouldReturnCachedInstance()
    {
        var temp = Directory.CreateTempSubdirectory("CacheTest");
        var repo = Repo.Configure(temp.FullName);
        var cache = new SourceCache(repo);
        var source = Source.Create(Known.Test);

        var cached = await cache.GetOrAdd(source);

        cached.Should().NotBeNull();
        cached.Name.Should().NotBeEmpty();
        cached.Location.Should().EndWith(".L5Z");
        cached.Location.Should().Contain(temp.FullName);

        temp.Delete(true);
    }

    [Test]
    public async Task GetOrAdd_MarkupSourceTwice_SecondTimeShouldNotCreateSecondCachedSource()
    {
        var temp = Directory.CreateTempSubdirectory("CacheTest");
        var repo = Repo.Configure(temp.FullName);
        var cache = new SourceCache(repo);
        var source = Source.Create(Known.Test);

        var first = await cache.GetOrAdd(source);
        var second = await cache.GetOrAdd(source);

        first.Should().BeEquivalentTo(second);
        cache.Sources.Should().HaveCount(1);

        temp.Delete(true);
    }

    [Test]
    public async Task GetOrAdd_ArchiveSource_ShouldReturnCachedInstance()
    {
        var temp = Directory.CreateTempSubdirectory("CacheTest");
        var repo = Repo.Configure(temp.FullName);
        var cache = new SourceCache(repo);
        var source = Source.Create(Known.Archive);

        var cached = await cache.GetOrAdd(source);

        cached.Should().NotBeNull();
        cached.Name.Should().NotBeEmpty();
        cached.Location.Should().EndWith(".L5Z");
        cached.Location.Should().Contain(temp.FullName);

        temp.Delete(true);
    }

    [Test]
    public void ClearCache_EmptyCache_ShouldHaveNoSources()
    {
        var cache = SourceCache.Local;

        cache.Clear();

        cache.Sources.Should().BeEmpty();
    }

    [Test]
    public async Task ClearCache_CachedCache_ShouldHaveNoSources()
    {
        var temp = Directory.CreateTempSubdirectory("CacheTest");
        var repo = Repo.Configure(temp.FullName);
        var cache = new SourceCache(repo);
        var source = Source.Create(Known.Test);
        await cache.GetOrAdd(source);
        cache.Sources.Should().HaveCount(1);

        cache.Clear();

        cache.Sources.Should().BeEmpty();
        temp.Delete(true);
    }
}