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
        private readonly ICloudDbService _dynamoDbService;

        public FinancialDataService(ICloudDbService dynamoDbService)
        {
            _dynamoDbService = dynamoDbService;
        }

        public async Task<FinancialBreakdown> GetFinancialBreakdownAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var statements = await _dynamoDbService.GetStatementsByUserIdAsync(userId);
            // Filter transactions by date range
            var filteredStatements = statements
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToList();

            var incomeBreakdown = filteredStatements
                .Where(s => s.Type == "Income")
                .GroupBy(s => s.Source)
                .ToDictionary(g => g.Key, g => g.Sum(s => s.Amount));

            var expenseBreakdown = filteredStatements
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
