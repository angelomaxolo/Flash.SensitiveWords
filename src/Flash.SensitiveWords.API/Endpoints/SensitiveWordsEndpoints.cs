using Flash.SensitiveWords.Application.Exceptions;
using Flash.SensitiveWords.Application.Services;
using Flash.SensitiveWords.Contracts.Common;
using Flash.SensitiveWords.Contracts.Requests;
using Flash.SensitiveWords.Contracts.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Flash.SensitiveWords.API.Endpoints;

/// <summary>
/// Provides the minimal API endpoint mappings for sensitive word management and filtering.
/// Maps Application layer results to API contracts and handles exception mapping.
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

        group.MapGet("", GetAllSensitiveWords)
            .WithName("GetAllSensitiveWords")
            .WithSummary("Retrieve all sensitive words")
            .WithDescription("Returns the complete list of sensitive words stored in the database.")
            .Produces<ApiResponse<List<SensitiveWordDto>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);

        group.MapGet("/{id:guid}", GetSensitiveWordById)
            .WithName("GetSensitiveWordById")
            .WithSummary("Retrieve a sensitive word by ID")
            .WithDescription("Returns a single sensitive word by its unique identifier.")
            .Produces<ApiResponse<SensitiveWordDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);

        group.MapPost("", CreateSensitiveWord)
            .WithName("CreateSensitiveWord")
            .WithSummary("Add a new sensitive word")
            .WithDescription("Creates a new sensitive word entry in the database for filtering messages.")
            .Accepts<CreateSensitiveWordRequest>("application/json")
            .Produces<ApiResponse<SensitiveWordDto>>(StatusCodes.Status201Created)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);

        group.MapPut("/{id:guid}", UpdateSensitiveWord)
            .WithName("UpdateSensitiveWord")
            .WithSummary("Update an existing sensitive word")
            .WithDescription("Updates the value of a sensitive word by ID.")
            .Accepts<UpdateSensitiveWordRequest>("application/json")
            .Produces<ApiResponse<SensitiveWordDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);

        group.MapDelete("/{id:guid}", DeleteSensitiveWord)
            .WithName("DeleteSensitiveWord")
            .WithSummary("Delete a sensitive word")
            .WithDescription("Removes a sensitive word from the database using its ID.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status404NotFound)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);

        group.MapPost("/filter", FilterMessage)
            .WithName("FilterMessage")
            .WithSummary("Filter a chat message")
            .WithDescription("Stars out all sensitive words in the provided message and returns the amended text.")
            .Accepts<FilterMessageRequest>("application/json")
            .Produces<ApiResponse<FilterMessageResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse<ErrorResponse>>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetAllSensitiveWords(ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            var words = await service.GetAllAsync(cancellationToken);
            var dtos = words.Select(MapToDto).ToList();
            var response = new ApiResponse<List<SensitiveWordDto>>
            {
                Success = true,
                Message = "Sensitive words retrieved successfully",
                Result = dtos
            };
            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving sensitive words");
        }
    }

    private static async Task<IResult> GetSensitiveWordById(Guid id, ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            var word = await service.GetByIdAsync(id, cancellationToken);
            var dto = MapToDto(word);
            var response = new ApiResponse<SensitiveWordDto>
            {
                Success = true,
                Message = "Sensitive word retrieved successfully",
                Result = dto
            };
            return Results.Ok(response);
        }
        catch (SensitiveWordNotFoundException)
        {
            return NotFoundError($"Sensitive word with ID '{id}' not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error retrieving sensitive word");
        }
    }

    private static async Task<IResult> CreateSensitiveWord(CreateSensitiveWordRequest request, ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.AddAsync(request.Word, cancellationToken);
            var dto = MapToDto(result);
            var response = new ApiResponse<SensitiveWordDto>
            {
                Success = true,
                Message = "Sensitive word created successfully",
                Result = dto
            };
            return Results.Created($"/sensitivewords/{result.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return BadRequestError(ex.Message);
        }
        catch (SensitiveWordAlreadyExistsException ex)
        {
            return BadRequestError(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error creating sensitive word");
        }
    }

    private static async Task<IResult> UpdateSensitiveWord(Guid id, UpdateSensitiveWordRequest request, ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            var result = await service.UpdateAsync(id, request.Word, cancellationToken);
            var dto = MapToDto(result);
            var response = new ApiResponse<SensitiveWordDto>
            {
                Success = true,
                Message = "Sensitive word updated successfully",
                Result = dto
            };
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequestError(ex.Message);
        }
        catch (SensitiveWordNotFoundException)
        {
            return NotFoundError($"Sensitive word with ID '{id}' not found");
        }
        catch (SensitiveWordAlreadyExistsException ex)
        {
            return BadRequestError(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error updating sensitive word");
        }
    }

    private static async Task<IResult> DeleteSensitiveWord(Guid id, ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            await service.DeleteAsync(id, cancellationToken);
            return Results.NoContent();
        }
        catch (SensitiveWordNotFoundException)
        {
            return NotFoundError($"Sensitive word with ID '{id}' not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error deleting sensitive word");
        }
    }

    private static async Task<IResult> FilterMessage(FilterMessageRequest request, ISensitiveWordService service, CancellationToken cancellationToken)
    {
        try
        {
            var filterResult = await service.FilterMessageAsync(request.Message, cancellationToken);
            var filterResponse = new FilterMessageResponse
            {
                OriginalMessage = filterResult.OriginalMessage,
                FilteredMessage = filterResult.FilteredMessage
            };
            var response = new ApiResponse<FilterMessageResponse>
            {
                Success = true,
                Message = "Message filtered successfully",
                Result = filterResponse
            };
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequestError(ex.Message);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error filtering message");
        }
    }

    /// <summary>
    /// Maps Application layer result to API contract.
    /// </summary>
    private static SensitiveWordDto MapToDto(Application.Models.SensitiveWordResult result)
    {
        return new SensitiveWordDto
        {
            Id = result.Id,
            Word = result.Word
        };
    }

    /// <summary>
    /// Handles application exceptions and returns appropriate error responses.
    /// </summary>
    private static IResult HandleException(Exception ex, string message)
    {
        var errorResponse = new ApiResponse<ErrorResponse>
        {
            Success = false,
            Message = message,
            Result = new ErrorResponse
            {
                Code = ex.GetType().Name,
                Message = ex.Message
            }
        };

        if (ex is SensitiveWordNotFoundException)
        {
            return Results.NotFound(errorResponse);
        }

        return Results.Json(errorResponse, statusCode: StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Returns a 400 Bad Request error response.
    /// </summary>
    private static IResult BadRequestError(string message)
    {
        var response = new ApiResponse<ErrorResponse>
        {
            Success = false,
            Message = message,
            Result = new ErrorResponse
            {
                Code = "BadRequest",
                Message = message
            }
        };
        return Results.BadRequest(response);
    }

    /// <summary>
    /// Returns a 404 Not Found error response.
    /// </summary>
    private static IResult NotFoundError(string message)
    {
        var response = new ApiResponse<ErrorResponse>
        {
            Success = false,
            Message = message,
            Result = new ErrorResponse
            {
                Code = "NotFound",
                Message = message
            }
        };
        return Results.NotFound(response);
    }
}
