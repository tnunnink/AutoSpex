using System;
using CommunityToolkit.Mvvm.Messaging.Messages;
using JetBrains.Annotations;

namespace AutoSpex.Client.Features.Projects;

[PublicAPI]
public class ShowNewProjectDialogMessage : AsyncRequestMessage<Uri?>
{
}