namespace BalatroSaveToolkit.Objects;

public interface IBalatroSave {
    State State { get; set; }
    BalatroBlind Blind { get; set; }
    BalatroBack Back { get; set; }
    BalatroTags Tags { get; set; }
    BalatroCardAreas CardAreas { get; set; }
    BalatroGame Game { get; set; }
    BalatroVersion Version { get; set; }
}
