using AutoSpex.Client.Features.Projects;
using FluentAssertions;

namespace AutoSpex.Client.Tests.Models;

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
        project.Path.Should().Be(@"C:\Users\admin\Documents\Spex\MyProject.db");
    }
}