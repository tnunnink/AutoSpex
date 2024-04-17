using AutoSpex.Client.Observers;
using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace AutoSpex.Client.Components;

public class EvaluationTemplates : IDataTemplate
{
    private const string PassedTemplate = "EvaluationPassedTemplate";
    private const string FailedTemplate = "EvaluationFailedTemplate";
    private const string ErrorTemplate = "EvaluationErrorTemplate";
    
    public Control? Build(object? param)
    {
        if (param is not EvaluationObserver evaluation) return default;

        if (evaluation.Result == ResultState.Passed)
            return GetTemplate(PassedTemplate)?.Build(param);
        
        if (evaluation.Result == ResultState.Failed)
            return GetTemplate(FailedTemplate)?.Build(param);
        
        if (evaluation.Result == ResultState.Error)
            return GetTemplate(ErrorTemplate)?.Build(param);

        return default;
    }

    private IDataTemplate? GetTemplate(string key) => Application.Current?.FindResource(key) as IDataTemplate;

    public bool Match(object? data) => data is EvaluationObserver;
}