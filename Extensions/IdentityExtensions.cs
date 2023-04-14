using System.Security.Claims;
using System.Security.Principal;

namespace BeastieHunter.Extensions
{
    public static class IdentityExtensions
    {
        public static int? GetCompanyId(this IIdentity identity)
        {
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId");

            int result;
            if( claim != null)
            {
                result = int.Parse(claim.Value);
            }
            else
            {
                result = 0;
            }

            return result;
        }
    }
}
