using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dispose.Scope.AspNetCore
{
    /// <summary>
    /// DisposeScopeMiddleware
    /// </summary>
    public class DisposeScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly DisposeScopeOptions _disposeScopeOptions;

        public DisposeScopeMiddleware(RequestDelegate next, IOptions<DisposeScopeOptions> pooledScopeOptions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _disposeScopeOptions = pooledScopeOptions.Value
                                  ?? new DisposeScopeOptions();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var scope = DisposeScope.BeginScope(_disposeScopeOptions.Option,
                _disposeScopeOptions.DisposeObjListDefaultSize);
            httpContext.Response.RegisterForDispose(scope);
            await _next(httpContext);
        }
    }
}