using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticDocIngestor.Domain.Exceptions
{
    public class UnsupportedFileTypeException(string message) : Exception(message)
    {
    }
}
