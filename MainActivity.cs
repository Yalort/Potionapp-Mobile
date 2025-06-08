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
        List<string>? selectedPotions;
        ArrayAdapter<string>? listAdapter;

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

            var increments = new[] { 1, 5, 10, 100 };
            foreach (var name in ingredientFields.Keys)
            {
                foreach (var inc in increments)
                {
                    var plusId = Resources.GetIdentifier($"{name}_plus{inc}", "id", PackageName);
                    var minusId = Resources.GetIdentifier($"{name}_minus{inc}", "id", PackageName);
                    FindViewById<Button>(plusId)?.Click += (s, e) => AdjustIngredient(name, inc);
                    FindViewById<Button>(minusId)?.Click += (s, e) => AdjustIngredient(name, -inc);
                }
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

            addButton.Click += (s, e) =>
            {
                var selected = potionSpinner.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selected))
                {
                    selectedPotions!.Add(selected);
                    listAdapter!.NotifyDataSetChanged();
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
            };
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