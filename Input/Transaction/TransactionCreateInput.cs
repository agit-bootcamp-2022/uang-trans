using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using uang_trans.Input.Seller;
using uang_trans.Models;

namespace uang_trans.Input.Transaction
{
    public class TransactionCreateInput
    {
        [Required]
        public int BuyerId { get; set; }
        [Required]
        public double AmountBuyer { get; set; }
        [Required]
        public int CourierId { get; set; }
        [Required]
        public double AmountCourier { get; set; }
        [Required]
        public List<SellerCreateInput> Sellers { get; set; }
    }
}