using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.Queries.Outputs;
using Core.Queries.Queries;
using Dapper;

namespace Core.Queries;

public class GetCurrentAccountQueryHandler(SqlConnectionFactory connectionFactory)
    : QueryHandler<GetCurrentAccountQuery, AccountDetailsOutput>
{
    public async Task<Result<AccountDetailsOutput>> Handle(
        GetCurrentAccountQuery query,
        CancellationToken _
    )
    {
        var sql =
            """select "Id", "UserName", "Email", "Activated" from "Accounts" where "Id" = @Id""";

        var connection = connectionFactory.GetConnection();

        var found = await connection.QuerySingleAsync<AccountDetailsOutput>(sql, new { query.Id });

        if (found is null)
        {
            return new NoSuch<Account>();
        }

        return found;
    }
}
