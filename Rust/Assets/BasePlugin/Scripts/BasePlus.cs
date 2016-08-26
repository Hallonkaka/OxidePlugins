//
//  By
//      Ron Dekker (www.RonDekker.nl, @RedKenrok)
//  
//  Distribution
//      http://www.GitHub.com/RedKenrok/OxidePlugins
//
//  License
//      GNU GENERAL PUBLIC LICENSE (Version 3, 29 June 2007)
//
using Oxide.Core;
using Oxide.Game.Rust.Cui;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace Oxide.Plugins {
    [Info("NewPlugin", "RedKenrok", "1.0.0", ResourceId = 99999)]
    [Description("A new and totally funtional plugin.")]
    // Rename 'NewPlugin' to the name of your plugin.
    public class NewPlugin : BasePlugin.BasePlus {
        /// <summary>The name of the plugin.</summary>
        public override string pluginName {
            get {
                // Change this to same name as the class.
                return "NewPlugin";
            }
        }
    }
}

namespace Oxide.Plugins.BasePlugin {
    /// <summary>Base class to extend plugins from.</summary>
    [Info("BasePlus", "RedKenrok", "1.0.0")]
    // Rename 'BasePlus' to 'base' followed by the name of your plugin.
    public class BasePlus : RustPlugin {
        #region Enums
        /// <summary>The different container types.</summary>
        public enum ContainerTypes { Static = 0, Dynamic = 1 };
        /// <summary>The amount of different container types.</summary>
        public static readonly int containerTypesCount = EnumPlus.Count<ContainerTypes>();

        /// <summary>The different component types.</summary>
        public enum ComponentTypes { Image = 0, RectTransform = 1, Text = 2 };
        /// <summary>The amount of different component types.</summary>
        public static readonly int componentTypesCount = EnumPlus.Count<ComponentTypes>();
        #endregion

        #region Variables
        /// <summary>The name of the plugin.</summary>
        public virtual string pluginName {
            get {
                // Change this to same name as the class.
                return "BasePlus";
            }
        }

        /// <summary>This instance of the plugin.</summary>
        public static BasePlus instance = null;

        /// <summary>Please use gameObject instead of this.</summary>
        /// <seealso cref="gameObject"/>
        private static GameObject _gameObject = null;
        /// <summary>The game object belonging to this plugin.</summary>
        public static GameObject gameObject {
            get {
                if (_gameObject == null) {
                    _gameObject = new GameObject(instance.pluginName + "Object");
                }
                return _gameObject;
            }
        }
        // Don't use the following. For some reason that breaks it...
        //public static GameObject gameObject = new GameObject(pluginName + "Object");

        /// <summary>The name of the parent of the ui elements's container.</summary>
        public static readonly string ContainerParent = "Overlay";
        #endregion

        #region Constructor
        public BasePlus() {
            // Adds singleton pattern.
            instance = this;
        }
        #endregion

        #region Configuration
        /// <summary>Default configuration loading function is overridden and has no functionality.</summary>
        protected override void LoadDefaultConfig() { }

        /// <summary>Checks the configuration file if the data is already present, if not it adds a default value.</summary>
        /// <typeparam name="T">The type of data.</typeparam>
        /// <param name="configName">The name of the data packet.</param>
        /// <param name="defaultValue">The default data to add if none is present.</param>
        /// <param name="defaultAddedToConfig">Wether or not a new field is added to the config.</param>
        /// <returns>Returns the data currently in the configuration file.</returns>
        public T CheckConfigFile<T>(string configName, T defaultValue, ref bool defaultApplied) {
            if (Config[configName] != null) {
                return (T)Config[configName];
            }
            else {
                Config[configName] = defaultValue;
                defaultApplied = true;
                return defaultValue;
            }
        }
        #endregion

        #region Cui
        /// <summary>The properties that make up an image component.</summary>
        public class ImageProperties {
            public static readonly string color = "Color";
            public static readonly string uri = "Uri";
        }

