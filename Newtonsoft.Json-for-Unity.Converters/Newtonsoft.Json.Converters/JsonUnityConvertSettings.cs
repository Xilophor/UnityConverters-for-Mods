using Newtonsoft.Json.UnityConverters.Helpers;
using UnityEngine;

namespace Newtonsoft.Json.UnityConverters;

public static class JsonUnityConvertSettings
{
    /// <summary>
    /// Create the converter instances.
    /// </summary>
    /// <returns>The converters.</returns>
    internal static List<JsonConverter> CreateUnityConverters()
    {
        var converterTypes = new List<Type>();
        var grouping = ConverterGrouping.Create(FindConverters());
        converterTypes.AddRange(grouping.unityConverters);

        var result = new List<JsonConverter>();
        result.AddRange(converterTypes.Select(CreateConverter)!);
        return result;
    }
    
    /// <summary>
    /// Create the converter instances.
    /// </summary>
    /// <returns>The converters.</returns>
    internal static List<JsonConverter> CreateConverters()
    {
        var converterTypes = new List<Type>();
        var grouping = ConverterGrouping.Create(FindConverters());
        converterTypes.AddRange(grouping.outsideConverters);
        converterTypes.AddRange(grouping.unityConverters);
        converterTypes.AddRange(grouping.jsonNetConverters);

        var result = new List<JsonConverter>();
        result.AddRange(converterTypes.Select(CreateConverter)!);
        return result;
    }
    
    /// <summary>
    /// Finds all the valid converter types inside the <c>Newtonsoft.Json</c> assembly.
    /// </summary>
    /// <returns>The types.</returns>
    internal static IEnumerable<Type> FindConverters()
    {
#if UNITY_2019_2_OR_NEWER && UNITY_EDITOR
            var types = UnityEditor.TypeCache.GetTypesDerivedFrom<JsonConverter>();
#else
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(dll => dll.GetLoadableTypes());
#endif
        return FilterToJsonConvertersAndOrder(types);
    }

    private static IEnumerable<Type> FilterToJsonConvertersAndOrder(IEnumerable<Type> types)
    {
        return types
            .Where(type
                => typeof(JsonConverter).IsAssignableFrom(type)
                   && !type.IsAbstract && !type.IsGenericTypeDefinition
                   && type.GetConstructor(Array.Empty<Type>()) != null
            )
            .OrderBy(type => type.FullName);
    }
    
    /// <summary>
    /// Try to create the converter of specified type.
    /// </summary>
    /// <returns>The converter.</returns>
    /// <param name="jsonConverterType">Type.</param>
    private static JsonConverter? CreateConverter(Type jsonConverterType)
    {
        try
        {
            return (JsonConverter)Activator.CreateInstance(jsonConverterType)!;
        }
        catch (Exception exception)
        {
            Debug.LogErrorFormat("Cannot create JsonConverter '{0}':\n{1}", jsonConverterType?.FullName, exception);
        }

        return null;
    }

    public static JsonSerializerSettings UnityConverters => new() { Converters = CreateUnityConverters() };
    public static JsonSerializerSettings AllConverters => new() { Converters = CreateConverters() };
}
