﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.42000
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GronkhTV_DL.Properties {
    using System;
    
    
    /// <summary>
    ///   Eine stark typisierte Ressourcenklasse zum Suchen von lokalisierten Zeichenfolgen usw.
    /// </summary>
    // Diese Klasse wurde von der StronglyTypedResourceBuilder automatisch generiert
    // -Klasse über ein Tool wie ResGen oder Visual Studio automatisch generiert.
    // Um einen Member hinzuzufügen oder zu entfernen, bearbeiten Sie die .ResX-Datei und führen dann ResGen
    // mit der /str-Option erneut aus, oder Sie erstellen Ihr VS-Projekt neu.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Gibt die zwischengespeicherte ResourceManager-Instanz zurück, die von dieser Klasse verwendet wird.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GronkhTV_DL.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Überschreibt die CurrentUICulture-Eigenschaft des aktuellen Threads für alle
        ///   Ressourcenzuordnungen, die diese stark typisierte Ressourcenklasse verwenden.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die var CScallback = arguments[arguments.length - 1];
        ///async function makeRequest(id) {
        ///    var data = await fetch(`https://api.gronkh.tv/v1/video/playlist?episode=${id}`);
        ///    try {
        ///        data = await data.json();
        ///        if (data[&quot;playlist_url&quot;]) {
        ///            return CScallback(JSON.stringify(data[&quot;playlist_url&quot;]));
        ///        }
        ///        return CScallback(JSON.stringify(data));
        ///    }
        ///    catch (e) {
        ///        return CScallback(JSON.stringify(e));
        ///    }
        ///};
        ///
        ///makeRequest(arguments[0]); ähnelt.
        /// </summary>
        public static string fetchPlaylist {
            get {
                return ResourceManager.GetString("fetchPlaylist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die var CScallback = arguments[arguments.length - 1];
        ///async function makeRequest(url) {
        ///    console.log(url);
        ///    var data = await fetch(url);
        ///    try {
        ///        data = await data.text();
        ///        return CScallback(data);
        ///    }
        ///    catch (e) {
        ///        return CScallback(JSON.stringify(e));
        ///    }
        ///};
        ///
        ///makeRequest(arguments[0]); ähnelt.
        /// </summary>
        public static string fetchPlaylistDetailed {
            get {
                return ResourceManager.GetString("fetchPlaylistDetailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Sucht eine lokalisierte Zeichenfolge, die var CScallback = arguments[arguments.length - 1];
        ///async function makeRequest() {
        ///    var data = await fetch(&quot;https://api.gronkh.tv/v1/search?first=24&amp;direction=desc&amp;sort=date&quot;);
        ///    try {
        ///        data = await data.json();
        ///        if (data[&quot;result&quot;]) {
        ///            if (data[&quot;result&quot;][&quot;videos&quot;]) {
        ///                if (data[&quot;result&quot;][&quot;videos&quot;].length &gt; 0) {
        ///                    return CScallback(JSON.stringify(data[&quot;result&quot;][&quot;videos&quot;]));
        ///                }
        ///            }
        ///            return CScallback(JSO [Rest der Zeichenfolge wurde abgeschnitten]&quot;; ähnelt.
        /// </summary>
        public static string fetchStreams {
            get {
                return ResourceManager.GetString("fetchStreams", resourceCulture);
            }
        }
    }
}
