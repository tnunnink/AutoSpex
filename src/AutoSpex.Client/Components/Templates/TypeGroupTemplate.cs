using AutoSpex.Engine;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using L5Sharp.Core;

namespace AutoSpex.Client.Components;

public class TypeGroupTemplate : IDataTemplate
{
    private const string DefaultGroup = "DefaultGroupTemplate";
    private const string BooleanGroup = "BooleanGroupTemplate";
    private const string NumberGroup = "NumberGroupTemplate";
    private const string TextGroup = "TextGroupTemplate";
    private const string DateGroup = "DateGroupTemplate";
    private const string EnumGroup = "EnumGroupTemplate";
    private const string CollectionGroup = "CollectionGroupTemplate";
    private const string LogixComponentTemplate = "LogixComponentTemplate";
    private const string LogixCodeTemplate = "LogixCodeTemplate";
    private const string LogixTypeTemplate = "LogixTypeTemplate";
    private const string TagTemplate = "TagTemplate";
    private const string RungTemplate = "RungTemplate";
    private const string DataTypeMemberTemplate = "DataTypeMemberTemplate";
    private const string ParameterTemplate = "ParameterTemplate";

    
    public Control? Build(object? param)
    {
        if (param is null) return default;

        //Check for custom Logix type templates
        if (param is Tag) return GetTemplate(TagTemplate)?.Build(param);
        if (param is DataTypeMember) return GetTemplate(DataTypeMemberTemplate)?.Build(param);
        if (param is Parameter) return GetTemplate(ParameterTemplate)?.Build(param);
        if (param is Rung) return GetTemplate(RungTemplate)?.Build(param);
        if (param is LogixComponent) return GetTemplate(LogixComponentTemplate)?.Build(param);
        if (param is LogixCode) return GetTemplate(LogixCodeTemplate)?.Build(param);
        if (param is LogixData) return GetTemplate(LogixTypeTemplate)?.Build(param);
        
        //Otherwise get the type group and use those templates
        var group = TypeGroup.FromType(param.GetType());
        IDataTemplate? template = null;

        group
            .When(TypeGroup.Boolean).Then(() => template = GetTemplate(BooleanGroup))
            .When(TypeGroup.Number).Then(() => template = GetTemplate(NumberGroup))
            .When(TypeGroup.Text).Then(() => template = GetTemplate(TextGroup))
            .When(TypeGroup.Enum).Then(() => template = GetTemplate(EnumGroup))
            .When(TypeGroup.Date).Then(() => template = GetTemplate(DateGroup))
            .When(TypeGroup.Collection).Then(() => template = GetTemplate(CollectionGroup))
            .Default(() => template = GetTemplate(DefaultGroup));

        return template?.Build(param) ?? default;
    }

    public bool Match(object? data) => data is not null;
    private IDataTemplate? GetTemplate(string key) => Application.Current?.FindResource(key) as IDataTemplate;
}