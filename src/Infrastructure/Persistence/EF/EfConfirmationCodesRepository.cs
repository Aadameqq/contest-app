using System.Security.Cryptography;
using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.EF;

public class EfConfirmationCodesRepository(DatabaseContext ctx) : ConfirmationCodesRepository
{
    public async Task Create(ConfirmationCode confirmationCode)
    {
        await ctx.ConfirmationCodes.AddAsync(confirmationCode);
    }

    public Task<ConfirmationCode?> FindByCode(string code, ConfirmableAction action)
    {
        return ctx.ConfirmationCodes.FirstOrDefaultAsync(c => c.Code == code && c.Action == action);
    }

    public Task<ConfirmationCode?> FindByCode(string code)
    {
        return ctx.ConfirmationCodes.FirstOrDefaultAsync(c => c.Code == code);
    }

    public Task<ConfirmationCode?> FindByAccount(Account account, ConfirmableAction action)
    {
        return ctx.ConfirmationCodes.FirstOrDefaultAsync(c =>
            c.OwnerId == account.Id && c.Action == action
        );
    }

    public Task Delete(ConfirmationCode confirmationCode)
    {
        ctx.ConfirmationCodes.Remove(confirmationCode);
        return Task.CompletedTask;
    }

    public Task Update(ConfirmationCode confirmationCode)
    {
        ctx.ConfirmationCodes.Update(confirmationCode);
        return Task.CompletedTask;
    }

    public string GenerateCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var randomNumber = new byte[4];

        rng.GetBytes(randomNumber);

        var generatedNumber = BitConverter.ToInt32(randomNumber, 0) & 0x7FFFFFFF;

        return (generatedNumber % 1000000).ToString("D6");
    }
}
