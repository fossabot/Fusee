using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusee.Base.Imp.WebAsm
{
    /// <summary>
    /// Useful extensions while working with Microsoft.JSInterop methods like getting an object reference or
    /// a global reference to e. g. window
    /// </summary>
    public static class WebAsmExtensions
    {
        public static void SetObjectProperty<T>(this IJSInProcessObjectReference reference, string propertyIdentifier, T val)
        {
            // TODO(MR): Implement in static injectable javascript
            reference.Invoke<T>("SetObjectProperty", propertyIdentifier, val);
        }

        public static T GetObjectProperty<T>(this IJSInProcessObjectReference reference, string property)
        {
            // TODO(MR): Implement in static injectable javascript
            return reference.Invoke<T>("GetObjectProperty", property);
        }

        public static T GetObjectProperty<T>(this IJSInProcessObjectReference[] reference, string property)
        {
            // TODO(MR): Implement in static injectable javascript
            return reference[0].Invoke<T>("GetObjectProperty", property);
        }

        public static void SetObjectProperty<T>(this IJSObjectReference reference, string propertyIdentifier, T val)
        {
            // TODO(MR): Implement in static injectable javascript
            ((IJSInProcessObjectReference)reference).Invoke<T>("SetObjectProperty", propertyIdentifier, val);
        }

        public static T GetObjectProperty<T>(this IJSObjectReference reference, string property)
        {
            // TODO(MR): Implement in static injectable javascript
            return ((IJSInProcessObjectReference)reference).Invoke<T>("GetObjectProperty", property);
        }

        public static T GetGlobalObject<T>(this IJSRuntime runtime, string objectToRetrive)
        {
            return ((IJSInProcessRuntime)runtime).Invoke<T>("GetObject", objectToRetrive);
        }

        public static T GetGlobalObject<T>(this IJSInProcessRuntime runtime, string objectToRetrive)
        {
            return runtime.Invoke<T>("GetObject", objectToRetrive);
        }
    }
}
