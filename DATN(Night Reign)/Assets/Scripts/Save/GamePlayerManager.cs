public static class GameDataHolder
{
    public static Player_Characters LoadedPlayerCharactersData { get; set; }
    public static void ClearLoadedData()
    {
        LoadedPlayerCharactersData = null;
    }
}