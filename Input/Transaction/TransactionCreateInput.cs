using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uang_trans.Input.Seller;
using uang_trans.Models;

namespace uang_trans.Input.Transaction
{
    public record TransactionCreateInput
    (
        int BuyerId,
        double AmountBuyer,
        int CourierId,
        double AmountCourier,
        List<SellerCreateInput> Sellers
    );
}