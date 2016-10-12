using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.UI.Notify;
using Orchard.Teams.Models;
using Orchard.Teams.Services;
using Orchard.Teams.ViewModels;
using Orchard.Mvc.Extensions;
using Orchard.Settings;
using Orchard.UI.Navigation;
using Orchard.Users;
using Orchard.Users.Events;
using Orchard.Users.Models;

namespace Orchard.Teams.Controllers {
    [ValidateInput(false)]
    public class AdminController : Controller, IUpdateModel {
        private readonly IMembershipService _membershipService;
        private readonly ITeamService _teamService;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly ISiteService _siteService;
        private readonly IOrchardServices _orchardServices;

        public AdminController(
            IOrchardServices orchardServices,
            IMembershipService membershipService,
            ITeamService teamService,
            IShapeFactory shapeFactory,
            IUserEventHandler userEventHandlers,
            ISiteService siteService) {
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _teamService = teamService;
            _userEventHandlers = userEventHandlers;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Index(TeamIndexOptions options, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to list Teams")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            // default options
            if (options == null)
                options = new TeamIndexOptions();

            var Teams = _orchardServices.ContentManager
                .Query<TeamPart, TeamPartRecord>();

            if(!string.IsNullOrWhiteSpace(options.Search)) {
                Teams = Teams.Where(u => u.TeamName.Contains(options.Search) || u.Email.Contains(options.Search));
            }

            var pagerShape = Shape.Pager(pager).TotalItemCount(Teams.Count());

            switch (options.Order) {
                case TeamsOrder.Name:
                    Teams = Teams.OrderBy(u => u.TeamName);
                    break;
                case TeamsOrder.Email:
                    Teams = Teams.OrderBy(u => u.Email);
                    break;
                case TeamsOrder.CreatedUtc:
                    Teams = Teams.OrderBy(u => u.CreatedUtc);
                    break;
                case TeamsOrder.LastLoginUtc:
                    Teams = Teams.OrderBy(u => u.LastLoginUtc);
                    break;
            }

            var results = Teams
                .Slice(pager.GetStartIndex(), pager.PageSize)
                .ToList();

            var model = new TeamsIndexViewModel {
                Teams = results
                    .Select(x => new TeamEntry { Team = x.Record ,
                    Teams = x.As<IUser>().Teams.Select(tId => _orchardServices.ContentManager.Get<IUser>(tId).UserName)
                    })
                    .ToList(),
                    Options = options,
                    Pager = pagerShape
            };

            // maintain previous route data when generating page links
            var routeData = new RouteData();
            routeData.Values.Add("Options.Search", options.Search);
            routeData.Values.Add("Options.Order", options.Order);

            pagerShape.RouteData(routeData);
            
            return View(model);
        }

        [HttpPost]
        [FormValueRequired("submit.BulkEdit")]
        public ActionResult Index(FormCollection input) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            var viewModel = new TeamsIndexViewModel {Teams = new List<TeamEntry>(), Options = new TeamIndexOptions()};
            UpdateModel(viewModel);

            var checkedEntries = viewModel.Teams.Where(c => c.IsChecked);
            switch (viewModel.Options.BulkAction) {
                case TeamsBulkAction.None:
                    break;
                case TeamsBulkAction.Delete:
                    foreach (var entry in checkedEntries) {
                        Delete(entry.Team.Id);
                    }
                    break;
            }

            return RedirectToAction("Index", ControllerContext.RouteData.Values);
        }

        public ActionResult Create() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            var Team = _orchardServices.ContentManager.New<IUser>("Team");
            var editor = Shape.EditorTemplate(TemplateName: "Parts/Team.Create", Model: new TeamCreateViewModel(), Prefix: null);
            editor.Metadata.Position = "2";
            var model = _orchardServices.ContentManager.BuildEditor(Team);
            model.Content.Add(editor);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(TeamCreateViewModel createModel) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            if (!string.IsNullOrEmpty(createModel.TeamName)) {
                if (!_teamService.VerifyTeamUnicity(createModel.TeamName, createModel.Email)) {
                    AddModelError("NotUniqueTeamName", T("Team with that Teamname and/or email already exists."));
                }
            }
            
