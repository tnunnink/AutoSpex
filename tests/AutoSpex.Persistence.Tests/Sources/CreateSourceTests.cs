﻿using L5Sharp.Core;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Persistence.Tests.Sources;

[TestFixture]
public class CreateSourceTests
{
    [Test]
    public async Task CreateSource_NewEmptyInstance_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source();

        var result = await mediator.Send(new CreateSource(source));
        
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task CreateSource_NewTestInstance_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));

        var result = await mediator.Send(new CreateSource(source));
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateSource_NewExpample_ShouldBeSuccess()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(@"C:\Users\tnunn\Documents\L5X\ET_CLE_Skid.L5X"));

        var result = await mediator.Send(new CreateSource(source));
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task CreateSource_NewTestInstance_ShouldSeedReferences()
    {
        using var context = new TestContext();
        var mediator = context.Resolve<IMediator>();
        var source = new Source(L5X.Load(Known.Test));
        
        await mediator.Send(new CreateSource(source));

        var references = await mediator.Send(new ListReferences(source.Name));
        
        references.Should().NotBeEmpty();
    }
}