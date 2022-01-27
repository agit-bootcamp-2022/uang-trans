using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Transaction
{
    public class TransferBalanceInput
    {
        [Required]
        public double Amount { get; set; }
        [Required]
        public int CustomerDebitId { get; set; }
        [Required]
        public int CustomerCreditId { get; set; }
    }
}