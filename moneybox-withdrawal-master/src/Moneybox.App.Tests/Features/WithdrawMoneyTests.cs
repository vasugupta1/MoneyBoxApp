using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
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
        var withdrawAmount = 10m;
        var account = new Account { Id = Guid.NewGuid(), Balance = 1000m, PaidIn = 0m, Withdrawn = 0};
        _accountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);
        
        GetSut().Execute(account.Id, withdrawAmount);
        
        _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == 990m && a.Withdrawn == withdrawAmount)), Times.Once);
        _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void GivenAmountCausesLowBalance_WhenExecuteIsCalled_ThenNotifyFundLowIsCalled()
    {
        var withdrawAmount = 100m;
        var account = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 0m, Withdrawn = 0m, User = new User() { Email = "fake"}};
        _accountRepository.Setup(x => x.GetAccountById(It.IsAny<Guid>())).Returns(account);
        
        GetSut().Execute(account.Id, withdrawAmount);
        
        _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == 400m && a.Withdrawn == withdrawAmount)), Times.Once);
        _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Once);
    }
    
    private WithdrawMoney GetSut() => new(_accountRepository.Object, _notificationService.Object);
}