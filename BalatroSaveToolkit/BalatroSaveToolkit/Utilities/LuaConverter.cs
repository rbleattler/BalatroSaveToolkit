using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BalatroSaveToolkit.Utilities;

public static class LuaTableConverter {
    /// <summary>
    /// Converts a Lua table (in string form) into a Dictionary<object, T>.
    /// This simple converter supports a limited subset of Lua table notation.
    /// Example Lua table:
    /// {
    ///     name = "Lua",
    ///     version = 5.4,
    ///     features = { "lightweight", "embeddable", "fast" },
    ///     nested = {
    ///         key1 = "value1",
    ///         key2 = { subkey = "subvalue" }
    ///     }
    /// }
    /// </summary>
    /// <typeparam name="T">
    /// The type for values in the dictionary. (For nested tables, you might use object.)
    /// </typeparam>
    /// <param name="luaTable">The Lua table as a string.</param>
    /// <returns>A Dictionary<object, T> representing the Lua table.</returns>
    public static Dictionary<object, T> ConvertLuaTableToDictionary<T>(string luaTable) {
        // 1. Normalize whitespace.
        string normalized = Regex.Replace(luaTable, @"\s+", " ").Trim();

        // 2. Convert Lua-style key assignments like: name = "Lua"
        //    into JSON-like properties: "name": "Lua"
        normalized = Regex.Replace(normalized, @"(\w+)\s*=", "\"$1\":");

        // 3. Convert bracket notation for keys, e.g. [ "key" ] =
        normalized = Regex.Replace(
                                   normalized,
                                   @"

\[\s*""([^""]+)""\s*\]

\s*=",
                                   "\"$1\":"
                                  );

        // 4. Replace single quotes with double quotes.
        normalized = normalized.Replace("'", "\"");

        // 5. Remove trailing commas (which are invalid in JSON).
        normalized = Regex.Replace(normalized, @",\s*(\}|])", "$1");

        // 6. Convert Lua array-like tables to JSON arrays.
        //    Pure arrays will appear as curly-brace blocks without key-value pairs.
        normalized = ConvertArrayTables(normalized);

        // 7. If the top-level element is an array (has no colon), convert outer {} to [].
        if (normalized.StartsWith("{") &&
            normalized.EndsWith("}") &&
            normalized.IndexOf(":") == -1) {
            normalized = "[" + normalized.Substring(1, normalized.Length - 2).Trim() + "]";
        }

        try {
            // System.Text.Json does not support Dictionary<object, T> directly,
            // so first deserialize to Dictionary<string, T> and then copy to Dictionary<object, T>.
            var intermediate =
                JsonSerializer.Deserialize<Dictionary<string, T>>(
                                                                  normalized,
                                                                  new JsonSerializerOptions {
                                                                      PropertyNameCaseInsensitive = true
                                                                  }
                                                                 );

            Dictionary<object, T> result = new Dictionary<object, T>();
            if (intermediate != null) {
                foreach (var kvp in intermediate) { result.Add((object)kvp.Key, kvp.Value); }
            }

            return result;
        }
        catch (Exception ex) {
            Console.WriteLine("Error parsing Lua table: " + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Looks for inner table blocks that appear to be arrays (with no colons after key conversion)
    /// and replaces the curly braces with square brackets.
    /// </summary>
    private static string ConvertArrayTables(string input) {
        // This regex looks for a colon followed by a table that does not contain a colon.
        string pattern = @":\s*\{([^{}:]*?)\}";
        string result  = input;
        string newResult = Regex.Replace(
                                         result,
                                         pattern,
                                         m => {
                                             // Replace the outer braces with square brackets.
                                             return ": [" + m.Groups[1].Value + "]";
                                         }
                                        );

        // Loop until no more replacements occur (to handle nested array blocks).
        while (newResult != result) {
            result    = newResult;
            newResult = Regex.Replace(result, pattern, m => { return ": [" + m.Groups[1].Value + "]"; });
        }

        return result;
    }
}

// Example usage:
// public class LuaTest
// {
//     public static void Main()
//     {
//         string luaTableString = @"
//         {
//             name = ""Lua"",
//             version = 5.4,
//             features = { ""lightweight"", ""embeddable"", ""fast"" },
//             nested = {
//                 key1 = ""value1"",
//                 key2 = { subkey = ""subvalue"" }
//             }
//         }";
//
//         // Using object for T to allow nested tables to be represented as further dictionaries or JSON elements.
//         var result = LuaTableConverter.ConvertLuaTableToDictionary<object>(luaTableString);
//
//         if (result != null)
//         {
//             Console.WriteLine("Converted Dictionary:");
//             foreach (var kvp in result)
//             {
//                 Console.WriteLine($"{kvp.Key}: {kvp.Value}");
//             }
//         }
//         else
//         {
//             Console.WriteLine("Conversion failed.");
//         }
//     }
// }
