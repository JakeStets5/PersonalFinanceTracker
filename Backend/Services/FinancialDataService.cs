using PersonalFinanceTracker.Backend.Interfaces;
using PersonalFinanceTracker.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Services
{
    public class FinancialDataService : IFinancialDataService
    {
        private readonly IAwsDynamoDbService _dynamoDbService;

        public FinancialDataService(IAwsDynamoDbService dynamoDbService)
        {
            _dynamoDbService = dynamoDbService;
        }

        public async Task<FinancialBreakdown> GetFinancialBreakdownAsync(string userId)
        {
            var statements = await _dynamoDbService.GetStatementsByUserIdAsync(userId);

            var incomeBreakdown = statements
                .Where(s => s.Type == "Income")
                .GroupBy(s => s.Source)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));

            var expenseBreakdown = statements
                .Where(s => s.Type == "Expense")
                .GroupBy(s => s.Source)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));

            return new FinancialBreakdown
            {
                IncomeByCategory = incomeBreakdown,
                ExpensesByCategory = expenseBreakdown
            };
        }
    }
}
