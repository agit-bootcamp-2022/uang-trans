using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using uang_rans.Models;
using uang_trans.Input.Profile;
using uang_trans.Input.Role;
using uang_trans.Models;
using UangTrans.Input.Wallet;

namespace uang_trans.GraphQL
{
    public class Query
    {

        [Authorize(Roles = new[] { "Admin" })]
        public IQueryable<Customer> GetCustomersAsync([Service] AppDbContext context) =>
         context.Customers.Include(c => c.Wallet);

        [Authorize(Roles = new[] { "Admin" })]
        public IQueryable<Transaction> GetTransactionsAsync([Service] AppDbContext context) =>
         context.Transactions.Include(t => t.Sellers);

        [Authorize(Roles = new[] { "Admin" })]
        public IQueryable<Wallet> GetWalletsAsync([Service] AppDbContext context) =>
          context.Wallets;

        [Authorize(Roles = new[] { "Admin" })]
        public List<Roles> GeRolesAsync([Service] AppDbContext context)
        {
            List<Roles> lstRole = new List<Roles>();
            var results = context.Roles;
            foreach (var role in results)
            {
                lstRole.Add(new Roles { Rolename = role.Name });
            }

            return lstRole;
        }

        [Authorize(Roles = new[] { "Admin" })]
        public IQueryable<WalletMutation> GetWalletMutationAsync([Service] AppDbContext context) =>
            context.WalletMutations;

        [Authorize(Roles = new[] { "Customer" })]
        public IQueryable<Wallet> GetWalletByCustomerIdAsync([Service] AppDbContext context,
                                                            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var custId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("Id").Value);
            return context.Wallets.Where(p => p.CustomerId == custId);
        }

        [Authorize(Roles = new[] { "Admin", "Customer" })]
        public IQueryable<ProfileOutput> GetProfileByCustomerIdAsync([Service] AppDbContext context,
                                                            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var custId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("Id").Value);
            return context.Customers.Select(p => new ProfileOutput()
            {
                Id = p.Id,
                Username = p.Username,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                CreatedDate = p.CreatedDate
            }).Where(x => x.Id == custId);
        }

        public class TableJoinResult
        {
            public WalletMutation WalletMutation { get; set; }
            public Wallet Wallet { get; set; }
        }

        [Authorize(Roles = new[] { "Customer" })]
        public IQueryable<TableJoinResult> GetWalletMutationIdAsync([Service] AppDbContext context,
                                                                    [Service] IHttpContextAccessor httpContextAccessor)
        {
            var custId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("Id").Value);

            return
                from wm in context.WalletMutations
                join w in context.Wallets on wm.WalletId equals w.Id
                where
                    w.CustomerId == custId
                select new TableJoinResult() { Wallet = w, WalletMutation = wm };

        }
    }
}
