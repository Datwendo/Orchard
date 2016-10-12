namespace Orchard.Teams.Models {
    public class TeamMembersPartRecord {
        public virtual int Id { get; set; }
        public virtual int TeamMemberType { get; set; }
        public virtual int UserId { get; set; }
        public virtual int TeamId { get; set; }
    }
}