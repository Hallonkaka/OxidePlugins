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
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

using UnityEngine;

namespace Oxide.Plugins {
    [Info("UiPlus", "RedKenrok", "1.1.3", ResourceId = 2088)]
    [Description("Adds user elements to the user interface containing; the active players count, maximum player slots, sleeping players, and ingame time.")]
    public class UiPlus : BasePlugin.BaseUiPlus {
        /// <summary>The name of the plugin.</summary>
        public override string pluginName {
            get {
                return "UiPlus";
            }
        }

        #region Enums
        /// <summary>The different panels that the plugin can display.</summary>
        private enum PanelTypes { Active = 0, Sleeping = 1, Clock = 2 };
        /// <summary>The amount of different panels.</summary>
        private static readonly int panelTypesCount = EnumPlus.Count<PanelTypes>();

        /// <summary>The different fields that the plugin can display per panel.</summary>
        private enum FieldTypes { PlayersActive, PlayerMax, PlayersSleeping, Time };
        #endregion

        #region Classes
        private class ComponentIconStatesConfig : ComponentIconConfig {
            public string colorAlternative = "1 1 1 0.5";

            public CuiImageComponent ToImageComponent(string uriKey, bool useAlternativeColor) {
                string iconId = default(string);
                FileManager.TryGetValue(uriKey.ToString(), out iconId);
                
                return new CuiImageComponent {
                    Color = useAlternativeColor ? colorAlternative : color,
                    Png = iconId,
                    Sprite = "assets/content/textures/generic/fulltransparent.tga"
                };
            }
        }

        private class PanelBaseConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool enabled { get; set; } = true;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public ComponentRectTransformConfig transform = new ComponentRectTransformConfig();

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string backgroundColor = "1 0.95 0.875 0.025";
        }
        
