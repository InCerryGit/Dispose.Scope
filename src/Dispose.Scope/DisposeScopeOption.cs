namespace Dispose.Scope
{
    public enum DisposeScopeOption
    {
        /// <summary>A DisposeScope is required by the scope. It uses an ambient DisposeScope if one already exists. Otherwise, it creates a new DisposeScope before entering the scope. This is the default value.</summary>
        Required,

        /// <summary>A new DisposeScope is always created for the scope.</summary>
        RequiresNew,

        /// <summary>The ambient DisposeScope context is suppressed when creating the scope. All operations within the scope are done without an ambient DisposeScope context.</summary>
        Suppress,
    }
}