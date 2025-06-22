using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual User CreatedBy { get; set; }
        public virtual ICollection<GroupMember> Members { get; set; }
        public virtual ICollection<Expense> Expenses { get; set; }
    }

}