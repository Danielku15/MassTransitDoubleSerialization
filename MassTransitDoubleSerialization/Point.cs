using System.Text.Json;
using System.Text.Json.Serialization;

namespace MassTransitDoubleSerialization;

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class PointConverter : JsonConverter<Point>
{
    public override Point? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Unexpected token, expected an object with x,y properties");
        }

        var originalDepth = reader.CurrentDepth;

        if (reader.TokenType == JsonTokenType.Null)
        {
            reader.Read();
            return null;
        }

        reader.Read();

        var x = double.NaN;
        var y = double.NaN;

        while (reader.TokenType == JsonTokenType.PropertyName)
        {
            var name = reader.GetString()?.ToLower();
            reader.Read();

            try
            {
                switch (name)
                {
                    case "x":
                        if (reader.TokenType == JsonTokenType.Number)
                        {
                            x = reader.GetDouble();
                        }
                        else
                        {
                            throw new JsonException($"{name.ToUpper()}-component was no number!");
                        }
                        break;
                    case "y":
                        if (reader.TokenType == JsonTokenType.Number)
                        {
                            y = reader.GetDouble();
                        }
                        else
                        {
                            throw new JsonException($"{name.ToUpper()}-component was no number!");
                        }
                        break;
                }
                reader.Read();
            }
            catch (Exception e) when (e is InvalidOperationException or FormatException)
            {
                throw new JsonException($"{name?.ToUpper()}-component of the coordinate was missing or malformatted");
            }
        }

        while (reader.TokenType != JsonTokenType.EndObject || reader.CurrentDepth != originalDepth)
        {
            reader.Read();
        }

        if (double.IsNaN(x))
        {
            throw new JsonException("X-component of the coordiante was missing or malformatted");
        }
        
        if (double.IsNaN(y))
        {
            throw new JsonException("Y-component of the coordiante was missing or malformatted");
        }

        return new Point { X = x, Y = y };
    }

    public override void Write(Utf8JsonWriter writer, Point? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WriteNumber("x", value.X);
        writer.WriteNumber("y", value.Y);
        writer.WriteEndObject();
    }
}