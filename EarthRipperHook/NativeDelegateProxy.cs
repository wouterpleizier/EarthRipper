using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using System.Reflection;

namespace EarthRipperHook
{
    internal static class NativeDelegateProxy<TNativeDelegate> where TNativeDelegate : Delegate
    {
        internal static TNativeDelegate OriginalFunction { get; }

        private static readonly object _lock;
        private static readonly Type _returnType;

        private static readonly TNativeDelegate _hookHandler;
        private static readonly Dictionary<HookToken<TNativeDelegate>, TNativeDelegate> _subscribers;
        private static (HookToken<TNativeDelegate> Token, TNativeDelegate Subscriber)? _exclusiveSubscriber;

        private static readonly ThreadLocal<Stack<DispatchContext>> _dispatchContextStack;
        private static int _dispatchContextVersion;

        static NativeDelegateProxy()
        {
            Type delegateType = typeof(TNativeDelegate);

            if (delegateType.GetCustomAttribute<X86FunctionAttribute>() == null
                && delegateType.GetCustomAttribute<X64FunctionAttribute>() == null)
            {
                throw new InvalidOperationException("No x86 or x64 function attribute found on delegate");
            }

            FunctionNameAttribute? functionNameAttribute = delegateType.GetCustomAttribute<FunctionNameAttribute>()
                ?? throw new InvalidOperationException("No function name attribute found on delegate");

            // The function library/DLL may be specified on the delegate itself or its containing type/module/assembly.
            FunctionLibraryAttribute? functionLibraryAttribute = delegateType.GetCustomAttribute<FunctionLibraryAttribute>();
            if (functionLibraryAttribute == null)
            {
                Type? declaringType = delegateType.DeclaringType;
                while (declaringType != null && functionLibraryAttribute == null)
                {
                    functionLibraryAttribute = declaringType.GetCustomAttribute<FunctionLibraryAttribute>();

                    if (functionLibraryAttribute == null)
                    {
                        declaringType = declaringType.DeclaringType;
                    }
                }
            }

            functionLibraryAttribute ??= delegateType.Module.GetCustomAttribute<FunctionLibraryAttribute>()
                ?? delegateType.Assembly.GetCustomAttribute<FunctionLibraryAttribute>()
                ?? throw new InvalidOperationException("No function library attribute found on delegate or its containing type(s), module or assembly");

            MethodInfo delegateInvokeMethod = delegateType.GetMethod("Invoke")
                ?? throw new InvalidOperationException("No Invoke method found on delegate");

            _returnType = delegateInvokeMethod.ReturnType;
            bool isVoid = _returnType == typeof(void);

            Type[] delegateParameterTypes = delegateInvokeMethod.GetParameters()
                .Select(param => param.ParameterType)
                .ToArray();

            MethodInfo genericHandlerMethod = typeof(NativeDelegateProxy<TNativeDelegate>)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single
                (
                    method => method.Name == nameof(HandleHookedFunctionCalled)
                    && method.GetParameters().Length == delegateParameterTypes.Length
                    && method.GetGenericArguments().Length == delegateParameterTypes.Length + (isVoid ? 0 : 1)
                );

            MethodInfo handlerMethod = genericHandlerMethod.MakeGenericMethod(isVoid
                ? [.. delegateParameterTypes]
                : [.. delegateParameterTypes, _returnType]);

            _hookHandler = handlerMethod.CreateDelegate<TNativeDelegate>();
            long address = (long)HookUtil.GetProcAddress(functionLibraryAttribute.Library, functionNameAttribute.Name);

            IHook<TNativeDelegate> hook = ReloadedHooks.Instance.CreateHook(_hookHandler, address);

            OriginalFunction = hook.OriginalFunction;
            _lock = new object();
            _subscribers = [];
            _dispatchContextStack = new ThreadLocal<Stack<DispatchContext>>(() => new Stack<DispatchContext>());

            hook.Activate();
        }

        internal static void EnsureInitialized()
        {
            // No need to do anything here besides letting the static constructor run.
        }

