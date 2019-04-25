using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Baseline;
using Marten.Linq.Fields;
using Marten.Schema;
using Marten.Util;

namespace Marten.Linq.Parsing
{
    /// <summary>
    /// Implement Equals for <see cref="int"/>, <see cref="long"/>, <see cref="decimal"/>, <see cref="Guid"/>, <see cref="bool"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <remarks>Equals(object) calls into <see cref="Convert.ChangeType(object, Type)"/>. Equals(null) is converted to "is null" query.</remarks>
    public class SimpleEqualsParser : IMethodCallParser
    {
        private static readonly List<Type> SupportedTypes = new List<Type> {
            typeof(int), typeof(long), typeof(decimal), typeof(Guid), typeof(bool)
        };

        private readonly string _equalsOperator;
        private readonly string _isOperator;

        static SimpleEqualsParser()
        {
            SupportedTypes.AddRange(TypeMappings.ResolveTypes(NpgsqlTypes.NpgsqlDbType.Timestamp));
            SupportedTypes.AddRange(TypeMappings.ResolveTypes(NpgsqlTypes.NpgsqlDbType.TimestampTz));
        }

        public SimpleEqualsParser(string equalsOperator = "=", string isOperator = "is")
        {
            _equalsOperator = equalsOperator;
            _isOperator = isOperator;
        }

        public bool Matches(MethodCallExpression expression)
        {
            return SupportedTypes.Contains(expression.Method.DeclaringType) &&
                   expression.Method.Name.Equals("Equals", StringComparison.Ordinal);
        }

        public IWhereFragment Parse(IQueryableDocument mapping, ISerializer serializer, MethodCallExpression expression)
        {
            var field = GetField(mapping, expression);
            var locator = field.TypedLocator;

            var value = expression.Arguments.OfType<ConstantExpression>().FirstOrDefault();
            if (value == null) throw new BadLinqExpressionException("Could not extract value from {0}.".ToFormat(expression), null);

            object valueToQuery = value.Value;

            if (valueToQuery == null)
            {
                return new WhereFragment($"({field.RawLocator}) {_isOperator} null");
            }

            if (valueToQuery.GetType() != expression.Method.DeclaringType)
            {
                try
                {
                    valueToQuery = Convert.ChangeType(value.Value, expression.Method.DeclaringType);
                }
                catch (Exception e)
                {
                    throw new BadLinqExpressionException(
                        $"Could not convert {value.Value.GetType().FullName} to {expression.Method.DeclaringType}", e);
                }
            }
    
            return new WhereFragment($"{locator} {_equalsOperator} ?", valueToQuery);
        }

        private static IField GetField(IQueryableDocument mapping, MethodCallExpression expression)
        {
            IField GetField(Expression e)
            {
                return mapping.FieldFor(e);
            }

            if (!expression.Method.IsStatic && expression.Object != null && expression.Object.NodeType != ExpressionType.Constant)
            {
                // x.member.Equals(...)
                return GetField(expression.Object);
            }
            if (expression.Arguments[0].NodeType == ExpressionType.Constant)
            {
                // type.Equals("value", x.member) [decimal]
                return GetField(expression.Arguments[1]);
            }
            // type.Equals(x.member, "value") [decimal]
            return GetField(expression.Arguments[0]);
        }
    }
}