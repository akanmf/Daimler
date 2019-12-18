using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daimler.Api.Validators
{
    public class ApplicationValidator
    {
        public static bool Validate(string Application)
        {
            if (Application.ToLower() == "go" || Application.ToLower() == "gems" || Application.ToLower() == "autoline")
                return true;
            else
                return false;
        }
    }
}
