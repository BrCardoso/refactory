using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetCoreJobsMicroservice.Converters
{
	public class DoubleConverter : JsonConverter<double>
	{
		public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			double currentValue = reader.GetDouble();
			if (currentValue == 0)
				return currentValue;

			string value = currentValue.ToString("#.##");
			if (!string.IsNullOrEmpty(value))
			{
				var d = double.Parse(value);
				return d;
			}

			return currentValue;
		}

		public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
		{
			if (value == 0)
			{
				writer.WriteNumberValue(value);
				return;
			}

			string s = value.ToString("#.##");

			if (!string.IsNullOrEmpty(s))
			{
				var d = double.Parse(s);
				writer.WriteNumberValue(d);
				return;
			}
		}
	}
}