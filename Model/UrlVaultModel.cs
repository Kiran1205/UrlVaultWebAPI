using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace UrlVaultWebAPI.Model
{
    public class UrlVaultModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Url UrlOfLink { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string LoggedInUser { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public string Comments { get; set; }

        public int RoleId { get; set; }

    }
}
