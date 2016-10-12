using System.Collections.Generic;
using Orchard.ContentManagement;

namespace Orchard.Security {
    public interface ITeams : IContent { //IContent needed by As<ITeams>()
        IList<int> Teams { get; }
    }
    public interface IUserTeams : IContent {
        IList<int> Teams { get; }
    }
}