using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uang_trans.Models;

namespace uang_trans.Input.Wallet
{
    public record WalletMutationCreateInput
    (
        int CustomerId,
        double Amount,
        MutationType MutationType
    );
}