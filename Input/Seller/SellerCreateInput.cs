using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Seller
{
    public record SellerCreateInput
    (
        int SellerId,
        double AmountSeller
    );
}