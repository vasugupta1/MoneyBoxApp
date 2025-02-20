using AutoFixture;

namespace Moneybox.App.Tests.Generator;

public static class AccountGenerator
{
    public static Account Generate(decimal balance = 0m, decimal withdraw = 0m, decimal paidIn = 0m)
    {
        var fixture = new Fixture();
        var id = Guid.NewGuid();
        var user = fixture.Create<User>();
        return new Account(id, user, balance, withdraw, paidIn);
    }
}