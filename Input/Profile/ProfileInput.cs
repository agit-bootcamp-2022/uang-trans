using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Profile
{
    public record ProfileInput
    (
        string Username,
        string FirstName,
        string LastName,
        string Email
    );
}