namespace Orchard.ContentManagement.Handlers {
    interface IContentTemplateFilter : IContentFilter {
        void GetContentItemMetadata(GetContentItemMetadataContext context);
        void BuildDisplayShape(BuildDisplayContext context);
        void BuildEditorShape(BuildEditorContext context);
        void UpdateEditorShape(UpdateEditorContext context);
        // CS 25/5
        void BuildFrontEditorShape(BuildFrontEditorContext context);
        // CS 25/5
        void UpdateFrontEditorShape(UpdateFrontEditorContext context);
    }
}
