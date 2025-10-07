using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Contracts
{
    public interface ICacheProvider
    {
        void Set<T>(string key, T value, TimeSpan ttl);
        bool TryGet<T>(string key, out T? value);
        void Remove(string key);
    }
}
