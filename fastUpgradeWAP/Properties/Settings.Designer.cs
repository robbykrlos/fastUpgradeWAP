﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace fastUpgradeWAP.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.7.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\DATA\\Apache\\Apache2.4\\")]
        public string APACHE_ROOT {
            get {
                return ((string)(this["APACHE_ROOT"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("c:\\DATA\\Apache\\PHP-IIS\\")]
        public string PHP_ROOT {
            get {
                return ((string)(this["PHP_ROOT"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Apache2.4")]
        public string APACHE_RELATED_SERVICES_TO_STOP {
            get {
                return ((string)(this["APACHE_RELATED_SERVICES_TO_STOP"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("conf,modules\\mod_fcgid.so")]
        public string APACHE_FILES_TO_COPY {
            get {
                return ((string)(this["APACHE_FILES_TO_COPY"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"php.ini,tmp_sessions,tmp_uploads,ext\php_redis.dll,ext\cairo.dll,ext\expat.dll,ext\fontconfig.dll,ext\gobject-2.dll,ext\libharfbuzz-0.dll,ext\librrd-4.dll,ext\pango-1.dll,ext\pangocairo-1.dll,ext\pangoft2-1.dll,ext\pangowin32-1.dll,ext\php_rrd.dll,ext\pixman-1.dll")]
        public string PHP_FILES_TO_COPY {
            get {
                return ((string)(this["PHP_FILES_TO_COPY"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string PHP_RELATED_SERVICES_TO_STOP {
            get {
                return ((string)(this["PHP_RELATED_SERVICES_TO_STOP"]));
            }
        }
    }
}
