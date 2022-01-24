using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class Wallet
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int Balance { get; set; }
        public DateTime CreatedDate { get; set; }

        public ICollection<WalletMutation> WalletMutations { get; set; }

        public Customer Customers { get; set; }
    }
}