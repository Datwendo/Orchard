using System;
using System.Web.Security;
using Orchard.ContentManagement.Drivers;
using Orchard.Teams.Models;

namespace Orchard.Teams.Drivers {
    /// <summary>
    /// This class intentionnaly has no Display method to prevent external access to this information through standard 
    /// Content Item display methods.
    /// </summary>
    public class TeamPartDriver : ContentPartDriver<TeamPart> {

        protected override void Importing(TeamPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            part.TeamName = context.Attribute(part.PartDefinition.Name, "TeamName");
        }

        protected override void Exporting(TeamPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("TeamName", part.TeamName);
        }
    }
}