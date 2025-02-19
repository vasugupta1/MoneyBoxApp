using System;

namespace Moneybox.App
{
    public class Account
    {
        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }

        public decimal PaidIn { get; private set; }
     
        private const decimal PayInLimit = 4000m;

        private const decimal NotificationThreshold = 500m;

        /// <summary>
        /// This method is responsible for updating balances and withdrawn based on amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>boolean flag which indicates if notification for low funds should be made</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool HandleWithdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Withdrawal amount must be greater than zero");
            }
            
            if (Balance < amount)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }
            
            Balance -= amount;
            Withdrawn += amount;
            return Balance < NotificationThreshold;
        }
        
        /// <summary>
        /// This method is responsible for updating balances and paidin based on amount
        /// </summary>
        /// <param name="amount"></param>
        /// <returns>boolean flag which indicates if payin limit is being reached</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public bool HandleDeposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Deposit amount must be greater than zero");
            }

            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("Account pay in limit reached");
            }

            PaidIn += amount;
            Balance += amount;
            
            return PayInLimit - PaidIn < NotificationThreshold;
        }
    }
}
