using AutoSpex.Engine;
using FluentResults;
using MediatR;

namespace AutoSpex.Persistence;

public record MoveNodeRequest(Node Node) : IRequest<Result>;
