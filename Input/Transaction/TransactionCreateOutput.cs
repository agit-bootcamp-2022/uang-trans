using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Transaction
{
    public record TransactionCreateOutput
    (
        string Message,
        int TransactionId
    );
}