        private class PanelIconConfig : PanelBaseConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public ComponentIconStatesConfig icon = new ComponentIconStatesConfig();
        }

        private class PanelTextConfig : PanelBaseConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public ComponentIconConfig icon = new ComponentIconConfig();

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public ComponentTextConfig text = new ComponentTextConfig();

            public CuiElementContainer ToStaticContainer(PanelTypes type) {
                CuiElementContainer container = new CuiElementContainer();
                
                container.Add(new CuiElement {
                    Name = UniqueElementName(ContainerTypes.Static, ContainerParent, type.ToString()),
                    Parent = ContainerParent,
                    Components = {
                        new CuiImageComponent {
                            Color = backgroundColor
                        },
                        transform.ToRectComponent()
                    }
                });

                container.Add(new CuiElement {
                    Name = UniqueElementName(ContainerTypes.Static, ContainerParent, type.ToString() + "Icon"),
                    Parent = UniqueElementName(ContainerTypes.Static, ContainerParent, type.ToString()),
                    Components = {
                        icon.ToImageComponent(type.ToString()),
                        icon.ToRectComponent()
                    }
                });

                return container;
            }

            public CuiElementContainer ToDynamicContainer(PanelTypes type, params StringPlus.ReplacementData[] replacements) {
                CuiElementContainer container = new CuiElementContainer();

                string textData = StringPlus.Replace(text.text, replacements);

                container.Add(new CuiElement {
                    Name = UniqueElementName(ContainerTypes.Dynamic, ContainerParent, type.ToString()),
                    Parent = ContainerParent,
                    Components = {
                        text.ToTextComponent(textData),
                        text.ToRectComponent()
                    }
                });
                
                return container;
            }
        }

        private class PanelActiveConfig : PanelTextConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool fillText = true;
        }

        private class PanelClockConfig : PanelTextConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool hourFormat24 = true;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool showSeconds = false;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public float updateInterval = 2f;
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool useSystemTime = false;
        }
        
        private class PanelsConfig {
            public PanelActiveConfig active = new PanelActiveConfig {
                enabled = true,
                fillText = true,
                transform = new ComponentRectTransformConfig {
                    height = 0.048f,
                    width = 0.036f,
                    positionX = 81,
                    positionY = 72
                },
                icon = new ComponentIconConfig {
                    height = 0.325f,
                    width = 0.75f,
                    positionX = 2,
                    positionY = 3,
                    color = "0.7 0.7 0.7 1",
                    uri = "http://i.imgur.com/UY0y5ZI.png"
                },
                text = new ComponentTextConfig {
                    height = 1,
                    width = 1,
                    positionX = 24,
                    positionY = 0,
                    alignment = TextAnchor.MiddleLeft,
                    color = "0.85 0.85 0.85 0.5",
                    text = replacePrefix + FieldTypes.PlayersActive.ToString().ToUpper() + replaceSufix + "/" + replacePrefix + FieldTypes.PlayerMax.ToString().ToUpper() + replaceSufix
                }
            };

            public PanelTextConfig sleeping = new PanelTextConfig {
                enabled = true,
                transform = new ComponentRectTransformConfig {
                    height = 0.049f,
                    width = 0.036f,
                    positionX = 145,
                    positionY = 72
                },
                icon = new ComponentIconConfig {
                    height = 0.325f,
                    width = 0.75f,
                    positionX = 2,
                    positionY = 3,
                    color = "0.7 0.7 0.7 1",
                    uri = "http://i.imgur.com/mvUBBOB.png"
                },
                text = new ComponentTextConfig {
                    height = 1,
                    width = 1,
                    positionX = 24,
                    positionY = 0,
                    alignment = TextAnchor.MiddleLeft,
                    color = "0.85 0.85 0.85 0.5",
                    text = replacePrefix + FieldTypes.PlayersSleeping.ToString().ToUpper() + replaceSufix
                }
            };

            public PanelClockConfig clock = new PanelClockConfig {
                enabled = true,
                hourFormat24 = true,
                showSeconds = false,
                updateInterval = 2f,
                useSystemTime = false,
                transform = new ComponentRectTransformConfig {
                    height = 0.049f,
                    width = 0.036f,
                    positionX = 16,
                    positionY = 72
                },
                icon = new ComponentIconConfig {
                    height = 0.325f,
                    width = 0.75f,
                    positionX = 2,
                    positionY = 3,
                    color = "0.7 0.7 0.7 1",
                    uri = "http://i.imgur.com/CycsoyW.png"
                },
                text = new ComponentTextConfig {
                    height = 1,
                    width = 1,
                    positionX = 24,
                    positionY = 0,
                    alignment = TextAnchor.MiddleLeft,
                    color = "0.85 0.85 0.85 0.5",
                    text = replacePrefix + FieldTypes.Time.ToString().ToUpper() + replaceSufix
                }
            };

            private bool[] _enabled = null;
            public bool[] enabled {
                get {
                    if (_enabled == null) {
                        _enabled = new bool[panelTypesCount];

                        _enabled[0] = active.enabled;
                        _enabled[1] = sleeping.enabled;
                        _enabled[2] = clock.enabled;
                    }
                    return _enabled;
                }
            }
        }
        #endregion

        #region Variables
        /// <summary>The static containers in the form of an array ordered according to the panels enumerator.</summary>
        private CuiElementContainer[] staticContainers;

        /// <summary>The character used in between values of the Json.</summary>
        private static readonly char valueSeperator = ' ';

        /// <summary>The character put before the display field value in order to signal it has to be replaced.</summary>
        private static readonly char replacePrefix = '{';
        /// <summary>The character put after the display field value in order to signal it has to be replaced.</summary>
        private static readonly char replaceSufix = '}';

        private PanelsConfig panelsConfig = new PanelsConfig();

        /* For counting the amount of online players who are dead.
        /// <summary>Holds the amount of players currently dead.</summary>
        private int DEADCOUNT = 0;*/

        private string _timeFormat = null;
        private string timeFormat {
            get {
                if (_timeFormat == null) {
                    if (panelsConfig.clock.hourFormat24) {
                        if (panelsConfig.clock.showSeconds) {
                            _timeFormat = "HH:mm:ss";
                        }
                        else {
                            _timeFormat = "HH:mm";
                        }
                    }
                    else {
                        if (panelsConfig.clock.showSeconds) {
                            _timeFormat = "h:mm:ss tt";
                        }
                        else {
                            _timeFormat = "h:mm tt";
                        }
                    }
                }
                return _timeFormat;
            }
        }

        /// <summary>Directly retrieves time from the game.</summary>
        private string TIME_RAW {
            get {
                return (panelsConfig.clock.useSystemTime ? DateTime.Now : TOD_Sky.Instance.Cycle.DateTime).ToString(timeFormat);
            }
        }

        /// <summary>Holds the time as a string value.</summary>
        private string TIME = "";
        #endregion
        
        #region Configuration
        /// <summary>Retrieves all the data from the configuration file and adds default data if it is not present.</summary>
        private void InitializeConfiguration() {
            bool defaultApplied = false;
            
            panelsConfig.active = CheckConfigFile("Online players panel", panelsConfig.active, ref defaultApplied);
            panelsConfig.sleeping = CheckConfigFile("Sleeping players panel", panelsConfig.sleeping, ref defaultApplied);
            panelsConfig.clock = CheckConfigFile("Clock panel", panelsConfig.clock, ref defaultApplied);

            SaveConfig();

            if (defaultApplied) {
                PrintWarning("New field(s) added to the configuration file please view and edit if necessary.");
            }
        }
        #endregion

        #region Hooks
        /// <summary>Called when a plugin has finished loading.</summary>
        [HookMethod("Loaded")]
        private void Loaded() {
            // Initializes the configuration.
            //PrintWarning("Config read and write disabled.");
            InitializeConfiguration();

            /* For counting the amount of online players who are dead.
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                if (BasePlayer.activePlayerList[i].IsDead()) {
                    DEADCOUNT++;
                }
            }*/
        }

        /// <summary>Called after the server startup has been completed and is awaiting connections.</summary>
        [HookMethod("OnServerInitialized")]
        private void OnServerInitialized() {
            // Initializes the static containers for each panel.
            staticContainers = new CuiElementContainer[panelTypesCount];
            // Starts retrieving the icons and rebuilds the static panels once they are loaded.
            FileManager.OnFileLoaded onIconLoaded = new FileManager.OnFileLoaded(IconLoaded);
            FileManager.InitializeFile(pluginName, PanelTypes.Active.ToString(), panelsConfig.active.icon.uri, onIconLoaded);
            FileManager.InitializeFile(pluginName, PanelTypes.Sleeping.ToString(), panelsConfig.sleeping.icon.uri, onIconLoaded);
            FileManager.InitializeFile(pluginName, PanelTypes.Clock.ToString(), panelsConfig.clock.icon.uri, onIconLoaded);

            // Adds and updates the panels for each active player.
            for (int i = 0; i < BasePlayer.activePlayerList.Count * panelTypesCount; i++) {
                if (panelsConfig.enabled[i % panelTypesCount]) {
                    UpdateField(BasePlayer.activePlayerList[i / panelTypesCount], (PanelTypes)(i % panelTypesCount));
                }
            }

            if (panelsConfig.enabled[(int)PanelTypes.Clock]) {
                StartRepeatingFieldUpdate(PanelTypes.Clock, panelsConfig.clock.updateInterval);
            }
        }

        /// <summary>Called when a plugin is being unloaded.</summary>
        [HookMethod("Unload")]
        private void Unload() {
            // Removes all panels for the players.
            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                for (int j = 0; j < panelTypesCount * containerTypesCount; j++) {
                    if (panelsConfig.enabled[j % panelTypesCount]) {
                        CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], UniqueElementName(((ContainerTypes)(j / panelTypesCount)), ContainerParent, ((PanelTypes)(j % panelTypesCount)).ToString()));
                    }
                }
            }
        }

        /// <summary>Called when the player is initializing (after they’ve connected, before they wake up).</summary>
        /// <param name="player"></param>
        [HookMethod("OnPlayerInit")]
        private void OnPlayerInit(BasePlayer player) {
            /* For counting the amount of online players who are dead.
            if (player.IsDead()) {
                DEADCOUNT++;
            }*/

            if (panelsConfig.enabled[(int)PanelTypes.Active]) {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                    UpdateFieldDelayed(BasePlayer.activePlayerList[i], PanelTypes.Active);
                }
            }
        }

        /// <summary>Called when the player awakes.</summary>
        /// <param name="player"></param>
        [HookMethod("OnPlayerSleepEnded")]
        private void OnPlayerSleepEnded(BasePlayer player) {
            for (int i = 0; i < panelTypesCount; i++) {
                if (panelsConfig.enabled[i]) {
                    CuiHelper.DestroyUi(player, UniqueElementName(ContainerTypes.Static, ContainerParent, ((PanelTypes) i).ToString()));
                    CuiHelper.AddUi(player, staticContainers[i]);
                    UpdateFieldDelayed(player, (PanelTypes)i);
                }
            }

            if (panelsConfig.enabled[(int)PanelTypes.Sleeping]) {
                for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                    UpdateFieldDelayed(BasePlayer.activePlayerList[i], PanelTypes.Sleeping);
                }
            }
        }

        /// <summary>Called when an entity dies.</summary>
        /// <param name="entity"></param>
        /// <param name="info"></param>
        [HookMethod("OnEntityDeath")]
        private void OnEntityDeath(BaseEntity entity, HitInfo info) {
            BasePlayer player = entity as BasePlayer;
            if (player == null) {
                return;
            }
            /* For counting the amount of online players who are dead.
            DEADCOUNT++;*/

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                if (panelsConfig.enabled[(int)PanelTypes.Sleeping]) {
                    UpdateField(BasePlayer.activePlayerList[i], PanelTypes.Sleeping);
                }
            }
            
            for (int j = 0; j < panelTypesCount * containerTypesCount; j++) {
                if (panelsConfig.enabled[j % panelTypesCount]) {
                    CuiHelper.DestroyUi(player, UniqueElementName(((ContainerTypes)(j / panelTypesCount)), ContainerParent, ((PanelTypes)(j % panelTypesCount)).ToString()));
                }
            }
        }

        /// <summary>Called when the player has respawned (specifically when they click the “Respawn” button).</summary>
        /// <param name="player"></param>
        [HookMethod("OnPlayerRespawned")]
        private void OnPlayerRespawned(BasePlayer player) {
            /* For counting the amount of online players who are dead.
            DEADCOUNT--;
            if (DEADCOUNT < 0) {
                PrintWarning("Deadcount below zero, is value of " + DEADCOUNT.ToString() + ".");
                DEADCOUNT = 0;
            }*/

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                if (panelsConfig.enabled[(int)PanelTypes.Sleeping]) {
                    UpdateField(BasePlayer.activePlayerList[i], PanelTypes.Sleeping);
                }
            }
        }

        /// <summary>Called after the player has disconnected from the server.</summary>
        /// <param name="player"></param>
        /// <param name="reason"></param>
        [HookMethod("OnPlayerDisconnected")]
        private void OnPlayerDisconnected(BasePlayer player, string reason) {
            /* For counting the amount of online players who are dead.
            if (player.IsDead()) {
                DEADCOUNT--;
            }*/

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                if (panelsConfig.enabled[(int)PanelTypes.Active]) {
                    UpdateFieldDelayed(BasePlayer.activePlayerList[i], PanelTypes.Active);
                }
                if (panelsConfig.enabled[(int)PanelTypes.Sleeping]) {
                    UpdateFieldDelayed(BasePlayer.activePlayerList[i], PanelTypes.Sleeping);
                }
            }
        }
        #endregion

        #region InitializeUi
        private CuiElementContainer GetStaticContainer(PanelTypes type) {
            switch (type) {
                default:
                    return new CuiElementContainer();
                case PanelTypes.Active:
                    return panelsConfig.active.ToStaticContainer(type);
                case PanelTypes.Sleeping:
                    return panelsConfig.sleeping.ToStaticContainer(type);
                case PanelTypes.Clock:
                    return panelsConfig.clock.ToStaticContainer(type);
            }
        }

        /// <summary>Reinitializes the static Ui.</summary>
        /// <param name="key">The key of the icon (equal to the panel it belongs to).</param>
        /// <param name="value">The value of the icon.</param>
        private void IconLoaded(string key, string value) {
            PanelTypes type = EnumPlus.ToEnum<PanelTypes>(key);

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                CuiHelper.DestroyUi(BasePlayer.activePlayerList[i], UniqueElementName(ContainerTypes.Static, ContainerParent, type.ToString()));
            }

            staticContainers[(int)type] = GetStaticContainer(type);

            for (int j = 0; j < BasePlayer.activePlayerList.Count; j++) {
                if (!BasePlayer.activePlayerList[j].IsDead()) {
                    CuiHelper.AddUi(BasePlayer.activePlayerList[j], staticContainers[(int)type]);
                    UpdateField(BasePlayer.activePlayerList[j], type);
                }
            }
        }
        #endregion

        #region UpdateUi
        /// <summary>Updates the player's panel.</summary>
        /// <param name="player">The player for who the panel should be updated.</param>
        /// <param name="panelType">The panel type that should be updated.</param>
        private void UpdateField(BasePlayer player, PanelTypes panelType) {
            if (player.IsDead() || player.IsSleeping()) {
                return;
            }

            CuiElementContainer container = null;

            switch (panelType) {
                case PanelTypes.Active:
                    container = panelsConfig.active.ToDynamicContainer(PanelTypes.Active,
                        new StringPlus.ReplacementData(replacePrefix + FieldTypes.PlayersActive.ToString().ToUpper() + replaceSufix, (panelsConfig.active.fillText) ? BasePlayer.activePlayerList.Count.ToString().PadLeft(ConVar.Server.maxplayers.ToString().Length, '0') : BasePlayer.activePlayerList.Count.ToString()),
                        new StringPlus.ReplacementData(replacePrefix + FieldTypes.PlayerMax.ToString().ToUpper() + replaceSufix, ConVar.Server.maxplayers.ToString())
                        );
                    break;
                case PanelTypes.Sleeping:
                    container = panelsConfig.sleeping.ToDynamicContainer(PanelTypes.Sleeping,
                        new StringPlus.ReplacementData(replacePrefix + FieldTypes.PlayersSleeping.ToString().ToUpper() + replaceSufix, BasePlayer.sleepingPlayerList.Count.ToString())
                        );
                    break;
                case PanelTypes.Clock:
                    container = panelsConfig.clock.ToDynamicContainer(PanelTypes.Clock,
                        new StringPlus.ReplacementData(replacePrefix + FieldTypes.Time.ToString().ToUpper() + replaceSufix, TIME)
                        );
                    break;
            }

            if (container != null) {
                CuiHelper.DestroyUi(player, UniqueElementName(ContainerTypes.Dynamic, ContainerParent, panelType.ToString()));
                CuiHelper.AddUi(player, container);
            }
        }

        /// <summary>Updates the player's panel with a delay of one frame.</summary>
        /// <param name="player">The player for who the panel should be updated.</param>
        /// <param name="panel">The panel type that should be updated.</param>
        private void UpdateFieldDelayed(BasePlayer player, PanelTypes panelType) {
            NextFrame(() => {
                UpdateField(player, panelType);
            });
        }

        /// <summary>Start a function that repeats updating a panel for each active player.</summary>
        /// <param name="panelType">The panel that should be updated.</param>
        /// <param name="updateInterval">The time in between updating the panel in milliseconds.</param>
        private void StartRepeatingFieldUpdate(PanelTypes panelType, float updateInterval) {
            // Updates the time if the repeating panel is the clock.
            if (panelType == PanelTypes.Clock) {
                TIME = TIME_RAW;
            }

            for (int i = 0; i < BasePlayer.activePlayerList.Count; i++) {
                if (!BasePlayer.activePlayerList[i].IsSleeping()) {
                    UpdateField(BasePlayer.activePlayerList[i], panelType);
                }
            }

            timer.Once(updateInterval / 1000f, () => {
                StartRepeatingFieldUpdate(panelType, updateInterval);
            });
        }
        #endregion
    }
}

