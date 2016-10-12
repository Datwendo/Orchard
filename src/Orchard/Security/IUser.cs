using Orchard.ContentManagement;
using System.Collections.Generic;

namespace Orchard.Security {
    /// <summary>
    /// Interface provided by the "User" model. 
    /// </summary>
    public interface IUser : IContent, IUserTeams {
        string UserName { get; }
        string Email { get; }
        TeamMemberType TeamMemberType { get; }
    }
}
