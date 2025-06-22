using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SplitwiseCloneWebAPI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("name=DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseShare> ExpenseShares { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Turn off cascade delete on ExpenseShare -> OwedByUser
            modelBuilder.Entity<ExpenseShare>()
                .HasRequired(es => es.OwedByUser)
                .WithMany(u => u.ExpenseShares)
                .HasForeignKey(es => es.OwedByUserId)
                .WillCascadeOnDelete(false);

            // Turn off cascade delete on ExpenseShare -> Expense
            modelBuilder.Entity<ExpenseShare>()
                .HasRequired(es => es.Expense)
                .WithMany(e => e.ExpenseShares)
                .HasForeignKey(es => es.ExpenseId)
                .WillCascadeOnDelete(false);

            // Optional: Apply same for GroupMember if needed
            modelBuilder.Entity<GroupMember>()
                .HasRequired(gm => gm.User)
                .WithMany(u => u.GroupMemberships)
                .HasForeignKey(gm => gm.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<GroupMember>()
                .HasRequired(gm => gm.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(gm => gm.GroupId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}