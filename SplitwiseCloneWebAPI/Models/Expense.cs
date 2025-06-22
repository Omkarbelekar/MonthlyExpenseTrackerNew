using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        public int GroupId { get; set; }

        public int PaidByUserId { get; set; }

        // Navigation
        public virtual Group Group { get; set; }
        public virtual User PaidByUser { get; set; }
        public virtual ICollection<ExpenseShare> ExpenseShares { get; set; }
    }

}