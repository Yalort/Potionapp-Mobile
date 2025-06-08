using Android.Content;
namespace Potionapp_Mobile;

public static class IngredientStorage
{
    const string PrefName = "ingredient_prefs";

    public static void SaveValue(Context context, string key, int value)
    {
        var prefs = context.GetSharedPreferences(PrefName, FileCreationMode.Private);
        using var editor = prefs.Edit();
        editor.PutInt(key, value);
        editor.Apply();
    }

    public static int GetValue(Context context, string key, int defaultValue = 0)
    {
        var prefs = context.GetSharedPreferences(PrefName, FileCreationMode.Private);
        return prefs.GetInt(key, defaultValue);
    }
}

