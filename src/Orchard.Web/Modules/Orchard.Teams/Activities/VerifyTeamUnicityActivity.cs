using System.Collections.Generic;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Teams.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Teams.Activities {
    [OrchardFeature("Orchard.Teams.Workflows")]
    public class VerifyTeamUnicityActivity : Task {
        private readonly ITeamService _teamService;

        public VerifyTeamUnicityActivity(ITeamService teamService) {
            _teamService = teamService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override string Name {
            get { return "VerifyTeamUnicity"; }
        }

        public override LocalizedString Category {
            get { return T("Team"); }
        }

        public override LocalizedString Description {
            get { return T("Verifies if the specified team name and email address are unique."); }
        }

        public override string Form {
            get { return "VerifyTeamUnicity"; }
        }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] {
                T("Unique"), 
                T("NotUnique")
            };
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var teamName = activityContext.GetState<string>("TeamName");
            var email = activityContext.GetState<string>("Email");

            if (_teamService.VerifyTeamUnicity(teamName, email)) {
                yield return T("Unique");
            }
            else {
                yield return T("NotUnique");
            }
        }
    }
}