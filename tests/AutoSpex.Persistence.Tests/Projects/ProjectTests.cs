using FluentAssertions;

namespace AutoSpex.Persistence.Tests.Projects;

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
        project.Directory.Should().Be(@"C:\Users\admin\Documents\Spex");
        project.Exists.Should().BeFalse();
        project.ConnectionString.Should().NotBeEmpty();
    }
}