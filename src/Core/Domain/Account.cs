using Core.Exceptions;

namespace Core.Domain;

public class Account
{
    private bool activated;
    private List<AuthSession> sessions = [];

    public Account(string userName, string email, string password)
    {
        UserName = userName;
        Email = email;
        Password = password;
    }

    public Account(string userName, string email)
    {
        UserName = userName;
        Email = email;
        activated = true;
    }

#pragma warning disable CS8618
    private Account() { }
#pragma warning restore CS8618

    public Role Role { get; private set; } = Role.None;
    public Guid Id { get; } = Guid.NewGuid();
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string? Password { get; private set; }

    public bool HasBeenActivated()
    {
        return activated;
    }

    public void Activate()
    {
        activated = true;
    }

    public bool HasPassword()
    {
        return Password is not null;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (Password is null)
        {
            return;
        }

        Password = newPasswordHash;
    }

    public Result AssignRole(Role role, Guid issuerId)
    {
        if (issuerId == Id)
        {
            return new CannotManageOwn<Role>();
        }

        if (Role != Role.None)
        {
            return new RoleAlreadyAssigned();
        }

        Role = role;
        DestroyAllSessions();
        return Result.Success();
    }

    public Result RemoveRole(Guid issuerId)
    {
        if (issuerId == Id)
        {
            return new CannotManageOwn<Role>();
        }

        Role = Role.None;

        return Result.Success();
    }

    public Result<RefreshToken> CreateSession(DateTime now)
    {
        if (!HasBeenActivated())
        {
            return new AccountNotActivated();
        }

        var created = new AuthSession(Id, now);

        sessions.Add(created);

        return created.CurrentToken;
    }

    public void DestroySession(Guid sessionId)
    {
        sessions.RemoveAll(session => session.Id == sessionId);
    }

    public void DestroyAllSessions()
    {
        sessions = [];
    }

    public Result<RefreshToken> RefreshSession(RefreshToken token, DateTime now)
    {
        var session = sessions.Find(session => session.Id == token.SessionId);

        if (session is null)
        {
            return new NoSuch<AuthSession>();
        }

        if (session.CurrentToken.Id != token.Id)
        {
            DestroyAllSessions();
            return new InvalidToken();
        }

        var result = session.Refresh(now);

        if (result is { IsFailure: true, Exception: SessionInactive })
        {
            DestroySession(session.Id);
            return new NoSuch<AuthSession>();
        }

        return result.Value;
    }

    public void ResetPassword(string newPasswordHash)
    {
        ChangePassword(newPasswordHash);
        DestroyAllSessions();
    }

    public RefreshToken? GetSessionCurrentToken(Guid sessionId)
    {
        return sessions.Find(s => s.Id == sessionId)?.CurrentToken;
    }
}
