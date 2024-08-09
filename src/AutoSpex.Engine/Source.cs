using System.Text.Json.Serialization;
using System.Xml.Linq;
using JetBrains.Annotations;
using L5Sharp.Core;

namespace AutoSpex.Engine;

/// <summary>
/// Represents a L5X file that has been added to an environment. We use the sources to run against a set of specifications.
/// Sources are not loaded until a specification or set or specifications are run.
/// </summary>
public class Source
{
    //The loaded L5X content. Once loaded, we don't want to reload each time a spec is run, so I am providing a reference
    //to it and the means for releasing its memory when done.
    private L5X? _content;

    /// <summary>
    /// The internal override lookup for this source.
    /// </summary>
    private Dictionary<Guid, Variable>? _overrides;

    [UsedImplicitly]
    public Source()
    {
        Uri = default!;
    }

    /// <summary>
    /// Creates a new <see cref="Source"/> provided the location on disc of the L5X file.
    /// </summary>
    public Source(Uri location)
    {
        Uri = location ?? throw new ArgumentNullException(nameof(location));
        Name = Path.GetFileNameWithoutExtension(Uri.LocalPath);
    }

    /// <summary>
    /// Represents the unique identifier of a source file.
    /// </summary>
    [JsonInclude]
    public Guid SourceId { get; private init; } = Guid.NewGuid();

    /// <summary>
    /// Represents a location (file path or remote URL) of a source file.
    /// </summary>
    [JsonInclude]
    public Uri Uri { get; private set; }

    /// <summary>
    /// Gets or sets the name of the source file.
    /// </summary>
    /// <remarks>
    /// This property represents the name of the source file without the file extension.
    /// </remarks>
    [JsonInclude]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Represents the physical location of the L5X file associated with a source.
    /// </summary>
    /// <value>
    /// The location of the L5X file.
    /// </value>
    [JsonIgnore]
    public string Location => Uri.LocalPath;

    /// <summary>
    /// Gets the file name of the source file.
    /// </summary>
    /// <remarks>
    /// This property returns the file name of the source file specified by the <see cref="Uri"/> property.
    /// </remarks>
    [JsonIgnore]
    public string FileName => Path.GetFileName(Uri.LocalPath);

    /// <summary>
    /// Represents a directory path where the source file is located.
    /// </summary>
    /// <remarks>
    /// This property returns the directory path of the source file specified by the <see cref="Uri"/> property.
    /// </remarks>
    [JsonIgnore]
    public string? Directory => Path.GetDirectoryName(Uri.LocalPath);

    /// <summary>
    /// Gets a value indicating whether the source file exists.
    /// </summary>
    /// <remarks>
    /// This property checks if the source file exists at the specified <see cref="Uri"/> location.
    /// </remarks>
    [JsonIgnore]
    public bool Exists => File.Exists(Uri.LocalPath);

    /// <summary>
    /// Gets the collection of <see cref="Variable"/> objects representing the overrides that allow the user to
    /// change the input data to variables that are referenced on any node in the project.
    /// </summary>
    public List<Variable> Overrides { get; init; } = [];

    /// <summary>
    /// Adds an override value for a specified variable in the environment.
    /// </summary>
    /// <param name="variable">The overriden variable object.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="variable"/> is null.</exception>
    public void Add(Variable variable)
    {
        ArgumentNullException.ThrowIfNull(variable);
        Overrides.Add(variable);
    }

    /// <summary>
    /// Loads the content of the source from the specified file path and returns it as an instance of L5X.
    /// If the content has already been loaded, it is returned from memory without loading it again from the file.
    /// </summary>
    /// <returns>An instance of L5X representing the content of the source.</returns>
    public L5X Load()
    {
        if (_content is not null)
            return _content;

        _content = L5X.Load(Uri.LocalPath);
        InjectMetadata(_content);
        return _content;
    }

    /// <summary>
    /// Releases the loaded content of the source, freeing up memory.
    /// </summary>
    public void Release()
    {
        _content = null;
        GC.Collect();
    }

    /// <summary>
    /// Overrides the values of the provided variables using the configured <see cref="Overrides"/> collection.
    /// </summary>
    /// <param name="variables">The variables whose values should be overridden.</param>
    public void Override(IEnumerable<Variable> variables)
    {
        _overrides ??= Overrides.ToDictionary(x => x.VariableId);

        foreach (var variable in variables)
        {
            if (!_overrides.TryGetValue(variable.VariableId, out var match)) continue;
            variable.Value = match.Value;
        }
    }

    /// <summary>
    /// Creates a `FileSystemWatcher` object for the source file.
    /// The watcher is configured to monitor the directory containing the source file,
    /// and it raises events when the source file or its attributes are changed.
    /// </summary>
    /// <returns>
    /// A `FileSystemWatcher` object if the source file exists and its directory is not null or empty; otherwise, null.
    /// </returns>
    public FileSystemWatcher? CreateWatcher()
    {
        if (!Exists || string.IsNullOrEmpty(Directory)) return null;
        var watcher = new FileSystemWatcher(Directory);
        watcher.EnableRaisingEvents = true;
        watcher.IncludeSubdirectories = false;
        watcher.Filter = FileName;
        return watcher;
    }

    /// <summary>
    /// Adds some metadata to the L5X content for this source. The Guid, name, and physical location of the file on disc.
    /// This is so we can read back this data from the element data without having to find the source information.
    /// </summary>
    private void InjectMetadata(ILogixSerializable content)
    {
        content.Serialize().Add(new XAttribute("SourceId", SourceId.ToString()));
        content.Serialize().Add(new XAttribute("SourceName", Name));
        content.Serialize().Add(new XAttribute("SourcePath", Uri.LocalPath));
    }
}