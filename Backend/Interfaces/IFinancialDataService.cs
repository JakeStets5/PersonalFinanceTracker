using PersonalFinanceTracker.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Backend.Interfaces
{
    public interface IFinancialDataService
    {
        Task<FinancialBreakdown> GetFinancialBreakdownAsync(string userId);
    }
}
