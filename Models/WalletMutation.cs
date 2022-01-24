using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class WalletMutation
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public double Debit { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}