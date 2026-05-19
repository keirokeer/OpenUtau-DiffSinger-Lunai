using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenUtau.Core.Format {
  internal sealed class SvpFileJson {
    public JToken? version { get; set; }
    public SvpTimeJson? time { get; set; }
    public List<SvpGroupJson>? library { get; set; }
    public List<SvpTrackJson>? tracks { get; set; }
  }

  internal sealed class SvpTimeJson {
    public List<SvpMeterJson>? meter { get; set; }
    public List<SvpTempoJson>? tempo { get; set; }
  }

  internal sealed class SvpMeterJson {
    public int index { get; set; }
    public int numerator { get; set; }
    public int denominator { get; set; }
  }

  internal sealed class SvpTempoJson {
    public double position { get; set; }
    public double bpm { get; set; }
  }

  internal sealed class SvpTrackJson {
    public string? name { get; set; }
    public int dispOrder { get; set; }
    public SvpParametersJson? parameters { get; set; }
    public SvpGroupJson? mainGroup { get; set; }
    public SvpRefJson? mainRef { get; set; }
    public List<SvpRefJson>? groups { get; set; }
  }

  internal sealed class SvpGroupJson {
    public string? name { get; set; }
    public string? uuid { get; set; }
    public List<SvpNoteJson>? notes { get; set; }
    public SvpParametersJson? parameters { get; set; }
  }

  internal sealed class SvpRefJson {
    public string? groupID { get; set; }
    public double blickOffset { get; set; }
    public int pitchOffset { get; set; }
    public SvpVoiceJson? voice { get; set; }
    public SvpCurveJson? systemPitchDelta { get; set; }
  }

  internal sealed class SvpVoiceJson {
    [JsonConverter(typeof(SvpFlexibleDoubleDictionaryConverter))]
    public Dictionary<string, double>? vocalModeParams { get; set; }
  }

  internal sealed class SvpParametersJson {
    public SvpCurveJson? pitchDelta { get; set; }
    public SvpCurveJson? vibratoEnv { get; set; }
  }

  internal sealed class SvpCurveJson {
    public string? mode { get; set; }
    public JToken? points { get; set; }
  }

  internal sealed class SvpNoteJson {
    public double onset { get; set; }
    public double duration { get; set; }
    public string? lyrics { get; set; }
    public string? phonemes { get; set; }
    public int pitch { get; set; }
    [JsonConverter(typeof(SvpFlexibleDoubleDictionaryConverter))]
    public Dictionary<string, double>? attributes { get; set; }
  }

  /// <summary>Reads numeric JSON object values into double dictionaries (ignores non-numbers).</summary>
  internal sealed class SvpFlexibleDoubleDictionaryConverter : JsonConverter<Dictionary<string, double>?> {
    public override Dictionary<string, double>? ReadJson(
        JsonReader reader, System.Type objectType, Dictionary<string, double>? existingValue,
        bool hasExistingValue, JsonSerializer serializer) {
      if (reader.TokenType == JsonToken.Null) {
        return null;
      }
      var obj = JObject.Load(reader);
      var dict = new Dictionary<string, double>();
      foreach (var prop in obj.Properties()) {
        if (prop.Value.Type is JTokenType.Integer or JTokenType.Float) {
          dict[prop.Name] = prop.Value.Value<double>();
        }
      }
      return dict;
    }

    public override void WriteJson(JsonWriter writer, Dictionary<string, double>? value, JsonSerializer serializer) {
      serializer.Serialize(writer, value);
    }
  }
}
