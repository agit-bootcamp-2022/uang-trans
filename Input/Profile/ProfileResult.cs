using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace uang_trans.Input.Profile
{
    public record ProfileResult
    (
        string Message,
        ProfileOutput Data
    );
}