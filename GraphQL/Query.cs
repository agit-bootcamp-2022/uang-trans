using System.Linq;
using HotChocolate;
using uang_trans.Models;

namespace uang_trans.GraphQL
{
    public class Query
    {
        public IQueryable<Customer> GetCustomersAsync([Service] AppDbContext context) =>
         context.Customers;

        public IQueryable<WalletMutation> GetTransactionsAsync([Service] AppDbContext context) =>
         context.WalletMutations;
    }
}
