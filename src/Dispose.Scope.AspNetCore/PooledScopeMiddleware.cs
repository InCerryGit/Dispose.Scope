using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Dispose.Scope.AspNetCore
{
    /// <summary>
    /// PooledScopeMiddleware
    /// </summary>
    public class PooledScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PooledScopeOptions _pooledScopeOptions;

        public PooledScopeMiddleware(RequestDelegate next, IOptions<PooledScopeOptions> pooledScopeOptions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _pooledScopeOptions = pooledScopeOptions.Value
                                  ?? new PooledScopeOptions();
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var scope = DisposeScope.BeginScope(_pooledScopeOptions.Option,
                _pooledScopeOptions.DisposeObjListDefaultSize);
            httpContext.Response.RegisterForDispose(scope);
            await _next(httpContext);
        }
    }
}