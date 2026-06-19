namespace Capisoft.Lib.BaUnifiedUI.Fluent
{
    public enum BaPanelRecipe
    {
        /// <summary>370px docked action panel (route / walk controls).</summary>
        ActionPanel,
        [System.Obsolete("Use ActionPanel.")]
        DockedHud = ActionPanel,
        /// <summary>420px+ map list panel — same chrome as <see cref="ActionPanel"/>; width drives header widen.</summary>
        WideMapPanel,
        Modal,
        Settings,
        MainPanel,
        Banner,
        /// <summary>White in-game BizMan modal (employee assign, factory dialogs).</summary>
        BizManLight
    }

    public enum BaButtonStyle
    {
        Blue,
        Grey,
        Green,
        Red
    }

    public enum BaTextStyle
    {
        Title,
        Body,
        Muted,
        Warning
    }

    public enum BaDock
    {
        BottomLeft,
        BottomCenter,
        TopCenter,
        Center
    }

    public enum BaIcons
    {
        Settings,
        Pin,
        Add,
        Car,
        Focus,
        History,
        Close
    }
}