        /// <summary>The properties that make up a rect transform component.</summary>
        public class RectTransformProperties {
            public static readonly string anchorMin = "Anchor Min";
            public static readonly string anchorMax = "Anchor Max";
            public static readonly string offset = "Offset";
        }

        /// <summary>The properties that make up a text component.</summary>
        public class TextProperties {
            public static readonly string align = "Alignment";
            public static readonly string color = "Color";
            public static readonly string font = "Font";
            public static readonly string fontSize = "Font size";
            public static readonly string text = "Format";
        }

        /// <summary>A class able to contain the data making up a UI component.</summary>
        public struct ComponentData {
            /// <summary>The type of component.</summary>
            public ComponentTypes type;
            /// <summary>The data making up the component in the form of a dictionary setup using the properties class's.</summary>
            public Dictionary<string, object> data;

            /// <summary>Constructor call for initialing the data of the ComponentData.</summary>
            /// <param name="type">The type of component.</param>
            /// <param name="data">The data making up the component in the form of a dictionary setup using the properties class's.</param>
            public ComponentData(ComponentTypes type, Dictionary<string, object> data) {
                this.type = type;
                this.data = data;
            }

            /// <summary>Turns this ComponentData in a CuiImageComponent.</summary>
            /// <returns>A CuiImageComponent based on the data of this ComponentData.</returns>
            public CuiImageComponent ToImageComponent() {
                return ToImageComponent(data);
            }

            /// <summary>Turns data into a CuiImageComponent.</summary>
            /// <param name="data">The data to base the CuiImageComponent on.</param>
            /// <returns>A CuiImageComponent based on the data.</returns>
            public static CuiImageComponent ToImageComponent(Dictionary<string, object> data) {
                CuiImageComponent imageComponent = new CuiImageComponent();

                if (data.ContainsKey(ImageProperties.color)) {
                    imageComponent.Color = data[ImageProperties.color].ToString();
                }
                if (data.ContainsKey(ImageProperties.uri)) {
                    imageComponent.Png = data[ImageProperties.uri].ToString();
                    imageComponent.Sprite = "assets/content/textures/generic/fulltransparent.tga";
                }
                return imageComponent;
            }

            /// <summary>Turns this ComponentData in a CuiRectTransformComponent.</summary>
            /// <returns>A CuiRectTransformComponent based on the data of this ComponentData.</returns>
            public CuiRectTransformComponent ToRectTransformComponent() {
                return ToRectTransformComponent(data);
            }

            /// <summary>Turns data into a CuiRectTransformComponent.</summary>
            /// <param name="data">The data to base the CuiRectTransformComponent on.</param>
            /// <returns>A CuiRectTransformComponent based on the data.</returns>
            public static CuiRectTransformComponent ToRectTransformComponent(Dictionary<string, object> data) {
                CuiRectTransformComponent rectTransformComponent = new CuiRectTransformComponent();

                if (data.ContainsKey(RectTransformProperties.anchorMin)) {
                    rectTransformComponent.AnchorMin = data[RectTransformProperties.anchorMin].ToString();
                }
                if (data.ContainsKey(RectTransformProperties.anchorMax)) {
                    rectTransformComponent.AnchorMax = data[RectTransformProperties.anchorMax].ToString();
                }
                if (data.ContainsKey(RectTransformProperties.offset)) {
                    rectTransformComponent.OffsetMin = data[RectTransformProperties.offset].ToString();
                    rectTransformComponent.OffsetMax = data[RectTransformProperties.offset].ToString();
                }

                return rectTransformComponent;
            }

            /// <summary>Turns this ComponentData in a CuiTextComponent.</summary>
            /// <returns>A CuiTextComponent based on the data of this ComponentData.</returns>
            public CuiTextComponent ToTextComponent() {
                return ToTextComponent(data);
            }

