﻿namespace AutoSpex.Engine.Tests.ElementTests;

public class RedundancyInfoTests
{
    [Test]
    public void New_WhenCalled_ShouldNotBeNull()
    {
        var element = Element.RedundancyInfo;

        element.Should().NotBeNull();
    }
}