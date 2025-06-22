using SplitwiseCloneWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SplitwiseCloneWebAPI.Controllers
{
    public class GroupsController : ApiController
    {
        private AppDbContext db = new AppDbContext();

        [HttpPost]
        [Route("api/Groups/Create")]
        public IHttpActionResult CreateGroup(Group group)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest("Invalid group data.");

            group.CreatedAt = DateTime.Now;

            db.Groups.Add(group);
            db.SaveChanges();

            // Optionally: Add creator as member
            var groupMember = new GroupMember
            {
                GroupId = group.Id,
                UserId = group.CreatedByUserId
            };

            db.GroupMembers.Add(groupMember);
            db.SaveChanges();

            return Ok(group);
        }

        [HttpGet]
        [Route("api/Groups")]
        public IHttpActionResult GetAllGroups()
        {
            var groups = db.Groups.Select(g => new {
                g.Id,
                g.Name,
                g.CreatedAt
            }).ToList();

            return Ok(groups);
        }
        [HttpGet]
        [Route("api/groups/{groupId}/Balances")]
        public IHttpActionResult GetGroupBalances(int groupId)
        {
            var expenses = db.Expenses
                .Where(e => e.GroupId == groupId)
                .Select(e => new
                {
                    e.Amount,
                    e.PaidByUserId,
                    Shares = e.ExpenseShares.Select(s => new
                    {
                        s.OwedByUserId,
                        s.ShareAmount
                    })
                }).ToList();

            var userBalances = new Dictionary<int, decimal>();

            foreach (var expense in expenses)
            {
                foreach (var share in expense.Shares)
                {
                    if (!userBalances.ContainsKey(share.OwedByUserId))
                        userBalances[share.OwedByUserId] = 0;

                    userBalances[share.OwedByUserId] -= share.ShareAmount;
                }

                if (!userBalances.ContainsKey(expense.PaidByUserId))
                    userBalances[expense.PaidByUserId] = 0;

                userBalances[expense.PaidByUserId] += expense.Amount;
            }

            // Now convert to list of who owes whom
            var balances = new List<object>();

            var positiveBalances = userBalances
                .Where(kvp => kvp.Value > 0)
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            var negativeBalances = userBalances
                .Where(kvp => kvp.Value < 0)
                .OrderBy(kvp => kvp.Value)
                .ToList();

            int i = 0, j = 0;

            while (i < negativeBalances.Count && j < positiveBalances.Count)
            {
                var debtor = negativeBalances[i];
                var creditor = positiveBalances[j];

                var minAmount = Math.Min(Math.Abs(debtor.Value), creditor.Value);

                balances.Add(new
                {
                    FromUserId = debtor.Key,
                    ToUserId = creditor.Key,
                    Amount = minAmount
                });

                negativeBalances[i] = new KeyValuePair<int, decimal>(debtor.Key, debtor.Value + minAmount);
                positiveBalances[j] = new KeyValuePair<int, decimal>(creditor.Key, creditor.Value - minAmount);

                if (Math.Abs(negativeBalances[i].Value) < 0.01m) i++;
                if (positiveBalances[j].Value < 0.01m) j++;
            }

            return Ok(balances);
        }
        [HttpGet]
        [Route("api/groups/{userId}/userId")]
        public IHttpActionResult GetUserGroups(int userId)
        {
            var groups = db.GroupMembers
                           .Where(gm => gm.UserId == userId)
                           .Select(gm => gm.Group)
                           .ToList();
            return Ok(groups);
        }

    }
}