            /// <summary>Turns data into a CuiTextComponent.</summary>
            /// <param name="data">The data to base the CuiTextComponent on.</param>
            /// <returns>A CuiTextComponent based on the data.</returns>
            public static CuiTextComponent ToTextComponent(Dictionary<string, object> data) {
                CuiTextComponent textComponent = new CuiTextComponent();

                if (data.ContainsKey(TextProperties.align)) {
                    textComponent.Align = EnumPlus.ToEnum<TextAnchor>(data[TextProperties.align].ToString());
                }
                if (data.ContainsKey(TextProperties.color)) {
                    textComponent.Color = data[TextProperties.color].ToString();
                }
                if (data.ContainsKey(TextProperties.font)) {
                    textComponent.Font = data[TextProperties.font].ToString();
                }
                if (data.ContainsKey(TextProperties.fontSize)) {
                    int fontSize = textComponent.FontSize;
                    if (!int.TryParse(data[TextProperties.fontSize].ToString(), out fontSize)) {
                        instance.PrintWarning("Could not succesfully parse " + data[TextProperties.fontSize].ToString() + ", a value retrieved from the configuration file, as a font size. Returned a default value of " + fontSize + " instead.");
                    }
                    textComponent.FontSize = fontSize;
                }
                if (data.ContainsKey(TextProperties.text)) {
                    textComponent.Text = data[TextProperties.text].ToString();
                }

                return textComponent;
            }
        }

        /// <summary>A class able to contain the data making up a UI element.</summary>
        public class ElementData {
            /// <summary>The name of the element.</summary>
            public string name;
            /// <summary>The name of the parent of the element.</summary>
            public string parent;
            /// <summary>The ComponentData of the CuiRectTransform component.</summary>
            public ComponentData rectTransformComponentData;
            /// <summary>The ComponentData of the other component making up the element.</summary>
            public ComponentData otherComponentData;

            /// <summary>Constructor call for initializing the data of the ElementData.</summary>
            /// <param name="name">The name of the element.</param>
            /// <param name="parent">The name of the parent of the element.</param>
            /// <param name="rectTransformComponentData">The ComponentData of the CuiRectTransform component.</param>
            /// <param name="otherComponentData">The ComponentData of the other component making up the element.</param>
            public ElementData(string name, string parent, ComponentData rectTransformComponentData, ComponentData otherComponentData) {
                this.name = name;
                this.parent = parent;
                this.rectTransformComponentData = rectTransformComponentData;
                this.otherComponentData = otherComponentData;
            }

            /// <summary>Turns the class into a CuiElement.</summary>
            /// <returns>The CuiElement based on the class' data.</returns>
            public CuiElement ToElement() {
                return ToElement(name, parent, rectTransformComponentData, otherComponentData);
            }

            /// <summary>Turns the givend data into a CuiElement.</summary>
            /// <param name="name">The name of the name of the element.</param>
            /// <param name="parent">The parent of the element.</param>
            /// <param name="rectTransformComponentData">The ComponentData of the CuiRectTransform component.</param>
            /// <param name="otherComponentData">The ComponentData of the other component making up the element.</param>
            /// <returns>The CuiElement based on the given data.</returns>
            public static CuiElement ToElement(string name, string parent, ComponentData rectTransformComponentData, ComponentData otherComponentData) {
                CuiElement element = new CuiElement {
                    Name = name,
                    Parent = parent
                };

                switch (otherComponentData.type) {
                    case ComponentTypes.Image:
                        element.Components.Add(otherComponentData.ToImageComponent());
                        break;
                    case ComponentTypes.Text:
                        element.Components.Add(otherComponentData.ToTextComponent());
                        break;
                }

                element.Components.Add(rectTransformComponentData.ToRectTransformComponent());

                return element;
            }
        }

        /// <summary>Turns an array of ElementData into a CuiElementContainer.</summary>
        /// <param name="elementsData">The array of ElementData.</param>
        /// <returns>The resulting CuiElementContainer.</returns>
        public static CuiElementContainer ToElementContainer(params ElementData[] elementsData) {
            CuiElementContainer elementContainer = new CuiElementContainer();
            for (int i = 0; i < elementsData.Length; i++) {
                elementContainer.Add(elementsData[i].ToElement());
            }
            return elementContainer;
        }

