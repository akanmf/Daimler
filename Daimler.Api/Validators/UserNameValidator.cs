using Daimler.Api.Data;
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

            //DaimlerDBContext context = new DaimlerDBContext();
            //var userExists = context.UserInformation.Where(x => x.UserName == userName).Any();
            //return userExists;
            if (userName != "")
                return true;
            else
                return false;

        }
    }
}
