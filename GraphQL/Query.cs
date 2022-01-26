using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using uang_rans.Models;
using uang_trans.Input.Profile;
using uang_trans.Input.Role;
using uang_trans.Models;

namespace uang_trans.GraphQL
{
    public class Query
    {
        public IQueryable<Customer> GetCustomersAsync([Service] AppDbContext context) =>
         context.Customers;

        public IQueryable<Transaction> GetTransactionsAsync([Service] AppDbContext context) =>
         context.Transactions;

        public IQueryable<Wallet> GetWalletsAsync([Service] AppDbContext context) =>
          context.Wallets;

        //[Authorize(Roles = new [] {"Admin"})]
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
        
        public IQueryable<WalletMutation> GetWalletMutationAsync([Service] AppDbContext context) =>
            context.WalletMutations;

        // [Authorize(Roles = new[] { "Customer" })]
        public IQueryable<Wallet> GetWalletByCustomerIdAsync([Service] AppDbContext context,
                                                            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var custId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("Id").Value);
            return context.Wallets.Where(p => p.CustomerId == custId);
        }

        // [Authorize(Roles = new[] { "Admin","Customer" })]
        public IQueryable<Customer> GetProfileByCustomerIdAsync([Service] AppDbContext context,
                                                            [Service] IHttpContextAccessor httpContextAccessor)
        {
            var custId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("Id").Value);
            return context.Customers.Where(p=> p.Id == custId);    
        }   
    }
}
