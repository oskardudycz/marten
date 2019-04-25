using System;
using System.Linq.Expressions;
using Marten.Linq;
using Marten.Schema.Identity;
using Marten.Storage;

namespace Marten.Schema
{
    public interface IDocumentMapping
    {
        IDocumentMapping Root { get; }

        Type DocumentType { get; }

        IDocumentStorage BuildStorage(StoreOptions options);

        DbObjectName Table { get; }

        void DeleteAllDocuments(ITenant factory);

        IdAssignment<T> ToIdAssignment<T>(ITenant tenant);

        IQueryableDocument ToQueryableDocument();

        Type IdType { get; }
        TenancyStyle TenancyStyle { get; }
    }

    public static class DocumentMappingExtensions
    {
        public static string JsonLocator(this IQueryableDocument mapping, Expression expression)
        {
            var field = mapping.FieldFor(expression);

            return field.TypedLocator;
        }
    }
}