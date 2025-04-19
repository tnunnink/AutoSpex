using System.Diagnostics;
using System.Security.Cryptography;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class ProofTests
{
    [Test]
    public async Task Run_SpecWithValidConfig_ShouldBePassedAndExpectedValues()
    {
        var source = await L5X.LoadAsync(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        var verifications = (await spec.RunAsync(source)).ToList();

        verifications.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
        verifications.Should().HaveCount(1);
    }

    [Test]
    public async Task Run_SpecManyTimes_ShouldRunRelativelyQuickly()
    {
        var source = await L5X.LoadAsync(Known.Test);
        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Tag);
            c.Where("TagName", Operation.EqualTo, "TestSimpleTag");
            c.Verify("DataType", Operation.EqualTo, "SimpleType");
        });

        var stopwatch = Stopwatch.StartNew();
        var evaluations = new List<Verification>();

        for (var i = 0; i < 100; i++)
        {
            var result = await spec.RunAsync(source);
            evaluations.AddRange(result);
        }

        stopwatch.Stop();

        Console.WriteLine(stopwatch.ElapsedMilliseconds);
        evaluations.Should().NotBeEmpty();
    }

    [Test]
    public async Task Run_5094IB16A_ShouldBeAtRevision_2_011()
    {
        var source = await L5X.LoadAsync(Known.Test);

        var spec = Spec.Configure(c =>
        {
            c.Query(Element.Module);
            c.Where("CatalogNumber", Operation.EqualTo, "5094-IB16/A");
            c.Verify("Revision", Operation.EqualTo, "2.1");
        });

        var results = await spec.RunAsync(source);

        results.Should().AllSatisfy(e => e.Result.Should().Be(ResultState.Passed));
    }

    [Test]
    public void ValueExtraction()
    {
        var source = L5X.Load(Known.Example);

        var tagNames = source.Query<Tag>()
            .Select(t => new
            {
                t.DataType,
                Members = t.TagNames().Where(x => !string.IsNullOrWhiteSpace(x.Path)).Select(n => n.Path).ToList()
            })
            .DistinctBy(t => t.DataType)
            .ToList();

        tagNames.Should().NotBeEmpty();
    }

    [Test]
    public void HowToReadFileContentForOpenFile()
    {
        const string openFile =
            @"C:\Users\tnunnink\Documents\Projects\L5Sharp\tests\L5Sharp.Samples\Test.ACD";

        using var stream = File.Open(openFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var hash = MD5.HashData(stream);
        var result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

        result.Should().NotBeEmpty();
    }

    [Test]
    public void GetElementTextProperties()
    {
        var source = L5X.Load(Known.Example);

        var properties = Element.Selectable
            .SelectMany(e => e.Properties.Where(p => p.Group == TypeGroup.Text || p.Group == TypeGroup.Number))
            .ToList();

        foreach (var prop in properties.Where(prop => prop.Name is not "Scope"))
        {
            Console.WriteLine(prop.Key);
        }
    }
}