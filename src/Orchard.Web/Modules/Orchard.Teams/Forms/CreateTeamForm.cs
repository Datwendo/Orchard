using System;
using Orchard.Environment.Extensions;
using Orchard.Forms.Services;

namespace Orchard.Teams.Forms {
    [OrchardFeature("Orchard.Teams.Workflows")]
    public class CreateTeamForm : Component, IFormProvider, IFormEventHandler {
        void IFormProvider.Describe(DescribeContext context) {
            context.Form("CreateTeam", factory => {
                var shape = (dynamic) factory;
                var form = shape.Form(
                    Id: "createTeam",
                    _TeamName: shape.Textbox(
                        Id: "teamName",
                        Name: "TeamName",
                        Title: T("Team Name"),
                        Description: T("The team name of the team to be created."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _Email: shape.Textbox(
                        Id: "email",
                        Name: "Email",
                        Title: T("Email"),
                        Description: T("The email address of the team to be created."),
                        Classes: new[] { "text", "large", "tokenized" }),
                    _User: shape.Textbox(
                            Id: "user",
                            Name: "User",
                            Title: T("User Name"),
                            Description: T("The name of the first user in the team."),
                            Classes: new[] { "text", "large", "tokenized" }));
            return form;
            });
        }

        void IFormEventHandler.Validating(ValidatingContext context) {
            if (context.FormName != "CreateTeam") return;

            var teamName = context.ValueProvider.GetValue("TeamName").AttemptedValue;
            var email = context.ValueProvider.GetValue("Email").AttemptedValue;
            var userName = context.ValueProvider.GetValue("User").AttemptedValue;

            if (String.IsNullOrWhiteSpace(teamName)) {
                context.ModelState.AddModelError("TeamName", T("You must specify a teamname or a token that evaluates to a teamname.").Text);
            }

            if (String.IsNullOrWhiteSpace(email)) {
                context.ModelState.AddModelError("Email", T("You must specify an email address or a token that evaluates to an email address.").Text);
            }            
            if (String.IsNullOrWhiteSpace(userName)) {
                context.ModelState.AddModelError("User", T("You must specify an usr name or a token that evaluates to an user name.").Text);
            }
        }

        void IFormEventHandler.Building(BuildingContext context) {}
        void IFormEventHandler.Built(BuildingContext context) {}
        void IFormEventHandler.Validated(ValidatingContext context) {}
    }
}