        public static string UniqueElementName(ContainerTypes containerType, string parentElementName, string elementName) {
            return instance.pluginName + containerType.ToString() + parentElementName + elementName;
        }
        #endregion

        #region FileStorage
        /// <summary>Unity script component for adding a file storage system readable by clients of the server.</summary>
        public class FileManager : MonoBehaviour {
            /// <summary>Please use instance instead of this.</summary>
            /// <seealso cref="instance"/>
            private static FileManager _instance = null;
            /// <summary>The instance of this script, on first call it will create the component attached to the plugins own game object.</summary>
            public static FileManager instance {
                get {
                    if (_instance == null) {
                        _instance = BaseUiPlus.gameObject.AddComponent<FileManager>();
                    }
                    return _instance;
                }
            }
            // Don't use the following. For some reason that breaks it...
            //public static FileManager instance = BasePlus.gameObject.AddComponent<FileManager>();

            /// <summary>The path leading to the data directory of the plugin.</summary>
            private static readonly string dataDirectoryPath = "file://" + Interface.Oxide.DataDirectory + Path.DirectorySeparatorChar;

            /// <summary>Dictionary containing the files by key.</summary>
            private static Dictionary<string, string> fileDictionary = new Dictionary<string, string>();

            /// <summary>Gets a collection containing the keys.</summary>
            public static Dictionary<string, string>.KeyCollection keys {
                get {
                    return fileDictionary.Keys;
                }
            }

            /// <summary>Gets a collection containing the values.</summary>
            public static Dictionary<string, string>.ValueCollection values {
                get {
                    return fileDictionary.Values;
                }
            }

            /// <summary>Determines whether key is present in the manager.</summary>
            /// <param name="key">The key.</param>
            /// <returns>Returns whether the key is contained in the manager.</returns>
            public static bool ContainsKey(string key) {
                return fileDictionary.ContainsKey(key);
            }

            /// <summary>Determines whether value is present in the manager.</summary>
            /// <param name="value">The value.</param>
            /// <returns>Returns whether the key is contained in the manager.</returns>
            public static bool ContainsValue(string value) {
                return fileDictionary.ContainsValue(value);
            }

            /// <summary>Gets the value associated with the specified key.</summary>
            /// <param name="key">The key.</param>
            /// <returns>Returns the value.</returns>
            public static string GetValue(string key) {
                return fileDictionary[key];
            }

            /// <summary>Gets the value associated with the specified key.</summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value associated.</param>
            /// <returns>Returns whether the retrieval was succesful</returns>
            public static bool TryGetValue(string key, out string value) {
                return fileDictionary.TryGetValue(key, out value);
            }

            /// <summary></summary>
            /// <param name="key"></param>
            public delegate void OnFileLoaded(string key, string value);

            /// <summary>Intializes the file into the file dictionary.</summary>
            /// <param name="key">The key by wich you will be able to request the file from the file dictionary.</param>
            /// <param name="uri">The path to the file.</param>
            /// <param name="Callbacks"></param>
            public static void InitializeFile(string pluginName, string key, string uri, params OnFileLoaded[] Callbacks) {
                StringBuilder uriBuilder = new StringBuilder();
                if (!uri.StartsWith("file:///") && !uri.StartsWith(("http://"))) {
                    uriBuilder.Append(dataDirectoryPath + pluginName + Path.DirectorySeparatorChar);
                }
                uriBuilder.Append(uri);
                instance.StartCoroutine(WaitForRequest(key, uriBuilder.ToString(), Callbacks));
            }

            /// <summary></summary>
            /// <param name="key"></param>
            /// <param name="uri"></param>
            /// <param name="Callbacks"></param>
            /// <returns></returns>
            private static IEnumerator WaitForRequest(string key, string uri, params OnFileLoaded[] Callbacks) {
                WWW www = new WWW(uri);
                yield return www;

                if (string.IsNullOrEmpty(www.error)) {
                    MemoryStream stream = new MemoryStream();
                    stream.Write(www.bytes, 0, www.bytes.Length);

                    if (!fileDictionary.ContainsKey(key)) {
                        fileDictionary.Add(key, "");
                    }
                    string value = fileDictionary[key] = FileStorage.server.Store(stream, FileStorage.Type.png, uint.MaxValue).ToString();

                    for (int i = 0; i < Callbacks.Length; i++) {
                        Callbacks[i](key, value);
                    }
                }
                else {
                    BasePlus.instance.PrintWarning(www.error);
                }
            }
        }
        #endregion

