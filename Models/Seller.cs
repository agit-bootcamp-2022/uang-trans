using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class Seller
    {
        [Key]
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int TransactionId { get; set; }
        public double AmountSeller { get; set; }
        
        [ForeignKey(nameof(TransactionId))]
        public virtual Transaction Transaction { get; set; }
    }
}