using MediatR;
using System;
using System.Collections.Generic;
using TransactionService.Application.Transactions.DTOs;

namespace TransactionService.Application.Transactions.Queries;

public record SearchLogsQuery(string Query, string DateRange) : IRequest<IEnumerable<LogResultDto>>;

public record GetDocumentQuery(string DocumentId) : IRequest<DocumentResultDto>;

public record SearchKnowledgeQuery(string Query) : IRequest<IEnumerable<KnowledgeResultDto>>;