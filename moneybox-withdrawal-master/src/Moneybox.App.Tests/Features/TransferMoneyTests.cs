using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Shouldly;
using Xunit;

namespace Moneybox.App.Tests
{
    public class TransferMoneyTests
    {
        private readonly Mock<IAccountRepository> _accountRepository;
        private readonly Mock<INotificationService> _notificationService;

        public TransferMoneyTests()
        {
            _accountRepository = new Mock<IAccountRepository>();
            _notificationService = new Mock<INotificationService>();
        }

        [Fact]
        public void GivenValidAccounts_WhenExecuteIsCalled_ThenNoNotificationIsCalled()
        {
            var fromAccount = new Account { Id = Guid.NewGuid(), Balance = 1000m, PaidIn = 0m };
            var toAccount = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 2000m };
            var amount = 200m;

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
            
            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);

            _accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            _accountRepository.Verify(x => x.Update(toAccount), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void GivenTransferCauseLowBalance_WhenExecutedIsCalled_ThenNotifyFundsLowIsCalled()
        {
            var fromAccount = new Account { Id = Guid.NewGuid(), Balance = 300m, PaidIn = 0m, User = new User() { Email = "fake"}};
            var toAccount = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 2000m };
            var amount = 100m;
        
            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
        
            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);
        
            _accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            _accountRepository.Verify(x => x.Update(toAccount), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact] 
        public void GivenTransferCausePayLimit_WhenExecutedIsCalled_ThenApproachingPayInLimitNotificationIsCalled()
        {
            var fromAccount = new Account { Id = Guid.NewGuid(), Balance = 1000m, PaidIn = 0m };
            var toAccount = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 3900m, User = new User() {Email = "fake"}};
            var amount = 100m;
        
            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
        
            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);
        
            _accountRepository.Verify(x => x.Update(fromAccount), Times.Once);
            _accountRepository.Verify(x => x.Update(toAccount), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Once);
        }
        
        [Fact]
        public void TransferWithPayInLimitExceeded_WhenExecuteIsCalled_ThenExceptionIsThrown()
        {
            var fromAccount = new Account { Id = Guid.NewGuid(), Balance = 1000m, PaidIn = 0m };
            var toAccount = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 4000m };
            var amount = 100m;
            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
            
        
            Should.Throw<InvalidOperationException>(() => GetSut().Execute(fromAccount.Id, toAccount.Id, amount));
            
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void TransferWithNegativeAmount_WhenExecuteIsCalled_ThenExceptionIsThrown()
        {
            var fromAccount = new Account { Id = Guid.NewGuid(), Balance = 1000m, PaidIn = 0m };
            var toAccount = new Account { Id = Guid.NewGuid(), Balance = 500m, PaidIn = 2000m };
            var amount = -200m;
            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);
        
            
            Should.Throw<InvalidOperationException>(() => GetSut().Execute(fromAccount.Id, toAccount.Id, amount));
            
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        private TransferMoney GetSut() => new(_accountRepository.Object, _notificationService.Object);
    }
}
