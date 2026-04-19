namespace RecoilHelper.Core;

public static class Lang
{
    public static string Get(string textID, string textEN, string textAR)
    {
        if (AppState.Settings.Language == "AR") return textAR;
        return AppState.Settings.Language == "EN" ? textEN : textID;
    }
}
