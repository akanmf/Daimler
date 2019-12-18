using Daimler.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Daimler.Api.Validators
{
    public class EmailValidator
    {
        public static bool Validate(string Email)
        {
            Regex rg = new Regex(@".{2}\@\w+\.com");
            return rg.IsMatch(Email);
            //return Email.Contains("@daimler.com");
        }
    }
}
