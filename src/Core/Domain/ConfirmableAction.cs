namespace Core.Domain;

public class ConfirmableAction
{
    public static readonly ConfirmableAction AccountActivation;
    public static readonly ConfirmableAction PasswordReset;

    static ConfirmableAction()
    {
        AccountActivation = Register(nameof(AccountActivation));
        PasswordReset = Register(nameof(PasswordReset));
    }

    private ConfirmableAction(string name)
    {
        Name = name;
    }

    public string Name { get; }

    private static List<ConfirmableAction> Actions { get; } = [];

    public static bool TryParse(string name, out ConfirmableAction action)
    {
        action = null!;
        var found = Actions.Find(r => r.Name == name);

        if (found is null)
        {
            return false;
        }

        action = found;
        return true;
    }

    public static ConfirmableAction ParseOrFail(string name)
    {
        return TryParse(name, out var action)
            ? action
            : throw new InvalidOperationException($"Invalid confirmable action name '{name}'");
    }

    private static ConfirmableAction Register(string name)
    {
        var action = new ConfirmableAction(name);
        Actions.Add(action);
        return action;
    }
}
