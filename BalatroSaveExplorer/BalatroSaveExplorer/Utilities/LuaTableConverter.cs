using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BalatroSaveExplorer.Utilities;

public static class LuaTableConverter
{
  // Serialize C# object to Lua table string
  public static string Serialize<T>(T obj)
  {
    var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      WriteIndented = false
    });
    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json, new JsonSerializerOptions
    {
      Converters = { new ObjectToInferredTypesConverter() }
    });
    return SerializeLuaTable(dict!);
  }

  // Deserialize Lua table string into C# object
  public static T Deserialize<T>(string luaTableString)
  {
    var dict = ParseLuaTable(luaTableString);
    var json = JsonSerializer.Serialize(dict);
    return JsonSerializer.Deserialize<T>(json)!;
  }

  // Convert Lua table string to JSON string
  public static string ToJson(string luaTableString)
  {
    var dict = ParseLuaTable(luaTableString);
    return JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
  }

  // Convert JSON string to Lua table string
  public static string FromJson<T>(string json)
  {
    var obj = JsonSerializer.Deserialize<T>(json)!;
    return Serialize(obj);
  }

  // -------------------- Lua Table Serialization --------------------

#nullable enable
  private static string SerializeLuaTable(object? obj)
  {
    if (obj is null)
      return "nil";

    if (obj is string s)
      return $"\"{s.Replace("\"", "\\\"")}\"";

    if (obj is bool b)
      return b ? "true" : "false";

    if (obj is double or float or decimal or int or long or short or byte)
      return Convert.ToString(obj, CultureInfo.InvariantCulture)!;

    if (obj is IList list)
    {
      var sb = new StringBuilder();
      sb.Append("{ ");
      for (int i = 0; i < list.Count; i++)
      {
        sb.Append($"[{i + 1}] = {SerializeLuaTable(list[i])}");
        if (i < list.Count - 1)
          sb.Append(", ");
      }
      sb.Append(" }");
      return sb.ToString();
    }

    if (obj is IDictionary<string, object> dict)
    {
      var sb = new StringBuilder();
      sb.Append("{ ");
      int count = 0;
      foreach (var (key, value) in dict)
      {
        sb.Append($"[\"{key}\"] = {SerializeLuaTable(value)}");
        if (++count < dict.Count)
          sb.Append(", ");
      }
      sb.Append(" }");
      return sb.ToString();
    }

    throw new NotSupportedException($"Unsupported type: {obj.GetType()}");
  }

  // -------------------- Lua Table Parsing (very basic recursive parser) --------------------

  public static Dictionary<string, object> ParseLuaTable(string input)
  {
    int index = 0;
    int line = 1;
    int col = 1;

    return ParseTable();

    Dictionary<string, object> ParseTable()
    {
      var dict = new Dictionary<string, object>();
      Expect('{');
      SkipWhitespace();

      while (Peek() != '}')
      {
        SkipWhitespace();
        string key = ParseKey();
        SkipWhitespace();
        Expect('=');
        SkipWhitespace();
        object value = ParseValue();
        dict[key] = value;
        SkipWhitespace();

        if (Peek() == ',')
        {
          Advance(); // skip comma
          SkipWhitespace();
        }
        else
        {
          break;
        }
      }

      Expect('}');
      return dict;
    }

    object ParseValue()
    {
      char c = Peek();
      if (c == '"')
        return ParseString();
      if (char.IsDigit(c) || c == '-' || c == '.')
        return ParseNumber();
      if (c == '{')
        return ParseTable();
      if (Match("true"))
        return true;
      if (Match("false"))
        return false;
      throw Error($"Unexpected character: '{c}'");
    }

    string ParseKey()
    {
      Expect('[');
      SkipWhitespace();
      string key;
      if (Peek() == '"')
        key = ParseString();
      else
        key = ParseNumber().ToString()!;
      SkipWhitespace();
      Expect(']');
      return key;
    }

    string ParseString()
    {
      Expect('"');
      var sb = new StringBuilder();
      while (true)
      {
        char c = Peek();
        if (c == '"')
          break;

        if (c == '\\')
        {
          Advance();
          char esc = Peek();
          Advance();
          sb.Append(esc); // Simplified escaping
        }
        else
        {
          sb.Append(c);
          Advance();
        }
      }
      Expect('"');
      return sb.ToString();
    }

    object ParseNumber()
    {
      int start = index;
      while (index < input.Length && (char.IsDigit(input[index]) || ".eE-+".Contains(input[index])))
        Advance();
      string numStr = input[start..index];
      if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
        return d;
      throw Error($"Invalid number format: '{numStr}'");
    }

    void SkipWhitespace()
    {
      while (index < input.Length && char.IsWhiteSpace(input[index]))
      {
        if (input[index] == '\n')
        {
          line++;
          col = 1;
        }
        else
        {
          col++;
        }
        index++;
      }
    }

    void Expect(char expected)
    {
      char actual = Peek();
      if (actual != expected)
        throw Error($"Expected '{expected}' but got '{actual}'");
      Advance();
    }

    bool Match(string token)
    {
      if (input[index..].StartsWith(token))
      {
        for (int i = 0; i < token.Length; i++) Advance();
        return true;
      }
      return false;
    }

    char Peek()
    {
      if (index >= input.Length)
        throw Error("Unexpected end of input");
      return input[index];
    }

    void Advance()
    {
      if (index < input.Length)
      {
        if (input[index] == '\n')
        {
          line++;
          col = 1;
        }
        else
        {
          col++;
        }
        index++;
      }
    }

    Exception Error(string message)
    {
      return new FormatException($"{message} (line {line}, char {col})");
    }
  }


  // -------------------- JSON Dynamic Type Converter --------------------

  private class ObjectToInferredTypesConverter : JsonConverter<object>
  {
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      return reader.TokenType switch
      {
        JsonTokenType.String => reader.GetString(),
        JsonTokenType.Number => reader.TryGetInt64(out long l) ? l : reader.GetDouble(),
        JsonTokenType.True => true,
        JsonTokenType.False => false,
        JsonTokenType.StartObject => JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options),
        JsonTokenType.StartArray => JsonSerializer.Deserialize<List<object>>(ref reader, options),
        JsonTokenType.Null => null,
        _ => throw new NotSupportedException($"Unsupported token: {reader.TokenType}")
      };
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
      JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
  }
}
