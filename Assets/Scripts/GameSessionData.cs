public static class GameSessionData
{
    public static int LastScore;

    /// <summary>
    /// Seed for the current match's deterministic RNG (Phase 4.5). Same seed = identical
    /// spawn sequence, which is what Skillz needs to verify fairness between players in
    /// the same match.
    ///
    /// WIRING POINT FOR SKILLZ (Phase 5, not built yet): when real Skillz match setup
    /// exists, call ResetSeedForNewMatch() then set MatchSeed to the value Skillz provides
    /// for this match BEFORE loading GameScene. Until then, a random seed is generated
    /// automatically per match so gameplay works normally.
    /// </summary>
    public static int MatchSeed;

    private static bool seedAssigned;

    /// <summary>
    /// Returns the seed to use for this match. If nothing has explicitly set MatchSeed
    /// since the last reset, generates one and locks it in so every system that calls
    /// this during the SAME match gets the SAME seed.
    /// </summary>
    public static int GetOrCreateMatchSeed()
    {
        if (!seedAssigned)
        {
            if (MatchSeed == 0)
            {
                // No seed supplied externally (e.g. Skillz not wired in yet) — generate one.
                MatchSeed = unchecked(System.Guid.NewGuid().GetHashCode());
            }
            seedAssigned = true;
            UnityEngine.Debug.Log($"[TapRush] Match seed = {MatchSeed}");
        }
        return MatchSeed;
    }

    /// <summary>
    /// Call at the start of a new match (before GetOrCreateMatchSeed() is first called)
    /// so a fresh seed gets generated instead of reusing the previous match's seed.
    /// If Skillz needs to force a specific seed for this match, set MatchSeed AFTER
    /// calling this, then GetOrCreateMatchSeed() will use that value instead of
    /// generating a random one.
    /// </summary>
    public static void ResetSeedForNewMatch()
    {
        seedAssigned = false;
        MatchSeed = 0;
    }
}
