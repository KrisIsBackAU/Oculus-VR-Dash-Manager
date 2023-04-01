using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OVR_Dash_Manager.Functions
{
    public static class JSON_Functions
    {
        public static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            MaxDepth = 99999,
            NullValueHandling = NullValueHandling.Ignore,
            Error = (s, e) => { e.ErrorContext.Handled = true; },
            Formatting = Formatting.None
        };

        public static string SerializeClass(object Class, JsonSerializerSettings Settings = null)
        {
            string Serialed = "";

            if (Settings == null)
                Settings = JsonSettings;

            try
            {
                Serialed = JsonConvert.SerializeObject(Class, Settings);
            }
            catch (Exception)
            {
            }

            return Serialed;
        }

        public static object DeseralizeClass(string Data, Type DataType, JsonSerializerSettings Settings = null)
        {
            object Deseralized = null;

            if (Settings == null)
                Settings = JsonSettings;

            try
            {
                Deseralized = JsonConvert.DeserializeObject(Data, DataType, Settings);
            }
            catch (Exception)
            {
            }

            return Deseralized;
        }

        public static T DeseralizeClass<T>(string Data, JsonSerializerSettings Settings = null, params string[] IgnoreFields)
        {
            T Deseralized;

            if (Settings == null)
                Settings = JsonSettings;

            if (IgnoreFields.Length > 0)
            {
                IgnorableSerializerContractResolver jsonResolver = new IgnorableSerializerContractResolver();
                jsonResolver.Ignore(typeof(T), IgnoreFields);
                JsonSerializerSettings NewSettings = DeepCopySettings(Settings);
                NewSettings.ContractResolver = jsonResolver;
            }

            try
            {
                Deseralized = JsonConvert.DeserializeObject<T>(Data, Settings);
            }
            catch (Exception)
            {
                Deseralized = default(T);
            }

            return Deseralized;
        }

        public static JsonSerializerSettings DeepCopySettings(JsonSerializerSettings serializer)
        {
            var copiedSerializer = new JsonSerializerSettings
            {
                Context = serializer.Context,
                Culture = serializer.Culture,
                ContractResolver = serializer.ContractResolver,
                Converters = serializer.Converters,
                ConstructorHandling = serializer.ConstructorHandling,
                CheckAdditionalContent = serializer.CheckAdditionalContent,
                DateFormatHandling = serializer.DateFormatHandling,
                DateFormatString = serializer.DateFormatString,
                DateParseHandling = serializer.DateParseHandling,
                DateTimeZoneHandling = serializer.DateTimeZoneHandling,
                DefaultValueHandling = serializer.DefaultValueHandling,
                EqualityComparer = serializer.EqualityComparer,
                FloatFormatHandling = serializer.FloatFormatHandling,
                Formatting = serializer.Formatting,
                FloatParseHandling = serializer.FloatParseHandling,
                MaxDepth = serializer.MaxDepth,
                MetadataPropertyHandling = serializer.MetadataPropertyHandling,
                MissingMemberHandling = serializer.MissingMemberHandling,
                NullValueHandling = serializer.NullValueHandling,
                ObjectCreationHandling = serializer.ObjectCreationHandling,
                PreserveReferencesHandling = serializer.PreserveReferencesHandling,
                ReferenceLoopHandling = serializer.ReferenceLoopHandling,
                StringEscapeHandling = serializer.StringEscapeHandling,
                TraceWriter = serializer.TraceWriter,
                TypeNameHandling = serializer.TypeNameHandling,
                SerializationBinder = serializer.SerializationBinder,
                TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling
            };

            return copiedSerializer;
        }
    }

    /// <summary>
    /// Special JsonConvert resolver that allows you to ignore properties.  See https://stackoverflow.com/a/13588192/1037948
    /// </summary>
    public class IgnorableSerializerContractResolver : DefaultContractResolver
    {
        protected readonly Dictionary<Type, HashSet<string>> Ignores;

        public IgnorableSerializerContractResolver()
        {
            this.Ignores = new Dictionary<Type, HashSet<string>>();
        }

        /// <summary>
        /// Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void Ignore(Type type, params string[] propertyName)
        {
            // start bucket if DNE
            if (!this.Ignores.ContainsKey(type)) this.Ignores[type] = new HashSet<string>();

            foreach (var prop in propertyName)
            {
                this.Ignores[type].Add(prop);
            }
        }

        /// <summary>
        /// Is the given property for the given type ignored?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsIgnored(Type type, string propertyName)
        {
            if (!this.Ignores.ContainsKey(type)) return false;

            // if no properties provided, ignore the type entirely
            if (this.Ignores[type].Count == 0) return true;

            return this.Ignores[type].Contains(propertyName);
        }

        /// <summary>
        /// The decision logic goes here
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (this.IsIgnored(property.DeclaringType, property.PropertyName))
            {
                property.ShouldSerialize = instance => { return false; };
            }

            return property;
        }
    }
}