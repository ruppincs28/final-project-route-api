using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace final_project_route_api.Models
{
    public class ClientCompaniesWithAddresses
    {
        List<string> companiesWithAddresses;

        public ClientCompaniesWithAddresses(List<string> companiesWithAddresses)
        {
            CompaniesWithAddresses = companiesWithAddresses;
        }

        public List<string> CompaniesWithAddresses { get => companiesWithAddresses; set => companiesWithAddresses = value; }
    }
}