using System;
using System.Collections.Generic;
using MediatR;
using TransactionService.Application.Documents.DTOs;

namespace TransactionService.Application.Documents.Queries;

public record SearchLogsQuery(string Query, string DateRange) : IRequest<IEnumerable<LogResultDto>>;

public record GetDocumentQuery(string DocumentId) : IRequest<DocumentResultDto>;

public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeResultDto>>;