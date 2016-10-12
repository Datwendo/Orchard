using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.ContentManagement;
using Orchard.Teams.Models;
using Orchard.Security;
using Orchard.Data;
using Orchard.Teams.Events;
using Orchard.Users.Models;
using Orchard.Users.Events;
using Orchard.Caching;
using System.Collections;
using Orchard.Core.Common.Models;

namespace Orchard.Teams.Services {
    public class TeamService : ITeamService {
        private const string SignalName = "Orchard.Teams.Services.TeamService";

        private readonly IContentManager _contentManager;
        private readonly IRepository<TeamMembersPartRecord> _teamUsersRepository;
        private readonly ITeamEventHandler _teamEventHandlers;
        private readonly IUserEventHandler _userEventHandlers;
        private readonly ISignals _signals;
        private readonly ICacheManager _cacheManager;

        public TeamService(IContentManager contentManager
                            , ISignals signals
                            , IRepository<TeamMembersPartRecord> teamUsersRepository
                            , ITeamEventHandler teamEventHandlers
                            , IUserEventHandler userEventHandlers
                            , ICacheManager cacheManager
                            ) {
            _cacheManager = cacheManager;
            _signals = signals;
            _teamEventHandlers = teamEventHandlers;
            _userEventHandlers = userEventHandlers;
            _contentManager = contentManager;
            _teamUsersRepository = teamUsersRepository;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }
        public IUser CreateTeam(string teamName, string email, string firstUserName) {
            if(string.IsNullOrWhiteSpace(teamName)) {
                Logger.Debug("Can't create Team with empty name");
                return null;
            }
            if(string.IsNullOrWhiteSpace(firstUserName)) {
                Logger.Debug("Can't create Team '{0}' with email'{1}' username empty", teamName, email);
                return null;
            }
            var query = _contentManager.Query(new string[] { "User" })
                .Join<UserPartRecord>().Where(u => u.UserName == firstUserName);
            var user = query.List().FirstOrDefault();
            if ( user == null) {
                var query2 = _contentManager.Query(new string[] {"Team" })
                        .Join<TeamPartRecord>().Where(u => u.TeamName == firstUserName);
                user = query2.List().FirstOrDefault();
            }
            if(user == null || user.As<IUser>() == null) {
                Logger.Debug("Can't create Team '{0}' with email'{1}' user not found", teamName, email);
                return null;
            }

            return CreateTeam(teamName, email, user.As<IUser>());
        }

        public IUser CreateTeam(string teamName, string email, IUser firstUser) {

            if(!VerifyTeamUnicity(teamName, email)) {
                Logger.Debug("Can't create Team '{0}' with email'{1}' teamName/Email not unique", teamName, email);
                return null;
            }
            var teamParameters = new CreateTeamParams(teamName, email, firstUser);
            TeamCreateContext tcc = new TeamCreateContext { Team = null, Cancel = false, TeamParameters = teamParameters };
            _teamEventHandlers.Creating(tcc);
            if(tcc.Cancel) {
                Logger.Debug("Can't create Team '{0}' with email'{1}' and firstUser '{2}'", teamName, email, firstUser.UserName);
                return null;
            }
            var ucp = new CreateUserParams(teamName, null, email, null, null, false, firstUser.UserName);
            CreateUserContext ucc = new CreateUserContext { User = null, Cancel = false, UserParameters = ucp };
            _userEventHandlers.Creating(ucc);
            if(ucc.Cancel) {
                Logger.Debug("Can't create User '{0}' with email'{1}'", teamName, email);
                return null;
            }
            var item = _contentManager.New<IUser>("Team");
            var teamPart = item.ContentItem.As<TeamPart>();
            teamPart.TeamName = teamName;
            teamPart.NormalizedTeamName = teamName.ToLowerInvariant();
            teamPart.Email = email;
            _contentManager.Create(item);
            _teamEventHandlers.Created(tcc);
            _userEventHandlers.Created(ucc);
            AddUser2Team(item.Id, firstUser);
            return item;
        }

