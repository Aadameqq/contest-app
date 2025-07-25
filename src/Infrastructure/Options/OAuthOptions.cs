using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class OAuthOptions
{
    [Required]
    public required string UserAgentName { get; init; }

    [Required]
    public required string UserAgentVersion { get; init; }

    [Required]
    public required string StateTokenSecret { get; init; }

    [Required]
    public required int StateTokenLifetimeInMinutes { get; init; }

    [Required]
    public required string GithubClientId { get; init; }

    [Required]
    public required string GithubSecret { get; init; }

    [Required]
    public required string OAuthRedirectUrl { get; init; }
}
