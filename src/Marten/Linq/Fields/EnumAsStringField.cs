using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Marten.Linq.Fields
{
    public class EnumAsStringField : FieldBase
    {
        public EnumAsStringField(string dataLocator, Casing casing, MemberInfo[] members) 
            : base(dataLocator, "varchar", casing, members)
        {
            if (!FieldType.IsEnum) throw new ArgumentOutOfRangeException(nameof(members), "Not an Enum type");
        }

        public override object GetValueForCompiledQueryParameter(Expression expression)
        {
            var raw = expression.Value();
            return Enum.GetName(FieldType, raw);
        }
    }
}