using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using Orchard.Mvc.Filters;
using System;

namespace Orchard.Users.Services {

    /// <summary>
    /// This class is responsible for redirecting the user to the authentication page 
    /// of the current tenant.
    /// </summary>
    public class AuthenticationRedirectionFilter : FilterProvider, IAuthenticationFilter {

        public void OnAuthentication(AuthenticationContext filterContext) {
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext) {
            if (filterContext.Result is HttpUnauthorizedResult) {
                // CS 19/6
                var statusDescription = ((HttpUnauthorizedResult)filterContext.Result).StatusDescription;
                if (!string.IsNullOrEmpty(statusDescription)) {
                    if ( string.Equals(statusDescription, "Not Member", StringComparison.InvariantCultureIgnoreCase) ||
                        string.Equals(statusDescription, "Not Customer", StringComparison.InvariantCultureIgnoreCase))
                        return;
                }
                filterContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                {
                    { "controller", "Account" },
                    { "action", "AccessDenied" },
                    { "area", "Orchard.Users"},
                    { "ReturnUrl", filterContext.HttpContext.Request.RawUrl }
                });
            }
        }
    }
}