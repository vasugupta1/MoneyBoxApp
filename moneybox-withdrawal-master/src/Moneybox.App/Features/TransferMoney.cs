using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository _accountRepository;
        private readonly INotificationService _notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = _accountRepository.GetAccountById(fromAccountId) 
                       ?? throw new ArgumentNullException(nameof(fromAccountId),"from account not found");
            var to = _accountRepository.GetAccountById(toAccountId) 
                     ?? throw new ArgumentNullException(nameof(toAccountId), "to account not found");

            var isLowBalance = from.ProcessWithdraw(amount);
            if (isLowBalance)
            {
                _notificationService.NotifyFundsLow(from.User.Email);
            }

            var approachingPayInLimit = to.ProcessDeposit(amount);
            if (approachingPayInLimit)
            {
                _notificationService.NotifyApproachingPayInLimit(to.User.Email);
            }
            
            _accountRepository.Update(from);
            _accountRepository.Update(to);
        }
    }
}
