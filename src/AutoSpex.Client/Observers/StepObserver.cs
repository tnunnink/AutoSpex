using System;
using System.Collections.Generic;
using System.Linq;
using AutoSpex.Client.Resources;
using AutoSpex.Client.Shared;
using AutoSpex.Engine;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Observers;

/// <summary>
/// Base class for all step types. We need this to use with generic ObserverCollection in QueryObserver
/// </summary>
public abstract partial class StepObserver(Step model) : Observer<Step>(model),
    IRecipient<Observer.Get<StepObserver>>,
    IRecipient<Observer.Deleted>
{
    /// <inheritdoc />
    protected override bool PromptForDeletion => false;

    /// <summary>
    /// Inserts a new filter step after this step in the parent query.
    /// </summary>
    [RelayCommand]
    private void InsertFilterBefore()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;
        
        var index = query.Steps.IndexOf(this);
        var step = new FilterObserver(new Filter(new Criterion()));
        query.Steps.Insert(index, step);
    }
    
    /// <summary>
    /// Inserts a new filter step after this step in the parent query.
    /// </summary>
    [RelayCommand]
    private void InsertFilterAfter()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;
        
        var index = query.Steps.IndexOf(this) + 1;
        if (index > query.Steps.Count) return;
        
        var step = new FilterObserver(new Filter(new Criterion()));
        query.Steps.Insert(index, step);
    }
    
    /// <summary>
    /// Inserts a new selection step after this step in the parent query.
    /// </summary>
    [RelayCommand]
    private void InsertSelectBefore()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;
        
        var index = query.Steps.IndexOf(this);
        var step = new SelectObserver(new Select(new Selection()));
        query.Steps.Insert(index, step);
    }
    
    /// <summary>
    /// Inserts a new selection step after this step in the parent query.
    /// </summary>
    [RelayCommand]
    private void InsertSelectAfter()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;
        
        var index = query.Steps.IndexOf(this) + 1;
        if (index > query.Steps.Count) return;
        
        var step = new SelectObserver(new Select(new Selection()));
        query.Steps.Insert(index, step);
    }

    /// <summary>
    /// Command to move this step up to the step before it.
    /// </summary>
    [RelayCommand]
    private void MoveUp()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;

        var index = query.Steps.IndexOf(this);
        if (index <= 0) return;

        query.Steps.Move(index, index - 1);
    }

    /// <summary>
    /// Command to move this step down to the step after it.
    /// </summary>
    [RelayCommand]
    private void MoveDown()
    {
        var query = GetObserver<QueryObserver>(x => x.Steps.Has(this));
        if (query is null) return;

        var index = query.Steps.IndexOf(this);
        if (index == query.Steps.Count - 1) return;

        query.Steps.Move(index, index + 1);
    }

    /// <summary>
    /// A message that is sent by a <see cref="StepObserver"/> to retrieve the current input property for the step.
    /// This is neede to resolve property and argument types for criteria of the step.
    /// </summary>
    /// <param name="step">The step instance for which to get the input.</param>
    public class GetInputTo(StepObserver step) : RequestMessage<Property>
    {
        /// <summary>
        /// The step instance requesting the property input.
        /// </summary>
        public StepObserver Step { get; } = step;
    }

    /// <summary>
    /// Handles the request to get the observer that passes the provied predicate.
    /// </summary>
    public void Receive(Get<StepObserver> message)
    {
        if (message.HasReceivedResponse) return;

        if (message.Predicate.Invoke(this))
        {
            message.Reply(this);
        }
    }

    /// <summary>
    /// Handles a delete message by determining if the provided observer is a type that this step could contain and
    /// if this step is the appropriate step type. If so, will attempt to remove any instance of the observer if found.
    /// </summary>
    public void Receive(Deleted message)
    {
        switch (message.Observer)
        {
            case CriterionObserver criterion when this is FilterObserver filter:
                filter.Criteria.Remove(criterion);
                break;
            case CriterionObserver criterion when this is VerifyObserver verify:
                verify.Criteria.Remove(criterion);
                break;
            case SelectionObserver selection when this is SelectObserver select:
                select.Selections.Remove(selection);
                break;
        }
    }

    /// <summary>
    /// Tries to find a criterion based on the specified predicate. This method searches through the Criteria collection
    /// of FilterObserver and VerifyObserver instances to locate the first CriterionObserver that satisfies the predicate.
    /// </summary>
    /// <param name="predicate">The predicate function used to check each CriterionObserver</param>
    /// <param name="criterion">When this method returns, contains the CriterionObserver that satisfies the predicate, if found; otherwise, null</param>
    /// <returns>True if a CriterionObserver satisfying the predicate was found; otherwise, false</returns>
    public bool TryFind(Func<CriterionObserver, bool> predicate, out CriterionObserver criterion)
    {
        if (this is FilterObserver filter && filter.Criteria.Any(predicate))
        {
            criterion = filter.Criteria.First(predicate);
            return true;
        }

        if (this is VerifyObserver verify && verify.Criteria.Any(predicate))
        {
            criterion = verify.Criteria.First(predicate);
            return true;
        }

        criterion = null!;
        return false;
    }

    /// <summary>
    /// Checks if this step contains the provided criterion instance.
    /// </summary>
    /// <param name="criterion">The CriterionObserver to check for.</param>
    /// <returns>True if any of the criterion associated with this step contain the provided criterion, otherwise false.</returns>
    public bool Contains(CriterionObserver criterion)
    {
        if (this is SelectObserver) return false;

        var criteria = this switch
        {
            FilterObserver filter => filter.Criteria,
            VerifyObserver verify => verify.Criteria,
            _ => []
        };

        return criteria.Has(criterion);
    }

    /// <summary>
    /// Checks if this step contains the provided argument input instance.
    /// </summary>
    /// <param name="argument">The ArgumentInput to check for.</param>
    /// <returns>True if any of the criterion associated with this step contain the provided argument, otherwise false.</returns>
    public bool Contains(ArgumentInput argument)
    {
        if (this is SelectObserver) return false;

        var criteria = this switch
        {
            FilterObserver filter => filter.Criteria,
            VerifyObserver verify => verify.Criteria,
            _ => []
        };

        return criteria.Any(c => c.Contains(argument));
    }

    /// <summary>
    /// Checks if this step contains the provided selection instance.
    /// </summary>
    /// <param name="selection">The SelectionObserver to check for.</param>
    /// <returns>True if any of the selections associated with this step contain the provided selection, otherwise false.</returns>
    public bool Contains(SelectionObserver selection)
    {
        if (this is not SelectObserver select) return false;
        return select.Selections.Has(selection);
    }

    /// <summary>
    /// Sends the message to determine the current input to this step observer. This will change depending on where
    /// this step exists in the query or specification
    /// </summary>
    protected Property DetermineInput()
    {
        return Messenger.Send(new GetInputTo(this)).Response;
    }

    /// <inheritdoc />
    protected override IEnumerable<MenuActionItem> GenerateMenuItems()
    {
        yield return new MenuActionItem
        {
            Header = "Insert Filter Before",
            Icon = Resource.Find("IconFilledFunnel"),
            Command = InsertFilterBeforeCommand
        };
        
        yield return new MenuActionItem
        {
            Header = "Insert Filter After",
            Icon = Resource.Find("IconFilledFunnel"),
            Command = InsertFilterAfterCommand
        };
        
        yield return MenuActionItem.Separator;
        
        yield return new MenuActionItem
        {
            Header = "Insert Select Before",
            Icon = Resource.Find("IconFilledHand"),
            Command = InsertSelectBeforeCommand
        };
        
        yield return new MenuActionItem
        {
            Header = "Insert Select After",
            Icon = Resource.Find("IconFilledHand"),
            Command = InsertSelectAfterCommand
        };
        
        yield return MenuActionItem.Separator;
        
        yield return new MenuActionItem
        {
            Header = "Move Step Up",
            Icon = Resource.Find("IconFilledChevronUp"),
            Command = MoveUpCommand
        };

        yield return new MenuActionItem
        {
            Header = "Move Step Down",
            Icon = Resource.Find("IconFilledChevronDown"),
            Command = MoveDownCommand
        };
        
        yield return MenuActionItem.Separator;

        yield return new MenuActionItem
        {
            Header = "Paste",
            Icon = Resource.Find("IconFilledPaste"),
            Command = PasteCommand
        };

        yield return new MenuActionItem
        {
            Header = "Copy All",
            Icon = Resource.Find("IconFilledCopy"),
            Command = CopyCommand
        };

        yield return new MenuActionItem
        {
            Header = "Duplicate Step",
            Icon = Resource.Find("IconFilledClone"),
            Command = DuplicateCommand
        };

        yield return new MenuActionItem
        {
            Header = "Delete Step",
            Icon = Resource.Find("IconFilledTrash"),
            Classes = "danger",
            Command = DeleteCommand
        };
    }
}

/// <summary>
/// Base class for all step types. We need this to use with generic ObserverCollection in QueryObserver
/// </summary>
public abstract class StepObserver<TStep> : StepObserver where TStep : Step
{
    /// <summary>
    /// Base class for all step observer types.
    /// This observer implements the common criteria collection functionality.
    /// </summary>
    protected StepObserver(TStep model) : base(model)
    {
        Model = model;
    }

    /// <summary>
    /// The underlying model object that is being wrapped by the observer.
    /// </summary>
    protected new TStep Model { get; }
}