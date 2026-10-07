using System.Text.Json.Serialization;

namespace clock.Services;

[JsonSerializable(typeof(TimeStateSnapshot))]
public partial class TimeStateJsonContext : JsonSerializerContext { }
