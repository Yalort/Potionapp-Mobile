using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using System.Collections.Generic;
using System.Linq;
namespace Potionapp_Mobile
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        readonly Dictionary<string, Dictionary<string, int>> potionRequirements = new()
        {
            { "Healing Potion", new Dictionary<string, int>{{"berry",1},{"herb",1},{"solution",1}} },
            { "Magic Potion", new Dictionary<string, int>{{"magic",1},{"root",1},{"solution",1}} },
            { "Stone Skin Potion", new Dictionary<string, int>{{"fungi",1},{"mineral",1},{"solution",1}} },
            { "Animal Brew", new Dictionary<string, int>{{"animal",1},{"magic",1},{"solution",1}} }
        };

        readonly Dictionary<string, List<string>> potionSpecialRequirements = new();
        readonly List<string> specialIngredientsStock = new();

        Dictionary<string, EditText>? ingredientFields;
        Dictionary<string, TextView>? requirementViews;
        List<string>? selectedPotions;
        ArrayAdapter<string>? listAdapter;
        ArrayAdapter<string>? recipesAdapter;
        ArrayAdapter<string>? specialAdapter;
        List<string>? recipeNames;

        void UpdateRequirementsDisplay()
        {
            if (ingredientFields == null || requirementViews == null || selectedPotions == null)
                return;

            var totals = new Dictionary<string, int>();
            foreach (var potion in selectedPotions)
            {
                if (!potionRequirements.TryGetValue(potion, out var reqs))
                    continue;
                foreach (var kvp in reqs)
                {
                    totals[kvp.Key] = totals.TryGetValue(kvp.Key, out var v) ? v + kvp.Value : kvp.Value;
                }
            }

            foreach (var kvp in requirementViews)
            {
                var required = totals.TryGetValue(kvp.Key, out var r) ? r : 0;
                int stock = 0;
                if (ingredientFields.TryGetValue(kvp.Key, out var field) && int.TryParse(field.Text, out var val))
                    stock = val;
                var remaining = stock - required;
                kvp.Value.Text = $"{required} ({remaining})";
                kvp.Value.SetTextColor(required > stock ? Android.Graphics.Color.Red : Android.Graphics.Color.Black);
            }
        }
        void SaveAllValues()
        {
            if (ingredientFields == null)
                return;

            foreach (var kvp in ingredientFields)
            {
                if (int.TryParse(kvp.Value.Text, out int val))
                {
                    IngredientStorage.SaveValue(this, kvp.Key, val);
                }
            }
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.main_tabs);

            var tabHost = FindViewById<TabHost>(Android.Resource.Id.TabHost)!;
            tabHost.Setup();

            tabHost.AddTab(tabHost.NewTabSpec("potions").SetIndicator("Potions").SetContent(Resource.Id.tab_potions));
            tabHost.AddTab(tabHost.NewTabSpec("recipes").SetIndicator("Recipes").SetContent(Resource.Id.tab_recipes));
            tabHost.AddTab(tabHost.NewTabSpec("ingredients").SetIndicator("Ingredients").SetContent(Resource.Id.tab_ingredients));

            LayoutInflater.Inflate(
                Resource.Layout.activity_main,
                tabHost.TabContentView.FindViewById<LinearLayout>(Resource.Id.tab_potions),
                true);
            LayoutInflater.Inflate(
                Resource.Layout.recipes_tab,
                tabHost.TabContentView.FindViewById<LinearLayout>(Resource.Id.tab_recipes),
                true);
            LayoutInflater.Inflate(
                Resource.Layout.ingredients_tab,
                tabHost.TabContentView.FindViewById<LinearLayout>(Resource.Id.tab_ingredients),
                true);

            ingredientFields = new Dictionary<string, EditText>
            {
                { "animal", FindViewById<EditText>(Resource.Id.animal_amount)! },
                { "berry", FindViewById<EditText>(Resource.Id.berry_amount)! },
                { "fungi", FindViewById<EditText>(Resource.Id.fungi_amount)! },
                { "herb", FindViewById<EditText>(Resource.Id.herb_amount)! },
                { "magic", FindViewById<EditText>(Resource.Id.magic_amount)! },
                { "mineral", FindViewById<EditText>(Resource.Id.mineral_amount)! },
                { "root", FindViewById<EditText>(Resource.Id.root_amount)! },
                { "solution", FindViewById<EditText>(Resource.Id.solution_amount)! }
            };

            var increments = new[] { 1, 5, 10, 100 };
            foreach (var name in ingredientFields.Keys)
            {
                foreach (var inc in increments)
                {
                    var plusId = Resources.GetIdentifier($"{name}_plus{inc}", "id", PackageName);
                    var minusId = Resources.GetIdentifier($"{name}_minus{inc}", "id", PackageName);
                    var plusBtn = FindViewById<Button>(plusId);
                    if (plusBtn != null)
                        plusBtn.Click += (s, e) => AdjustIngredient(name, inc);

                    var minusBtn = FindViewById<Button>(minusId);
                    if (minusBtn != null)
                        minusBtn.Click += (s, e) => AdjustIngredient(name, -inc);
                }
                
            requirementViews = new Dictionary<string, TextView>
            {
                { "animal", FindViewById<TextView>(Resource.Id.animal_needed)! },
                { "berry", FindViewById<TextView>(Resource.Id.berry_needed)! },
                { "fungi", FindViewById<TextView>(Resource.Id.fungi_needed)! },
                { "herb", FindViewById<TextView>(Resource.Id.herb_needed)! },
                { "magic", FindViewById<TextView>(Resource.Id.magic_needed)! },
                { "mineral", FindViewById<TextView>(Resource.Id.mineral_needed)! },
                { "root", FindViewById<TextView>(Resource.Id.root_needed)! },
                { "solution", FindViewById<TextView>(Resource.Id.solution_needed)! }
            };

            foreach (var kvp in ingredientFields)
            {
                var stored = IngredientStorage.GetValue(this, kvp.Key);
                kvp.Value.Text = stored.ToString();

                kvp.Value.TextChanged += (s, e) =>
                {
                    if (int.TryParse(kvp.Value.Text, out int val))
                        IngredientStorage.SaveValue(this, kvp.Key, val);
                    UpdateRequirementsDisplay();
                };
            }

            var potionSpinner = FindViewById<Spinner>(Resource.Id.potion_spinner)!;
            var potionNames = potionRequirements.Keys.ToList();
            potionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, potionNames);

            var addButton = FindViewById<Button>(Resource.Id.add_potion_button)!;
            var confirmButton = FindViewById<Button>(Resource.Id.confirm_button)!;
            var listView = FindViewById<ListView>(Resource.Id.selected_potions_list)!;

            selectedPotions = new List<string>();
            listAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, selectedPotions);
            listView.Adapter = listAdapter;

            UpdateRequirementsDisplay();

            addButton.Click += (s, e) =>
            {
                var selected = potionSpinner.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    selectedPotions!.Add(selected);
                    listAdapter!.NotifyDataSetChanged();
                    UpdateRequirementsDisplay();
                }
            };

            confirmButton.Click += (s, e) =>
            {
                foreach (var potion in selectedPotions!.ToList())
                {
                    if (!potionRequirements.TryGetValue(potion, out var reqs))
                        continue;

                    foreach (var kvp in reqs)
                    {
                        if (ingredientFields!.TryGetValue(kvp.Key, out var field))
                        {
                            if (int.TryParse(field.Text, out int val))
                            {
                                val = Math.Max(0, val - kvp.Value);
                                field.Text = val.ToString();
                            }
                        }
                    }

                    if (potionSpecialRequirements.TryGetValue(potion, out var specials))
                    {
                        foreach (var sp in specials)
                            specialIngredientsStock.Remove(sp);
                        specialAdapter?.NotifyDataSetChanged();
                    }
                }

                selectedPotions.Clear();
                listAdapter!.NotifyDataSetChanged();
                SaveAllValues();
                UpdateRequirementsDisplay();
            };

            // Recipes tab setup
            recipeNames = potionRequirements.Keys.ToList();
            var recipesList = FindViewById<ListView>(Resource.Id.recipes_list)!;
            recipesAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, recipeNames);
            recipesList.Adapter = recipesAdapter;
            recipesList.ChoiceMode = ChoiceMode.Single;

            var addRecipeBtn = FindViewById<Button>(Resource.Id.add_recipe_button)!;
            var removeRecipeBtn = FindViewById<Button>(Resource.Id.remove_recipe_button)!;

            addRecipeBtn.Click += (s, e) =>
            {
                var name = FindViewById<EditText>(Resource.Id.recipe_name)!.Text;
                if (string.IsNullOrWhiteSpace(name) || potionRequirements.ContainsKey(name))
                    return;

                var reqs = new Dictionary<string, int>();
                foreach (var key in ingredientFields!.Keys)
                {
                    var id = Resources.GetIdentifier($"recipe_{key}", "id", PackageName);
                    var field = FindViewById<EditText>(id)!;
                    if (int.TryParse(field.Text, out int val) && val > 0)
                        reqs[key] = val;
                }

                potionRequirements[name] = reqs;
                recipeNames!.Add(name);

                var specialsText = FindViewById<EditText>(Resource.Id.recipe_special)!.Text;
                if (!string.IsNullOrWhiteSpace(specialsText))
                    potionSpecialRequirements[name] = specialsText.Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

                recipesAdapter!.NotifyDataSetChanged();
                potionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, potionRequirements.Keys.ToList());
            };

            removeRecipeBtn.Click += (s, e) =>
            {
                if (recipesList.CheckedItemPosition >= 0 && recipesList.CheckedItemPosition < recipeNames!.Count)
                {
                    var name = recipeNames[recipesList.CheckedItemPosition];
                    recipeNames.RemoveAt(recipesList.CheckedItemPosition);
                    potionRequirements.Remove(name);
                    potionSpecialRequirements.Remove(name);
                    recipesAdapter!.NotifyDataSetChanged();
                    potionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, potionRequirements.Keys.ToList());
                }
            };

            // Special ingredients tab setup
            var specialList = FindViewById<ListView>(Resource.Id.special_list)!;
            specialAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, specialIngredientsStock);
            specialList.Adapter = specialAdapter;
            specialList.ChoiceMode = ChoiceMode.Single;

            var addSpecialBtn = FindViewById<Button>(Resource.Id.add_special_button)!;
            var removeSpecialBtn = FindViewById<Button>(Resource.Id.remove_special_button)!;

            addSpecialBtn.Click += (s, e) =>
            {
                var name = FindViewById<EditText>(Resource.Id.special_name)!.Text;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    specialIngredientsStock.Add(name.Trim());
                    specialAdapter!.NotifyDataSetChanged();
                }
            };

            removeSpecialBtn.Click += (s, e) =>
            {
                if (specialList.CheckedItemPosition >= 0 && specialList.CheckedItemPosition < specialIngredientsStock.Count)
                {
                    specialIngredientsStock.RemoveAt(specialList.CheckedItemPosition);
                    specialAdapter!.NotifyDataSetChanged();
                }
            };
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveAllValues();
        }

        void AdjustIngredient(string name, int delta)
        {
            if (ingredientFields!.TryGetValue(name, out var field))
            {
                if (!int.TryParse(field.Text, out int val))
                    val = 0;
                val = Math.Max(0, val + delta);
                field.Text = val.ToString();
            }
        }
    }
}
}
