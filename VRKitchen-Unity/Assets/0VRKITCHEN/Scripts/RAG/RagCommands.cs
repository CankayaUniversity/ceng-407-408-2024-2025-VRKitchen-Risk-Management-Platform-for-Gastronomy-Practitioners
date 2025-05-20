using UnityEngine;

public static class RagCommands
{
    // === Stove on and off ===
    public const string turnOnStove = "I have turned on the stove. What now?";
    public const string turnOffStove = "I turned off the stove. What now?";

    // === Timer Wait ===
    public static string TimerSet(int minutes)
    {
        return $"I have waited {minutes} minutes. What now?";
    }


    // === Cross Contamination Scenario ===
    public static readonly string CrossContaminationQuery =
        "Cross contamination happened in the game. What do I need to do ?";

    public static readonly string ThrowTheContaminatedFood =
        "I removed contaminated food in the game. What now?";

    public static readonly string SpongeWetness =
        "I wetted the sponge. What now?";

    public static readonly string CleanTheBoard =
        "I scrubbed the contaminated board with the sponge until it is clean in game. What now?";

    public static readonly string WashHands =
        "I washed my hands in game. What now?";


}