using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App.Tests.Generator;
using Moq;
using Xunit;

namespace Moneybox.App.Tests;

public class WithdrawMoneyTests
{
    private readonly Mock<IAccountRepository> _accountRepository;
    private readonly Mock<INotificationService> _notificationService;

    public WithdrawMoneyTests()
    {
        _accountRepository = new Mock<IAccountRepository>();
        _notificationService = new Mock<INotificationService>();
    }

    [Fact]
    public void GivenValidAccountId_WhenExecuteIsCalled_ThenAccountIsUpdated()
    {
        var initialBalance = 1000m;
        var withdrawAmount = 10m;
        var paidIn = 0m;
        var withdrawn = 0m;
        var account = AccountGenerator.Generate(balance: initialBalance, withdraw: withdrawn, paidIn: paidIn);
        _accountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);
        
        GetSut().Execute(account.Id, withdrawAmount);
        
        _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == initialBalance - withdrawAmount && a.Withdrawn == withdrawAmount)), Times.Once);
        _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GivenAmountCausesLowBalance_WhenExecuteIsCalled_ThenNotifyFundLowIsCalled()
    {
        var initialBalance = 500m;
        var withdrawAmount = 100m;
        var paidIn = 0m;
        var withdrawn = 0m;

        var account = AccountGenerator.Generate(balance: initialBalance, withdraw: withdrawn, paidIn: paidIn);

        _accountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);

        GetSut().Execute(account.Id, withdrawAmount);

        _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == initialBalance - withdrawAmount && a.Withdrawn == withdrawAmount)), Times.Once);
        _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Once);
    }
    
    private WithdrawMoney GetSut() => new(_accountRepository.Object, _notificationService.Object);
}