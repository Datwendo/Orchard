using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.Teams.Models;

namespace Orchard.Teams.Services {
    public class TeamResolverSelector : IIdentityResolverSelector {
        private readonly IContentManager _contentManager;

        public TeamResolverSelector(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IdentityResolverSelectorResult GetResolver(ContentIdentity contentIdentity) {
            if (contentIdentity.Has("Team.TeamName")) {
                return new IdentityResolverSelectorResult {
                    Priority = 0,
                    Resolve = ResolveIdentity
                };
            }

            return null;
        }

        private IEnumerable<ContentItem> ResolveIdentity(ContentIdentity identity) {
            var identifier = identity.Get("Team.TeamName");

            if (identifier == null) {
                return null;
            }

            return _contentManager
                .Query<TeamPart, TeamPartRecord>(VersionOptions.Latest)
                .Where(p => p.TeamName == identifier)
                .List<ContentItem>()
                .Where(c => ContentIdentity.ContentIdentityEqualityComparer.AreEquivalent(
                    identity, _contentManager.GetItemMetadata(c).Identity));
        }
    }
}