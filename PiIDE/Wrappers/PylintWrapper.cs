using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PiIDE.Wrappers;

internal static class PylintWrapper {

    public const string PylintPath = "pylint";

    public static async Task<PylintMessage[]> GetLintingAsync(string[] filePaths) {

        string args = $"--output-format=json --msg-template=\"{{path}}({{line}}): [{{msg_id}}{{obj}}] {{msg}}\" -j 0 \"{string.Join("\" \"", filePaths)}\"";

        Process pylintProcess = new() {
            StartInfo = new ProcessStartInfo {
                UseShellExecute = false,
                FileName = PylintPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            }
        };

        pylintProcess.Start();
        string output = await pylintProcess.StandardOutput.ReadToEndAsync();
        await pylintProcess.WaitForExitAsync();
        try {
            return JsonSerializer.Deserialize<PylintMessage[]>(output) ?? Array.Empty<PylintMessage>();
        } catch {
            return Array.Empty<PylintMessage>();
        }
    }
}

#pragma warning disable IDE0079
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
#pragma warning restore IDE0079
// ReSharper disable once ClassNeverInstantiated.Global
public class PylintMessage {
    [JsonPropertyName("module")]
    public string Module { get; set; } = "";
    [JsonPropertyName("obj")]
    public string Obj { get; set; } = "";
    [JsonPropertyName("line")]
    public int Line { get; set; }
    [JsonPropertyName("column")]
    public int Column { get; set; }
    [JsonPropertyName("endLine")]
    public int? EndLine { get; set; }
    [JsonPropertyName("endColumn")]
    public int? EndColumn { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";
    [JsonPropertyName("message")]
    public string Message { get; set; } = "";
    [JsonPropertyName("message-id")]
    public string MessageId { get; set; } = "";

    private string PrivateType = "";

    [JsonPropertyName("type")]
    public string Type {
        get => PrivateType;
        set {
            ForegroundColor = PylintMessageColors.MessageTypeToColor(value);
            Icon = PylintMessageIcons.MessageTypeToIcon(value);
            Icon.Foreground = PylintMessageColors.MessageTypeToColor(value);
            PrivateType = value;
        }
    }

    public FontAwesome.WPF.FontAwesome? Icon { get; private set; }
    public Brush? ForegroundColor { get; private set; }
}