        public bool VerifyTeamUnicity(string teamName) {
            string normalizedteamName = teamName.ToLowerInvariant();

            if(_contentManager.Query<TeamPart, TeamPartRecord>()
                                   .Where(team =>
                                          team.NormalizedTeamName == normalizedteamName)
                                   .List().Any()) {
                return false;
            }
            return true;
        }
        public bool DeleteTeam(int id, IUser newOwner) {

            var teamPart = GetTeam(id);
            var teamUsers = teamPart.ContentItem.As<TeamMembersPart>();
            var removedUsers = teamUsers.Members.Select(m => m.Item2).Select(uId => _contentManager.Get<IUser>(uId));
            
            var teamParameters = new RemoveTeamParams { Users = removedUsers };
            TeamRemoveContext trc = new TeamRemoveContext { Team = teamPart, Cancel = false, TeamParameters = teamParameters };
            _teamEventHandlers.Removing(trc);
            if(trc.Cancel) {
                Logger.Debug("Can't delete Team '{0}' with email'{1}'", teamPart.TeamName, teamPart.Email);
                return false;
            }
            var userParameters = new RemoveUserParams(teamPart, teamPart.TeamName, teamPart.Email);
            RemoveUserContext urc = new RemoveUserContext { Cancel = false, User = teamPart, UserParameters = userParameters };
            _userEventHandlers.Removing(urc);
            if(urc.Cancel) {
                Logger.Debug("Can't delete Team as User '{0}' with email'{1}'", teamPart.TeamName, teamPart.Email);
                return false;
            }
            // Check contents with this team as owner 
            var ownedContent = _contentManager.Query<CommonPart, CommonPartRecord>().Where(c => c.OwnerId == id).List();
            if(ownedContent.Any()) {
                if(newOwner == null) {
                    Logger.Debug("Can't delete Team '{0}', {1} contentitems have it as owner", teamPart.TeamName, ownedContent.Count());
                    return false;
                }
                foreach(var oc in ownedContent) {
                    oc.Owner = newOwner;
                    Logger.Debug("Owner '{0}' changed for {1}", oc.Id,newOwner.UserName);
                }
            }
            foreach(int userId in teamUsers.Members.Select(m => m.Item2)) {
                var removeduser = _contentManager.Get<IUser>(userId);
                var truc = new UserRemoveContext {
                    Team = teamPart,
                    User = removeduser,
                    Cancel = false
                };
                _teamEventHandlers.UserRemoving(truc);
                if(!truc.Cancel) {
                    var tuRecs = _teamUsersRepository.Fetch(x => x.TeamId == teamPart.Id && x.UserId == userId);
                    TeamUserParams tup = new TeamUserParams(teamPart, removeduser.UserName, removeduser.Email);
                    var tuc = new TeamUserContext {
                        UserParameters = tup,
                        User = removeduser,
                        Cancel = false
                    };
                    _userEventHandlers.RemovingFromTeam(tuc);
                    if(!tuc.Cancel) {
                        foreach(var tuRec in tuRecs)
                            _teamUsersRepository.Delete(tuRec);
                        _teamEventHandlers.UserRemoved(truc);
                        _userEventHandlers.RemovedFromTeam(tuc);
                        Logger.Debug("User '{0}' removed from team '{1}'", removeduser.UserName, teamPart.TeamName);
                    }
                    else {
                        Logger.Debug("User handler Can't remove user '{0}' from team '{1}'", removeduser.UserName, teamPart.TeamName);
                        return false;
                    }
                }
                else {
                    Logger.Debug("Can't remove user '{0}' from team '{1}'", removeduser.UserName, teamPart.TeamName);
                    return false;
                }
            }
            _contentManager.Remove(teamPart.ContentItem);
            _teamEventHandlers.Removed(trc);
            _userEventHandlers.Removed(urc);
            Logger.Debug("Team '{0}' Deleted", teamPart.TeamName);
            TriggerSignal();
            return true;
        }
        public bool VerifyTeamUnicity(string teamName, string email) {
            string normalizedteamName = teamName.ToLowerInvariant();
            if ( !string.IsNullOrWhiteSpace(email) &&_contentManager.Query<TeamPart, TeamPartRecord>()
                                   .Where(team =>
                                          team.NormalizedTeamName == normalizedteamName || team.Email == email)
                                   .List().Any()) {
                return false;
            }
            if(string.IsNullOrWhiteSpace(email) && _contentManager.Query<TeamPart, TeamPartRecord>()
                                   .Where(team =>
                                          team.NormalizedTeamName == normalizedteamName)
                                   .List().Any()) {
                return false;
            }
            return true;
        }


