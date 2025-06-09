using System;
using System.Collections.Generic;
using System.Linq;

namespace Metron2ParserTests.Helpers;

public static class EnumerableTestDataExtensions
{
    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<T> source)
        => source.Select(entry => new object[] { entry });
    
    public static IEnumerable<object[]> ToTestData<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> source) =>
        source.Select((a, b) => new object[] { a, b });

    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<T> source, Func<T, object> selector) =>
        source.Select(entry => new[] { selector(entry) });

    public static IEnumerable<object[]> ToTestData<T>(this IEnumerable<T> source, params Func<T, object>[] selectors) =>
        source.Select(entry => selectors.Select(s => s(entry)).ToArray());

}