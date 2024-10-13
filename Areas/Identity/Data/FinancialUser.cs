using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialApp.Models;
using Microsoft.AspNetCore.Identity;

namespace FinancialApp.Areas.Identity.Data
{

//  New profile data
    public class FinancialUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }

        public FinancialUser()
        {
            Accounts = new List<Account>();
        }
    }
}

