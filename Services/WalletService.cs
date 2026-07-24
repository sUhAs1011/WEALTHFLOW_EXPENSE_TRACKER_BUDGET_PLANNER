using Microsoft.EntityFrameworkCore;
using C__Project.Data;
using C__Project.Models;

namespace C__Project.Services
{
    public class WalletService
    {
        private readonly ExpenseTrackerDbContext _context;

        public WalletService(ExpenseTrackerDbContext context)
        {
            _context = context;
        }

        public async Task<List<PaymentMethod>> GetPaymentMethodsAsync()
        {
            return await _context.PaymentMethods.OrderBy(p => p.Id).ToListAsync();
        }

        public async Task<PaymentMethod?> GetPaymentMethodByIdAsync(int id)
        {
            return await _context.PaymentMethods.FindAsync(id);
        }

        public async Task<PaymentMethod> AddPaymentMethodAsync(PaymentMethod paymentMethod)
        {
            _context.PaymentMethods.Add(paymentMethod);
            await _context.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
        {
            _context.Entry(paymentMethod).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeletePaymentMethodAsync(int id)
        {
            var paymentMethod = await _context.PaymentMethods.FindAsync(id);
            if (paymentMethod != null)
            {
                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalSpentForPaymentMethodAsync(int paymentMethodId, DateTime? month = null)
        {
            var query = _context.Expenses.Where(e => e.PaymentMethodId == paymentMethodId);
            
            if (month.HasValue)
            {
                query = query.Where(e => e.Date.Year == month.Value.Year && e.Date.Month == month.Value.Month);
            }
            
            return await query.SumAsync(e => e.Amount);
        }

        public async Task<double> GetCreditUtilizationAsync(int paymentMethodId)
        {
            var pm = await GetPaymentMethodByIdAsync(paymentMethodId);
            if (pm == null || pm.Type != PaymentType.Credit || !pm.CreditLimit.HasValue || pm.CreditLimit.Value == 0)
                return 0;

            var spent = await GetTotalSpentForPaymentMethodAsync(paymentMethodId);
            return (double)(spent / pm.CreditLimit.Value) * 100;
        }
        
        public async Task<bool> CheckLiquidityWarningAsync()
        {
            var pms = await GetPaymentMethodsAsync();
            decimal totalCreditSpent = 0;
            decimal totalLiquidBalance = 0; // In a real app this would track deposits, for now we will simulate based on known fixed limits or income

            foreach(var pm in pms)
            {
                var spent = await GetTotalSpentForPaymentMethodAsync(pm.Id);
                if (pm.Type == PaymentType.Credit)
                {
                    totalCreditSpent += spent;
                }
                else
                {
                    // Simulated balance for debit/cash just for demonstration, since we don't have income tracking yet
                    totalLiquidBalance += 5000; 
                }
            }

            // Warning if credit usage exceeds available liquid cash
            return totalCreditSpent > totalLiquidBalance;
        }
        
        public async Task<Dictionary<int, decimal>> GetSpendingSplitAsync()
        {
            var split = new Dictionary<int, decimal>();
            var pms = await GetPaymentMethodsAsync();
            foreach (var pm in pms)
            {
                split[pm.Id] = await GetTotalSpentForPaymentMethodAsync(pm.Id);
            }
            return split;
        }
    }
}
