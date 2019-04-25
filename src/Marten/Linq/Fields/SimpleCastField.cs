using System.Reflection;

namespace Marten.Linq.Fields
{
    public class SimpleCastField : FieldBase
    {
        public SimpleCastField(string dataLocator, string pgType, Casing casing, MemberInfo[] members) : base(dataLocator, pgType, casing, members)
        {
            TypedLocator = $"CAST({RawLocator} as {PgType})";
        }
    }
}