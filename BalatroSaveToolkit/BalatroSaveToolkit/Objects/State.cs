namespace BalatroSaveToolkit.Objects;

public enum State {
    SelectingHand = 1,
    HandPlayed = 2,
    DrawToHand = 3,
    GameOver = 4,
    Shop = 5,
    PlayTarot = 6,
    BlindSelect = 7,
    RoundEval = 8,
    TarotPack = 9,
    PlanetPack = 10,
    Menu = 11,
    Tutorial = 12,
    Splash = 13, // DO NOT CHANGE, this has a dependency in the SOUND_MANAGER
    Sandbox = 14,
    SpectralPack = 15,
    DemoCta = 16,
    StandardPack = 17,
    BuffoonPack = 18,
    NewRound = 19
}
