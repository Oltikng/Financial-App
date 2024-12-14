    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using PagedList;
    using PagedList.Mvc;

namespace FinancialApp.Models
    {
        public enum TransactionType
        {
            Income,
            Expense
        }

    public enum TransactionCategory
    {
        Booking,
        Food,
        Entertainment,
        Taxes,
        Salary,
        Investment,
        Rent,
        Debt,
        Transport,
        Groceries,
        OnlineShopping,
        Shopping,
        Other
    }
    public class Transaction
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public TransactionCategory Category { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
            public decimal Amount { get; set; }
            
            [Required]
            public DateTime Date { get; set; }

            [Required]
            [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid transaction type.")]
            public TransactionType Type { get; set; }
            
            [Required]
            public int AccountId { get; set; }
            [ForeignKey(nameof(AccountId))]

            [Required]
            public CurrencyType Currency { get; set; }
            public string Icon { get; set; }
        public Account Account { get; set; }
    }
   
}
