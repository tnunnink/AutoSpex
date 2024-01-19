using AutoSpex.Client.Observers;

namespace AutoSpex.Client.Messages;

public record ProjectOpenMessage(ProjectObserver Project);
public record ProjectRemoveMessage(ProjectObserver Project);
public record ProjectLocateMessage(ProjectObserver Project);
public record ProjectCopyPathMessage(ProjectObserver Project);

