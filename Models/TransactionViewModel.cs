using PagedList;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialApp.Models;
public class TransactionsViewModel
{
    public IPagedList<Transaction> Transactions { get; set; }
    public IEnumerable<object> IncomeData { get; set; }
    public IEnumerable<object> ExpenseData { get; set; }
    public int? AccountId { get; set; }
}