        public bool VerifyTeamUnicity(int id, string teamName, string email = null) {
            string normalizedteamName = teamName.ToLowerInvariant();

            if(_contentManager.Query<TeamPart, TeamPartRecord>()
                                   .Where(team =>
                                          team.NormalizedTeamName == normalizedteamName && (email != null && team.Email == email))
                                   .List().Any(team => team.Id != id)) {
                return false;
            }

            return true;
        }


        public void AddUser2Team(int teamId, IUser user) {
            if(_teamUsersRepository.Get(x => x.TeamId == teamId && x.UserId == user.Id) == null) {
                var tuRec = new TeamMembersPartRecord {
                    TeamId = teamId,
                    UserId = user.Id
                };
                var teamPart = GetTeam(teamId);
                var uac = new UserAddContext { Team = teamPart, User = user, TeamUserId = tuRec.Id, Cancel = false };
                _teamEventHandlers.UserAdding(uac);
                if(!uac.Cancel) {
                    _teamUsersRepository.Create(tuRec);
                    uac.TeamUserId = tuRec.Id;
                    _teamEventHandlers.UserAdded(uac);
                    TriggerSignal();
                    Logger.Debug("User '{0}' added to team '{1}'", user.UserName, teamPart.TeamName);
                }
                else {
                    Logger.Debug("Can't add user '{0}' to team '{1}'", user.UserName, teamPart.TeamName);
                }
            }
        }

        public TeamPart GetTeam(int id) {
            return _contentManager.Get<TeamPart>(id);
        }
        public TeamPart GetTeamByName(string teamName) {
            var normalizedTeamName = teamName;
            return _contentManager.Query<TeamPart, TeamPartRecord>().Where(t => t.NormalizedTeamName == normalizedTeamName).List().SingleOrDefault();
        }
        public TeamMembersPart GetTeamUsers(int id) {
            return _contentManager.Get<TeamMembersPart>(id);
        }
        public bool RemoveFromTeam(int id, IEnumerable<Tuple<TeamMemberType, int>> users2Rmove) {
            if(!users2Rmove.Any()) {
                Logger.Debug("Can't update a team with an empty users list");
                return false;
            }
            var teamPart = GetTeam(id);
            var teamUsers = teamPart.ContentItem.As<TeamMembersPart>();
            IEnumerable<Tuple<TeamMemberType, int>> users = teamUsers.Members.Where( m => !users2Rmove.Contains(m)).ToList();
            return UpdateTeam(id, null, users);
        }

