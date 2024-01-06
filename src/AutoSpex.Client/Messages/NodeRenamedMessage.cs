using AutoSpex.Client.Observers;

namespace AutoSpex.Client.Messages;

public record NodeRenamedMessage(NodeObserver Node);