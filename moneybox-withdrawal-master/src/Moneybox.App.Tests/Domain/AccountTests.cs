using Moneybox.App.Tests.Generator;
using Shouldly;
using Xunit;

namespace Moneybox.App.Tests.Domain;

public class AccountTests
{
    [Fact]
    public void HandleWithdraw_SuccessfulWithdrawal_ReturnsFalseWhenBalanceAboveThreshold()
    {
        var account = AccountGenerator.Generate(balance: 1000m);
        
        var lowFunds = account.ProcessWithdraw(200m);

        account.Balance.ShouldBe(800m);
        account.Withdrawn.ShouldBe(200m);
        lowFunds.ShouldBeFalse();
    }
    
    [Fact]
    public void HandleWithdraw_LowBalance_ReturnsTrueWhenBalanceBelowThreshold()
    {
        var account = AccountGenerator.Generate(balance: 600m);
        
        var lowFunds = account.ProcessWithdraw(200m);
        
        account.Balance.ShouldBe(400m);
        account.Withdrawn.ShouldBe(200m);
        lowFunds.ShouldBeTrue();
    }
    
    [Fact]
    public void HandleWithdraw_InsufficientFunds_ThrowsException()
    {
        var account = AccountGenerator.Generate(balance: 100m);
        
        Should.Throw<InvalidOperationException>(() => account.ProcessWithdraw(200m));
    }
    
    [Fact]
    public void HandleWithdraw_NegativeOrZeroAmount_ThrowsException()
    {
        var account = AccountGenerator.Generate(balance: 1000m);
        
        Should.Throw<InvalidOperationException>(() => account.ProcessWithdraw(0m));
        Should.Throw<InvalidOperationException>(() => account.ProcessWithdraw(-50m));
    }
    
    [Fact]
    public void HandleDeposit_SuccessfulDeposit_ReturnsFalseWhenPayInLimitNotReached()
    {
        var account = AccountGenerator.Generate(balance: 1000m, paidIn: 2000m);
        
        var result = account.ProcessDeposit(1000m);
        
        account.Balance.ShouldBe(2000m);
        account.PaidIn.ShouldBe(3000m);
        result.ShouldBeFalse();
    }
    
    [Fact]
    public void HandleDeposit_ApproachingPayInLimit_ReturnsTrueWhenThresholdExceeded()
    {
        var account = AccountGenerator.Generate(balance: 1000m, paidIn: 3600m);
        
        var result = account.ProcessDeposit(300m);
        
        account.Balance.ShouldBe(1300m);
        account.PaidIn.ShouldBe(3900m);
        result.ShouldBeTrue();
    }
    
    [Fact]
    public void HandleDeposit_PayInLimitExceeded_ThrowsException()
    {
        var account = AccountGenerator.Generate(balance: 1000m, paidIn: 3800m);
        
        Should.Throw<InvalidOperationException>(() => account.ProcessDeposit(300m));
    }

    [Fact]
    public void HandleDeposit_NegativeOrZeroAmount_ThrowsException()
    {
        var account = AccountGenerator.Generate(balance: 1000m);
        
        Should.Throw<InvalidOperationException>(() => account.ProcessDeposit(0m));
        Should.Throw<InvalidOperationException>(() => account.ProcessDeposit(-50m));
    }
}