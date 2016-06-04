using Orchard.DisplayManagement;
using System.Collections.Generic;

namespace Orchard.ContentManagement.Handlers {
    public class BuildEditorContext : BuildShapeContext {
        public BuildEditorContext(IShape model, IContent content, string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory) {
        }
    }
    // CS 25/5
    public class BuildFrontEditorContext : BuildShapeContext {
        // CS 30/5
        public string EditType { get; set; } // reuse DisplayType as it is not used in Editor contexts ???
        // CS 30/5
        public BuildFrontEditorContext(IShape model, IContent content, string editType,string groupId, IShapeFactory shapeFactory)
            : base(model, content, groupId, shapeFactory) {
            EditType = editType;
        }
    }
}