namespace Oxide.Plugins.BasePlugin {
    /// <summary>Base class to extend plugins from.</summary>
    public class BaseUiPlus : RustPlugin {
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
                return "BaseUiPlus";
            }
        }

        /// <summary>This instance of the plugin.</summary>
        public static BaseUiPlus instance = null;

        /// <summary>Please use gameObject instead of this.</summary>
        /// <seealso cref="gameObject"/>
        private static GameObject _gameObject = null;
        /// <summary>The game object belonging to this plugin.</summary>
        public static GameObject gameObject {
            get {
                if (_gameObject == null) {
                    _gameObject = GameObject.Find(instance.pluginName + "Object");
                    if (_gameObject == null) {
                        _gameObject = new GameObject(instance.pluginName + "Object");
                    }
                }
                return _gameObject;
            }
        }

        /// <summary>The name of the parent of the ui elements's container.</summary>
        public static readonly string ContainerParent = "Hud.Menu";
        #endregion

        #region Constructor
        public BaseUiPlus() {
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

        public class ComponentRectTransformConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public float width { get; set; } = 1f;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public float height { get; set; } = 1f;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public float positionX { get; set; } = 0f;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public float positionY { get; set; } = 0f;

            public virtual CuiRectTransformComponent ToRectComponent() {
                return new CuiRectTransformComponent {
                    AnchorMin = "0 0",
                    AnchorMax = width + " " + height,
                    OffsetMin = positionX + " " + positionY,
                    OffsetMax = positionX + " " + positionY
                };
            }
        }

        public class ComponentIconConfig : ComponentRectTransformConfig {
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string color { get; set; } = "1 1 1 1";

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string uri { get; set; } = "";
            
            public virtual CuiImageComponent ToImageComponent(string uriKey) {
                string iconId = default(string);
                FileManager.TryGetValue(uriKey.ToString(), out iconId);

                return new CuiImageComponent {
                    Color = color,
                    Png = iconId,
                    Sprite = "assets/content/textures/generic/fulltransparent.tga"
                };
            }
        }

        public class ComponentTextConfig : ComponentRectTransformConfig {
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public TextAnchor alignment { get; set; } = TextAnchor.MiddleLeft;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string color { get; set; } = "1 1 1 1";

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string font { get; set; } = new CuiTextComponent().Font;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int fontSize { get; set; } = new CuiTextComponent().FontSize;

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string text { get; set; } = "No text";

            public virtual CuiTextComponent ToTextComponent() {
                return new CuiTextComponent {
                    Align = alignment,
                    Color = color,
                    Font = font,
                    FontSize = fontSize,
                    Text = text
                };
            }

            public virtual CuiTextComponent ToTextComponent(string text) {
                return new CuiTextComponent {
                    Align = alignment,
                    Color = color,
                    Font = font,
                    FontSize = fontSize,
                    Text = text
                };
            }
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
                if (uri == "") {
                    return;
                }

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
                    BaseUiPlus.instance.PrintWarning(www.error);
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