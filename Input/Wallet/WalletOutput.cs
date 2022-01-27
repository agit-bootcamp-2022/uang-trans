using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UangTrans.Input.Wallet
{
    public class WalletOutput
    {
        public int Id {get; set; }
        public double Balance { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CustomerId { get; set; }

    }
}