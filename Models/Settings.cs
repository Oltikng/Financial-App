using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models
{
    public class Settings
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Preferred Currency")]
        [EnumDataType(typeof(CurrencyType))]
        public CurrencyType? PreferredCurrency { get; set; }
        public string UserId { get; set; }
    }

    public enum CurrencyType
    {
        USD,
        ALL,
        EUR,
        GBP
    }
}