﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PiIDE.Options.Editor.SyntaxHighlighter {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.5.0.0")]
    internal sealed partial class SyntaxHighlighterSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static SyntaxHighlighterSettings defaultInstance = ((SyntaxHighlighterSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new SyntaxHighlighterSettings())));
        
        public static SyntaxHighlighterSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool HighlightJediNames {
            get {
                return ((bool)(this["HighlightJediNames"]));
            }
            set {
                this["HighlightJediNames"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool HighlightKeywords {
            get {
                return ((bool)(this["HighlightKeywords"]));
            }
            set {
                this["HighlightKeywords"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool HighlightBrackets {
            get {
                return ((bool)(this["HighlightBrackets"]));
            }
            set {
                this["HighlightBrackets"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool HighlightIndentation {
            get {
                return ((bool)(this["HighlightIndentation"]));
            }
            set {
                this["HighlightIndentation"] = value;
            }
        }
    }
}
