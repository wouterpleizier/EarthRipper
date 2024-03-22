namespace EarthRipperHook
{
    internal abstract class DisposableToken(Action<DisposableToken> disposeAction) : IDisposable
    {
        private readonly Action<DisposableToken> _disposeAction = disposeAction;

        public void Dispose()
        {
            _disposeAction.Invoke(this);
        }
    }

    internal class HookToken<TNativeDelegate>(Action<DisposableToken> disposeAction) : DisposableToken(disposeAction);
    internal class ShaderOverrideToken(Action<DisposableToken> disposeAction) : DisposableToken(disposeAction);
}
