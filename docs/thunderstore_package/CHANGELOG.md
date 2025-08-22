## v0.6.0 (latest)

**Important!** This is a big update, I reworked several parts of the mod with significant changes to how they work.
Please do a backup before updating.

- Reworked configs
    - All default configs are now embedded into the mod itself - it is not intended for them to
      be editable. The mod can always fallback to a default config in case something goes wrong loading custom configs.
    - Installing the mod on a dedicated server will no longer require manually, moving the default configs to make
      sure the mod is functioning properly. Like mentioned earlier, they are now embedded into the mod itself.
- Project clean-up
    - Improvements to the project structure will make it easier to debug and fix bugs from now on.
- New integrations
    - CLLC (CreatureLevelAndLootControl)
- Removed integrations
    - DifficultyScaler
        - Removed as I'm not maintaining it anymore, and there are mods that does the same better.

## v0.5.19 (Latest)

- Adds more error messages to xp config loading process.
    - A warning will be written to the console if it failed to read a xp config file, including the reason it failed to
      load. The mod will continue to run, as long as one of each xp type config was able to loaded.<br>
      Likely causes for failing to load are: invalid JSON structure, and comments.
- Fixes an issue with the inability to add/remove skill points from some skills. The interact text would partially block
  some of the skill buttons with its big hitbox.
- Removes embedded third-party libraries that are no longer needed.

> That was all for this patch. I am still working on a major update that will significantly improve all aspects of the
> mod, such as, the stability, ease of use, UI rework to be consistent with the game, more customizability, and much
> more.

## v0.5.18

- Xp from kills should work properly now. I tested it on my new server-side test environment with success :-)

## v0.5.17

- Fixes xp from kills not given to players other than the host. I don't have a way to test this, please let me know if
  this update didn't fix the bug :-)

## v0.5.16

- Updates for Jotunn v2.23.2
- Fixes xp not being given to the player.

## v0.5.15

- Adds Ashlands monsters to xp table.

## v0.5.14

- Adds more error checks and hopefully fixes an error that'd occur when a client level up.
- Fixes rested xp bonus multiplier not being applied to all types of xp gains. It used to only affect xp gained from
  killing.
- Fixes fire and poison skill resistance.

## v0.5.13

- Updated for Ashlands.

## v0.5.12

- Fixed error occuring with the latest version of Auga

## v0.5.11

- Maybe fixed the issue with players gaining experience even if the interaction was unsuccessful.
- Fixed an issue with the skills menu still reacting to input after it's been closed.