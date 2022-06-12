namespace Dispose.Scope.AspNetCore
{
    public class DisposeScopeOptions
    {
        public DisposeScopeOption Option { get; set; } = DisposeScopeOption.Required;

        public int DisposeObjListDefaultSize { get; set; } = 8;
    }
}