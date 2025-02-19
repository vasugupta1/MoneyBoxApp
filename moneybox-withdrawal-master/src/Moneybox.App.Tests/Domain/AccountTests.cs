using Shouldly;
using Xunit;

namespace Moneybox.App.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void HandleWithdraw_SuccessfulWithdrawal_ReturnsFalseWhenBalanceAboveThreshold()
    {
        var account = new Account { Balance = 1000m };
        
        var lowFunds = account.HandleWithdraw(200m);

        account.Balance.ShouldBe(800m);
        account.Withdrawn.ShouldBe(200m);
        lowFunds.ShouldBeFalse();
    }
    
    [Fact]
    public void HandleWithdraw_LowBalance_ReturnsTrueWhenBalanceBelowThreshold()
    {
        var account = new Account { Balance = 600m };
        
        var lowFunds = account.HandleWithdraw(200m);
        
        account.Balance.ShouldBe(400m);
        account.Withdrawn.ShouldBe(200m);
        lowFunds.ShouldBeTrue();
    }
    
    [Fact]
    public void HandleWithdraw_InsufficientFunds_ThrowsException()
    {
        var account = new Account { Balance = 100m };
        
        Should.Throw<InvalidOperationException>(() => account.HandleWithdraw(200m));
    }
    
    [Fact]
    public void HandleWithdraw_NegativeOrZeroAmount_ThrowsException()
    {
        var account = new Account { Balance = 1000m };
        
        Should.Throw<InvalidOperationException>(() => account.HandleWithdraw(0m));
        Should.Throw<InvalidOperationException>(() => account.HandleWithdraw(-50m));
    }
    
    [Fact]
    public void HandleDeposit_SuccessfulDeposit_ReturnsFalseWhenPayInLimitNotReached()
    {
        var account = new Account { Balance = 1000m, PaidIn = 2000m };
        
        var result = account.HandleDeposit(1000m);
        
        account.Balance.ShouldBe(2000m);
        account.PaidIn.ShouldBe(3000m);
        result.ShouldBeFalse();
    }
    
    [Fact]
    public void HandleDeposit_ApproachingPayInLimit_ReturnsTrueWhenThresholdExceeded()
    {
        var account = new Account { Balance = 1000m, PaidIn = 3600m };
        
        var result = account.HandleDeposit(300m);
        
        account.Balance.ShouldBe(1300m);
        account.PaidIn.ShouldBe(3900m);
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void HandleDeposit_PayInLimitExceeded_ThrowsException()
    {
        var account = new Account { PaidIn = 3800m };
        
        Should.Throw<InvalidOperationException>(() => account.HandleDeposit(300m));
    }

    [Fact]
    public void HandleDeposit_NegativeOrZeroAmount_ThrowsException()
    {
        var account = new Account { Balance = 1000m };
        
        Should.Throw<InvalidOperationException>(() => account.HandleDeposit(0m));
        Should.Throw<InvalidOperationException>(() => account.HandleDeposit(-50m));
    }
}