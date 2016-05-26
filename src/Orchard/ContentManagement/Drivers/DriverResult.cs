using Orchard.ContentManagement.Handlers;

namespace Orchard.ContentManagement.Drivers {
    public class DriverResult {
        public virtual void Apply(BuildDisplayContext context) { }
        public virtual void Apply(BuildEditorContext context) { }
        // CS 25/5
        public virtual void Apply(BuildFrontEditorContext context) { }

        public ContentPart ContentPart { get; set; }
        public ContentField ContentField { get; set; }
    }
}
