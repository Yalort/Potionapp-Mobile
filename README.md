# PotionApp Mobile

PotionApp Mobile is a simple Android application for tracking potion recipes and ingredient stock. It is written in C# using the Xamarin/Android SDK and targets **net9.0-android**. The app provides a small toolset for keeping track of which ingredients are needed to brew various potions and lets you manage a shopping list for special ingredients.

## Features

- **Brewing Tab** – Select potions to brew from a spinner and maintain a queue. The app automatically sums up all required ingredients and compares them with your current stock.
- **Ingredients Tab** – Quickly adjust ingredient counts using plus/minus buttons. Ingredient amounts are saved using Android `SharedPreferences` so your inventory persists across sessions.
- **Recipes Tab** – Create custom potion recipes by specifying how many of each ingredient is required. Recipes can include special ingredients which are managed separately.
- **Special Ingredients List** – Keep a list of special items that are consumed when brewing certain recipes.
- **Persistent Storage** – Ingredient amounts are saved locally through the `IngredientStorage` helper class.

The UI is composed of a `DrawerLayout` with three tabs (Brewing, Ingredients and Recipes). Layout XML files live under `Resources/layout/` and are inflated in `MainActivity.cs`.

## Repository Layout

```
├── AndroidManifest.xml      – Android manifest and permissions
├── MainActivity.cs          – Main activity containing all UI logic
├── IngredientStorage.cs     – Simple helper for storing ingredient values
├── Resources/               – Layout XML, images and string resources
└── Potionapp Mobile.csproj  – .NET project file targeting net9.0-android
```

## Building

You can open the solution `Potionapp Mobile.sln` in Visual Studio 2022 (or later) with the Android workload installed. Alternatively, from a command line with the .NET SDK available, run:

```bash
dotnet build "Potionapp Mobile.sln"
```

This will compile the application for Android. Because the project is still a work in progress, there are no automated tests.

## Status

This project is experimental and not feature complete. Feel free to clone and modify it for your own potion‑tracking needs!
