﻿using System.Text.Json;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine.Tests;

[TestFixture]
public class VerificationTests
{
    [Test]
    public Task Serialized_Verification_ShouldBeVerified()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);

        var json = JsonSerializer.Serialize(verification);

        return VerifyJson(json);
    }

    [Test]
    public void Deserialize_JosnString_ShouldNotBeNull()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);
        var json = JsonSerializer.Serialize(verification);

        var result = JsonSerializer.Deserialize<Verification>(json);

        result.Should().NotBeNull();
    }

    [Test]
    public void Deserialize_JosnString_ShouldBeEquivalentToOriginal()
    {
        var criterion = new Criterion(Element.Tag.Property("Name"), Operation.Containing, "Test");
        var evaluation = Evaluation.Passed(criterion, new Tag("Test", 123), "Test");
        var verification = Verification.For(evaluation);
        var json = JsonSerializer.Serialize(verification);

        var result = JsonSerializer.Deserialize<Verification>(json);

        result.Should().BeEquivalentTo(verification);
    }
}