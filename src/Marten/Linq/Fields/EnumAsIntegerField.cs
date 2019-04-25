using System.Reflection;

namespace Marten.Linq.Fields
{
    public class EnumAsIntegerField : FieldBase
    {
        public EnumAsIntegerField(string dataLocator, Casing casing, MemberInfo[] members) : base(dataLocator, "integer", casing, members)
        {
            PgType = "integer";
            TypedLocator = $"CAST({dataLocator} ->> '{lastMemberName}' as {PgType})";
        }
    }
}