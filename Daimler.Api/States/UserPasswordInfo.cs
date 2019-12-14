using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daimler.Api.States
{
    public class UserPasswordInfo
    {
        public string Application { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        public int Counter { get; set; }
    }
}
