using Orchard.ContentManagement;
using Orchard.Security;
using Orchard.Teams.Models;
using System;
using System.Collections.Generic;

namespace Orchard.Teams.Services {
    public interface ITeamService : IDependency {
        IUser CreateTeam(string TeamName, string Email, string firstUserName);
        IUser CreateTeam(string TeamName, string Email, IUser firstUser);
        bool RemoveFromTeam(int id, IEnumerable<Tuple<TeamMemberType, int>> users2Rmove);
        bool UpdateTeam(int id, string teamName, IEnumerable<Tuple<TeamMemberType, int>> users);
        bool UpdateIUser(int id, IEnumerable<int> teams);
        bool DeleteTeam(int id, IUser newOwner);
        bool VerifyTeamUnicity(string teamName);
        bool VerifyTeamUnicity(string teamName, string email);
        bool VerifyTeamUnicity(int id, string teamName, string email = null);

        void AddUser2Team(int teamId, IUser user);
        TeamPart GetTeam(int id);
        TeamPart GetTeamByName(string teamName);
        TeamMembersPart GetTeamUsers(int id);
        IEnumerable<string> GetUsersForTeam(int id);
        IEnumerable<string> GetUsersForTeamByName(string name);
    }
}