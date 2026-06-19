using Capisoft.Lib.BaUnifiedUI.Assets;
using Capisoft.Lib.BaUnifiedUI.Fluent;
using Capisoft.Lib.BaUnifiedUI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Capisoft.Lib.BaUnifiedUI.Controls
{
    public sealed class BaUiListRow
    {
        public GameObject Root { get; internal set; }
        public RectTransform Rect { get; internal set; }
        public GameObject TypeIconRoot { get; internal set; }
        public Image TypeIcon { get; internal set; }
        public TextMeshProUGUI NameLabel { get; internal set; }
        public TextMeshProUGUI DistanceLabel { get; internal set; }
        public Button CenterButton { get; internal set; }
        public Image CenterButtonImage { get; internal set; }
        public TextMeshProUGUI CenterFallbackLabel { get; internal set; }
        public Button SetDestButton { get; internal set; }
        public TextMeshProUGUI SetDestLabel { get; internal set; }
        public Button DriveButton { get; internal set; }
        public TextMeshProUGUI DriveLabel { get; internal set; }
        public Button AddButton { get; internal set; }
        public TextMeshProUGUI AddLabel { get; internal set; }
        public Button DeleteButton { get; internal set; }

        public void SetActive(bool active)
        {
            if (Root != null)
                Root.SetActive(active);
        }
    }

    public static class BaUiListRows
    {
        private readonly struct RowRecipeFlags
        {
            internal readonly bool ShowDelete;
            internal readonly bool ShowCenter;
            internal readonly bool ShowSetDest;
            internal readonly bool ShowDrive;
            internal readonly bool ShowAdd;
            internal readonly bool SquareSetDest;

            internal RowRecipeFlags(
                bool showDelete = false,
                bool showCenter = false,
                bool showSetDest = false,
                bool showDrive = false,
                bool showAdd = false,
                bool squareSetDest = false)
            {
                ShowDelete = showDelete;
                ShowCenter = showCenter;
                ShowSetDest = showSetDest;
                ShowDrive = showDrive;
                ShowAdd = showAdd;
                SquareSetDest = squareSetDest;
            }
        }

        private static RowRecipeFlags GetRecipeFlags(BaUiListRowRecipe recipe) =>
            recipe switch
            {
                BaUiListRowRecipe.MapBookmark => new RowRecipeFlags(
                    showDelete: true, showCenter: true, showSetDest: true, showDrive: true),
                BaUiListRowRecipe.MapActions => new RowRecipeFlags(
                    showCenter: true, showSetDest: true, showDrive: true),
                BaUiListRowRecipe.VisitHistoryMap => new RowRecipeFlags(
                    showCenter: true, showSetDest: true, showDrive: true, showAdd: true, squareSetDest: true),
                BaUiListRowRecipe.VisitHistoryHud => new RowRecipeFlags(showDrive: true),
                _ => new RowRecipeFlags(showCenter: true, showSetDest: true, showDrive: true)
            };

        public static BaUiListRow Create(
            RectTransform parent,
            string name,
            float textScale,
            BaUiListRowRecipe recipe,
            float rowHorizontalInset = 0f,
            float rowTop = 0f)
        {
            var row = new BaUiListRow();
            row.Rect = BaUiWidgets.CreateRect(parent, name);
            row.Root = row.Rect.gameObject;
            row.Rect.anchorMin = new Vector2(0f, 1f);
            row.Rect.anchorMax = new Vector2(1f, 1f);
            row.Rect.pivot = new Vector2(0.5f, 1f);
            row.Rect.anchoredPosition = new Vector2(0f, rowTop);
            row.Rect.sizeDelta = new Vector2(-rowHorizontalInset, BaUiListMetrics.RowHeight);

            var flags = GetRecipeFlags(recipe);
            var buttonHeight = BaUiListMetrics.RowHeight - BaUiListMetrics.RowButtonPadY * 2f;

            BuildTypeIcon(row, row.Rect);
            BuildNameAndDistance(row, row.Rect, textScale, flags);

            if (flags.ShowAdd)
                BuildAddButton(row, row.Rect, textScale, buttonHeight, flags);
            if (flags.ShowDrive)
                BuildDriveButton(row, row.Rect, textScale, buttonHeight, flags);
            if (flags.ShowSetDest)
                BuildSetDestButton(row, row.Rect, textScale, buttonHeight, flags);
            if (flags.ShowCenter)
                BuildCenterButton(row, row.Rect, textScale, buttonHeight, flags);

            if (flags.ShowDelete)
            {
                row.DeleteButton = BaUiAssets.CreateRowCloseButton(row.Rect, textScale, null);
                LayoutRowButton(
                    row.DeleteButton.GetComponent<RectTransform>(),
                    0f,
                    BaUiListMetrics.RowActionButtonSize,
                    buttonHeight);
            }

            return row;
        }

        public static void Bind(
            this BaUiListRow row,
            UnityAction onCenter = null,
            UnityAction onSetDestination = null,
            UnityAction onDrive = null,
            UnityAction onAdd = null,
            UnityAction onDelete = null)
        {
            if (row.CenterButton != null && onCenter != null)
                BaUiAssets.BindButtonClick(row.CenterButton, onCenter);
            if (row.SetDestButton != null && onSetDestination != null)
                BaUiAssets.BindButtonClick(row.SetDestButton, onSetDestination);
            if (row.DriveButton != null && onDrive != null)
                BaUiAssets.BindButtonClick(row.DriveButton, onDrive);
            if (row.AddButton != null && onAdd != null)
                BaUiAssets.BindButtonClick(row.AddButton, onAdd);
            if (row.DeleteButton != null && onDelete != null)
                BaUiAssets.BindButtonClick(row.DeleteButton, onDelete);
        }

        public static void PositionInScroll(BaUiListRow row, float rowTop) =>
            BaUiScrollList.PositionRowInContent(row.Rect, rowTop);

        public static BaUiListRowTemplateBuilder Template(BaUiListRowRecipe recipe) =>
            new BaUiListRowTemplateBuilder(recipe);

        public static BaUiListRowTemplate MapBookmark() =>
            Template(BaUiListRowRecipe.MapBookmark).InScroll().Build();

        public static BaUiListRowTemplate MapActions() =>
            Template(BaUiListRowRecipe.MapActions).InScroll().Build();

        public static BaUiListRowTemplate VisitHistoryMap() =>
            Template(BaUiListRowRecipe.VisitHistoryMap).InScroll().Build();

        public static BaUiListRowTemplate VisitHistoryHud() =>
            Template(BaUiListRowRecipe.VisitHistoryHud).InScroll().Build();

        public static BaUiListRowTemplate VisitHistory() => VisitHistoryMap();

        private static void BuildTypeIcon(BaUiListRow row, RectTransform rowRect)
        {
            var iconGo = BaUiWidgets.CreateRect(rowRect, "TypeIcon");
            iconGo.anchorMin = iconGo.anchorMax = new Vector2(0f, 0.5f);
            iconGo.pivot = new Vector2(0f, 0.5f);
            iconGo.anchoredPosition = Vector2.zero;
            iconGo.sizeDelta = new Vector2(BaUiListMetrics.RowTypeIconSize, BaUiListMetrics.RowTypeIconSize);
            row.TypeIconRoot = iconGo.gameObject;

            var iconFgGo = BaUiWidgets.CreateRect(iconGo, "Foreground");
            BaUiWidgets.Stretch(iconFgGo);
            iconFgGo.offsetMin = new Vector2(2f, 2f);
            iconFgGo.offsetMax = new Vector2(-2f, -2f);
            row.TypeIcon = iconFgGo.gameObject.AddComponent<Image>();
            row.TypeIcon.raycastTarget = false;
            row.TypeIcon.preserveAspect = true;
            row.TypeIcon.color = Color.white;
            row.TypeIconRoot.SetActive(false);
        }

        private static void BuildNameAndDistance(BaUiListRow row, RectTransform rowRect, float textScale, in RowRecipeFlags flags)
        {
            var nameRightInset = ComputeRowNameRightInset(flags);
            var nameGo = BaUiWidgets.CreateRect(rowRect, "Name");
            nameGo.anchorMin = Vector2.zero;
            nameGo.anchorMax = new Vector2(1f, 1f);
            nameGo.offsetMin = new Vector2(BaUiListMetrics.RowTypeIconSize + 4f, 0f);
            nameGo.offsetMax = new Vector2(-nameRightInset, 0f);
            row.NameLabel = nameGo.gameObject.AddComponent<TextMeshProUGUI>();
            row.NameLabel.fontSize = 13f * textScale;
            row.NameLabel.color = BaUiAssets.BodyTextColor;
            row.NameLabel.alignment = TextAlignmentOptions.MidlineLeft;
            row.NameLabel.overflowMode = TextOverflowModes.Ellipsis;
            row.NameLabel.enableWordWrapping = false;
            BaUiAssets.ApplyButtonFont(row.NameLabel);

            var distGo = BaUiWidgets.CreateRect(rowRect, "Distance");
            distGo.anchorMin = new Vector2(1f, 0f);
            distGo.anchorMax = new Vector2(1f, 1f);
            distGo.pivot = new Vector2(1f, 0.5f);
            distGo.anchoredPosition = new Vector2(-ComputeRowDistanceRightInset(flags), 0f);
            distGo.sizeDelta = new Vector2(BaUiListMetrics.RowDistanceWidth, 0f);
            row.DistanceLabel = distGo.gameObject.AddComponent<TextMeshProUGUI>();
            row.DistanceLabel.fontSize = 12f * textScale;
            row.DistanceLabel.color = BaUiAssets.MutedBodyTextColor;
            row.DistanceLabel.alignment = TextAlignmentOptions.MidlineRight;
            row.DistanceLabel.overflowMode = TextOverflowModes.Overflow;
            BaUiAssets.ApplyButtonFont(row.DistanceLabel);
        }

        private static void BuildCenterButton(
            BaUiListRow row,
            RectTransform rowRect,
            float textScale,
            float buttonHeight,
            in RowRecipeFlags flags)
        {
            var centerGo = BaUiWidgets.CreateRect(rowRect, "CenterButton");
            LayoutRowButton(
                centerGo,
                ComputeCenterRightInset(flags),
                BaUiListMetrics.RowActionButtonSize,
                buttonHeight);
            row.CenterButtonImage = BaUiAssets.CreateButtonGraphic(
                centerGo, textScale, BaUiAssets.ApplyButtonBlue, 1f, bleedBottom: false);
            row.CenterButton = centerGo.gameObject.AddComponent<Button>();
            row.CenterButton.targetGraphic = row.CenterButtonImage;

            var centerIconGo = BaUiWidgets.CreateRect(centerGo, "Icon");
            BaUiWidgets.Stretch(centerIconGo);
            var iconPad = 6f * textScale;
            centerIconGo.offsetMin = new Vector2(iconPad, iconPad);
            centerIconGo.offsetMax = new Vector2(-iconPad, -iconPad);
            var centerIcon = centerIconGo.gameObject.AddComponent<Image>();
            centerIcon.raycastTarget = false;
            BaUiAssets.ConfigureOverlayIcon(centerIcon, row.CenterButtonImage);
            if (!BaUiAssets.TryApplyOverlayIcon(centerIcon, BaUiAssets.ApplySearchIcon))
            {
                var fallbackGo = BaUiWidgets.CreateRect(centerIconGo, "Fallback");
                BaUiWidgets.Stretch(fallbackGo);
                row.CenterFallbackLabel = fallbackGo.gameObject.AddComponent<TextMeshProUGUI>();
                row.CenterFallbackLabel.text = "\u2295";
                row.CenterFallbackLabel.fontSize = 14f * textScale;
                row.CenterFallbackLabel.alignment = TextAlignmentOptions.Center;
                row.CenterFallbackLabel.color = Color.white;
                row.CenterFallbackLabel.raycastTarget = false;
                BaUiAssets.ApplyButtonFont(row.CenterFallbackLabel);
            }
        }

        private static void BuildSetDestButton(
            BaUiListRow row,
            RectTransform rowRect,
            float textScale,
            float buttonHeight,
            in RowRecipeFlags flags)
        {
            var btnGo = BaUiWidgets.CreateRect(rowRect, "SetDestButton");
            LayoutRowButton(
                btnGo,
                ComputeSetDestRightInset(flags),
                RowSetDestButtonWidth(flags),
                buttonHeight);
            var btnImg = BaUiAssets.CreateButtonGraphic(btnGo, textScale, BaUiAssets.ApplyButtonBlue, bleedBottom: false);
            row.SetDestButton = btnGo.gameObject.AddComponent<Button>();
            row.SetDestButton.targetGraphic = btnImg;

            var iconGo = BaUiWidgets.CreateRect(btnGo, "Icon");
            BaUiWidgets.Stretch(iconGo);
            var iconPad = 5f * textScale;
            iconGo.offsetMin = new Vector2(iconPad, iconPad);
            iconGo.offsetMax = new Vector2(-iconPad, -iconPad);
            var icon = iconGo.gameObject.AddComponent<Image>();
            icon.raycastTarget = false;
            BaUiAssets.ConfigureOverlayIcon(icon, btnImg);
            var hasPinIcon = BaUiAssets.TryApplyPinOverlayIcon(icon);

            var btnLabelGo = BaUiWidgets.CreateRect(btnGo, "Label");
            BaUiWidgets.Stretch(btnLabelGo);
            row.SetDestLabel = btnLabelGo.gameObject.AddComponent<TextMeshProUGUI>();
            row.SetDestLabel.fontSize = 11f * textScale;
            row.SetDestLabel.fontStyle = FontStyles.UpperCase;
            row.SetDestLabel.alignment = TextAlignmentOptions.Center;
            row.SetDestLabel.color = Color.white;
            row.SetDestLabel.raycastTarget = false;
            row.SetDestLabel.enabled = !hasPinIcon;
            BaUiAssets.ApplyButtonFont(row.SetDestLabel);
        }

        private static void BuildDriveButton(
            BaUiListRow row,
            RectTransform rowRect,
            float textScale,
            float buttonHeight,
            in RowRecipeFlags flags)
        {
            var driveGo = BaUiWidgets.CreateRect(rowRect, "DriveButton");
            LayoutRowButton(
                driveGo,
                ComputeDriveRightInset(flags),
                BaUiListMetrics.RowSetButtonWidth,
                buttonHeight);
            var driveImg = BaUiAssets.CreateButtonGraphic(driveGo, textScale, BaUiAssets.ApplyButtonGreen, bleedBottom: false);
            row.DriveButton = driveGo.gameObject.AddComponent<Button>();
            row.DriveButton.targetGraphic = driveImg;

            var driveLabelGo = BaUiWidgets.CreateRect(driveGo, "Label");
            BaUiWidgets.Stretch(driveLabelGo);
            row.DriveLabel = driveLabelGo.gameObject.AddComponent<TextMeshProUGUI>();
            row.DriveLabel.fontSize = 11f * textScale;
            row.DriveLabel.fontStyle = FontStyles.UpperCase;
            row.DriveLabel.alignment = TextAlignmentOptions.Center;
            row.DriveLabel.color = Color.white;
            row.DriveLabel.raycastTarget = false;
            row.DriveLabel.text = "GO";
            BaUiAssets.ApplyButtonFont(row.DriveLabel);
        }

        private static void BuildAddButton(
            BaUiListRow row,
            RectTransform rowRect,
            float textScale,
            float buttonHeight,
            in RowRecipeFlags flags)
        {
            var addGo = BaUiWidgets.CreateRect(rowRect, "AddButton");
            LayoutRowButton(
                addGo,
                ComputeAddRightInset(flags),
                BaUiListMetrics.RowActionButtonSize,
                buttonHeight);
            var addImg = BaUiAssets.CreateButtonGraphic(addGo, textScale, BaUiAssets.ApplyButtonBlue, bleedBottom: false);
            row.AddButton = addGo.gameObject.AddComponent<Button>();
            row.AddButton.targetGraphic = addImg;

            var iconGo = BaUiWidgets.CreateRect(addGo, "Icon");
            BaUiWidgets.Stretch(iconGo);
            var iconPad = 6f * textScale;
            iconGo.offsetMin = new Vector2(iconPad, iconPad);
            iconGo.offsetMax = new Vector2(-iconPad, -iconPad);
            var icon = iconGo.gameObject.AddComponent<Image>();
            icon.raycastTarget = false;
            BaUiAssets.ConfigureOverlayIcon(icon, addImg);
            var hasAddIcon = BaUiAssets.TryApplyAddOverlayIcon(icon);

            var addLabelGo = BaUiWidgets.CreateRect(addGo, "Label");
            BaUiWidgets.Stretch(addLabelGo);
            row.AddLabel = addLabelGo.gameObject.AddComponent<TextMeshProUGUI>();
            row.AddLabel.fontSize = 11f * textScale;
            row.AddLabel.fontStyle = FontStyles.UpperCase;
            row.AddLabel.alignment = TextAlignmentOptions.Center;
            row.AddLabel.color = Color.white;
            row.AddLabel.raycastTarget = false;
            row.AddLabel.enabled = !hasAddIcon;
            BaUiAssets.ApplyButtonFont(row.AddLabel);
        }

        private static float ComputeDriveRightInset(in RowRecipeFlags flags)
        {
            var inset = 0f;
            if (flags.ShowDelete)
                inset += BaUiListMetrics.RowActionButtonSize + BaUiListMetrics.RowButtonGap;
            return inset;
        }

        private static float ComputeSetDestRightInset(in RowRecipeFlags flags) =>
            ComputeDriveRightInset(flags) +
            (flags.ShowDrive ? BaUiListMetrics.RowSetButtonWidth + BaUiListMetrics.RowButtonGap : 0f);

        private static float ComputeAddRightInset(in RowRecipeFlags flags) =>
            ComputeSetDestRightInset(flags) +
            (flags.ShowSetDest ? RowSetDestButtonWidth(flags) + BaUiListMetrics.RowButtonGap : 0f);

        private static float ComputeCenterRightInset(in RowRecipeFlags flags) =>
            ComputeAddRightInset(flags) +
            (flags.ShowAdd ? BaUiListMetrics.RowActionButtonSize + BaUiListMetrics.RowButtonGap : 0f);

        private static float ComputeRowActionsWidth(in RowRecipeFlags flags)
        {
            var width = 0f;
            if (flags.ShowDelete)
                width += BaUiListMetrics.RowActionButtonSize + BaUiListMetrics.RowButtonGap;
            if (flags.ShowDrive)
                width += BaUiListMetrics.RowSetButtonWidth + BaUiListMetrics.RowButtonGap;
            if (flags.ShowSetDest)
                width += RowSetDestButtonWidth(flags) + BaUiListMetrics.RowButtonGap;
            if (flags.ShowAdd)
                width += BaUiListMetrics.RowActionButtonSize + BaUiListMetrics.RowButtonGap;
            if (flags.ShowCenter)
                width += BaUiListMetrics.RowActionButtonSize;
            return width;
        }

        private static float ComputeRowDistanceRightInset(in RowRecipeFlags flags) =>
            ComputeRowActionsWidth(flags) + BaUiListMetrics.RowDistanceToCenterGap;

        private static float ComputeRowNameRightInset(in RowRecipeFlags flags) =>
            ComputeRowDistanceRightInset(flags) + BaUiListMetrics.RowDistanceWidth + BaUiListMetrics.RowNameToDistanceGap;

        private static float RowSetDestButtonWidth(in RowRecipeFlags flags) =>
            flags.SquareSetDest
                ? BaUiListMetrics.RowActionButtonSize
                : BaUiListMetrics.RowSetButtonWidth;

        private static void LayoutRowButton(RectTransform rect, float rightInset, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-rightInset, 0f);
            rect.sizeDelta = new Vector2(width, height);
        }
    }
}
