namespace Ai.Tutor.Api.Services;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

internal sealed class ProblemDetailsException : ProblemDetails
{
    public ProblemDetailsException(int status, string? detail)
    {
        this.Status = status;
        this.Title = ReasonPhrases.GetReasonPhrase(status);
        this.Detail = detail;
    }
}
