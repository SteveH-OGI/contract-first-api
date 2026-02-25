namespace Api.Models;

/// <summary>
/// Represents an error payload returned by API endpoints.
/// </summary>
/// <param name="Code">A short machine-friendly error code.</param>
/// <param name="Message">A human-friendly error message with details.</param>
internal record Error(string Code, string Message);
