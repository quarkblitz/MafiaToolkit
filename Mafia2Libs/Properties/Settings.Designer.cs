﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Mafia2Tool.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.7.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E:/Games/Steam/steamapps/common/Mafia II/pc/sds/city/extracted/chinatown")]
        public string SDSPath {
            get {
                return ((string)(this["SDSPath"]));
            }
            set {
                this["SDSPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E:\\Games\\Steam\\steamapps\\common\\Mafia II\\pc\\sds\\shops\\extracted\\gunshop")]
        public string SDSPath1 {
            get {
                return ((string)(this["SDSPath1"]));
            }
            set {
                this["SDSPath1"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Users\\Connor\\Desktop\\SteamLibrary\\steamapps\\common\\Mafia II\\pc\\sds\\shops\\extra" +
            "cted\\gas")]
        public string SDSPath2 {
            get {
                return ((string)(this["SDSPath2"]));
            }
            set {
                this["SDSPath2"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public int CurrentVersion {
            get {
                return ((int)(this["CurrentVersion"]));
            }
            set {
                this["CurrentVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-1")]
        public int CloudVersion {
            get {
                return ((int)(this["CloudVersion"]));
            }
            set {
                this["CloudVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("D:\\Users\\Connor\\Desktop\\SteamLibrary\\steamapps\\common\\Mafia II\\edit\\materials")]
        public string MaterialPath {
            get {
                return ((string)(this["MaterialPath"]));
            }
            set {
                this["MaterialPath"] = value;
            }
        }
    }
}
