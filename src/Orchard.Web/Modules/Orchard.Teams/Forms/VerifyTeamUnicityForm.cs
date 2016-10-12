using System;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;

namespace Orchard.Teams.Forms {
    [OrchardFeature("Orchard.Teams.Workflows")]
    public class VerifyTeamUnicityForm : Component, IFormProvider, IFormEventHandler {
        void IFormProvider.Describe(DescribeContext context) {
            context.Form("VerifyTeamUnicity", factory => {
                var shape = (dynamic) factory;
                var form = shape.Form(
                    Id: "verifyTeamUnicity",
                    _UserName: shape.Textbox(
                        Id: "teamName",
                        Name: "TeamName",
                        Title: T("Team Name"),
                        Description: T("The team name to be validated."),
                        Classes: new[]{"text", "large", "tokenized"}),
                    _Email: shape.Textbox(
                        Id: "email",
                        Name: "Email",
                        Title: T("Email"),
                        Description: T("The email address to be validated."),
                        Classes: new[] { "text", "large", "tokenized" }));

                return form;
            });
        }

        void IFormEventHandler.Validating(ValidatingContext context) {
            if (context.FormName != "VerifyTeamUnicity") return;

            var teamName = context.ValueProvider.GetValue("TeamName").AttemptedValue;
            var email = context.ValueProvider.GetValue("Email").AttemptedValue;

            if (String.IsNullOrWhiteSpace(teamName)) {
                context.ModelState.AddModelError("TeamName", T("You must specify a teamname or a token that evaluates to a teamname.").Text);
            }

            if (String.IsNullOrWhiteSpace(email)) {
                context.ModelState.AddModelError("Email", T("You must specify an email address or a token that evaluates to an email address.").Text);
            }
        }

        void IFormEventHandler.Building(BuildingContext context) {}
        void IFormEventHandler.Built(BuildingContext context) {}
        void IFormEventHandler.Validated(ValidatingContext context) {}
    }
}