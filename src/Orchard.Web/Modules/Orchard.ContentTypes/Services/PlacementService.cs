﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentTypes.Settings;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystems.VirtualPath;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Themes.Services;
using Orchard.UI.Zones;

namespace Orchard.ContentTypes.Services {

    public class DriverResultPlacement {
        public PlacementSettings PlacementSettings { get; set; }
        public DriverResult ShapeResult { get; set; }
        public dynamic Shape { get; set; }
    }

    public class PlacementService : IPlacementService {
        private readonly IContentManager _contentManager;
        private readonly ISiteThemeService _siteThemeService;
        private readonly IExtensionManager _extensionManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IShapeTableLocator _shapeTableLocator;
        private readonly RequestContext _requestContext;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IJsonConverter _jsonConverter;

        public PlacementService(
            IContentManager contentManager,
            ISiteThemeService siteThemeService,
            IExtensionManager extensionManager,
            IShapeFactory shapeFactory,
            IShapeTableLocator shapeTableLocator,
            RequestContext requestContext,
            IEnumerable<IContentPartDriver> contentPartDrivers,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IVirtualPathProvider virtualPathProvider,
            IWorkContextAccessor workContextAccessor,
            IJsonConverter jsonConverter
            ) 
        {
            _contentManager = contentManager;
            _siteThemeService = siteThemeService;
            _extensionManager = extensionManager;
            _shapeFactory = shapeFactory;
            _shapeTableLocator = shapeTableLocator;
            _requestContext = requestContext;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _virtualPathProvider = virtualPathProvider;
            _workContextAccessor = workContextAccessor;
            _jsonConverter = jsonConverter;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public IEnumerable<DriverResultPlacement> GetDisplayPlacement(string contentType) {
            var content = _contentManager.New(contentType);
            const string actualDisplayType = "Detail";

            dynamic itemShape = CreateItemShape("Content");
            itemShape.ContentItem = content;
            itemShape.Metadata.DisplayType = actualDisplayType;

            var context = new BuildDisplayContext(itemShape, content, actualDisplayType, String.Empty, _shapeFactory);
            var workContext = _workContextAccessor.GetContext(_requestContext.HttpContext);
            context.Layout = workContext.Layout;

            BindPlacement(context, actualDisplayType,null, "Content");

            var placementSettings = new List<DriverResultPlacement>();

            _contentPartDrivers.Invoke(driver => {
                var result = driver.BuildDisplay(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            _contentFieldDrivers.Invoke(driver => {
                var result = driver.BuildDisplayShape(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;    
            }
        }

        public IEnumerable<DriverResultPlacement> GetEditorPlacement(string contentType) {
            var content = _contentManager.New(contentType);

            dynamic itemShape = CreateItemShape("Content_Edit");
            itemShape.ContentItem = content;
            
            var context = new BuildEditorContext(itemShape, content, String.Empty, _shapeFactory);
            BindPlacement(context, null,null, "Content");

            var placementSettings = new List<DriverResultPlacement>();

            _contentPartDrivers.Invoke(driver => {
                var result = driver.BuildEditor(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            _contentFieldDrivers.Invoke(driver => {
                var result = driver.BuildEditorShape(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        // CS 25/5
        public IEnumerable<DriverResultPlacement> GetFrontEditorPlacement(string contentType,string editType) {
            var content = _contentManager.New(contentType);

            dynamic itemShape = CreateItemShape("Content_FrontEdit");
            itemShape.ContentItem = content;
            itemShape.Metadata.EditType = editType;

            var context = new BuildFrontEditorContext(itemShape, content, editType, String.Empty, _shapeFactory);
            BindPlacement(context, null, editType, "Content");

            var placementSettings = new List<DriverResultPlacement>();

            _contentPartDrivers.Invoke(driver => {
                var result = driver.BuildFrontEditor(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            _contentFieldDrivers.Invoke(driver => {
                var result = driver.BuildFrontEditorShape(context);
                if (result != null) {
                    placementSettings.AddRange(ExtractPlacement(result, context));
                }
            }, Logger);

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        public IEnumerable<string> GetZones() {
            var theme = _siteThemeService.GetSiteTheme();
            IEnumerable<string> zones = new List<string>();

            // get the zones for this theme
            if (theme.Zones != null)
                zones = theme.Zones.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

            // if this theme has no zones defined then walk the BaseTheme chain until we hit a theme which defines zones
            while (!zones.Any() && theme != null && !string.IsNullOrWhiteSpace(theme.BaseTheme)) {
                string baseTheme = theme.BaseTheme;
                theme = _extensionManager.GetExtension(baseTheme);
                if (theme != null && theme.Zones != null)
                    zones = theme.Zones.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToList();
            }

            return zones;
        }

        public IEnumerable<string> GetThemeZones(ExtensionDescriptor theme,string layer) {
            IEnumerable<string> zones = new List<string>();
            IEnumerable<string> layers = string.IsNullOrEmpty(theme.Layers) ? Enumerable.Empty<string>() : theme.Layers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (!string.IsNullOrEmpty(layer)
                && !string.IsNullOrEmpty(theme.Layers)
                && layers.Contains(layer, StringComparer.InvariantCultureIgnoreCase)) {
                Dictionary<string, string> layerZones = _jsonConverter.Deserialize<Dictionary<string, string>>(theme.LayerZones);
                if (layerZones.ContainsKey(layer)) {
                    var value = layerZones[layer];
                    zones = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToList();
                }
            }
            else if (string.IsNullOrEmpty(layer)
                || string.IsNullOrEmpty(theme.Layers)
                || !layers.Contains(layer, StringComparer.InvariantCultureIgnoreCase)) {
                // get the zones for this theme
                if (theme.Zones != null)
                    zones = theme.Zones.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Distinct()
                        .ToList();
            }
            return zones;
        }


        public IEnumerable<string> GetZones(string layer) {
            var theme = _siteThemeService.GetSiteTheme();
            string baseTheme = theme.BaseTheme;
            IEnumerable<string> zones = new List<string>();
            // if this theme has no zones defined then walk the BaseTheme chain until we hit a theme which defines zones
            do {
                zones = GetThemeZones(theme, layer);
                if (!zones.Any()) {
                    if (!string.IsNullOrEmpty(baseTheme)) {
                        theme = _extensionManager.GetExtension(baseTheme);
                        baseTheme = theme != null ? theme.BaseTheme : null;
                    }
                    else theme = null;
                }
            } while (!zones.Any() && theme != null );
            return zones;
        }

        private IEnumerable<DriverResultPlacement> ExtractPlacement(DriverResult result, BuildShapeContext context) {
            if (result is CombinedResult) {
                foreach (var subResult in ((CombinedResult) result).GetResults()) {
                    foreach (var placement in ExtractPlacement(subResult, context)) {
                        yield return placement;
                    }
                }
            }
            else if (result is ContentShapeResult) {
                var contentShapeResult = (ContentShapeResult) result;

                var placement = context.FindPlacement(
                    contentShapeResult.GetShapeType(),
                    contentShapeResult.GetDifferentiator(),
                    contentShapeResult.GetLocation()
                    );

                string zone = placement.Location;
                string position = String.Empty;

                // if no placement is found, it's hidden, e.g., no placement was found for the specific ContentType/DisplayType
                if (placement.Location != null) {
                    var delimiterIndex = placement.Location.IndexOf(':');
                    if (delimiterIndex >= 0) {
                        zone = placement.Location.Substring(0, delimiterIndex);
                        position = placement.Location.Substring(delimiterIndex + 1);
                    }
                }

                var content = _contentManager.New(context.ContentItem.ContentType);
                // CS 29/5
                dynamic itemShape = (context is BuildFrontEditorContext) ? CreateItemShape("Content_FrontEdit") : CreateItemShape("Content_Edit");
                itemShape.ContentItem = content;

                if(context is BuildDisplayContext) {
                    var newContext = new BuildDisplayContext(itemShape, content, "Detail", "", context.New);
                    BindPlacement(newContext, "Detail",null, "Content");
                    contentShapeResult.Apply(newContext);
                }
                // CS 25/5 + 2/7
                else if (context is BuildFrontEditorContext) {
                    var newContext = new BuildFrontEditorContext(itemShape, content, ((BuildFrontEditorContext)context).EditType, "", context.New);
                    BindPlacement(newContext, null,((BuildFrontEditorContext)context).EditType, "Content");
                    contentShapeResult.Apply(newContext);
                }
                else {
                    var newContext = new BuildEditorContext(itemShape, content, "", context.New);
                    BindPlacement(newContext, null, null,"Content");
                    contentShapeResult.Apply(newContext);
                }


                yield return new DriverResultPlacement {
                    Shape = itemShape.Content,
                    ShapeResult = contentShapeResult,
                    PlacementSettings = new PlacementSettings {
                        ShapeType = contentShapeResult.GetShapeType(),
                        Zone = zone,
                        Position = position,
                        Differentiator = contentShapeResult.GetDifferentiator() ?? String.Empty
                    }
                };
            }
        }

        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            zoneHolding.Metadata.Type = actualShapeType;
            return zoneHolding;
        }

        private void BindPlacement(BuildShapeContext context, string displayType, string editType, string stereotype) {
            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {

                var theme = _siteThemeService.GetSiteTheme();
                var shapeTable = _shapeTableLocator.Lookup(theme.Id);

                var request = _requestContext.HttpContext.Request;

                ShapeDescriptor descriptor;
                if (shapeTable.Descriptors.TryGetValue(partShapeType, out descriptor)) {
                    var placementContext = new ShapePlacementContext {
                        Content = context.ContentItem,
                        ContentType = context.ContentItem.ContentType,
                        Stereotype = stereotype,
                        DisplayType = displayType,
                        EditType = editType, 
                        Differentiator = differentiator,
                        Path = VirtualPathUtility.AppendTrailingSlash(_virtualPathProvider.ToAppRelative(request.Path)) // get the current app-relative path, i.e. ~/my-blog/foo
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
    }
}