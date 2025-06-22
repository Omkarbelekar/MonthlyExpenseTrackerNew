using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int FromUserId { get; set; }
        public int ToUserId { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; } = DateTime.Now;

        // Navigation
        public virtual Group Group { get; set; }
        public virtual User FromUser { get; set; }
        public virtual User ToUser { get; set; }
    }

}