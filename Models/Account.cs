using FinancialApp.Areas.Identity.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialApp.Models
{
    public enum AccountCategory
    {
        Bank,
        Cash,
        Crypto,
        Investment,
        Card
    }

    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public AccountCategory Category { get; set; } // Changed to use enum directly
        [Required]
        public string Icon { get; set; }

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual FinancialUser User { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }
        public Account()
        {
            Transactions = new List<Transaction>();
        }
    }
}