        public bool UpdateTeam(int id, string teamName, IEnumerable<Tuple<TeamMemberType, int>> users) {
            if(!users.Any()) {
                Logger.Debug("Can't update a team with an empty users list '{0}'", teamName);
                return false;
            }
            bool ret = true;
            var teamPart = GetTeam(id);
            var teamUsers = teamPart.ContentItem.As<TeamMembersPart>();
            // Update team name
            if(!string.IsNullOrWhiteSpace(teamName) && !string.Equals(teamPart.TeamName, teamName)) {
                teamPart.TeamName = teamName;
            }

            var currentusers = new List<Tuple<TeamMemberType, int>>(teamUsers.Members);
            teamUsers.Members.Clear();
            foreach(var userId in users) {
                var addeduser = _contentManager.Get<IUser>(userId.Item2);
                if(!currentusers.Contains(userId)) {
                    var tucp = new TeamUserParams(teamPart, addeduser.UserName, addeduser.Email);
                    var uc = new TeamUserContext {
                        UserParameters = tucp,
                        User = addeduser,
                        Cancel = false
                    };
                    _userEventHandlers.Adding2Team(uc);
                    bool added = false;
                    if(!uc.Cancel) {
                        var tuRec = new TeamMembersPartRecord {
                            TeamId = teamPart.Id,
                            TeamMemberType = (int)userId.Item1,
                            UserId = userId.Item2
                        };
                        var tuc = new UserAddContext { Team = teamPart, User = addeduser, TeamUserId = tuRec.Id, Cancel = false };
                        _teamEventHandlers.UserAdding(tuc);
                        if(!tuc.Cancel) {
                            _teamUsersRepository.Create(tuRec);
                            _teamEventHandlers.UserAdded(tuc);
                            _userEventHandlers.Added2Team(uc);
                            Logger.Debug("User '{0}' added to team '{1}'", addeduser.UserName, teamPart.TeamName);
                            added = true;
                        }
                    }
                    if(!added) {
                        Logger.Debug("Can't add user '{0}' to team '{1}'", addeduser.UserName, teamPart.TeamName);
                        ret = false;
                        continue;
                    }
                }
                teamUsers.Members.Add(userId);
            }
            foreach(var member in currentusers) {
                if(!teamUsers.Members.Contains(member)) {
                    bool removed = false;
                    bool couldRemove = false;
                    UserRemoveContext truc = null;
                    TeamUserContext urc = null;
                    var removeduser = _contentManager.Get<IUser>(member.Item2);
                    if(removeduser != null) {
                        truc = new UserRemoveContext {
                            Team = teamPart,
                            User = removeduser,
                            Cancel = false
                        };
                        _teamEventHandlers.UserRemoving(truc);
                        if(!truc.Cancel) {
                            var urcp = new TeamUserParams(teamPart, removeduser.UserName, removeduser.Email);
                            urc = new TeamUserContext {
                                User = removeduser,
                                UserParameters = urcp,
                                Cancel = false
                            };
                            _userEventHandlers.RemovingFromTeam(urc);
                            if(!urc.Cancel) {
                                couldRemove = true;
                                var tuRecs = _teamUsersRepository.Fetch(x => x.TeamId == teamPart.Id && x.UserId == member.Item2);
                                foreach(var tuRec in tuRecs)
                                    _teamUsersRepository.Delete(tuRec);
                                _teamEventHandlers.UserRemoved(truc);
                                _userEventHandlers.RemovedFromTeam(urc);
                                removed = true;
                                Logger.Debug("User '{0}' removed from team '{1}'", removeduser.UserName, teamPart.TeamName);
                            }
                        }
                    }
                    else couldRemove = true;
                    if(couldRemove) {
                        var tuRecs = _teamUsersRepository.Fetch(x => x.TeamId == teamPart.Id && x.UserId == member.Item2);
                        foreach(var tuRec in tuRecs)
                            _teamUsersRepository.Delete(tuRec);
                        if (truc != null )
                            _teamEventHandlers.UserRemoved(truc);
                        if (urc != null)
                            _userEventHandlers.RemovedFromTeam(urc);
                        removed = true;
                        Logger.Debug("User '{0}' removed from team '{1}'", removeduser.UserName, teamPart.TeamName);
                    }
                    if(!removed) {
                        ret = false;
                        Logger.Debug("Can't remove user '{0}' from team '{1}'", removeduser.UserName, teamPart.TeamName);
                    }
                }
            }
            // We must keep one user in list
            if(!teamUsers.Members.Any()) {
                ret = false;
                foreach(var member in currentusers) {
                    var addeduser = _contentManager.Get<IUser>(member.Item2);
                    var ucp = new TeamUserParams(teamPart, addeduser.UserName, addeduser.Email);
                    var uc = new TeamUserContext {
                        UserParameters = ucp,
                        User = addeduser,
                        Cancel = false
                    };
                    _userEventHandlers.Adding2Team(uc);
                    bool added = false;
                    if(!uc.Cancel) {
                        var tuRec = new TeamMembersPartRecord {
                            TeamId = teamPart.Id,
                            TeamMemberType = (int)member.Item1,
                            UserId = member.Item2
                        };
                        var tuc = new UserAddContext { Team = teamPart, User = addeduser, TeamUserId = tuRec.Id, Cancel = false };
                        _teamEventHandlers.UserAdding(tuc);
                        if(!tuc.Cancel) {
                            _teamUsersRepository.Create(tuRec);
                            _teamEventHandlers.UserAdded(tuc);
                            _userEventHandlers.Added2Team(uc);
                            Logger.Debug("User '{0}' added back to team '{1}'", addeduser.UserName, teamPart.TeamName);
                            added = true;
                        }
                    }
                    if(!added) {
                        Logger.Debug("Can't add back user '{0}' to team '{1}'", addeduser.UserName, teamPart.TeamName);
                        continue;
                    }
                    teamUsers.Members.Add(member);
                    break; // Keep only first
                }
            }
            TriggerSignal();
            return ret;
        }

