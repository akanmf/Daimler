using System;
using System.Collections.Generic;

namespace Daimler.Api.Data
{
    public partial class UserInformation
    {
        public int Id { get; set; }
        public string ApplicationName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
