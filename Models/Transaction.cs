using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public double AmountBuyer { get; set; }
        public int CourierId { get; set; }
        public double AmountCourier { get; set; }
        public Status TransactionStatus { get; set; }
        public virtual List<Seller> Sellers { get; set; }
    }
    
    public enum Status
    {
        Paid, Delivered
    }
}