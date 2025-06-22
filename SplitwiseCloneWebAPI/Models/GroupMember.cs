using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class GroupMember
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public int UserId { get; set; }

        // Navigation
        public virtual Group Group { get; set; }
        public virtual User User { get; set; }
    }

}