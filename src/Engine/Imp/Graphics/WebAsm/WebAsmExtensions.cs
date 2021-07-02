using Fusee.Engine.Imp.Graphics.WebAsm;
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
        public static IJSRuntime Runtime;

        public static void SetObjectProperty<T>(this IJSInProcessObjectReference reference, string propertyIdentifier, T val)
        {
            ((IJSInProcessRuntime)Runtime).Invoke<T>("SetObjectProperty", reference, propertyIdentifier, val);
        }

        public static T GetObjectProperty<T>(this IJSInProcessObjectReference reference, string property)
        {

            return ((IJSInProcessRuntime)Runtime).Invoke<T>("GetObjectProperty", reference, property);
        }

        public static T GetObjectProperty<T>(this IJSInProcessObjectReference[] reference, string property)
        {
            return ((IJSInProcessRuntime)Runtime).Invoke<T>("GetObjectProperty", reference, property);
        }

        public static void SetObjectProperty<T>(this IJSObjectReference reference, string propertyIdentifier, T val)
        {
            ((IJSInProcessRuntime)Runtime).Invoke<T>("SetObjectProperty", reference, propertyIdentifier, val);
        }

        public static T GetObjectProperty<T>(this IJSObjectReference reference, IJSRuntime runtime, string property)
        {
            return ((IJSInProcessRuntime)runtime).Invoke<T>("GetObjectProperty", reference, property);
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
