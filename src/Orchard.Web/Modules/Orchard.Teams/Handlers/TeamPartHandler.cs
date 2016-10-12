using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Teams.Models;

namespace Orchard.Teams.Handlers {
    public class TeamPartHandler : ContentHandler {
        public TeamPartHandler(IRepository<TeamPartRecord> repository) {
            Filters.Add(new ActivatingFilter<TeamPart>("Team"));
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var part = context.ContentItem.As<TeamPart>();

            if (part != null) {
                context.Metadata.Identity.Add("User.TeamName", part.TeamName);
                context.Metadata.DisplayText = part.TeamName;
            }
        }
    }
}