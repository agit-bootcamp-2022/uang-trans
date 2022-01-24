using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int WalletId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Password { get; set; }
        public bool isLock { get; set; }

        public Wallet Wallet { get; set; }

    }
}