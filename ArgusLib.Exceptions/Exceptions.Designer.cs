﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ArgusLib {
    using System;
    using System.Reflection;
    
    
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
    public class Exceptions {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Exceptions() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ArgusLib.Exceptions", typeof(Exceptions).GetTypeInfo().Assembly);
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
        ///   Looks up a localized string similar to Must not be an empty array..
        /// </summary>
        public static string Argument_ArrayMustNotBeEmpty {
            get {
                return ResourceManager.GetString("Argument_ArrayMustNotBeEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be a number (not NaN)..
        /// </summary>
        public static string Argument_NotNaN {
            get {
                return ResourceManager.GetString("Argument_NotNaN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be finite..
        /// </summary>
        public static string ArgumentOutOfRange_MustBeFinite {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_MustBeFinite", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be greater than {0}..
        /// </summary>
        public static string ArgumentOutOfRange_MustBeGreaterThan {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_MustBeGreaterThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be in range {0}..
        /// </summary>
        public static string ArgumentOutOfRange_MustBeInRange {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_MustBeInRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be smaller than {0}..
        /// </summary>
        public static string ArgumentOutOfRange_MustBeSmallerThan {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_MustBeSmallerThan", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must not be negative..
        /// </summary>
        public static string ArgumentOutOfRange_MustNotBeNegative {
            get {
                return ResourceManager.GetString("ArgumentOutOfRange_MustNotBeNegative", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This code path should never have been reached..
        /// </summary>
        public static string BugException {
            get {
                return ResourceManager.GetString("BugException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type &apos;{0}&apos; is not supported as generic type parameter..
        /// </summary>
        public static string GenericTypeParameterNotSupportetException {
            get {
                return ResourceManager.GetString("GenericTypeParameterNotSupportetException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsupported operator: &apos;{0}&apos;.
        /// </summary>
        public static string OperatorProvider_UnsupportedOperator {
            get {
                return ResourceManager.GetString("OperatorProvider_UnsupportedOperator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid polynomial ({0}): Must have least significant bet set..
        /// </summary>
        public static string PrbsGenerator_InvalidPolynomial {
            get {
                return ResourceManager.GetString("PrbsGenerator_InvalidPolynomial", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must not be 0 (zero)..
        /// </summary>
        public static string PrbsGenerator_SeedMustNotBeZero {
            get {
                return ResourceManager.GetString("PrbsGenerator_SeedMustNotBeZero", resourceCulture);
            }
        }
    }
}