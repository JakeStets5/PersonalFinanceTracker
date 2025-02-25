using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Models
{
    public class FinancialBreakdown
    {
        public Dictionary<string, decimal> IncomeByCategory { get; set; } = new();
        public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    }
}
