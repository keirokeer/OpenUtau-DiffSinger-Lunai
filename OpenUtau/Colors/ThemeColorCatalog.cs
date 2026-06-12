using System.Linq;

namespace OpenUtau.Colors;

public static class ThemeColorCatalog {
    public sealed record Section(string Title, params string[] Keys);

    public static readonly Section[] Sections = [
        new("General", [
            "BackgroundColor",
            "BackgroundColorPointerOver",
            "BackgroundColorPressed",
            "BackgroundColorDisabled",
            "ForegroundColor",
            "ForegroundColorPointerOver",
            "ForegroundColorPressed",
            "ForegroundColorDisabled",
            "BorderColor",
            "BorderColorPointerOver",
            "WarningColor",
        ]),
        new("Accent", [
            "SystemAccentColor",
            "SystemAccentColorLight1",
            "SystemAccentColorDark1",
            "NeutralAccentColor",
            "NeutralAccentColorPointerOver",
            "AccentColor1",
            "AccentColor1Note",
            "AccentColor2",
            "AccentColor3",
            "NoteBorderColor",
            "NoteBorderColorPressed",
        ]),
        new("Workspace", [
            "WorkspaceCanvasColor",
            "WorkspaceCardColor",
            "WorkspaceElevatedSurfaceColor",
            "TrackBackgroundAltColor",
            "MutedIconColor",
        ]),
        new("Toolbar & tooltips", [
            "TransportToolbarOffHoverColor",
            "ToolbarCheckedHoverColor",
            "ToolTipForegroundColor",
            "PianoRollToolbarStripColor",
            "PianoRollToolbarButtonHoverColor",
        ]),
        new("Top bar", [
            "AppTopBarTransportStripColor",
            "AppTopBarTransportHoverColor",
            "AppTopBarValueStripColor",
            "AppTopBarValueDividerColor",
        ]),
        new("Piano roll", [
            "TickLineColor",
            "BarNumberColor",
            "FinalPitchColor",
            "PianoRollWaveformPeakColor",
            "PianoRollTimelineStripColor",
        ]),
        new("Piano keyboard", [
            "WhiteKeyColorLeft",
            "WhiteKeyColorRight",
            "ToolbarCheckedPianoLightColor",
            "WhiteKeyNameColor",
            "CenterKeyColorLeft",
            "CenterKeyColorRight",
            "CenterKeyNameColor",
            "BlackKeyColorLeft",
            "BlackKeyColorRight",
            "BlackKeyNameColor",
        ]),
    ];

    public static string[] AllResourceKeys { get; } = Sections
        .SelectMany(section => section.Keys)
        .Distinct()
        .ToArray();
}
