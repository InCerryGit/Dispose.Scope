using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Dispose.Scope.AspNetCore
{
    public static class PooledScopeApplicationBuilderExtensions
    {
        /// <summary>
        /// Add the PooledScope middleware to the pipeline.
        /// </summary>
        /// <param name="app">see <see cref="IApplicationBuilder"/></param>
        /// <returns>see <see cref="IApplicationBuilder"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UsePooledScope(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            UsePooledScopeCore(app, Array.Empty<object>());
            return app;
        }

        /// <summary>
        /// Add the PooledScope middleware to the pipeline.
        /// </summary>
        /// <param name="app">see <see cref="IApplicationBuilder"/></param>
        /// <param name="options">The middleware options.see <see cref="PooledScopeOptions"/></param>
        /// <returns>see <see cref="IApplicationBuilder"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UsePooledScope(this IApplicationBuilder app, PooledScopeOptions options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            UsePooledScopeCore(app, new object[] {options});
            return app;
        }

        /// <summary>
        /// Add the PooledScope middleware to the pipeline.
        /// </summary>
        /// <param name="app">see <see cref="IApplicationBuilder"/></param>
        /// <param name="action"> configure the middleware.see <see cref="PooledScopeOptions"/></param>
        /// <returns>see <see cref="IApplicationBuilder"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IApplicationBuilder UsePooledScope(this IApplicationBuilder app,
            Action<PooledScopeOptions> action)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            var options = new PooledScopeOptions();
            action?.Invoke(options);
            return UsePooledScopeCore(app, new object[] {Options.Create(options)});
        }

        /// <summary>
        /// Add the PooledScope middleware to the pipeline.
        /// </summary>
        /// <param name="app">see <see cref="IApplicationBuilder"/></param>
        /// <param name="args">The arguments to pass to the middleware type instance's constructor.</param>
        /// <returns>see <see cref="IApplicationBuilder"/></returns>
        private static IApplicationBuilder UsePooledScopeCore(IApplicationBuilder app, object[] args)
        {
            return app.UseMiddleware<PooledScopeMiddleware>(args);
        }
    }
}