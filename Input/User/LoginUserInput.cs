using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.User
{
    public record LoginUserInput
    (
        string Username,
        string Password
    );
}