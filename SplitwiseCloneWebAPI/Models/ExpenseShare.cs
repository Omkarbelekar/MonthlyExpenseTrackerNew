using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class ExpenseShare
    {
        public int Id { get; set; }

        public int ExpenseId { get; set; }
        public int OwedByUserId { get; set; }

        public decimal ShareAmount { get; set; }

        // Navigation
        public virtual Expense Expense { get; set; }
        public virtual User OwedByUser { get; set; }
    }

}