using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class SourceCacheTests
{
    [Test]
    public void Default_WhenCalled_ShouldBeExpected()
    {
        var cache = SourceCache.In;

        cache.Should().NotBeNull();
        cache.Repo.Location.Should().Be(@"..\cache");
        cache.Repo.Name.Should().Be("cache");
        Directory.Exists(cache.Repo.Location).Should().BeTrue();

        Directory.Delete(cache.Repo.Location);
    }

    [Test]
    public async Task GetOrAdd_MarkupSource_ShouldReturnCachedInstance()
    {
        var cache = SourceCache.In;
        var source = Source.Create(Known.Test);

        var cached = await cache.GetOrAdd(source);

        cached.Should().NotBeNull();
        cached.Name.Should().Be(source.Hash);
        cached.Location.Should().EndWith(".L5Z");
        cached.Location.Should().Contain(@"\cache\");

        Directory.Delete(cache.Repo.Location, true);
    }
    
    [Test]
    public async Task GetOrAdd_MarkupSourceTwice_SecondTimeShouldNotCreateSecondCachedSource()
    {
        var cache = SourceCache.In;
        var source = Source.Create(Known.Test);

        var first = await cache.GetOrAdd(source);
        var second = await cache.GetOrAdd(source);

        first.Should().BeEquivalentTo(second);
        cache.Repo.FindSources().Should().HaveCount(1);

        Directory.Delete(cache.Repo.Location, true);
    }

    [Test]
    public async Task GetOrAdd_ArchiveSource_ShouldReturnCachedInstance()
    {
        var cache = SourceCache.In;
        var source = Source.Create(Known.Archive);

        var cached = await cache.GetOrAdd(source);

        cached.Should().NotBeNull();
        cached.Name.Should().Be(source.Hash);
        cached.Location.Should().EndWith(".L5Z");
        cached.Location.Should().Contain(@"\cache\");

        Directory.Delete(cache.Repo.Location, true);
    }

    [Test]
    public void ClearCache_EmptyCache_ShouldHaveNoSources()
    {
        var cache = SourceCache.In;

        cache.ClearCache();

        cache.Repo.FindSources().Should().BeEmpty();

        Directory.Delete(cache.Repo.Location);
    }

    [Test]
    public async Task ClearCache_CachedCache_ShouldHaveNoSources()
    {
        var cache = SourceCache.In;
        var source = Source.Create(Known.Test);
        await cache.GetOrAdd(source);
        cache.Repo.FindSources().Should().HaveCount(1);

        cache.ClearCache();

        cache.Repo.FindSources().Should().BeEmpty();
        Directory.Delete(cache.Repo.Location);
    }
}