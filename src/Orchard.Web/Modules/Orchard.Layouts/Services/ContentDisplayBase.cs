﻿using System;
using System.Web;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.FileSystems.VirtualPath;
using Orchard.UI.Zones;
using System.Collections.Generic;

namespace Orchard.Layouts.Services {
    // TODO: This class contains duplicated code from the DefaultContentDisplay class.
    // Consider combining this class with DefaultContentDisplay to reuse shared code, for example by inheriting from a common base class.
    public abstract class ContentDisplayBase : Component {
        private readonly IShapeFactory _shapeFactory;
        private readonly Lazy<IShapeTableLocator> _shapeTableLocator;
        private readonly RequestContext _requestContext;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWorkContextAccessor _workContextAccessor;

        protected ContentDisplayBase(
            IShapeFactory shapeFactory,
            Lazy<IShapeTableLocator> shapeTableLocator,
            RequestContext requestContext,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor) {

            _shapeFactory = shapeFactory;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _virtualPathProvider = virtualPathProvider;
            _workContextAccessor = workContextAccessor;

        }

        public abstract string DefaultStereotype { get; }

        public BuildDisplayContext BuildDisplayContext(IContent content, string displayType, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = DefaultStereotype;

            var actualShapeType = stereotype;
            var actualDisplayType = String.IsNullOrWhiteSpace(displayType) ? "Detail" : displayType;
            var itemShape = CreateItemShape(actualShapeType);

            itemShape.ContentItem = content.ContentItem;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, groupId, _shapeFactory);
            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            context.Layout = workContext.Layout;

            BindPlacement(context, actualDisplayType, stereotype);

            return context;
        }

        public BuildEditorContext BuildEditorContext(IContent content, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = DefaultStereotype;

            var actualShapeType = stereotype + "_Edit";
            var itemShape = CreateItemShape(actualShapeType);

            itemShape.ContentItem = content.ContentItem;

            // Adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit.
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new BuildEditorContext(itemShape, content, groupId, _shapeFactory);
            BindPlacement(context, null, stereotype);

            return context;
        }

        public UpdateEditorContext UpdateEditorContext(IContent content, IUpdateModel updater, string groupInfoId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = DefaultStereotype;

            var actualShapeType = stereotype + "_Edit";
            var itemShape = CreateItemShape(actualShapeType);

            itemShape.ContentItem = content.ContentItem;

            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            var theme = workContext.CurrentTheme;
            var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

            // Adding an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit.
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);

            var context = new UpdateEditorContext(itemShape, content, updater, groupInfoId, _shapeFactory, shapeTable, GetPath());
            BindPlacement(context, null, stereotype);

            return context;
        }
        // CS 25/5
        public BuildFrontEditorContext BuildFrontEditorContext(IContent content, string editType, string groupId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = DefaultStereotype;

            var actualShapeType = stereotype + "_FrontEdit";
            var actualEditType = String.IsNullOrWhiteSpace(editType) ? ""/*"Detail"*/ : editType;
            var itemShape = CreateItemShape(actualShapeType);

            itemShape.ContentItem = content.ContentItem;

            // Adding an alternate for [Stereotype]_FrontEdit__[ContentType] e.g. Content.FrontEdit-Menu
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);
            // Adding an alternate for [Stereotype]_FrontEdit_[EditType] e.g. Content.FrontEdit.Write
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "_" + editType);
            // Adding an alternate for [Stereotype]_FrontEdit_[EditType]__[ContentType] e.g. Content.FrontEdit-Menu.Write
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "_" + editType + "__" + content.ContentItem.ContentType);

            var context = new BuildFrontEditorContext(itemShape, content, editType, groupId, _shapeFactory);
            BindPlacement(context, actualEditType, stereotype);

            return context;
        }
        // CS 25/5
        public UpdateFrontEditorContext UpdateFrontEditorContext(IContent content, IUpdateModel updater, string editType, string groupInfoId) {
            var contentTypeDefinition = content.ContentItem.TypeDefinition;
            string stereotype;
            if (!contentTypeDefinition.Settings.TryGetValue("Stereotype", out stereotype))
                stereotype = DefaultStereotype;

            var actualShapeType = stereotype + "_FrontEdit";
            var actualEditType = String.IsNullOrWhiteSpace(editType) ? ""/*"Detail"*/ : editType;
            var itemShape = CreateItemShape(actualShapeType);

            itemShape.ContentItem = content.ContentItem;

            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            var theme = workContext.CurrentTheme;
            var shapeTable = _shapeTableLocator.Value.Lookup(theme.Id);

            // Adding an alternate for [Stereotype]_FrontEdit__[ContentType] e.g. Content.FrontEdit-Menu
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "__" + content.ContentItem.ContentType);
            // Adding an alternate for [Stereotype]_FrontEdit_[EditType] e.g. Content.FrontEdit.Write
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "_" + editType);
            // Adding an alternate for [Stereotype]_FrontEdit_[EditType]__[ContentType] e.g. Content.FrontEdit-Menu.Write
            ((IShape)itemShape).Metadata.Alternates.Add(actualShapeType + "_" + editType + "__" + content.ContentItem.ContentType );

            var context = new UpdateFrontEditorContext(itemShape, content, updater, editType, groupInfoId, _shapeFactory, shapeTable, GetPath());
            BindPlacement(context, actualEditType, stereotype);

            return context;
        }

        private dynamic CreateItemShape(string actualShapeType) {
            return _shapeFactory.Create(actualShapeType, Arguments.Empty(), () => new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty())));
        }

        // TODO: This is the exact same code as in DefaultContentDisplay. Consider combining this class with DefaultContentDisplay to reuse shared code.
        private void BindPlacement(BuildShapeContext context, string displayType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {
                var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
                var shapeTable = workContext.HttpContext != null
                    ? _shapeTableLocator.Value.Lookup(workContext.CurrentTheme.Id)
                    : _shapeTableLocator.Value.Lookup(null);

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = context.ContentItem,
                        ContentType = context.ContentItem.ContentType,
                        Stereotype = stereotype,
                        DisplayType = (context is BuildFrontEditorContext) ? null : displayType,
                        EditType = (context is BuildFrontEditorContext) ? displayType:null,
                        Differentiator = differentiator,
                        Path = GetPath()
                    };

                    // define which location should be used if none placement is hit
                    descriptor.DefaultPlacement = defaultLocation;

                    var placement = descriptor.Placement(placementContext);
                    if (placement != null) {
                        placement.Source = placementContext.Source;
                        return placement;
                    }
                }

                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = String.Empty
                };
            };
        }

        /// <summary>
        /// Gets the current app-relative path, i.e. ~/my-blog/foo.
        /// </summary>
        private string GetPath() {
            return VirtualPathUtility.AppendTrailingSlash(_virtualPathProvider.ToAppRelative(_requestContext.HttpContext.Request.Path));
        }
    }
}