            if (!Regex.IsMatch(createModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                ModelState.AddModelError("Email", T("You must specify a valid email address."));
            }

            var userName = createModel.UserName;

            if (string.IsNullOrWhiteSpace(userName)) {
                ModelState.AddModelError("User", T("You must specify a valid user name."));
            }
            var query = _orchardServices.ContentManager.Query(new string[] { "User", "Team" })
                .List()
                .Select(c => c.WithIUser())
                .Where(u => u.UserName.ToLowerInvariant() == userName.ToLowerInvariant());
            var user = query.FirstOrDefault();
            if (user == null ) {
                ModelState.AddModelError("User", T("You must specify a valid user name."));
            }

            IUser Team = null;
            if (ModelState.IsValid) {
                Team = _teamService.CreateTeam(createModel.TeamName,
                                                  createModel.Email, user);
            }
            /*
            if (Team == null) {
                _orchardServices.TransactionManager.Cancel();
                var editor = Shape.EditorTemplate(TemplateName: "Parts/Team.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                var nmodel = _orchardServices.ContentManager.BuildEditor(Team);
                nmodel.Content.Add(editor);
                ModelState.AddModelError("Team", T("Error creating this team."));
                return View(nmodel);
            }
            */
            var model = _orchardServices.ContentManager.UpdateEditor(Team, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Team.Create", Model: createModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);
                return View(model);
            }
            _orchardServices.Notifier.Success(T("Team created"));
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            var Team = _orchardServices.ContentManager.Get<TeamPart>(id);

            if (Team == null)
                return HttpNotFound();

            var editor = Shape.EditorTemplate(TemplateName: "Parts/Team.Edit", Model: new TeamEditViewModel {Team = Team}, Prefix: null);
            editor.Metadata.Position = "2";
            var model = _orchardServices.ContentManager.BuildEditor(Team);
            model.Content.Add(editor);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        public ActionResult EditPOST(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            var Team = _orchardServices.ContentManager.Get<TeamPart>(id, VersionOptions.DraftRequired);

            if (Team == null)
                return HttpNotFound();

            string previousName = Team.TeamName;

            var model = _orchardServices.ContentManager.UpdateEditor(Team, this);

            var editModel = new TeamEditViewModel { Team = Team };
            if (TryUpdateModel(editModel)) {
                if (!_teamService.VerifyTeamUnicity(id, editModel.TeamName, editModel.Email)) {
                    AddModelError("NotUniqueTeamName", T("Team with that Teamname and/or email already exists."));
                }
                else if (!Regex.IsMatch(editModel.Email ?? "", UserPart.EmailPattern, RegexOptions.IgnoreCase)) {
                    // http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx    
                    ModelState.AddModelError("Email", T("You must specify a valid email address."));
                }
                else {

                    Team.NormalizedTeamName = editModel.TeamName.ToLowerInvariant();
                }
            }

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();

                var editor = Shape.EditorTemplate(TemplateName: "Parts/Team.Edit", Model: editModel, Prefix: null);
                editor.Metadata.Position = "2";
                model.Content.Add(editor);

                return View(model);
            }

            _orchardServices.ContentManager.Publish(Team.ContentItem);

            _orchardServices.Notifier.Success(T("Team information updated"));
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageUsers, T("Not authorized to manage Teams")))
                return new HttpUnauthorizedResult();

            var Team = _orchardServices.ContentManager.Get<IUser>(id);

            if (Team == null)
                return HttpNotFound();

            if (_teamService.DeleteTeam(Team.Id,_orchardServices.WorkContext.CurrentUser)) {
                _orchardServices.Notifier.Success(T("Team {0} deleted", Team.ContentItem.As<TeamPart>().TeamName));
            }
            else {
                _orchardServices.Notifier.Error(T("Can't delete Team {0}", Team.ContentItem.As<TeamPart>().TeamName));
            }
            return RedirectToAction("Index");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        public void AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }

}
