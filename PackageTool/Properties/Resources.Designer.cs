﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PackageTool.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PackageTool.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
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
        ///   Looks up a localized string similar to http://webservice2.intdesignservices.com/.
        /// </summary>
        public static string ActivationUrl {
            get {
                return ResourceManager.GetString("ActivationUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt; 
        ///&lt;recentdata&gt;
        ///  &lt;!-- Export tab --&gt;
        ///  &lt;version&gt;1.1&lt;/version&gt;
        ///  &lt;type /&gt;
        ///  &lt;cfg /&gt;
        ///  &lt;packagedirectory /&gt;
        ///  &lt;date /&gt;
        ///  &lt;printerinstance /&gt;
        ///  &lt;!-- Transmittal tab --&gt;
        ///  &lt;projectnumber /&gt;
        ///  &lt;transmittalnumber /&gt;
        ///  &lt;projectname /&gt;
        ///  &lt;location /&gt;
        ///  &lt;remarks /&gt;
        ///  &lt;attention /&gt;
        ///  &lt;signatory /&gt;
        ///  &lt;outputdirectory /&gt;
        ///&lt;/recentdata&gt;
        ///.
        /// </summary>
        public static string pkgrecentdata {
            get {
                return ResourceManager.GetString("pkgrecentdata", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://idsftpserver.com/rd/update/.
        /// </summary>
        public static string ServerPath {
            get {
                return ResourceManager.GetString("ServerPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 21.1.
        /// </summary>
        public static string TeklaTargetVersion {
            get {
                return ResourceManager.GetString("TeklaTargetVersion", resourceCulture);
            }
        }
    }
}
