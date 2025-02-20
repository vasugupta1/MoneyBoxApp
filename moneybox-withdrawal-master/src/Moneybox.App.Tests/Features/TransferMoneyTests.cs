using Moq;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App.Tests.Generator;
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
            var fromAccountBalance = 1000m;
            var fromAccountPaidIn = 0m;
            var fromAccountWithdrawn = 0m;

            var toAccountBalance = 500m;
            var toAccountPaidIn = 2000m;
            var toAccountWithdrawn = 0m;

            var amount = 200m;

            var fromAccount = AccountGenerator.Generate(balance: fromAccountBalance, withdraw: fromAccountWithdrawn, paidIn: fromAccountPaidIn);
            var toAccount = AccountGenerator.Generate(balance: toAccountBalance, withdraw: toAccountWithdrawn, paidIn: toAccountPaidIn);

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);

            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == fromAccountBalance - amount && a.Withdrawn == fromAccountWithdrawn + amount)), Times.Once);
            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == toAccountBalance + amount && a.PaidIn == toAccountPaidIn + amount)), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void GivenTransferCauseLowBalance_WhenExecutedIsCalled_ThenNotifyFundsLowIsCalled()
        {
            var fromAccountBalance = 300m;
            var fromAccountPaidIn = 0m;
            var fromAccountWithdrawn = 0m;

            var toAccountBalance = 500m;
            var toAccountPaidIn = 2000m;
            var toAccountWithdrawn = 0m;

            var amount = 100m;

            var fromAccount = AccountGenerator.Generate(balance: fromAccountBalance, withdraw: fromAccountWithdrawn, paidIn: fromAccountPaidIn);

            var toAccount = AccountGenerator.Generate(balance: toAccountBalance, withdraw: toAccountWithdrawn, paidIn: toAccountPaidIn);

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);

            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == fromAccountBalance - amount && a.Withdrawn == fromAccountWithdrawn + amount)), Times.Once);
            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == toAccountBalance + amount && a.PaidIn == toAccountPaidIn + amount)), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(fromAccount.User.Email), Times.Once);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact] 
        public void GivenTransferCausePayLimit_WhenExecutedIsCalled_ThenApproachingPayInLimitNotificationIsCalled()
        {
            var fromAccountBalance = 1000m;
            var fromAccountPaidIn = 0m;
            var fromAccountWithdrawn = 0m;

            var toAccountBalance = 500m;
            var toAccountPaidIn = 3900m;
            var toAccountWithdrawn = 0m;

            var amount = 100m;

            var fromAccount = AccountGenerator.Generate(balance: fromAccountBalance, withdraw: fromAccountWithdrawn, paidIn: fromAccountPaidIn);
            var toAccount = AccountGenerator.Generate(balance: toAccountBalance, withdraw: toAccountWithdrawn, paidIn: toAccountPaidIn);
           

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            GetSut().Execute(fromAccount.Id, toAccount.Id, amount);

            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == fromAccountBalance - amount && a.Withdrawn == fromAccountWithdrawn + amount)), Times.Once);
            _accountRepository.Verify(x => x.Update(It.Is<Account>(a => a.Balance == toAccountBalance + amount && a.PaidIn == toAccountPaidIn + amount)), Times.Once);
            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(toAccount.User.Email), Times.Once);
        }
        
        [Fact]
        public void TransferWithPayInLimitExceeded_WhenExecuteIsCalled_ThenExceptionIsThrown()
        {
            var fromAccountBalance = 1000m;
            var fromAccountPaidIn = 0m;
            var fromAccountWithdrawn = 0m;

            var toAccountBalance = 500m;
            var toAccountPaidIn = 4000m;
            var toAccountWithdrawn = 0m;

            var amount = 100m;

            var fromAccount = AccountGenerator.Generate(balance: fromAccountBalance, withdraw: fromAccountWithdrawn, paidIn: fromAccountPaidIn);
            var toAccount = AccountGenerator.Generate(balance: toAccountBalance, withdraw: toAccountWithdrawn, paidIn: toAccountPaidIn);

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            Should.Throw<InvalidOperationException>(() => GetSut().Execute(fromAccount.Id, toAccount.Id, amount));

            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void TransferWithNegativeAmount_WhenExecuteIsCalled_ThenExceptionIsThrown()
        {
            var fromAccountBalance = 1000m;
            var fromAccountPaidIn = 0m;
            var fromAccountWithdrawn = 0m;

            var toAccountBalance = 500m;
            var toAccountPaidIn = 2000m;
            var toAccountWithdrawn = 0m;

            var amount = -200m;

            var fromAccount = AccountGenerator.Generate(balance: fromAccountBalance, withdraw: fromAccountWithdrawn, paidIn: fromAccountPaidIn);
            var toAccount = AccountGenerator.Generate(balance: toAccountBalance, withdraw: toAccountWithdrawn, paidIn: toAccountPaidIn);

            _accountRepository.Setup(x => x.GetAccountById(fromAccount.Id)).Returns(fromAccount);
            _accountRepository.Setup(x => x.GetAccountById(toAccount.Id)).Returns(toAccount);

            Should.Throw<InvalidOperationException>(() => GetSut().Execute(fromAccount.Id, toAccount.Id, amount));

            _notificationService.Verify(x => x.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationService.Verify(x => x.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        private TransferMoney GetSut() => new(_accountRepository.Object, _notificationService.Object);
    }
}
