using Orchard.Events;

namespace Orchard.Teams.Events {
    public interface ITeamEventHandler : IEventHandler {
        void Creating(TeamCreateContext context);
        void Created(TeamCreateContext context);
        void Removing(TeamRemoveContext context);
        void Removed(TeamRemoveContext context);
        void Renaming(TeamRenameContext context);
        void Renamed(TeamRenameContext context);
        void UserAdding(UserAddContext context);
        void UserAdded(UserAddContext context);
        void UserRemoving(UserRemoveContext context);
        void UserRemoved(UserRemoveContext context);
    }
}