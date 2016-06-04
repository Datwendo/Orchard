using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using System.Collections.Generic;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateEditorContext : BuildEditorContext {

        public UpdateEditorContext(IShape model, IContent content, IUpdateModel updater, string groupInfoId, IShapeFactory shapeFactory, ShapeTable shapeTable, string path)
            : base(model, content, groupInfoId, shapeFactory) {
            
            ShapeTable = shapeTable;
            Updater = updater;
            Path = path;
        }

        public IUpdateModel Updater { get; private set; }
        public ShapeTable ShapeTable { get; private set; }
        public string Path { get; private set; }
    }
    // CS 25/5
    public class UpdateFrontEditorContext : BuildFrontEditorContext {

        public UpdateFrontEditorContext(IShape model, IContent content, IUpdateModel updater, string editType, string groupInfoId, IShapeFactory shapeFactory, ShapeTable shapeTable, string path)
            : base(model, content, editType, groupInfoId, shapeFactory) {

            ShapeTable = shapeTable;
            Updater = updater;
            Path = path;

        }

        public IUpdateModel Updater { get; private set; }
        public ShapeTable ShapeTable { get; private set; }
        public string Path { get; private set; }
    }
}