using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using uang_rans.Models;
using uang_trans.Input.Role;
using uang_trans.Models;

namespace uang_trans.GraphQL
{
    public class Query
    {

        public IQueryable<Customer> GetCustomersAsync([Service] AppDbContext context) =>
         context.Customers;

        public IQueryable<WalletMutation> GetTransactionsAsync([Service] AppDbContext context) =>
         context.WalletMutations;

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
    }
}
