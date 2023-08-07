# Mystical Mayhem
A mod for Pathfinder: Wrath of the Righteous that adds character building options, both from tabletop and homebrew, mostly for spellcasters.

## This mod adds new blueprints and create a save dependency.

## Please do NOT report bugs to Owlcat when you have mods enabled.

Installation
-----------
* Download and install [Unity Mod Manager](https://www.nexusmods.com/site/mods/21).
* Download and install [ModFinder](https://github.com/Pathfinder-WOTR-Modding-Community/ModFinder) and search for ModMenu.
  * Install ModMenu.
* Search for MysticalMayhem in ModFinder and click Install.
  * Alternatively, download a [release](https://github.com/kreaddy/MysticalMayhem/releases) and drag & drop it into Unity Mod Manager.

Content
-----------
| Option | Type | Description | Homebrew |
|:-----:|:----:|-------------------------------------|:---:|
|Flagellant|Archetype|A cleric archetype designed by Kobold Press.<br>Can get bonuses to DC and CL by sacrificing health. Very powerful with the right domains for a casting-focused cleric.|Yes|
|Invoker|Archetype|A warlock archetype. Sacrifices patron abilities to obtain an at-will ray attack reminiscent of D&D warlocks.|Yes|
|Pact Wizard|Archetype|Haunted Heroes Handbook version. A wizard who trades bonus feats for a witch patron, an oracle curse and other goodies.|No|
|Razmiran Priest|Archetype|Not the prestige class but the sorcerer archetype. Loses a few bloodline spells and one bloodline power in exchange for infinite scrolls and wands as long as you have enough spells per day to cover them.<br>Slightly nerfed from their tabletop equivalent as their abilities only work with spells on the cleric list instead of all divine spells. Still very overpowered but you have to live with the shame of worshipping a level 19 wizard who has to beg Tar-Baphon to leave him alone.|No|
|Warsighted|Archetype|A more fighter-y oracle archetype who loses revelations in favor of fighter bonus feats.|No|
|Warlock|Class|3pp class designed by [Optical Sheanigans Games](https://www.d20pfsrd.com/classes/3rd-party-classes/optimal-shenanigans-games/warlock/). It's a full caster with a gimped spell list like the Witch but very powerful patron abilities that can completely change its role. It has no archetypes so I made one from scratch (the Invoker). I might make more later.|Yes|
|Acadamae Graduate|Feat|Cast full-round summoning spells as standard actions but with a Fortitude save to avoid being fatigued.|No|
|Earth Magic|Feat|+1 CL when on your favorite terrain.|No|
|Purity of Sin|Feat|Thassilonian Specialist only. +2 CL for the school of magic associated with the sin.<br>Becomes an arcane discovery if Tabletop Tweaks Base is installed.|Yes|
|School Expertise|Feat|Arcane school powers can be used 3 additional times. Can be taken multiple times.<br>Becomes an arcane discovery if Tabletop Tweaks Base is installed.|Yes|
|Staff-like Wand|Feat|Calculate wands CL and DC with the wizard's caster level and stats.<br>Becomes an arcane discovery if Tabletop Tweaks Base is installed.|No|
|Circle of Order|Magus Arcana|Adds half the magus level to AC against Chaotic enemies for 1 round.|No|
|Malice|Magus Arcana (Hexcrafter only)|Originally designed by Rite Publishing.<br>The hexcrafter's attacks deal an additional 2d6 unholy damage to targets under the effect of their hexes.|Yes|
|Abundant Spell Synthesis|Mythic Ability|Spell Synthesis can be used one extra time per 3 mythic ranks. This ability can't be taken before rank 3 so it's basically 3 extra uses at rank 9.|Yes|
|Material Freedom|Mythic Ability|Can ignore a specific material spell component (Diamond, Jade, etc...).|Yes|
|Purity of Sin (Mythic)|Mythic Feat|Purity of Sin also grants +2 bonus to DC.|Yes|
|Spell Synthesis|Class Feature|Add the Mystic Theurge's capstone.|No|
|Manifestations|Class Feature|Add the capstones to Shaman spirits. They have been adapted to the game the same way Owlcat adapted the final revelations for Oracle mysteries (they're virtually identical).|No|
|Meteor Swarm|Spell|Add the level 9 Evocation spell Meteor Swarm.<br>It's quite simplified from its TT version because having to pick 4 targets every time you cast it would be annoying. It now simply deals 28d6 fire damage in an area (to counterbalance the fact the save penalty after the touch attack doesn't exist anymore). Might need a balance pass.|No|
|Baldur's Gate Stoneskin|Spell|**Disabled by default!**<br>Change Stoneskin to be closer to its [Baldur's Gate incarnation](https://baldursgate.fandom.com/wiki/Stoneskin).|Yes|
|Sure Casting|Spell|Add the level 1 Divination spell [Sure Casting](https://www.d20pfsrd.com/magic/all-spells/s/sure-casting/).|No|
|Bloody Tears and Jagged Smile|Spell|Add the level 2 Necromancy spell [Bloody Tears and Jagged Smile](https://www.d20pfsrd.com/magic/all-spells/b/bloody-tears-and-jagged-smile/).|No|
|Draconic Malice|Spell|Add the level 3 Enchantment spell [Draconic Malice](https://www.d20pfsrd.com/magic/all-spells/d/draconic-malice).|No|
|Mighty Strength|Spell|Add the level 4 Transmutation spell [Mighty Strength](https://www.d20pfsrd.com/magic/all-spells/m/mighty-strength/).|No|
|Spell Turning|Spell|Add the level 7 Abjuration spell [Spell Turning](https://www.d20pfsrd.com/magic/all-spells/s/spell-turning).<br>The resonating field behavior of the spell is not implemented, mostly because I find it a poor fit for a video game. Instead, the spell behaves like its Baldur's Gate counterpart: if Spell Turning absorbs a spell of higher level than it has left then the spell is not reflected, it simply fizzles. Other than that, it works exactly like tabletop.|No|

Settings
-----------
Because this mod uses Mod Menu, settings are in the game settings: Settings > Mods > Mystical Mayhem.

FAQ & Troubleshooting
-----------
**Q:** Something doesn't work!
<br>**A:** Create an [issue](https://github.com/kreaddy/MysticalMayhem/issues) and describe **what** doesn't work and how I could reproduce it, if possible. You can also ping me in the #mod-user-general channel of the [Owlcat discord](https://discord.com/invite/owlcat).

**Q:** What about Magic Time?
<br>**A:** Magic Time was the first mod I ever made for a Unity game so I felt the need to rebuild it from the ground up.

**Q:** No Blood Arcanist or metamagic?
<br>**A:** Most of my old metamagic feats and arcane discoveries have been remade in [Tabletop Tweaks](https://github.com/Vek17/TabletopTweaks-Base). Likewise, Blood Arcanist and Mythic Poison are now in [Homebrew Archetypes](https://www.nexusmods.com/pathfinderwrathoftherighteous/mods/279).

**Q:** Can you add X?
<br>**A:** Maybe. Make a feature request and I'll add it if I feel like it. No promises though.

**Q:** Your naming sense sucks.
<br>**A:** I'm just a fan of Ogre Battle and Might & Magic ;_;

Credits
-----------
* [Kifusou](https://github.com/Kifusou) and [nixgnot](https://github.com/nixgnot) for translating the mod into Chinese.
* bubbles for [BubblePrints](https://github.com/factubsio/BubblePrints) and for being generally very helpful.
* Vek17 for [Tabletop Tweaks](https://github.com/Vek17/TabletopTweaks-Core), which has been very useful to learn when I started modding.
* WittleWolfie for releasing Mod Menu, finally freeing us from the horror of ImGui.
* The mod community on the [Owlcat discord](https://discord.com/invite/owlcat).
