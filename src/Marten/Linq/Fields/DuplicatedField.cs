using System;
using System.Linq.Expressions;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Marten.Schema.Arguments;
using Marten.Storage;
using Marten.Util;
using NpgsqlTypes;

namespace Marten.Linq.Fields
{
    public class DuplicatedField : Field, IField
    {
        private readonly Func<Expression, object> _parseObject = expression => expression.Value();
        private readonly bool useTimestampWithoutTimeZoneForDateTime;
        private string _columnName;

        public DuplicatedField(EnumStorage enumStorage, MemberInfo[] memberPath, bool useTimestampWithoutTimeZoneForDateTime = true) : base(enumStorage, memberPath)
        {
            ColumnName = MemberName.ToTableAlias();
            this.useTimestampWithoutTimeZoneForDateTime = useTimestampWithoutTimeZoneForDateTime;

            if (FieldType.IsEnum)
            {
                if (enumStorage == EnumStorage.AsString)
                {
                    DbType = NpgsqlDbType.Varchar;
                    PgType = "varchar";

                    _parseObject = expression =>
                    {
                        var raw = expression.Value();
                        return Enum.GetName(FieldType, raw);
                    };
                }
                else
                {
                    DbType = NpgsqlDbType.Integer;
                    PgType = "integer";
                }
            }
            else if (FieldType.IsDateTime())
            {
                PgType = this.useTimestampWithoutTimeZoneForDateTime ? "timestamp without time zone" : "timestamp with time zone";
                DbType = this.useTimestampWithoutTimeZoneForDateTime ? NpgsqlDbType.Timestamp : NpgsqlDbType.TimestampTz;
            }
            else if (FieldType == typeof(DateTimeOffset) || FieldType == typeof(DateTimeOffset?))
            {
                PgType = "timestamp with time zone";
                DbType = NpgsqlDbType.TimestampTz;
            }
            else
            {
                DbType = TypeMappings.ToDbType(FieldType);
            }
        }

        /// <summary>
        /// Used to override the assigned DbType used by Npgsql when a parameter
        /// is used in a query against this column
        /// </summary>
        public NpgsqlDbType DbType { get; set; }


        public UpsertArgument UpsertArgument => new UpsertArgument
        {
            Arg = "arg_" + ColumnName.ToLower(),
            Column = ColumnName.ToLower(),
            PostgresType = PgType,
            Members = Members,
            DbType = DbType
        };

        public string RawLocator => TypedLocator;

        public string ColumnName
        {
            get { return _columnName; }
            set
            {
                _columnName = value;
                TypedLocator = "d." + _columnName;
            }
        }
        
        internal IField MatchingNonDuplicatedField { get; set; }

        // TODO -- have this take in CommandBuilder
        public string UpdateSqlFragment()
        {
            // HOKEY, but I'm letting it pass for now.
            var sqlLocator = MatchingNonDuplicatedField.TypedLocator.Replace("d.", "");

            return $"{ColumnName} = {sqlLocator}";
        }

        public object GetValueForCompiledQueryParameter(Expression valueExpression)
        {
            return _parseObject(valueExpression);
        }

        public string JSONBLocator { get; set; }

        public string LocatorFor(string rootTableAlias)
        {
            return $"{rootTableAlias}.{_columnName}";
        }

        public string TypedLocator { get; set; }

        public static DuplicatedField For<T>(EnumStorage enumStorage, Expression<Func<T, object>> expression, bool useTimestampWithoutTimeZoneForDateTime = true)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);

            // Hokey, but it's just for testing for now.
            if (accessor is PropertyChain)
            {
                throw new NotSupportedException("Not yet supporting deep properties yet. Soon.");
            }

            return new DuplicatedField(enumStorage, new MemberInfo[] { accessor.InnerProperty }, useTimestampWithoutTimeZoneForDateTime);
        }

        // I say you don't need a ForeignKey
        public virtual TableColumn ToColumn()
        {
            return new TableColumn(ColumnName, PgType);
        }
    }
}