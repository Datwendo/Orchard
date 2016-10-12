using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Users.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.DynamicForms.Bindings {
    [OrchardFeature("Orchard.DynamicForms.Bindings.Users")]
    public class UserPartBindings : Component, IBindingProvider {
        private readonly IMembershipService _membershipService;
        public UserPartBindings(IEnumerable<IMembershipService> membershipServices) {
            _membershipService = membershipServices.Where(m => m.IsMain).First();
        }

        public void Describe(BindingDescribeContext context) {
            context.For<UserPart>()
                .Binding("UserName", (contentItem, part, s) => {
                    part.UserName = s;
                    part.NormalizedUserName = s.ToLowerInvariant();
                })
                .Binding("Email", (contentItem, part, s) => part.Email = s)
                .Binding("Password", (contentItem, part, s) => {
                    part.HashAlgorithm = "SHA1";
                    _membershipService.SetPassword(part, s);
                });
        }
    }
}