        internal static HookToken<TNativeDelegate>? Hook(TNativeDelegate subscriber, bool exclusive = false, bool throwOnFailure = false)
        {
            lock (_lock)
            {
                if (exclusive)
                {
                    if (_exclusiveSubscriber == null)
                    {
                        var token = new HookToken<TNativeDelegate>(token => Unhook((HookToken<TNativeDelegate>)token));

                        _exclusiveSubscriber = (token, subscriber);
                        _dispatchContextVersion++;
                        return token;
                    }
                    else if (throwOnFailure)
                    {
                        throw new InvalidOperationException($"Exclusive access to {typeof(TNativeDelegate)} is already held by another subscriber");
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (!_subscribers.ContainsValue(subscriber))
                    {
                        var token = new HookToken<TNativeDelegate>(token => Unhook((HookToken<TNativeDelegate>)token));

                        _subscribers.Add(token, subscriber);
                        _dispatchContextVersion++;
                        return token;
                    }
                    else if (throwOnFailure)
                    {
                        throw new InvalidOperationException($"Shared access to {typeof(TNativeDelegate)} is already held by specified subscriber");
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        internal static void Unhook(HookToken<TNativeDelegate> token, bool throwOnFailure = false)
        {
            lock (_lock)
            {
                if (_exclusiveSubscriber?.Token == token)
                {
                    _exclusiveSubscriber = null;
                    _dispatchContextVersion++;
                }
                else if (_subscribers.Remove(token))
                {
                    _dispatchContextVersion++;
                }
                else if (throwOnFailure)
                {
                    throw new InvalidOperationException($"Specified token has no corresponding subscriber to {typeof(TNativeDelegate)}");
                }
            }
        }

        internal static bool SuppressOriginal(object? returnValue = null)
        {
            if (_returnType == typeof(void) && returnValue != null
                || _returnType != typeof(void) && returnValue?.GetType() != _returnType)
            {
                throw new ArgumentException("Return value must match delegate");
            }

            lock (_lock)
            {
                if (_dispatchContextStack.Value!.TryPeek(out DispatchContext? dispatchContext))
                {
                    dispatchContext.SuppressOriginalFunction = true;
                    dispatchContext.OverrideReturnValue = returnValue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal static void InvokeAfterCompletion(Action action)
        {
            lock (_lock)
            {
                if (_dispatchContextStack.Value!.TryPeek(out DispatchContext? dispatchContext))
                {
                    dispatchContext.PostCompletionActions.Enqueue(action);
                }
            }
        }

        private static void HandleHookedFunctionCalled() => Dispatch();
        private static void HandleHookedFunctionCalled<T1>(T1 arg1) => Dispatch(arg1);
        private static void HandleHookedFunctionCalled<T1, T2>(T1 arg1, T2 arg2) => Dispatch(arg1, arg2);
        private static void HandleHookedFunctionCalled<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) => Dispatch(arg1, arg2, arg3);
        private static void HandleHookedFunctionCalled<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Dispatch(arg1, arg2, arg3, arg4);
        private static void HandleHookedFunctionCalled<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => Dispatch(arg1, arg2, arg3, arg4, arg5);
        private static void HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => Dispatch(arg1, arg2, arg3, arg4, arg5, arg6);
        private static void HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => Dispatch(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        private static void HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => Dispatch(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);

        private static TResult HandleHookedFunctionCalled<TResult>() => (TResult)Dispatch()!;
        private static TResult HandleHookedFunctionCalled<T1, TResult>(T1 arg1) => (TResult)Dispatch(arg1)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, TResult>(T1 arg1, T2 arg2) => (TResult)Dispatch(arg1, arg2)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3) => (TResult)Dispatch(arg1, arg2, arg3)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => (TResult)Dispatch(arg1, arg2, arg3, arg4)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, T4, T5, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => (TResult)Dispatch(arg1, arg2, arg3, arg4, arg5)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => (TResult)Dispatch(arg1, arg2, arg3, arg4, arg5, arg6)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => (TResult)Dispatch(arg1, arg2, arg3, arg4, arg5, arg6, arg7)!;
        private static TResult HandleHookedFunctionCalled<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => (TResult)Dispatch(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)!;

        private static object? Dispatch(params object?[] args)
        {
            NativeDelegateProxy<TNativeDelegate>.DispatchContext context = PushDispatchContext();
            try
            {
                object? result = null;

                Queue<TNativeDelegate> queuedSubscribers = new Queue<TNativeDelegate>(context.Subscribers);
                if (queuedSubscribers.Count > 0)
                {
                    List<TNativeDelegate> handledSubscribers = [];
                    while (queuedSubscribers.Count > 0)
                    {
                        TNativeDelegate queuedSubscriber = queuedSubscribers.Dequeue();

                        // Should probably use expressions or some other magic here, but this is fast enough for now.
                        queuedSubscriber.DynamicInvoke(args);

                        handledSubscribers.Add(queuedSubscriber);

                        if (SyncDispatchContext(context))
                        {
                            queuedSubscribers = new Queue<TNativeDelegate>(context.Subscribers.Except(handledSubscribers));
                        }
                    }
                }

                if (context.SuppressOriginalFunction)
                {
                    result = context.OverrideReturnValue;
                }
                else
                {
                    // Should probably use expressions or some other magic here, but this is fast enough for now.
                    result = OriginalFunction.DynamicInvoke(args)!;
                }

                Queue<Action> postCompletionActions = new Queue<Action>(context.PostCompletionActions);
                while (postCompletionActions.Count > 0)
                {
                    Action action = postCompletionActions.Dequeue();
                    action.Invoke();
                }

                return result;
            }
            finally
            {
                PopDispatchContext();
            }
        }

        private class DispatchContext
        {
            internal int Version { get; set; }
            internal List<TNativeDelegate> Subscribers { get; set; } = [];
            internal Queue<Action> PostCompletionActions { get; set; } = [];
            internal bool SuppressOriginalFunction { get; set; }
            internal object? OverrideReturnValue { get; set; }
        }

        private static DispatchContext PushDispatchContext()
        {
            lock (_lock)
            {
                bool suppressOriginalFunction = false;
                object? overrideReturnValue = null;
                if (_dispatchContextStack.Value!.TryPeek(out DispatchContext? current))
                {
                    suppressOriginalFunction = current.SuppressOriginalFunction;
                    overrideReturnValue = current.OverrideReturnValue;
                }

                DispatchContext newDispatchContext = new DispatchContext()
                {
                    SuppressOriginalFunction = suppressOriginalFunction,
                    OverrideReturnValue = overrideReturnValue
                };

                SyncDispatchContext(newDispatchContext);
                _dispatchContextStack.Value.Push(newDispatchContext);

                return newDispatchContext;
            }
        }

        private static bool SyncDispatchContext(DispatchContext dispatchContext)
        {
            lock (_lock)
            {
                if (dispatchContext.Version != _dispatchContextVersion)
                {
                    dispatchContext.Version = _dispatchContextVersion;
                    dispatchContext.Subscribers = _exclusiveSubscriber != null
                        ? [_exclusiveSubscriber.Value.Subscriber]
                        : [.. _subscribers.Values];

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static void PopDispatchContext()
        {
            lock (_lock)
            {
                _dispatchContextStack.Value!.Pop();
            }
        }
    }
}
