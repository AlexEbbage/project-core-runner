namespace CoreRacer.Gameplay.Run
{
    public enum RunState
    {
        None,
        MainMenu,
        Starting,
        Running,
        Paused,
        Crashed,
        ContinueOffered,
        GameOver,
        ReturningToMenu
    }

    public enum RunEndReason
    {
        Unknown,
        PlayerDeath,
        QuitToMenu,
        CompletedDebugRun
    }
}
