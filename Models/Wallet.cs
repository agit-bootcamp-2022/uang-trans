using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }
        public double Balance { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<WalletMutation> WalletMutations { get; set; }


        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

    }
}