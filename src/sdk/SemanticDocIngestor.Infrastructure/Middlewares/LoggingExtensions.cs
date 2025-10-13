using Elastic.Apm.NetCoreAll;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using System.Linq;

namespace SemanticDocIngestor.Infrastructure.Middlewares
{
    public static class SemanticDocIngestorLoggingExtensions
    {
        public static Logger AddSerilogLogging(IConfiguration Configuration)
        {
            var logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithCorrelationIdHeader()
                .Filter.ByExcluding(c => c.Properties.Any(p => p.Value.ToString().Contains("swagger")))
                .ReadFrom.Configuration(Configuration)
            .CreateLogger();

            return logger;
        }

        public static IServiceCollection AddApm(this IServiceCollection services)
        {
            return services.AddAllElasticApm();
        }

        public static IApplicationBuilder UseLogging(this IApplicationBuilder app, IConfiguration Configuration, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSerilog();

            return app;
        }
    }
}
