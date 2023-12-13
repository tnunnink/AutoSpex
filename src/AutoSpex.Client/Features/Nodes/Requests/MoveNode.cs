using FluentResults;
using MediatR;

namespace AutoSpex.Client.Features.Nodes;

public record MoveNodeRequest(Node Node) : IRequest<Result>;
