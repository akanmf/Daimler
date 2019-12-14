using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daimler.Api.Validators
{
    public class UserNameValidator
    {
        public static bool Validate(string userName)
        {
            //TODO: Database den sorgu yapılmalı

            return (!string.IsNullOrWhiteSpace(userName));
        }
    }
}
