using Core.Commands;
using Core.Commands.Commands;
using Core.Commands.Outputs;
using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using NSubstitute;

namespace Core.Tests.UseCases;

public class CreateAccountCommandHandlerTests
{
    private readonly AccountsRepository accountsRepositoryMock =
        Substitute.For<AccountsRepository>();

    private readonly CreateAccountCommandHandler commandHandler;

    private readonly ConfirmationService confirmationServiceMock =
        Substitute.For<ConfirmationService>();

    private readonly Account existingAccount = new("userName", "email", "password");

    private readonly PasswordHasher passwordHasherMock = Substitute.For<PasswordHasher>();

    private readonly SessionCreator sessionCreatorMock = Substitute.For<SessionCreator>();

    private readonly TestAccount testAccount = new("new-userName", "new-email", "new-password");

    private readonly UnitOfWork uowMock = Substitute.For<UnitOfWork>();

    public CreateAccountCommandHandlerTests()
    {
        commandHandler = new CreateAccountCommandHandler(
            uowMock,
            passwordHasherMock,
            confirmationServiceMock,
            sessionCreatorMock
        );

        uowMock.GetAccountsRepository().Returns(accountsRepositoryMock);

        accountsRepositoryMock.FindByEmail(existingAccount.Email).Returns(existingAccount);

        passwordHasherMock
            .HashPassword(Arg.Any<string>())
            .Returns(args => GenerateTestHash(args.Arg<string>()));
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailAlreadyExists_ShouldFail()
    {
        var result = await RunCommand("test-username", existingAccount.Email, "test-password");

        Assert.True(result.IsFailure);
        Assert.IsType<AlreadyExists<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldSucceed()
    {
        var result = await RunCommand("new-userName", "new-email", "new-password");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldPersistAccountWithCorrectData()
    {
        await RunCommand(testAccount.UserName, testAccount.Email, testAccount.Password);

        await accountsRepositoryMock.Received().Create(Arg.Is<Account>(a => IsExpectedAccount(a)));
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldBeginConfirmation()
    {
        await RunCommand(testAccount.UserName, testAccount.Email, testAccount.Password);

        await confirmationServiceMock
            .Received()
            .BeginConfirmation(
                Arg.Is<Account>(a => IsExpectedAccount(a)),
                ConfirmableAction.AccountActivation
            );
    }

    private bool IsExpectedAccount(Account? account)
    {
        return account is not null
            && account.UserName == testAccount.UserName
            && account.Email == testAccount.Email
            && account.Password == GenerateTestHash(testAccount.Password);
    }

    private void AssertNoChanges()
    {
        uowMock.DidNotReceive().Flush();
        sessionCreatorMock.DidNotReceive().CreateSession(Arg.Any<Account>());
    }

    private Task<Result<TokenPairOutput>> RunCommand(string userName, string email, string password)
    {
        var cmd = new CreateAccountCommand(userName, email, password);
        return commandHandler.Handle(cmd, CancellationToken.None);
    }

    private string GenerateTestHash(string plainPassword)
    {
        return plainPassword + "hash";
    }

    private record TestAccount(string UserName, string Email, string Password);
}
