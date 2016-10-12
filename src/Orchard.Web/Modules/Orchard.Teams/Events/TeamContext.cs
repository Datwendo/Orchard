using Orchard.Security;

namespace Orchard.Teams.Events {
    public class TeamContext {
        public IUser Team { get; set; }
        public bool Cancel { get; set; }
    }
}