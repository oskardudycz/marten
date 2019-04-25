using System.Reflection;

namespace Marten.Linq.Fields
{
    public class ArrayField : FieldBase
    {
        public ArrayField(string dataLocator, string pgType, Casing casing, MemberInfo[] members) : base(dataLocator, pgType, casing, members)
        {
            var rawLocator = RawLocator;
            RawLocator = $"CAST({rawLocator} as jsonb)";

            if (PgType == "jsonb[]")
            {
                PgType = "jsonb";
            }
            
            TypedLocator = $"CAST({rawLocator} as {PgType})";
        }
    }
}