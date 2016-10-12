using Orchard.DynamicForms.Services;
using Orchard.DynamicForms.Services.Models;
using Orchard.Environment.Extensions;
using Orchard.Security;
using Orchard.Teams.Models;

namespace Orchard.DynamicForms.Bindings {
    [OrchardFeature("Orchard.DynamicForms.Bindings.Users")]
    public class TeamPartBindings : Component, IBindingProvider {
        public TeamPartBindings() {
        }

        public void Describe(BindingDescribeContext context) {
            context.For<TeamPart>()
                .Binding("TeamName", (contentItem, part, s) => {
                    part.TeamName = s;
                    part.NormalizedTeamName = s.ToLowerInvariant();
                })
                .Binding("Email", (contentItem, part, s) => part.Email = s);
        }
    }
}