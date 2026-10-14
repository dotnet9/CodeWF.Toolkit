using System.Text.Json.Serialization;

namespace CodeWF.Toolkit.Core.UpdateChecking;

/// <summary>
/// 更新检查模型的 JSON 源生成上下文：NativeAOT / 裁剪下不依赖反射序列化器，
/// 也避免 <c>JsonSerializer.Deserialize(string, JsonSerializerOptions)</c> 在新 SDK 下的重载歧义。
/// 选项与 <see cref="GitHubJsonOptions.DefaultOptions"/> 对齐（camelCase、忽略 null）。
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(UpdateCheckState))]
internal sealed partial class UpdateJsonContext : JsonSerializerContext;
