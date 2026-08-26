namespace Capisoft.Lib.BaUnifiedUI.Shortcuts
{
    public enum BaKeybindConflictKind
    {
        None = 0,
        GameInput = 1,
        BaUnifiedUiOption = 2
    }

    /// <summary>Describes why a shortcut is currently unavailable.</summary>
    public readonly struct BaKeybindConflict
    {
        public static BaKeybindConflict None => default;

        public BaKeybindConflictKind Kind { get; }

        public string OwnerModId { get; }

        public string OwnerOptionId { get; }

        public bool HasConflict => Kind != BaKeybindConflictKind.None;

        internal BaKeybindConflict(BaKeybindConflictKind kind, string ownerModId, string ownerOptionId)
        {
            Kind = kind;
            OwnerModId = ownerModId ?? string.Empty;
            OwnerOptionId = ownerOptionId ?? string.Empty;
        }

        public override string ToString()
        {
            return Kind switch
            {
                BaKeybindConflictKind.GameInput => "Big Ambitions input binding",
                BaKeybindConflictKind.BaUnifiedUiOption =>
                    string.IsNullOrEmpty(OwnerOptionId)
                        ? OwnerModId
                        : OwnerModId + ":" + OwnerOptionId,
                _ => string.Empty
            };
        }
    }
}
