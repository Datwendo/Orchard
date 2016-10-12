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

namespace Orchard.Teams.Drivers {
    public class UserTeamsPartDriver : ContentPartDriver<UserTeamsPart> {
        private readonly IRepository<TeamMembersPartRecord> _teamUsersRepository;
        private readonly INotifier _notifier;
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private const string TemplateName = "Parts/Users.Teams";
        private readonly ITeamService _teamService;
        private readonly ITeamEventHandler _teamEventHandlers;
        private readonly IOrchardServices _orchardServices;

        public UserTeamsPartDriver(
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
                return "Teams";
            }
        }

        public Localizer T { get; set; }

        protected override DriverResult Editor(UserTeamsPart userTeamsPart, dynamic shapeHelper) {
            // don't show editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.ManageUsers, _authenticationService.GetAuthenticatedUser(), userTeamsPart))
                return null;

            return ContentShape("Parts_Users_Teams_Edit",
                    () => {
                       var teams = _orchardServices.ContentManager.Query<TeamPart, TeamPartRecord> ()
                                                        .Where (t => t.Id != userTeamsPart.Id)
                                                        .List ()
                                                        .Select(u => u.As<IUser>())
                                                        .Select(x => new UserTeamEntry {
                                                                        Id = x.Id,
                                                                        Name = x.UserName,
                                                                        Checked = userTeamsPart.Teams.Contains(x.Id)
                                                                        });
                       var model = new UserTeamsViewModel {
                           User = userTeamsPart,
                           Teams = teams.ToList(),
                       };
                       return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix);
                    });
        }

        protected override DriverResult Editor(UserTeamsPart userTeamsPart, IUpdateModel updater, dynamic shapeHelper) {
            // don't apply editor without apply roles permission
            if (!_authorizationService.TryCheckAccess(Permissions.ManageUsers, _authenticationService.GetAuthenticatedUser(), userTeamsPart))
                return null;

            var model = BuildEditorViewModel(userTeamsPart);
            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                var iuser = userTeamsPart.ContentItem.As<IUser>();
                var targetUserTeams = model.Teams.Where(x => x.Checked).Select(x => x.Id);
                _teamService.UpdateIUser(iuser.Id, targetUserTeams);
            }
            return Editor(userTeamsPart, shapeHelper);
        }

        private UserTeamsViewModel BuildEditorViewModel(UserTeamsPart userTeamsPart) {
            return new UserTeamsViewModel { User = userTeamsPart };
        }

        protected override void Importing(UserTeamsPart part, ContentManagement.Handlers.ImportContentContext context) {
            // Don't do anything if the tag is not specified.
            if (context.Data.Element(part.PartDefinition.Name) == null) {
                return;
            }

            context.ImportAttribute(part.PartDefinition.Name, "UserTeams", ts => {

                var TeamUsers = ts.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
               
            });
        }

        protected override void Exporting(UserTeamsPart part, ContentManagement.Handlers.ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("UserTeams", string.Join(",", part.Teams));
        }
    }
}