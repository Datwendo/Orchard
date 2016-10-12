using System;
using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Teams.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using Orchard.Users.Services;
using Orchard.ContentManagement;
using Orchard.Users.Models;
using Orchard.Teams.Models;
using System.Linq;

namespace Orchard.Teams.Activities {
    [OrchardFeature("Orchard.Teams.Workflows")]
    public class CreateteamActivity : Task {
        private readonly ITeamService _teamService;
        private readonly IContentManager _contentManager;

        public CreateteamActivity(ITeamService teamService, IContentManager contentManager) {
            _contentManager = contentManager;
            _teamService = teamService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "CreateTeam"; }
        }

        public override LocalizedString Category {
            get { return T("Team"); }
        }

        public override LocalizedString Description {
            get { return T("Creates a new Team based on the specified values."); }
        }

        public override string Form {
            get { return "CreateTeam"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("InvalidTeamNameOrEmail"),
                T("teamNameOrEmailNotUnique"),
                T("NotValidUserName"),
                T("Done")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var teamName = activityContext.GetState<string>("TeamName");
            var email = activityContext.GetState<string>("Email");
            var userName = activityContext.GetState<string>("User");


            if (String.IsNullOrWhiteSpace(teamName) || String.IsNullOrWhiteSpace(email)) {
                yield return T("InvalidTeamNameOrEmail");
                yield break;
            }

            if (!_teamService.VerifyTeamUnicity(teamName, email)) {
                yield return T("TeamNameOrEmailNotUnique");
                yield break;
            }
            if (string.IsNullOrWhiteSpace(userName)) {
                yield return T("NotValidUserName");
                yield break;
            }
            var query = _contentManager.Query(new string[] { "User", "Team" })
                .Join<UserPartRecord>().Where(u => u.UserName == userName)
                .Join<TeamPartRecord>().Where(u => u.TeamName == userName);
            var user = query.List().FirstOrDefault();
            if (user == null) {
                yield return T("NotValidUserName");
                yield break;
            }

            var team = _teamService.CreateTeam(teamName,email,user.As<IUser>());

            workflowContext.Content = team;

            yield return T("Done");
        }
    }
}