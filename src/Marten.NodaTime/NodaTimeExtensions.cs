using System;
using System.Linq;
using System.Reflection;
using Baseline;
using Marten.Linq.Fields;
using Marten.Services;
using Marten.Util;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Npgsql;

namespace Marten.NodaTime
{
    public static class NodaTimeExtensions
    {
        /// <summary>
        /// Sets up NodaTime mappings for the PostgreSQL date/time types.
        /// 
        /// By setting up NodaTime mappings - you're opting out of DateTime type handling. Using DateTime in your Document will end up getting NotSupportedException exception.
        /// </summary>
        /// <param name="storeOptions">store options that you're extending</param>
        /// <param name="shouldConfigureJsonNetSerializer">sets if NodaTime configuration should be setup for JsonNetSerializer. Set value to false if you're using different serializer type or you'd like to maintain your own configuration.</param>
        public static void UseNodaTime(this StoreOptions storeOptions, bool shouldConfigureJsonNetSerializer = true)
        {
            NpgsqlConnection.GlobalTypeMapper.UseNodaTime();

            if (shouldConfigureJsonNetSerializer)
            {
                var serializer = storeOptions.Serializer();
                (serializer as JsonNetSerializer)?.Customize(s => s.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));
                storeOptions.Serializer(serializer);
                
                
            }
            
            storeOptions.FieldSources.Add(new DateTimeNotSupported());
        }


        internal class DateTimeNotSupported : IFieldSource
        {
            private readonly Type[] _matchedTypes = new Type[]
                {typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset?), typeof(DateTimeOffset?)};
            
            public bool TryResolve(string dataLocator, StoreOptions options, ISerializer serializer, Type documentType,
                MemberInfo[] members, out IField field)
            {
                field = null;

                var memberType = members.Last().GetMemberType();
                if (_matchedTypes.Contains(memberType))
                {
                    throw new NotSupportedException("The CLR type System.DateTime isn't natively supported by Npgsql or your PostgreSQL. To use it with a PostgreSQL composite you need to specify DataTypeName or to map it, please refer to the documentation.");
                }
                
                return false;
            }
        }
    }
}