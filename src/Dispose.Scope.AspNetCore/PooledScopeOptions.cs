namespace Dispose.Scope.AspNetCore
{
    public class PooledScopeOptions
    {
        public DisposeScopeOption Option { get; set; } = DisposeScopeOption.Required;

        public int DisposeObjListDefaultSize { get; set; } = 8;
    }
}