        public bool UpdateIUser(int id, IEnumerable<int> teams) {
            bool ret = true;
            var iuser = _contentManager.Get<IUser>(id);
            var userTeams = iuser.As<UserTeamsPart>();


            var currentTeams = new List<int>(userTeams.Teams);
            userTeams.Teams.Clear();
            foreach(var teamId in teams) {
                if(!currentTeams.Contains(teamId)) {
                    var addedteam = _contentManager.Get<TeamPart>(teamId);
                    var tucp = new TeamUserParams(addedteam, iuser.UserName, iuser.Email);
                    var uc = new TeamUserContext {
                        UserParameters = tucp,
                        User = iuser,
                        Cancel = false
                    };
                    _userEventHandlers.Adding2Team(uc);
                    bool added = false;
                    if(!uc.Cancel) {
                        var tuRec = new TeamMembersPartRecord {
                            TeamId = teamId,
                            TeamMemberType = (int)iuser.TeamMemberType,
                            UserId = id
                        };
                        var tuc = new UserAddContext { Team = addedteam, User = iuser, TeamUserId = tuRec.Id, Cancel = false };
                        _teamEventHandlers.UserAdding(tuc);
                        if(!tuc.Cancel) {
                            _teamUsersRepository.Create(tuRec);
                            _teamEventHandlers.UserAdded(tuc);
                            _userEventHandlers.Added2Team(uc);
                            Logger.Debug("User '{0}' added to team '{1}'", iuser.UserName, addedteam.TeamName);
                            added = true;
                        }
                    }
                    if(!added) {
                        Logger.Debug("Can't add user '{0}' to team '{1}'", iuser.UserName, addedteam.TeamName);
                        ret = false;
                        continue;
                    }
                }
                userTeams.Teams.Add(teamId);
            }
            foreach(var team in currentTeams) {
                if(!userTeams.Teams.Contains(team)) {
                    bool removed = false;
                    var removedteam = _contentManager.Get<TeamPart>(team);
                    var truc = new UserRemoveContext {
                        Team = removedteam,
                        User = iuser,
                        Cancel = false
                    };
                    _teamEventHandlers.UserRemoving(truc);
                    if(!truc.Cancel) {
                        var urcp = new TeamUserParams(removedteam, iuser.UserName, iuser.Email);
                        var urc = new TeamUserContext {
                            User = iuser,
                            UserParameters = urcp,
                            Cancel = false
                        };
                        _userEventHandlers.RemovingFromTeam(urc);
                        if(!urc.Cancel) {
                            var tuRecs = _teamUsersRepository.Fetch(x => x.TeamId == team && x.UserId == iuser.Id);
                            foreach(var tuRec in tuRecs)
                                _teamUsersRepository.Delete(tuRec);
                            _teamEventHandlers.UserRemoved(truc);
                            _userEventHandlers.RemovedFromTeam(urc);
                            removed = true;
                            Logger.Debug("User '{0}' removed from team '{1}'", iuser.UserName, removedteam.TeamName);
                        }
                    }
                    if(!removed) {
                        ret = false;
                        Logger.Debug("Can't remove user '{0}' from team '{1}'", iuser.UserName, removedteam.TeamName);
                    }
                }
            }
            TriggerSignal();
            return ret;
        }

        public IEnumerable<string> GetUsersForTeam(int id) {
            var users = new List<string>();
            TeamMembersPart teamUsersPart = GetTeamUsers(id);
            foreach(var userId in teamUsersPart.Members.Where(m => m.Item1 == TeamMemberType.user).Select(m => m.Item2)) {
                users.Add(_contentManager.Get<IUser>(userId).UserName);
            }
            return users;
        }

        public IEnumerable<string> GetUsersForTeamByName(string name) {
            return _cacheManager.Get(name, ctx => {
                MonitorSignal(ctx);
                return GetUsersForTeamByNameInner(name).ToList();
            });
        }

        IEnumerable<string> GetUsersForTeamByNameInner(string name) {
            var roleRecord = GetTeamByName(name);
            return roleRecord == null ? Enumerable.Empty<string>() : GetUsersForTeam(roleRecord.Id);
        }


        private void MonitorSignal(AcquireContext<string> ctx) {
            ctx.Monitor(_signals.When(SignalName));
        }

        private void TriggerSignal() {
            _signals.Trigger(SignalName);
        }

    }
}