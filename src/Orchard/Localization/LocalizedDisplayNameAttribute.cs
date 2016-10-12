using System;
using System.ComponentModel;
// CS 26/6 Harmony
namespace Orchard.Localization {
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute {
        public LocalizedDisplayNameAttribute() { }
        public LocalizedDisplayNameAttribute(string displayName) : base(displayName) { }
    }
}
