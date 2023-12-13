using System.Dynamic;
using AutoSpex.Client.Features.Projects;
using FluentAssertions;

namespace AutoSpex.Client.Tests.Projects;

[TestFixture]
public class ProjectTests
{
    [Test]
    public void New_ValidParameters_ShouldNotBeNull()
    {
        var project = new Project(new Uri(@"C:\Users\admin\Documents\Spex\MyProject.db"));

        project.Should().NotBeNull();
    }

    [Test]
    public void New_ValidParameters_ShouldHaveExpectedValues()
    {
        var project = new Project(new Uri(@"C:\Users\admin\Documents\Spex\MyProject.db"));

        project.Name.Should().Be("MyProject");
        project.Uri.Should().Be(@"C:\Users\admin\Documents\Spex\MyProject.db");
    }

    [Test]
    public void New_ValidRecord_ShouldHaveExpectedValues()
    {
        var project = new Project(@"C:\Users\admin\Documents\Spex\MyProject.db", DateTime.Now, 1);

        project.Should().NotBeNull();
        project.Uri.LocalPath.Should().Be(@"C:\Users\admin\Documents\Spex\MyProject.db");
        project.OpenedOn.Should().BeWithin(TimeSpan.FromSeconds(1));
        project.Pinned.Should().BeTrue();
    }
}