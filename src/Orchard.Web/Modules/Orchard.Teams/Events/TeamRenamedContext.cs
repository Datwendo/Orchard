namespace Orchard.Teams.Events {
    public class TeamRenameContext : TeamContext {
        public string PreviousTeamName { get; set; }
        public string NewTeamName { get; set; }
    }
}