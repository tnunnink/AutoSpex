﻿using System.Text.Json;
using JetBrains.Annotations;
using L5Sharp.Core;
using NLog;
using Task = System.Threading.Tasks.Task;

namespace AutoSpex.Engine;

/// <summary>
/// A <see cref="Run"/> represents a set of specs and source which should be executed together and produce a set of
/// outcomes or results. 
/// </summary>
public class Run
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<Guid, Outcome> _outcomes = [];

    public Run(Node node, Source source)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(source);

        Name = $"{node.Name}";

        //Build the set of outcomes based on the spec or specs 
        GenerateOutcomes(node);

        //Copy objects to get shallow state. We don't want to store the entire object, just basic info.
        Node = node.Copy();
        Source = source.Copy();
    }

    /// <summary>
    /// Dapper constructor.
    /// </summary>
    [UsedImplicitly]
    private Run(string runId, string name, string node, string source, string result,
        string ranOn, string ranBy, long duration, double passRate)
    {
        RunId = Guid.Parse(runId);
        Name = name;
        Node = JsonSerializer.Deserialize<Node>(node) ?? Node.Empty;
        Source = JsonSerializer.Deserialize<Source>(source) ?? Source.Empty();
        Result = ResultState.FromName(result);
        RanOn = DateTime.Parse(ranOn);
        RanBy = ranBy;
        Duration = duration;
        PassRate = passRate;
    }

    /// <summary>
    /// Dapper constructor.
    /// </summary>
    [UsedImplicitly]
    private Run(string runId, string name, string node, string source, string result,
        string ranOn, string ranBy, long duration, double passRate, string outcomes)
    {
        RunId = Guid.Parse(runId);
        Name = name;
        Node = JsonSerializer.Deserialize<Node>(node) ?? Node.Empty;
        Source = JsonSerializer.Deserialize<Source>(source) ?? Source.Empty();
        Result = ResultState.FromName(result);
        RanOn = DateTime.Parse(ranOn);
        RanBy = ranBy;
        Duration = duration;
        PassRate = passRate;
        _outcomes = JsonSerializer.Deserialize<List<Outcome>>(outcomes)?.ToDictionary(x => x.NodeId) ?? [];
    }

    public Guid RunId { get; } = Guid.NewGuid();
    public string Name { get; private set; }
    public Node Node { get; private set; }
    public Source Source { get; private set; }
    public ResultState Result { get; private set; } = ResultState.None;
    public DateTime RanOn { get; private set; }
    public string RanBy { get; private set; } = string.Empty;
    public long Duration { get; private set; }
    public double PassRate { get; private set; }
    public IEnumerable<Outcome> Outcomes => _outcomes.Values;

    /// <summary>
    /// Represents an empty instance of a Run.
    /// </summary>
    /// <remarks>
    /// This property returns a new instance of Run with default empty values for properties.
    /// </remarks>
    public static Run Empty => new(Node.Empty, Source.Empty());

    /// <summary>
    /// Executes the provided nodes against the provided source for this run.
    /// </summary>
    /// <param name="nodes">The specification nodes to run. These nodes should have any variables and specs loaded.</param>
    /// <param name="source">The source content to run the provided specs against.</param>
    /// <param name="running">A callback prior to running each spec.
    /// This action give the outcome instance that is about to be run so that the UI can respond and update the result state.</param>
    /// <param name="complete">A callback after the spec has been run to completion.
    /// This action provides the outcome instance that ran so that the UI can respond and update the result state and result data.</param>
    /// <param name="token">The cancellation token to cancel the execution of this run.</param>
    public async Task Execute(ICollection<Node> nodes, Source source,
        Action<Outcome>? running = null,
        Action<Outcome>? complete = null,
        CancellationToken token = default)
    {
        if (source is null)
            throw new ArgumentNullException(nameof(source));

        await RunAllNodes(nodes, source, running, complete, token);

        Result = ResultState.MaxOrDefault(_outcomes.Select(o => o.Value.Result).ToList());
        RanOn = DateTime.Now;
        RanBy = Environment.UserName;
        Duration = _outcomes.Sum(x => x.Value.Duration);
        PassRate = _outcomes.Average(x => x.Value.PassRate);
    }

    /// <summary>
    /// Iterates the provided node and runs the configured specification agains the provided source L5X file.
    /// Executes the callbacks if provided. Throws exception on calcellation requested.
    /// </summary>
    private async Task RunAllNodes(IEnumerable<Node> nodes, Source source,
        Action<Outcome>? running,
        Action<Outcome>? complete,
        CancellationToken token)
    {
        var content = source.Content ?? L5X.Empty();

        foreach (var node in nodes)
        {
            token.ThrowIfCancellationRequested();

            if (!_outcomes.TryGetValue(node.NodeId, out var outcome))
                continue;

            running?.Invoke(outcome);

            var verification = await node.Run(content, token);
            outcome.Apply(verification);
            complete?.Invoke(outcome);
        }
    }

    /// <summary>
    /// Builds the outcomes that will represent the specs to be run using the provided node. It is assumed that the node
    /// provided will contain all children required.
    /// </summary>
    private void GenerateOutcomes(Node node)
    {
        var specs = node.Descendants(NodeType.Spec);

        foreach (var spec in specs)
        {
            var outcome = new Outcome { RunId = RunId, NodeId = spec.NodeId, Name = spec.Name, Path = spec.Path };
            _outcomes.TryAdd(spec.NodeId, outcome);
        }
    }
}