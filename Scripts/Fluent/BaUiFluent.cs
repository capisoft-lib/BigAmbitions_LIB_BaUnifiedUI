using System;
using Capisoft.Lib.BaUi.Assets;
using Capisoft.Lib.BaUi.Chrome;
using Capisoft.Lib.BaUi.Controls;
using Capisoft.Lib.BaUi.Core;
using Capisoft.Lib.BaUi.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUi.Fluent
{
    public sealed class BaUiBuiltPanel
    {
        public GameObject Root { get; internal set; }
        public RectTransform Panel { get; internal set; }
        public RectTransform Header { get; internal set; }
        public RectTransform Body { get; internal set; }
        public float Scale { get; internal set; }
        public BaUiLayout.Metrics Metrics { get; internal set; }
    }

    public static class BaUi
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
        private BaPanelRecipe _recipe = BaPanelRecipe.DockedHud;
        private float _panelWidth = BaUiLayout.PanelWidth;
        private float _panelHeight;
        private float _headerExtraTrim;
        private Action<BaHeaderBuilder> _header;
        private Action<BaBodyBuilder> _body;

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

        public BaOverlayBuilder Panel(BaPanelRecipe recipe, float width, float height, float headerExtraTrim = 0f)
        {
            _recipe = recipe;
            _panelWidth = width;
            _panelHeight = height;
            _headerExtraTrim = headerExtraTrim;
            if (recipe == BaPanelRecipe.WideMapPanel && Mathf.Approximately(headerExtraTrim, 0f))
                _headerExtraTrim = BaUiLayout.ComputeWideMapPanelHeaderWidenTrim(width);
            if (recipe == BaPanelRecipe.Settings && Mathf.Approximately(headerExtraTrim, 0f))
                _headerExtraTrim = BaUiLayout.SettingsPanelHeaderWidenTrim;
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

        public BaUiBuiltPanel Build()
        {
            BaUiBootstrap.EnsureEventSystem();
            BaUiAssets.EnsureInitialized();

            var root = new GameObject(_rootName);
            UnityEngine.Object.DontDestroyOnLoad(root);
            BaUiChrome.SetupOverlayCanvas(root, _sortOrder);

            if (_modal)
            {
                var dimGo = new GameObject("Dimmer", typeof(RectTransform));
                dimGo.transform.SetParent(root.transform, false);
                var dimRect = dimGo.GetComponent<RectTransform>();
                dimRect.anchorMin = Vector2.zero;
                dimRect.anchorMax = Vector2.one;
                dimRect.offsetMin = dimRect.offsetMax = Vector2.zero;
                var dimImg = dimGo.AddComponent<Image>();
                dimImg.color = new Color(0f, 0f, 0f, _dimmerAlpha);
                var dimBtn = dimGo.AddComponent<Button>();
                dimBtn.transition = Selectable.Transition.None;
            }

            if (_panelHeight <= 0f)
            {
                var scale = _panelWidth / BaUiLayout.PanelWidth;
                _panelHeight = BaUiLayout.CreateMetrics(scale).PanelHeight;
            }

            var chrome = BaUiChrome.Build(root.transform, _panelWidth, _panelHeight, "Panel", _headerExtraTrim);
            if (_recipe == BaPanelRecipe.Modal)
                BaUiChrome.ApplyModalHeaderFrame(chrome.Header, chrome.Scale);
            else if (_recipe == BaPanelRecipe.MainPanel)
                BaUiChrome.ApplyMainPanelHeaderFrame(chrome.Header);

            var body = CreateBodyRect(chrome.Panel, chrome.Scale);

            var built = new BaUiBuiltPanel
            {
                Root = root,
                Panel = chrome.Panel,
                Header = chrome.Header,
                Body = body,
                Scale = chrome.Scale,
                Metrics = chrome.Metrics
            };

            ApplyDock(chrome.Panel);

            _header?.Invoke(new BaHeaderBuilder(chrome.Header, chrome.Scale));
            _body?.Invoke(new BaBodyBuilder(body, chrome.Scale, chrome.Metrics));

            BaUiChrome.ApplyUiLayer(root);
            return built;
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
        private int _iconCount;

        internal BaHeaderBuilder(RectTransform header, float scale)
        {
            _header = header;
            _scale = scale;
        }

        public BaHeaderBuilder Title(string text, int rightIconCount = 0)
        {
            _iconCount = rightIconCount;
            var reserve = rightIconCount > 0
                ? BaUiLayout.ComputeHeaderIconsTitleReserve(rightIconCount, _scale)
                : BaUiLayout.HeaderTextPaddingX * _scale;
            BaUiControls.CreateTitleLabel(_header, text, _scale, reserve);
            return this;
        }

        public BaHeaderBuilder CloseButton(UnityAction onClick, float extraOffsetX = 0f)
        {
            BaUiAssets.CreateHeaderCloseButton(_header, _scale, onClick, extraOffsetX);
            return this;
        }

        public BaHeaderBuilder IconButton(BaIcons icon, int slot, UnityAction onClick, string fallbackGlyph = null)
        {
            Action<Image> style = BaUiAssets.ApplyButtonGrey;
            Func<Image, bool> apply = icon switch
            {
                BaIcons.Settings => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplySettingsIcon),
                BaIcons.Pin => BaUiAssets.TryApplyPinOverlayIcon,
                BaIcons.Add => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyAddIcon),
                BaIcons.Car => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyCarIcon),
                BaIcons.Focus => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyFocusIcon),
                BaIcons.History => img => BaUiAssets.TryApplyOverlayIcon(img, BaUiAssets.ApplyHistoryIcon),
                _ => img => false
            };
            if (icon == BaIcons.Add)
                style = BaUiAssets.ApplyButtonGreen;
            else if (icon == BaIcons.Pin || icon == BaIcons.Car)
                style = BaUiAssets.ApplyButtonBlue;

            BaUiControls.CreateHeaderIconButton(_header, slot, _scale, style, apply, onClick, fallbackGlyph);
            return this;
        }

        public int IconCount => _iconCount;
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
