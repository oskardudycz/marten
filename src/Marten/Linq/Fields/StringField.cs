using System.Reflection;

namespace Marten.Linq.Fields
{
    public class StringField : FieldBase
    {
        public StringField(string dataLocator, Casing casing, MemberInfo[] members) : base(dataLocator, "varchar", casing, members)
        {
        }
    }
}