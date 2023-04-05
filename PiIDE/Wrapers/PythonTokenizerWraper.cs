using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows.Documents;

namespace PiIDE.Wrapers {
    public static class PythonTokenizerWraper {

        private static readonly ProcessStartInfo startInfo = new() {
            FileName = "Assets/Tokenizer/tokenize_python.exe",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
        };

        private static string output = "";
        private static bool recivedOutput;
        private static Process Process = new() {
            StartInfo = startInfo,
            EnableRaisingEvents = true,
        };

        static PythonTokenizerWraper() {

            Process.OutputDataReceived += (sender, e) => {
                if (e.Data == null)
                    return;
                output = e.Data;
                recivedOutput = true;
            };

            Process.ErrorDataReceived += (sender, e) => Debug.WriteLine("Error: " + e.Data);

            Process.Start();
            Process.BeginErrorReadLine();
            Process.BeginOutputReadLine();
        }

        public static TokenInfo[] Tokenize(string filePath) {

            Process.StandardInput.WriteLine(filePath);

            while (!recivedOutput)
                Thread.Sleep(10);
            recivedOutput = false;

            if (output == "")
                return Array.Empty<TokenInfo>();

            try {
                return JsonSerializer.Deserialize<TokenInfo[]>(output) ?? Array.Empty<TokenInfo>();
            } catch {
                return Array.Empty<TokenInfo>();
            }
        }

        public readonly struct TokenInfo {
            [JsonPropertyName("type")]
            public TokenName Type { get; init; }
            [JsonPropertyName("exact_type")]
            public TokenName ExactType { get; init; }
            [JsonPropertyName("string")]
            public string String { get; init; }
            [JsonPropertyName("start")]
            public int[] Start { get; init; }
            [JsonPropertyName("end")]
            public int[] End { get; init; }
        }

        public enum TokenName {
            ENDMARKER,
            NAME,
            NUMBER,
            STRING,
            NEWLINE,
            INDENT,
            DEDENT,
            LPAR,
            RPAR,
            LSQB,
            RSQB,
            COLON,
            COMMA,
            SEMI,
            PLUS,
            MINUS,
            STAR,
            SLASH,
            VBAR,
            AMPER,
            LESS,
            GREATER,
            EQUAL,
            DOT,
            PERCENT,
            LBRACE,
            RBRACE,
            EQEQUAL,
            NOTEQUAL,
            LESSEQUAL,
            GREATEREQUAL,
            TILDE,
            CIRCUMFLEX,
            LEFTSHIFT,
            RIGHTSHIFT,
            DOUBLESTAR,
            PLUSEQUAL,
            MINEQUAL,
            STAREQUAL,
            SLASHEQUAL,
            PERCENTEQUAL,
            AMPEREQUAL,
            VBAREQUAL,
            CIRCUMFLEXEQUAL,
            LEFTSHIFTEQUAL,
            RIGHTSHIFTEQUAL,
            DOUBLESTAREQUAL,
            DOUBLESLASH,
            DOUBLESLASHEQUAL,
            AT,
            ATEQUAL,
            RARROW,
            ELLIPSIS,
            COLONEQUAL,
            OP,
            AWAIT,
            ASYNC,
            TYPE_IGNORE,
            TYPE_COMMENT,
            SOFT_KEYWORD,
            ERRORTOKEN,
            COMMENT,
            NL,
            ENCODING,
            N_TOKENS,
            NT_OFFSET
        }
    }
}
