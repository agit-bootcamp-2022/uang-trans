using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Seller
{
    public class SellerCreateInput
    {
        [Required]
        public int SellerId { get; set; }
        [Required]
        public double AmountSeller { get; set; }
    }
}