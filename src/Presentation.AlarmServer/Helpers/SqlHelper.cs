using Microsoft.Data.SqlClient;
using System;

namespace Presentation.AlarmServer.Helpers;

class SqlHelper
{
    /// <summary>
    ///     Returns false if the i'th value in r is null, else the value.
    /// </summary>
    internal static bool BooleanOrNull(SqlDataReader r, int i)
    {
        if (r.IsDBNull(i))
            return false;
        return r.GetBoolean(i);
    }

    /// <summary>
    ///     Returns DateTime.MinValue if the i'th value in r is null, else the value.
    /// </summary>
    internal static DateTime DateTimeOrNull(SqlDataReader r, int i)
    {
        if (r.IsDBNull(i))
            return DateTime.MinValue;
        return r.GetDateTime(i);
    }

    /// <summary>
    ///     Returns NaN if the i'th value in r is null, else the value.
    /// </summary>
    internal static double DoubleOrNull(SqlDataReader r, int i)
    {
        if (r.IsDBNull(i))
            return double.NaN;
        return r.GetDouble(i);
    }

    /// <summary>
    ///     Returns Int32.MinValue if the i'th value in r is null, else the value.
    /// </summary>
    internal static int Int32OrNull(SqlDataReader r, int i)
    {
        if (r.IsDBNull(i))
            return int.MinValue;
        return r.GetInt32(i);
    }

    /// <summary>
    ///     Returns null if the i'th value in r is null, else the string.
    /// </summary>
    internal static string StringOrNull(SqlDataReader r, int i)
    {
        if (r.IsDBNull(i))
            return null;
        return r.GetString(i).Trim();
    }
}