using AutoSpex.Engine;
using FluentResults;
using MediatR;

namespace AutoSpex.Client.Components.Sources;

public record Create(Source Source) : IRequest<Result>;