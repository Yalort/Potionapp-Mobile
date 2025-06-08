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

        Dictionary<string, EditText>? ingredientFields;
        Dictionary<string, TextView>? requirementViews;
        List<string>? selectedPotions;
        ArrayAdapter<string>? listAdapter;

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

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

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
                }

                selectedPotions.Clear();
                listAdapter!.NotifyDataSetChanged();
                SaveAllValues();
                UpdateRequirementsDisplay();
            };
        }

        protected override void OnPause()
        {
            base.OnPause();
            SaveAllValues();
        }
    }
}