        #region EnumPlus
        /// <summary>Different helper function regarding enums.</summary>
        public class EnumPlus {
            /// <summary>Returns the item count of an enum.</summary>
            /// <typeparam name="T">The enum type.</typeparam>
            /// <returns>Returns the item count of the enum.</returns>
            public static int Count<T>() {
                return Enum.GetValues(typeof(T)).Length;
            }

            /// <summary>Turns a string into the correct enum value.</summary>
            /// <typeparam name="T">The enum type.</typeparam>
            /// <param name="typedString">The string of the same name as one of the values of the enum. It will automaticly remove empty spaces.</param>
            /// <param name="ignoreCapitalization">Whether to take capitalization into account.</param>
            /// <returns>Returns the enum value based on the given string.</returns>
            public static T ToEnum<T>(string typedString) {
                if (!typeof(T).IsEnum) {
                    instance.PrintError("Generic type T entered in EnumPlus must be an enumerated type.");
                }
                else if (string.IsNullOrEmpty(typedString)) {
                    instance.Puts("String entered is empty string, returned default value: " + default(T).ToString() + " instead.");
                }
                else {
                    foreach (T typedItem in Enum.GetValues(typeof(T))) {
                        if (typedItem.ToString().ToLower().Equals(typedString.Trim().ToLower())) {
                            return typedItem;
                        }
                    }
                    instance.PrintWarning("Unknown type entered: " + typedString + ", returned default value: " + default(T).ToString() + " instead.");
                }
                return default(T);
            }
        }
        #endregion

        #region MathPlus
        /// <summary></summary>
        public class MathPlus {
            /// <summary></summary>
            /// <param name="a"></param>
            /// <returns></returns>
            public static int PowOf2(int a) {
                return a * a;
            }

            /// <summary></summary>
            /// <param name="a"></param>
            /// <returns></returns>
            public static float PowOf2(float a) {
                return a * a;
            }

            /// <summary>Multiplies any number of vectors with each other.</summary>
            /// <param name="vectors">The vectors to be used in the equation</param>
            /// <returns>The total of each of the vectors.</returns>
            public static Vector4 Multiply(params Vector4[] vectors) {
                if (vectors.Length >= 1) {
                    Vector4 total = vectors[0];
                    for (int i = 1; i < vectors.Length; i++) {
                        total.x *= vectors[i].x;
                        total.y *= vectors[i].y;
                        total.z *= vectors[i].z;
                        total.w *= vectors[i].w;
                    }
                    return total;
                }
                return Vector4.zero;
            }

            /// <summary>Turns a string in any number of float values.</summary>
            /// <param name="s">The string that will be read.</param>
            /// <param name="seperator">By which character the values are seperated</param>
            /// <returns>An array filled with the split values.</returns>
            public static float[] ToFloatArray(string s, params char[] seperator) {
                string[] stringArray = s.Split(seperator);

                float[] floatArray = new float[stringArray.Length];
                for (int i = 0; i < stringArray.Length; i++) {
                    floatArray[i] = 0;
                    if (!float.TryParse(stringArray[i], out floatArray[i])) {
                        instance.PrintWarning("Could not succesfully parse " + stringArray[i] + ", a value retrieved from the configuration file, as a float value. Returned a default value of " + 0.ToString() + " instead.");
                    }
                }
                return floatArray;
            }

