using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daimler.Api.States
{
    public enum PdwResetStates
    {
        Initial,
        GetInfo,
        GetApproval,
        Completed
    }
}
