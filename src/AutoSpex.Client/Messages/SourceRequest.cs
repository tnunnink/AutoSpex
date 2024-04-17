using System.Collections.Generic;
using AutoSpex.Client.Observers;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace AutoSpex.Client.Messages;

public class SourceRequest : RequestMessage<IEnumerable<SourceObserver>>;