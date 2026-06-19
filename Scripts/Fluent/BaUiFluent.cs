using System;
using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Chrome;
using Capisoft.Lib.BaUnifiedUI.Core;
using Capisoft.Lib.BaUnifiedUI.Controls;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Fluent
{
    public sealed class BaUiBuiltPanel
    {
        public GameObject Root { get; internal set; }
        public RectTransform Panel { get; internal set; }
        public RectTransform Header { get; internal set; }
        public RectTransform Body { get; internal set; }
        public float Scale { get; internal set; }
        public float ContentInset { get; internal set; }
        public BaUiLayout.Metrics Metrics { get; internal set; }
        public float PanelHeight { get; internal set; }
    }

    public static partial class BaUi
    {
        public static BaOverlayBuilder Overlay(string rootName, int sortOrder) =>
            new BaOverlayBuilder(rootName, sortOrder, modal: false);

        public static BaOverlayBuilder Modal(string rootName, int sortOrder, float dimmerAlpha = 0.55f) =>
            new BaOverlayBuilder(rootName, sortOrder, modal: true, dimmerAlpha);

        public static BaUiBuiltPanel LoadFromBaXaml(string documentName) =>
            BaXaml.BaUiXamlLoader.Load(documentName);
    }

    public sealed class BaOverlayBuilder
    {
        private readonly string _rootName;
        private readonly int _sortOrder;
        private readonly bool _modal;
        private readonly float _dimmerAlpha;
        private BaDock _dock = BaDock.Center;
        private float _marginX = BaUiLayout.ScreenMarginX;
        private float _marginY = BaUiLayout.ScreenMarginMinY;
        private BaPanelRecipe _recipe = BaPanelRecipe.ActionPanel;
        private float _panelWidth = BaUiLayout.PanelWidth;
        private float _panelHeight;
        private float _headerExtraTrim;
        private Action<BaHeaderBuilder> _header;
        private Action<BaBodyBuilder> _body;
        private Action<BaPanelContentBuilder> _content;
        private UnityAction _onDismiss;
        private bool _skipBody;
        private bool _interactive = true;
        private Action<BaUiBuiltPanel> _afterPanelChildren;

        internal BaOverlayBuilder(string rootName, int sortOrder, bool modal, float dimmerAlpha = 0.55f)
        {
            _rootName = rootName;
            _sortOrder = sortOrder;
            _modal = modal;
            _dimmerAlpha = dimmerAlpha;
        }

        public BaOverlayBuilder Dock(BaDock dock, float marginX = 16f, float marginY = 36f)
        {
            _dock = dock;
            _marginX = marginX;
            _marginY = marginY;
            return this;
        }

        /// <summary>Width only — height is derived from <see cref="Content"/> when omitted.</summary>
        public BaOverlayBuilder Panel(BaPanelRecipe recipe, float width) =>
            Panel(recipe, width, height: 0f, headerExtraTrim: 0f);

        /// <summary>Fixed width + height. Always use the <c>height:</c> argument name (avoids trim overload ambiguity).</summary>
        public BaOverlayBuilder Panel(BaPanelRecipe recipe, float width, float height, float headerExtraTrim = 0f)
        {
            _recipe = recipe;
            _panelWidth = width;
            _panelHeight = height;
            _headerExtraTrim = headerExtraTrim;
            if (recipe == BaPanelRecipe.Settings && Mathf.Approximately(headerExtraTrim, 0f))
                _headerExtraTrim = BaUiLayout.SettingsPanelHeaderWidenTrim;
            else if (recipe == BaPanelRecipe.WideMapPanel && Mathf.Approximately(headerExtraTrim, 0f))
                _headerExtraTrim = BaUiLayout.ComputeWideMapPanelHeaderWidenTrim(width);
            return this;
        }

        public BaOverlayBuilder Header(Action<BaHeaderBuilder> configure)
        {
            _header = configure;
            return this;
        }

        public BaOverlayBuilder Body(Action<BaBodyBuilder> configure)
        {
            _body = configure;
            return this;
        }

        public BaOverlayBuilder SkipBody()
        {
            _skipBody = true;
            return this;
        }

        /// <summary>HUD overlays that never receive clicks — skips GraphicRaycaster on the root canvas.</summary>
        public BaOverlayBuilder NonInteractive()
        {
            _interactive = false;
            return this;
        }

        /// <summary>Panel children added after header/content and docked chrome are finalized (buttons stay above frame layers).</summary>
        public BaOverlayBuilder AfterPanelChildren(Action<BaUiBuiltPanel> configure)
        {
            _afterPanelChildren = configure;
            return this;
        }

        /// <summary>Build map-style sections directly on the panel (search, scroll list, footer).</summary>
        public BaOverlayBuilder Content(Action<BaPanelContentBuilder> configure)
        {
            _skipBody = true;
            _content = configure;
            return this;
        }

        public BaOverlayBuilder OnDismiss(UnityAction onDismiss)
        {
            _onDismiss = onDismiss;
            return this;
        }

        public BaUiBuiltPanel Build()
        {
            BaUiBootstrap.EnsureEventSystem();
            BaUiAssets.EnsureInitialized();

            var root = new GameObject(_rootName);
            try
            {
                UnityEngine.Object.DontDestroyOnLoad(root);
                var layoutStamp = root.AddComponent<BaUiLayoutStamp>();
                layoutStamp.LayoutRevision = BaUiVersion.LayoutRevision;
                BaUiChrome.SetupOverlayCanvas(root, _sortOrder, _interactive);

                if (_modal && _onDismiss != null)
                    BaUiWidgets.CreateModalDimmer(root.transform, _dimmerAlpha, _onDismiss);
                else if (_modal)
                {
                    var dimGo = new GameObject("Dimmer", typeof(RectTransform));
                    dimGo.transform.SetParent(root.transform, false);
                    var dimRect = dimGo.GetComponent<RectTransform>();
                    dimRect.anchorMin = Vector2.zero;
                    dimRect.anchorMax = Vector2.one;
                    dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
                    var dimImg = dimGo.AddComponent<Image>();
                    dimImg.color = new Color(0f, 0f, 0f, _dimmerAlpha);
                    dimGo.AddComponent<Button>().transition = Selectable.Transition.None;
                }

                if (_panelHeight <= 0f && _content != null)
                {
                    var measureScale = _panelWidth > 0f ? _panelWidth / BaUiLayout.PanelWidth : 1f;
                    var planner = new BaPanelContentBuilder(null, measureScale, BaUiLayout.ContentInset, measureOnly: true);
                    _content(planner);
                    _panelHeight = planner.PanelHeight;
                }

                if (_panelHeight <= 0f)
                {
                    if (_recipe == BaPanelRecipe.BizManLight)
                        _panelHeight = BaUiBizManLightLayout.ResolvePanelHeight();
                    else
                    {
                        var scale = _panelWidth / BaUiLayout.PanelWidth;
                        _panelHeight = BaUiLayout.CreateMetrics(scale).PanelHeight;
                    }
                }

                if (_recipe == BaPanelRecipe.BizManLight && _panelWidth <= 0f)
                    _panelWidth = BaUiBizManLightLayout.ResolvePanelWidth();

                BaUiChrome.BuiltChrome chrome;
                if (_recipe == BaPanelRecipe.BizManLight)
                {
                    chrome = BaUiBizManLightChrome.Build(root.transform, _panelWidth, _panelHeight, "Panel");
                }
                else
                {
                    chrome = BaUiChrome.Build(root.transform, _panelWidth, _panelHeight, "Panel", _headerExtraTrim);
                    ApplyHeaderRecipe(chrome.Header, chrome.Metrics);
                }

                RectTransform body = null;
                if (!_skipBody)
                {
                    body = _recipe == BaPanelRecipe.BizManLight
                        ? BaUiBizManLightChrome.CreateBodyRect(chrome.Panel, chrome.Scale)
                        : CreateBodyRect(chrome.Panel, chrome.Scale);
                }

                var built = new BaUiBuiltPanel
                {
                    Root = root,
                    Panel = chrome.Panel,
                    Header = chrome.Header,
                    Body = body,
                    Scale = chrome.Scale,
                    ContentInset = chrome.ContentInset,
                    Metrics = chrome.Metrics,
                    PanelHeight = _panelHeight
                };

                ApplyDock(chrome.Panel);

                _header?.Invoke(new BaHeaderBuilder(chrome.Header, chrome.Scale));
                if (body != null)
                    _body?.Invoke(new BaBodyBuilder(body, chrome.Scale, chrome.Metrics));
                else if (_content != null)
                    _content.Invoke(new BaPanelContentBuilder(chrome.Panel, chrome.Scale, chrome.ContentInset));

                BaUiChrome.FinalizeDockedHeader(
                    chrome.Panel,
                    chrome.Header,
                    _panelWidth,
                    _recipe == BaPanelRecipe.ActionPanel || _recipe == BaPanelRecipe.WideMapPanel,
                    _headerExtraTrim);

                if (NeedsPostLayoutChromeRestore(_recipe))
                {
                    Canvas.ForceUpdateCanvases();
                    BaUiChrome.RestorePanelChrome(chrome.Panel, _panelWidth, _headerExtraTrim);
                }

                _afterPanelChildren?.Invoke(built);

                BaUiChrome.ApplyUiLayer(root);
                return built;
            }
            catch
            {
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        private static bool NeedsPostLayoutChromeRestore(BaPanelRecipe recipe) =>
            recipe == BaPanelRecipe.ActionPanel
            || recipe == BaPanelRecipe.WideMapPanel
            || recipe == BaPanelRecipe.Settings;

        private void ApplyHeaderRecipe(RectTransform header, BaUiLayout.Metrics metrics)
        {
            switch (_recipe)
            {
                case BaPanelRecipe.Modal:
                    BaUiChrome.ApplyModalHeaderFrame(header, metrics.Scale);
                    break;
                case BaPanelRecipe.MainPanel:
                    BaUiChrome.ApplyMainPanelHeaderFrame(header);
                    break;
                case BaPanelRecipe.Settings:
                    BaUiChrome.ApplyHeaderFrame(header, metrics, _headerExtraTrim);
                    break;
                default:
                    BaUiChrome.ApplyDockedHeaderFrame(header, metrics, _headerExtraTrim);
                    break;
            }
        }

        private static RectTransform CreateBodyRect(RectTransform panel, float scale)
        {
            var go = new GameObject("Body", typeof(RectTransform));
            go.transform.SetParent(panel, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(BaUiLayout.ContentInset * scale, BaUiLayout.BodyBottomPadding * scale);
            rect.offsetMax = new Vector2(-BaUiLayout.ContentInset * scale, -BaUiLayout.HeaderHeight);
            return rect;
        }

        private void ApplyDock(RectTransform panel)
        {
            switch (_dock)
            {
                case BaDock.BottomLeft:
                    panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0f, 0f);
                    panel.anchoredPosition = BaUiLayout.GetScreenPosition(_marginY);
                    break;
                case BaDock.BottomCenter:
                    panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0f);
                    panel.pivot = new Vector2(0.5f, 0f);
                    panel.anchoredPosition = new Vector2(0f, _marginY);
                    break;
                case BaDock.TopCenter:
                    panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 1f);
                    panel.pivot = new Vector2(0.5f, 1f);
                    panel.anchoredPosition = new Vector2(0f, -_marginY);
                    break;
                default:
                    panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
                    panel.anchoredPosition = Vector2.zero;
                    break;
            }
        }
    }

    public sealed class BaHeaderBuilder
    {
        private readonly RectTransform _header;
        private readonly float _scale;
        private RectTransform _iconStrip;
        private RectTransform _titleRect;
        private int _stackedIconCount;

        internal BaHeaderBuilder(RectTransform header, float scale)
        {
            _header = header;
            _scale = scale;
        }

        public BaHeaderBuilder Title(string text, int rightIconCount = 0)
        {
            var count = rightIconCount > 0 ? rightIconCount : _stackedIconCount;
            var reserve = count > 0
                ? BaUiLayout.ComputeHeaderIconsTitleReserve(count, _scale)
                : BaUiLayout.HeaderTextPaddingX * _scale;
            BaUiControls.CreateTitleLabel(_header, text, _scale, reserve);
            return this;
        }

        public BaHeaderBuilder TitleBizManLeft(string text, bool upperCase = false)
        {
            var titleGo = BaUiWidgets.CreateRect(_header, "Title");
            BaUiWidgets.StretchFull(titleGo);
            _titleRect = titleGo;
            var label = titleGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiBizManLightLayout.TitleFontSize * _scale;
            label.fontStyle = (upperCase ? FontStyles.UpperCase : FontStyles.Normal) | FontStyles.Bold;
            label.color = BaUiAssets.BizManLightTitleColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);
            var padX = BaUiBizManLightLayout.HeaderTextPaddingX * _scale;
            titleGo.offsetMin = new Vector2(padX, 0f);
            titleGo.offsetMax = new Vector2(-padX, 0f);
            return this;
        }

        /// <summary>Left title — right inset grows automatically as stacked header icons are added.</summary>
        public BaHeaderBuilder TitleLeft(string text, bool upperCase = true)
        {
            var titleGo = BaUiWidgets.CreateRect(_header, "Title");
            BaUiWidgets.StretchFull(titleGo);
            _titleRect = titleGo;
            var label = titleGo.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = BaUiLayout.TitleFontSize * _scale;
            label.fontStyle = (upperCase ? FontStyles.UpperCase : FontStyles.Normal) | FontStyles.Bold;
            label.color = BaUiAssets.TitleColor;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.raycastTarget = false;
            BaUiAssets.ApplyTitleFont(label);
            SyncTitleReserve();
            return this;
        }

        [System.Obsolete("Icon count is inferred from stacked Icons. Use TitleLeft(text) only.")]
        public BaHeaderBuilder TitleLeft(string text, int rightIconCount, bool upperCase = true)
        {
            TitleLeft(text, upperCase);
            if (rightIconCount > _stackedIconCount)
            {
                _stackedIconCount = rightIconCount;
                SyncTitleReserve();
            }
            return this;
        }

        public BaHeaderBuilder TitleCenter(string text)
        {
            var label = BaUiWidgets.CreateHeaderTitleCenter(_header, _scale);
            label.text = text;
            return this;
        }

        public BaHeaderBuilder CloseButton(UnityAction onClick, float extraOffsetX = 0f)
        {
            BaUiAssets.CreateHeaderCloseButton(_header, _scale, onClick, extraOffsetX);
            return this;
        }

        /// <summary>Stacked header icons (right-aligned row). Call order = right-to-left on screen.</summary>
        public BaHeaderBuilder Icons(Action<BaHeaderIconsBuilder> configure)
        {
            configure(new BaHeaderIconsBuilder(this));
            return this;
        }

        public BaHeaderBuilder Icon(
            BaIcons icon,
            UnityAction onClick,
            string fallbackGlyph = null,
            BaButtonStyle? styleOverride = null) =>
            AddStackedIcon(icon, onClick, fallbackGlyph, styleOverride, null);

        /// <summary>Absolute slot from the right — for dynamic UI that inserts icons at fixed positions.</summary>
        public BaHeaderBuilder IconAt(
            BaIcons icon,
            int slotFromRight,
            UnityAction onClick,
            string fallbackGlyph = null,
            BaButtonStyle? styleOverride = null)
        {
            ResolveIcon(icon, styleOverride, out var style, out var apply);
            BaUiControls.CreateHeaderIconButton(_header, slotFromRight, _scale, style, apply, onClick, fallbackGlyph);
            return this;
        }

        [System.Obsolete("Use Icon() for stacked icons or IconAt() for positioned slots.")]
        public BaHeaderBuilder IconButton(
            BaIcons icon,
            int slot,
            UnityAction onClick,
            string fallbackGlyph = null,
            BaButtonStyle? styleOverride = null) =>
            IconAt(icon, slot, onClick, fallbackGlyph, styleOverride);

        /// <summary>Right-aligned icon strip created by stacked <see cref="Icon"/> calls.</summary>
        public RectTransform IconStrip => _iconStrip;

        public int StackedIconCount => _stackedIconCount;

        internal BaHeaderBuilder AddStackedIcon(
            BaIcons icon,
            UnityAction onClick,
            string fallbackGlyph,
            BaButtonStyle? styleOverride,
            string name)
        {
            EnsureIconStrip();
            ResolveIcon(icon, styleOverride, out var style, out var apply);
            BaUiControls.CreateHeaderIconInStrip(_iconStrip, _scale, style, apply, onClick, name, fallbackGlyph);
            _stackedIconCount++;
            SyncTitleReserve();
            return this;
        }

        private void EnsureIconStrip()
        {
            if (_iconStrip != null)
                return;
            _iconStrip = BaUiControls.CreateHeaderIconStrip(_header, _scale);
        }

        private void SyncTitleReserve()
        {
            if (_titleRect == null)
                return;

            var reserve = _stackedIconCount > 0
                ? BaUiLayout.ComputeHeaderIconsTitleReserve(_stackedIconCount, _scale)
                : BaUiLayout.HeaderTextPaddingX * _scale;
            BaUiLayout.ApplyHeaderTitleWithRightReserve(_titleRect, _scale, reserve);
        }

        private void ResolveIcon(
            BaIcons icon,
            BaButtonStyle? styleOverride,
            out Action<Image> style,
            out Func<Image, bool> apply)
        {
            style = styleOverride.HasValue
                ? StyleAction(styleOverride.Value)
                : BaUiAssets.ApplyButtonGrey;
            apply = icon switch
            {
                BaIcons.Settings => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplySettingsIcon),
                BaIcons.Pin => BaUiAssets.TryApplyPinOverlayIcon,
                BaIcons.Add => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyAddIcon),
                BaIcons.Car => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyCarIcon),
                BaIcons.Focus => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyFocusIcon),
                BaIcons.History => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyHistoryIcon),
                _ => img => false
            };
            if (!styleOverride.HasValue)
            {
                if (icon == BaIcons.Add)
                    style = BaUiAssets.ApplyButtonGreen;
                else if (icon == BaIcons.Pin || icon == BaIcons.Car)
                    style = BaUiAssets.ApplyButtonBlue;
            }
        }

        private static Action<Image> StyleAction(BaButtonStyle style) => style switch
        {
            BaButtonStyle.Grey => BaUiAssets.ApplyButtonGrey,
            BaButtonStyle.Green => BaUiAssets.ApplyButtonGreen,
            BaButtonStyle.Red => BaUiAssets.ApplyButtonRed,
            _ => BaUiAssets.ApplyButtonBlue
        };
    }

    /// <summary>Fluent grouping for stacked header icons.</summary>
    public sealed class BaHeaderIconsBuilder
    {
        private readonly BaHeaderBuilder _header;

        internal BaHeaderIconsBuilder(BaHeaderBuilder header) => _header = header;

        public BaHeaderIconsBuilder Icon(
            BaIcons icon,
            UnityAction onClick,
            string fallbackGlyph = null,
            BaButtonStyle? styleOverride = null)
        {
            _header.AddStackedIcon(icon, onClick, fallbackGlyph, styleOverride, null);
            return this;
        }
    }

    public sealed class BaBodyBuilder
    {
        private readonly RectTransform _body;
        private readonly float _scale;
        private readonly BaUiLayout.Metrics _metrics;
        private float _cursorY;

        internal BaBodyBuilder(RectTransform body, float scale, BaUiLayout.Metrics metrics)
        {
            _body = body;
            _scale = scale;
            _metrics = metrics;
            _cursorY = -BaUiLayout.BodyTopPadding * scale;
        }

        public BaBodyBuilder Gap(float pixels) { _cursorY -= pixels * _scale; return this; }

        public BaBodyBuilder Label(string text, BaTextStyle style = BaTextStyle.Body)
        {
            var label = BaUiControls.CreateBodyLabel(_body, text, _scale, style);
            var rect = label.rectTransform;
            rect.anchoredPosition = new Vector2(0f, _cursorY);
            _cursorY -= 28f * _scale;
            return this;
        }

        public BaBodyBuilder VanillaButton(string text, BaButtonStyle style, UnityAction onClick, float? width = null)
        {
            var w = width ?? _metrics.ContentWidth;
            var h = _metrics.ButtonHeight;
            var (button, _, _) = BaUiControls.CreateVanillaButton(_body, text, style, _scale, w, h, onClick);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, _cursorY - h * 0.5f);
            _cursorY -= h + _metrics.ButtonGap;
            return this;
        }

        public BaBodyBuilder Row(Action<BaRowBuilder> configure)
        {
            configure(new BaRowBuilder(_body, _scale, _metrics, ref _cursorY));
            return this;
        }
    }

    public sealed class BaRowBuilder
    {
        private readonly RectTransform _body;
        private readonly float _scale;
        private readonly BaUiLayout.Metrics _metrics;
        private readonly float _rowY;
        private int _index;

        internal BaRowBuilder(RectTransform body, float scale, BaUiLayout.Metrics metrics, ref float cursorY)
        {
            _body = body;
            _scale = scale;
            _metrics = metrics;
            _rowY = cursorY;
            cursorY -= metrics.ButtonHeight + metrics.ButtonGap;
        }

        public BaRowBuilder VanillaButton(string text, BaButtonStyle style, UnityAction onClick)
        {
            var half = _metrics.HalfButtonWidth;
            var x = _index == 0 ? _metrics.LeftButtonX : _metrics.RightButtonX;
            var (button, _, _) = BaUiControls.CreateVanillaButton(
                _body, text, style, _scale, half, _metrics.ButtonHeight, onClick);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, _rowY - _metrics.ButtonHeight * 0.5f);
            _index++;
            return this;
        }
    }
}
