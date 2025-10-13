﻿using System;

namespace SemanticDocIngestor.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class BsonCollectionAttribute(string collectionName) : Attribute
    {
        public string CollectionName { get; } = collectionName;
    }
}