            /// <summary>Turns a string into a Vector2</summary>
            /// <param name="s">The string that will be read.</param>
            /// <param name="seperators">By which character the values are seperated</param>
            /// <returns>A Vector2 with the split values.</returns>
            public static Vector4 ToVector(string s, params char[] seperators) {
                float[] vectors = ToFloatArray(s, seperators);
                switch (vectors.Length) {
                    case 0:
                        return Vector4.zero;
                    case 1:
                        return new Vector4(vectors[0], 0);
                    case 2:
                        return new Vector4(vectors[0], vectors[1]);
                    case 3:
                        return new Vector4(vectors[0], vectors[1], vectors[2]);
                    case 4:
                    default:
                        return new Vector4(vectors[0], vectors[1], vectors[2], vectors[3]);
                }
            }
        }
        #endregion

        #region StringPlus
        /// <summary>A helper class for the string type.</summary>
        public class StringPlus {
            /// <summary>A struct able to container data for replacing parts of a string with one another.</summary>
            public struct ReplacementData {
                /// <summary>The part of the string that will be replaced.</summary>
                public readonly string from;
                /// <summary>The part of the string that will be added.</summary>
                public readonly string to;

                /// <summary>Constructor call for the ReplacementData struct</summary>
                /// <param name="from">The part of the string that will be replaced.</param>
                /// <param name="to">The part of the string that will be added.</param>
                public ReplacementData(string from, string to) {
                    this.from = from;
                    this.to = to;
                }
            }

            /// <summary>Reverses the characters in the string.</summary>
            /// <param name="s">The string to be reversed.</param>
            /// <returns>The newly reversed string.</returns>
            public static string Reverse(string s) {
                char[] charArray = s.ToCharArray();
                Array.Reverse(charArray);
                return new string(charArray);
            }

            /// <summary>Makes sure the number is transformed into a string having the given amount of characters.</summary>
            /// <param name="s">The string to fill or trim.</param>
            /// <param name="targetCharCount">The target amount of characters that make up the string.</param>
            /// <returns>The newly edited string.</returns>
            public static string Trim(string s, int targetCharCount) {
                if (s.Length > 0 && s.Length > targetCharCount) {
                    Reverse(Reverse(s).Remove(s.Length - targetCharCount));
                }
                return s;
            }

            /// <summary>Replaces the first string in the array with the second string in the array.</summary>
            /// <param name="s">The text in which you want to replace characters.</param>
            /// <param name="replacements">A string array with which part [0] will be replaced by part [1] of the text property.</param>
            /// <returns>The new string after applying the replacements.</returns>
            public static string Replace(string s, params ReplacementData[] replacements) {
                for (int i = 0; i < replacements.Length; i++) {
                    if (s.Contains(replacements[i].from)) {
                        s = s.Replace(replacements[i].from, replacements[i].to);
                    }
                }
                return s;
            }

            /// <summary>Simple function for turning a Vector2 into a string readable in Json.</summary>
            /// <param name="vector">The vector to be transformed.</param>
            /// <param name="seperator">The character to devide the values by.</param>
            /// <returns>The string based off the parsed vectors.</returns>
            public static string ToString(Vector2 vector, char seperator = ' ') {
                return vector.x.ToString() + seperator + vector.y.ToString();
            }

            /// <summary>Simple function for turning a Vector3 into a string readable in Json.</summary>
            /// <param name="vector">The vector to be transformed.</param>
            /// <param name="seperator">The character to devide the values by.</param>
            /// <returns>The string based off the parsed vectors.</returns>
            public static string ToString(Vector3 vector, char seperator = ' ') {
                return vector.x.ToString() + seperator + vector.y.ToString() + seperator + vector.z.ToString();
            }

            /// <summary>Simple function for turning a Vector4 into a string readable in Json.</summary>
            /// <param name="vector">The vector to be transformed.</param>
            /// <param name="seperator">The character to devide the values by.</param>
            /// <returns>The string based off the parsed vectors.</returns>
            public static string ToString(Vector4 vector, char seperator = ' ') {
                return vector.x.ToString() + seperator + vector.y.ToString() + seperator + vector.z.ToString() + seperator + vector.w.ToString();
            }
        }
        #endregion
    }
}