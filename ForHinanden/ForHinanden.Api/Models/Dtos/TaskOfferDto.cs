using System;

namespace ForHinanden.Api.Models.Dtos;

public record TaskOfferDto(
    Guid Id,
    Guid TaskId,
    string OfferedBy,
    string? Message,
    OfferStatus Status,
    DateTime CreatedAt
);