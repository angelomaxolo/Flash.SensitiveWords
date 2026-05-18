using Flash.SensitiveWords.Application.Services;
using Flash.SensitiveWords.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Flash.SensitiveWords.API.Endpoints;

/// <summary>
/// Provides the minimal API endpoint mappings for sensitive word management and filtering.
/// </summary>
public static class SensitiveWordsEndpoints
{
    /// <summary>
    /// Registers the sensitive words API routes on the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void MapSensitiveWordsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/sensitivewords").WithTags("Sensitive Words");

        group.MapGet("", async (ISensitiveWordService service, CancellationToken cancellationToken) =>
                Results.Ok(await service.GetAllAsync(cancellationToken)))
            .WithName("GetAllSensitiveWords")
            .WithSummary("Retrieve all sensitive words")
            .WithDescription("Returns the complete list of sensitive words stored in the database.")
            .Produces<IEnumerable<SensitiveWord>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", async (Guid id, ISensitiveWordService service, CancellationToken cancellationToken) =>
            {
                var entity = await service.GetByIdAsync(id, cancellationToken);
                return entity is not null ? Results.Ok(entity) : Results.NotFound();
            })
            .WithName("GetSensitiveWordById")
            .WithSummary("Retrieve a sensitive word by ID")
            .WithDescription("Returns a single sensitive word entity using its unique identifier.")
            .Produces<SensitiveWord>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("", async (AddSensitiveWordRequest request, ISensitiveWordService service, CancellationToken cancellationToken) =>
            {
                var result = await service.AddAsync(request.Word, cancellationToken);
                return Results.Created($"/sensitivewords/{result.Id}", result);
            })
            .WithName("CreateSensitiveWord")
            .WithSummary("Add a new sensitive word")
            .WithDescription("Creates a new sensitive word entry in the database for filtering messages.")
            .Accepts<AddSensitiveWordRequest>("application/json")
            .Produces<SensitiveWord>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}", async (Guid id, UpdateSensitiveWordRequest request, ISensitiveWordService service, CancellationToken cancellationToken) =>
            {
                var result = await service.UpdateAsync(id, request.Word, cancellationToken);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
            .WithName("UpdateSensitiveWord")
            .WithSummary("Update an existing sensitive word")
            .WithDescription("Updates the value of a sensitive word by ID.")
            .Accepts<UpdateSensitiveWordRequest>("application/json")
            .Produces<SensitiveWord>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async (Guid id, ISensitiveWordService service, CancellationToken cancellationToken) =>
            {
                var deleted = await service.DeleteAsync(id, cancellationToken);
                return deleted ? Results.NoContent() : Results.NotFound();
            })
            .WithName("DeleteSensitiveWord")
            .WithSummary("Delete a sensitive word")
            .WithDescription("Removes a sensitive word from the database using its ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/filter", async (FilterMessageRequest request, ISensitiveWordService service, CancellationToken cancellationToken) =>
            {
                var filtered = await service.FilterMessageAsync(request.Message, cancellationToken);
                return Results.Ok(new FilterMessageResponse { OriginalMessage = request.Message, FilteredMessage = filtered });
            })
            .WithName("FilterMessage")
            .WithSummary("Filter a chat message")
            .WithDescription("Stars out all sensitive words in the provided message and returns the amended text.")
            .Accepts<FilterMessageRequest>("application/json")
            .Produces<FilterMessageResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Request model for creating a sensitive word.
    /// </summary>
    public sealed record AddSensitiveWordRequest
    {
        /// <summary>The sensitive word to add.</summary>
        public required string Word { get; init; }
    }

    /// <summary>
    /// Request model for updating a sensitive word.
    /// </summary>
    public sealed record UpdateSensitiveWordRequest
    {
        /// <summary>The new sensitive word text.</summary>
        public required string Word { get; init; }
    }

    /// <summary>
    /// Request model for filtering a chat message.
    /// </summary>
    public sealed record FilterMessageRequest
    {
        /// <summary>The original message to filter.</summary>
        public required string Message { get; init; }
    }

    /// <summary>
    /// Response model returned after filtering a chat message.
    /// </summary>
    public sealed record FilterMessageResponse
    {
        /// <summary>The original unfiltered message.</summary>
        public required string OriginalMessage { get; init; }

        /// <summary>The message after sensitive words have been starred out.</summary>
        public required string FilteredMessage { get; init; }
    }
}
