# Archon Config Updater

## Description

Archon Config Updater is a tool designed to update your World of Warcraft characters' talents based on Archon
recommended talent builds. It automates the process of applying these builds to enhance your gameplay experience.

## Installation

1. Install the Talents Loadout Ex addon for World of Warcraft.
2. Run the addon once to generate the necessary files.
3. Copy `settings.example.json` to `settings.json`.
4. Edit `settings.json` to change the character name and point the output file to the current Talents Loadout Ex file
   where talents are stored.

## Usage

1. Open the application.
2. Ensure your settings are correctly configured in `settings.json`.
3. Run the application to update your character's talents based on the recommended builds.

### Command-Line Options

The application supports the following command-line options:

- `--check-update`: Checks if an update is available and exits
- `--update`: Forces the application to download and install the latest update

Examples:
```
ArchonConfigUpdater --check-update
ArchonConfigUpdater --update
```

## Auto-Update Feature

The application has a built-in update mechanism that can:
- Automatically check for updates on GitHub
- Prompt you to download and install updates
- Automatically install updates if configured

You can configure the update behavior in `settings.json`:

```json
"update": {
  "autoCheckForUpdates": true,  // Set to false to disable update checks
  "autoInstallUpdates": false   // Set to true to automatically install updates without prompting
}
```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License.