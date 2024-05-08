using Reloaded.Hooks.Definitions.Helpers;
using System.Runtime.InteropServices;

namespace EarthRipperHook
{
    internal static class NativeExtensions
    {
        internal static void EnsureInitialized<TNativeDelegate>() where TNativeDelegate : Delegate
        {
            NativeDelegateProxy<TNativeDelegate>.EnsureInitialized();
        }

        internal static HookToken<TNativeDelegate>? Hook<TNativeDelegate>(TNativeDelegate handler, bool exclusive = false, bool throwOnFailure = false) where TNativeDelegate : Delegate
        {
            return NativeDelegateProxy<TNativeDelegate>.Hook(handler, exclusive, throwOnFailure);
        }

        internal static void Unhook<TNativeDelegate>(HookToken<TNativeDelegate> token, bool throwOnFailure) where TNativeDelegate : Delegate
        {
            NativeDelegateProxy<TNativeDelegate>.Unhook(token, throwOnFailure);
        }

        internal static bool SuppressOriginal<TNativeDelegate>(object? returnValue = null) where TNativeDelegate : Delegate
        {
            return NativeDelegateProxy<TNativeDelegate>.SuppressOriginal(returnValue);
        }

        internal static void InvokeAfterCompletion<TNativeDelegate>(Action<object?> action) where TNativeDelegate : Delegate
        {
            NativeDelegateProxy<TNativeDelegate>.InvokeAfterCompletion(action);
        }

        internal static TNativeDelegate Original<TNativeDelegate>() where TNativeDelegate : Delegate
        {
            return NativeDelegateProxy<TNativeDelegate>.OriginalFunction;
        }

        internal static string AsAnsiString(this nuint pointer)
        {
            return Marshal.PtrToStringAnsi(pointer.ToSigned()) ?? throw new NullReferenceException();
        }

        internal static string AsUTF8String(this nuint pointer)
        {
            return Marshal.PtrToStringUTF8(pointer.ToSigned()) ?? throw new NullReferenceException();
        }
    }
}
