using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Data.Common;

namespace SunnyRewards.Helios.ETL.Common.Mappings
{
    // inspired by
    // https://gist.github.com/bariloce/e65fe5db6c6ddf46e6f8
    // https://web.archive.org/web/20150214164507/http://blog.miraclespain.com/archive/2008/Mar-18.html
    // https://nhibernate.info/blog/2009/10/15/mapping-different-types-iusertype.html
    public class StringAsJsonb : IUserType
    {
        bool IUserType.Equals(object x, object y)
        {
            return string.Equals((string)x, (string)y);
        }

        public int GetHashCode(object x)
        {
            return x == null ? 0 : x.GetHashCode();
        }

        public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var value = rs[names[0]];
            return value != DBNull.Value
                ? value
                : null;
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            var parameter = (NpgsqlParameter)cmd.Parameters[index];
            parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
            parameter.Value = value ?? DBNull.Value;
        }

        public object DeepCopy(object value)
        {
            return value;
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Assemble(object cached, object owner)
        {
            return cached;
        }

        public object Disassemble(object value)
        {
            return value;
        }

        public SqlType[] SqlTypes => new[] { new SqlType(DbType.String) };

        public Type ReturnedType => typeof(string);

        public bool IsMutable => false;
    }
}

