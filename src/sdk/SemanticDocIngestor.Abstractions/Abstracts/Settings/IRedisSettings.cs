﻿namespace SemanticDocIngestor.Domain.Abstracts.Settings
{
    public static partial class ServiceRegistration
    {
        public interface IRedisSettings
        {
            public string ConnectionString { get; set; }
        }
    }
}
