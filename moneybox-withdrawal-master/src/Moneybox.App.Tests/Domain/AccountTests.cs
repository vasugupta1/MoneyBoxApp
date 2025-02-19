using Xunit;

namespace Moneybox.App.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void HandleDeposit_PayInLimitExceeded_ThrowsException()
    {
        // Arrange
        var account = new Account { PaidIn = 3800m };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.HandleDeposit(300m));
    }
}