using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // Navigation
        public virtual ICollection<GroupMember> GroupMemberships { get; set; }
        public virtual ICollection<Expense> ExpensesPaid { get; set; }
        public virtual ICollection<ExpenseShare> ExpenseShares { get; set; }
    }
}