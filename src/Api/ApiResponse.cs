using Microsoft.AspNetCore.Mvc;

namespace Api;

public static class ApiResponse
{
    public static ActionResult Forbid(string customMessage = "Forbidden")
    {
        return GenerateResponse(403, customMessage);
    }

    public static ActionResult Unauthorized(string customMessage = "Unauthorized")
    {
        return GenerateResponse(401, customMessage);
    }

    public static ActionResult Timeout(string customMessage = "Timeout")
    {
        return GenerateResponse(408, customMessage);
    }

    public static ActionResult Cooldown(
        string customMessage = "Too many attempts. Please wait a bit before trying again"
    )
    {
        return GenerateResponse(429, customMessage);
    }

    public static ActionResult Conflict(string customMessage = "Conflict")
    {
        return GenerateResponse(409, customMessage);
    }

    public static ActionResult Ok(string customMessage = "Operation successful")
    {
        return GenerateResponse(200, customMessage);
    }

    public static ActionResult BadRequest(string customMessage = "Bad Request")
    {
        return GenerateResponse(400, customMessage);
    }

    public static ActionResult NotFound(string customMessage = "Not Found")
    {
        return GenerateResponse(404, customMessage);
    }

    public static ActionResult ServiceUnavailable(string customMessage = "Service Unavailable")
    {
        return GenerateResponse(503, customMessage);
    }

    public static ActionResult UnprocessableEntity(string customMessage = "Unprocessable Entity")
    {
        return GenerateResponse(422, customMessage);
    }

    public static Task ApplyAsync(HttpContext ctx, ActionResult result)
    {
        return result.ExecuteResultAsync(new ActionContext { HttpContext = ctx });
    }

    private static ActionResult GenerateResponse(int statusCode, string message)
    {
        return new JsonResult(new { message }) { StatusCode = statusCode };
    }
}
