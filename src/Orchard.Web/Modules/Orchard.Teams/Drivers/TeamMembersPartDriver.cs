using System;
using System.Linq;
using Orchard.ContentManagement.Drivers;
using Orchard.Data;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Teams.Events;
using Orchard.Teams.Models;
using Orchard.Teams.Services;
using Orchard.Teams.ViewModels;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Users;
using Orchard.ContentManagement.Records;

namespace Orchard.Teams.Drivers {
    public class TeamMembersPartDriver : ContentPartDriver<TeamMembersPart> {
        private readonly IRepository<TeamMembersPartRecord> _teamUsersRepository;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private const string TemplateName = "Parts/Teams.TeamMembers";
        private readonly ITeamService _teamService;
        private readonly ITeamEventHandler _teamEventHandlers;
        private readonly IOrchardServices _orchardServices;

        public TeamMembersPartDriver(
            IRepository<TeamMembersPartRecord> teamUsersRepository,
            ITeamService teamService,
            IOrchardServices orchardServices,
            INotifier notifier,
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            ITeamEventHandler teamEventHandlers) {
            _orchardServices = orchardServices;
            _teamEventHandlers = teamEventHandlers;
            _teamService = teamService;
            _teamUsersRepository = teamUsersRepository;
            _notifier = notifier;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            T = NullLocalizer.Instance;
        }

        protected override string Prefix {
            get {
                return "TeamMembers";
            }
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(TeamMembersPart teamMembersPart, dynamic shapeHelper) {
            // don't show editor without apply roles permission
            if(!_authorizationService.TryCheckAccess (Permissions.ManageUsers, _authenticationService.GetAuthenticatedUser (), teamMembersPart))
                return null;

            return ContentShape ("Parts_Teams_TeamMembers_Edit",
                    () => {
                        var users = _orchardServices.ContentManager.Query(new String[] { "User" }).List ();
                        var teams = _orchardServices.ContentManager.Query< TeamPart, TeamPartRecord>()
                                 .Where(t => t.Id != teamMembersPart.Id).List();

                        var members = users.Select (u => u.As<IUser> ())
                                                         .Select (x => new TeamMemberEntry {
                                                             Id = x.Id,
                                                             Name = x.UserName,
                                                             TeamMemberType = x.TeamMemberType,
                                                             Checked = teamMembersPart.Members.Select (m => m.Item2).Contains (x.Id)
                                                         })
                                                                         .Union (teams.Select (u => u.As<IUser> ())
                                                                             .Select (x => new TeamMemberEntry {
                                                                                 Id = x.Id,
                                                                                 Name = x.UserName,
                                                                                 TeamMemberType = x.TeamMemberType,
                                                                                 Checked = teamMembersPart.Members.Select (m => m.Item2).Contains (x.Id)
                                                                             }));
                        var model = new TeamMembersViewModel {
                            Team = teamMembersPart,
                            Members = members.ToList (),
                        };
                        return shapeHelper.EditorTemplate (TemplateName: TemplateName, Model: model, Prefix: Prefix);
                    });
        }

        protected override DriverResult Editor(TeamMembersPart teamMembersPart, IUpdateModel updater, dynamic shapeHelper) {
            // don't apply editor without apply roles permission
            if(!_authorizationService.TryCheckAccess (Permissions.ManageUsers, _authenticationService.GetAuthenticatedUser (), teamMembersPart))
                return null;

            var model = BuildEditorViewModel (teamMembersPart);
            if(updater.TryUpdateModel (model, Prefix, null, null)) {
                var teamPart = teamMembersPart.ContentItem.As<TeamPart> ();
                var targetTeamUsers = model.Members.Where (x => x.Checked).Select (x => Tuple.Create (x.TeamMemberType, x.Id));
                _teamService.UpdateTeam (teamMembersPart.Id, teamPart.TeamName, targetTeamUsers);
            }
            return Editor (teamMembersPart, shapeHelper);
        }

        private TeamMembersViewModel BuildEditorViewModel(TeamMembersPart TeamMembersPart) {
            return new TeamMembersViewModel { Team = TeamMembersPart };
        }

        protected override void Importing(TeamMembersPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if(context.Data.Element (part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute (part.PartDefinition.Name, "TeamUsers", ts => {

                var TeamUsers = ts.Split (new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                /* create new users
                foreach (var userName in TeamUsers) {
                    var teamPart = _teamService.GetTeamByName(teamName);

                    // create the role if it doesn't already exist
                    if (teamPart == null) {
                        _teamService.CreateTeam(teamName,string.Empty);
                    }
                }
                
                var currentUserRoleRecords = _teamUsersRepository.Fetch(x => x.UserId == part.ContentItem.Id).ToList();
                var currentRoleRecords = currentUserRoleRecords.Select(x => x.Role).ToList();
                var targetRoleRecords = TeamUsers.Select(x => _teamService.GetRoleByName(x)).ToList();
                foreach (var addingRole in targetRoleRecords.Where(x => !currentRoleRecords.Contains(x))) {
                    _teamUsersRepository.Create(new TeamUsersPartRecord { UserId = part.ContentItem.Id, Role = addingRole });
                }
                */
            });
        }

        protected override void Exporting(TeamMembersPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element (part.PartDefinition.Name).SetAttributeValue ("TeamUsers", string.Join (",", part.Members));
        }
    }
}