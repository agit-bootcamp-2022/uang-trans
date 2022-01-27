using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Transaction
{
    public record TransferBalanceOutput
    (
        bool Succeed,
        int ReceiverWalletMutationId,
        string? Message
    );
}