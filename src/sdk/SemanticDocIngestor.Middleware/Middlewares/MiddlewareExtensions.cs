using Microsoft.AspNetCore.Builder;

namespace SemanticDocIngestor.Middleware.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSemanticDocIngestorExceptionHandler(this IApplicationBuilder app) =>
            app.UseMiddleware<ExceptionHandlingMiddleware>();

        public static IApplicationBuilder UseSemanticDocIngestorResiliency(this IApplicationBuilder app) =>
            app.UseMiddleware<ResiliencyMiddleware>();

        public static IApplicationBuilder UseSemanticDocIngestorRequestLogging(this IApplicationBuilder app) =>
            app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
