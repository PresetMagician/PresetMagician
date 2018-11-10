# PresetMagician

PresetMagician is a tool to automatically create NKSF presets for
specified VST plugins. It also automatically creates audio previews if the VST plugin is a synth.

## Quick Start

- Set your VST paths in the "VST Paths" tab (only required once per PC)
- Click "Scan Plugins" in the "VST Plugins" tab
- Select one or more plugins for which you want to export the presets, then click "Scan Presets". Use the shift button + click to mark several lines, ctrl + click to mark individual lines
- By default, all presets will be exported. Click "Export Presets" to export them.

- Be prepared to get many crashes!

## Implemented

- Automatically create audio previews
- Export NKSF presets from VST presets for VST instruments

## Limitations

- Supports 64bit VST2 only (will be changed later)

## Known Issues

* Preset names are sorted alphabetically, not naturally. This means that, for example, the order would be **Bass1, Bass10, Bass2** instead of **Bass1, Bass2, Bass10**
* Plugins with no offline rendering are not supported; I think only a few plugins, if any, have this problem
* NI doesn't allow certain characters for their user content directories; right now we strip everything except for A-Z and whitespaces. Please let me know if it gives any problems.
* It is currently not possible to cancel one of these actions:
  * Reading presets (could take a while if you have a plugin with a huge number of presets)
  * Creating previews
* The UI looks like crap. This is because we're using alpha components, and there are quite some bugs so I didn't spent any effort for styling so far.
* Yes, I love purple

## TODO

- Parse preset information from Vendors which do not use standard VST preset implementation, which are virtually all
  - Vendor List to implement (partially researched):
    - D16 Group
- Duplicate Detection (might be difficult to implement)
- Tagging based on preset names (fuzzy search)
- Audio Preview in GUI
- Assign and edit parameter pages
- Create version for 32 bit VSTs or a combined 32/64 bit VST version
- Allow filtering the preset list by name
- Allow mass selection/deselection of the export flag
- Add button to remove all unusable plugins
- Research if NKSF audio previews are allowed for effects