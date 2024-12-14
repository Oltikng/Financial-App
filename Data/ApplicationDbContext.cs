using FinancialApp.Areas.Identity.Data;
using FinancialApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;
using System.Reflection.Emit;

namespace FinancialApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<FinancialUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Settings> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);
            // Configure relationships and precision for the decimal properties
            builder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts) // Assuming a FinancialUser can have multiple Accounts
                .HasForeignKey(a => a.UserId);
        }
    }
}
