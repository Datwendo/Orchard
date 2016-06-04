using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Contents.Settings;

namespace Orchard.Core.Contents.Drivers {
    public class ContentsDriver : ContentPartDriver<ContentPart> {
        protected override DriverResult Display(ContentPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Contents_Publish",
                             () => shapeHelper.Parts_Contents_Publish()),
                ContentShape("Parts_Contents_Publish_Summary",
                             () => shapeHelper.Parts_Contents_Publish_Summary()),
                // CS 28/5
                ContentShape("Parts_Contents_Publish_FrontAdminSummary",
                             () => shapeHelper.Parts_Contents_Publish_FrontAdminSummary()),
                // CS 28/5
                ContentShape("Parts_Contents_Clone_FrontAdminSummary",
                             () => shapeHelper.Parts_Contents_Clone_FrontAdminSummary()),
                ContentShape("Parts_Contents_Publish_SummaryAdmin",
                             () => shapeHelper.Parts_Contents_Publish_SummaryAdmin()),
                ContentShape("Parts_Contents_Clone_SummaryAdmin",
                             () => shapeHelper.Parts_Contents_Clone_SummaryAdmin())
                );
        }

        protected override DriverResult Editor(ContentPart part, dynamic shapeHelper) {
            var results = new List<DriverResult> { ContentShape("Content_SaveButton", saveButton => saveButton) };

            if (part.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                results.Add(ContentShape("Content_PublishButton", publishButton => publishButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ContentPart part, IUpdateModel updater, dynamic shapeHelper) {
            return Editor(part, updater);
        }
        // CS 26/5
        protected override DriverResult FrontEditor(ContentPart part, string editType, dynamic shapeHelper) {
            var results = new List<DriverResult> { ContentShape("Content_SaveButton_FrontEdit", saveButton => saveButton) };

            if (part.TypeDefinition.Settings.GetModel<ContentTypeSettings>().Draftable)
                results.Add(ContentShape("Content_PublishButton_FrontEdit", publishButton => publishButton));

            return Combined(results.ToArray());
        }
        // CS 26/5
        protected override DriverResult FrontEditor(ContentPart part, string editType, IUpdateModel updater, dynamic shapeHelper) {
            return FrontEditor(part, editType, updater);
        }
    }
}