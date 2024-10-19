namespace AutoSpex.Client.Tests.ObserverTests;

[TestFixture]
public class PropertyObserverTests
{
    private TestContext? _context;

    [SetUp]
    public void Setup()
    {
        _context = new TestContext();
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }
}