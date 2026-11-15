using System.Collections.Generic;

namespace Beasts.Data;

// Beastcrafting recipes and talisman drops are up to date for 3.29.0 "Curse of the Allflame" (2026-07-24).
// Sources: official 3.29.0 patch notes (Talisman Revamp / Item Changes) and poewiki Talisman + Beastcrafting (09-10.2026).
public static class BeastsDatabase
{
    public static List<Beast> AllBeasts = new()
    {
        new Beast
        {
            DisplayName = "Vivid Watcher",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestSquidT3MemoryLine_",
            Crafts =
            [
                "Sacrifice an Exceptional Support Gem: for 3 high Level, high Quality Support Gems"
            ]
        },
        new Beast
        {
            DisplayName = "Vivid Vulture",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestVultureParasiteT3MemoryLine",
            Crafts = ["Transform an Item: Reroll a Synthesis Implicit Modifier on a Unique Item"]
        },
        new Beast
        {
            DisplayName = "Vivid Abberarach",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestPlatedScorpionT3MemoryLine",
            Crafts = ["Create an Item: Shaper Guardian, Elder Guardian or Conqueror Map"]
        },
        new Beast
        {
            DisplayName = "Wild Brambleback",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestBrambleHulkT3MemoryLine",
            Crafts =
            [
                "Transform an Item: Adds 500m Experience to an Exceptional Support Gem"
            ]
        },
        new Beast
        {
            DisplayName = "Wild Hellion Alpha",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestHellionT3MemoryLine",
            Crafts =
            [
                "Modify Mods on an Item: Reroll a Watcher's Eye Modifier"
            ]
        },
        new Beast
        {
            DisplayName = "Wild Bristle Matron",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestBeastT3MemoryLine_",
            Crafts =
            [
                "Modify Mods on an Item: Add a crafted Meta-modifier to a non-Unique Item"
            ]
        },
        new Beast
        {
            DisplayName = "Primal Crushclaw",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestNessaCrabT3MemoryLine_",
            Crafts =
            [
                "Create an Item: A valuable Scarab"
            ]
        },
        new Beast
        {
            DisplayName = "Primal Cystcaller",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestGoatmanT3MemoryLine",
            Crafts =
            [
                "Create an Item: Nightmare Map"
            ]
        },
        new Beast
        {
            DisplayName = "Primal Rhex Matriarch",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestRhexT3MemoryLine",
            Crafts =
            [
                "Create an Item: Synthesis Unique Map"
            ]
        },
        new Beast
        {
            DisplayName = "Black Mórrigan",
            Path = "Metadata/Monsters/LeagueAzmeri/GullGoliathBestiary_",
            Crafts =
            [
                "Apply a Hinekora's Lock: To a Magic Item (with Craicic Croaker)",
                "Create a Unique: Talisman",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Cat skill (with Farrul, First of the Plains)",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Avian skill (with Saqawal, First of the Sky)",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Spider skill (with Fenumus, First of the Night)",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Crab skill (with Craiceann, First of the Deep)",
                "Fracture two Modifiers: On a Rare Talisman with at least 6 modifiers (with Fenumal Plagued Arachnid)",
                "Modify an Item: to Have Maximum Possible Links (with Craicic Sand Spitter)",
                "Modify an Item: to Have Maximum Possible Number of Sockets (with Craicic Shield Crab)",
            ]
        },
        new Beast
        {
            DisplayName = "Vivid Vulture",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestVultureParasiteT3",
            Crafts = ["Transform an Item: Reroll a Synthesis Implicit Modifier on a Unique Item"]
        },
        new Beast
        {
            DisplayName = "Craicic Croaker",
            Path = "Metadata/Monsters/LeagueBestiary/GemFrogBestiary",
            Crafts =
            [
                "Create an Imprint: Of a Magic Item",
                "Apply a Hinekora's Lock: To a Magic Item (with Black Mórrigan)"
            ],
            DropsTalisman = "Croaker Talisman (+1 to Level of all Skill Gems)"
        },
        new Beast
        {
            DisplayName = "Wild Hellion Alpha",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestHellionT3",
            Crafts =
            [
                "Modify Mods on an Item: Reroll a Watcher's Eye Modifier"
            ]
        },
        new Beast
        {
            DisplayName = "Wild Bristle Matron",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestBeastT3",
            Crafts =
            [
                "Modify Mods on an Item: Add a crafted Meta-modifier to a non-Unique Item"
            ]
        },
        new Beast
        {
            DisplayName = "Fenumus, First of the Night",
            Path = "Metadata/Monsters/LeagueBestiary/SpiderPlatedBestiarySpiritBoss",
            Crafts =
            [
                "Craft an Aspect Skill onto an Item: Aspect of the Spider skill",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Spider skill (with Black Mórrigan)"
            ],
            DropsTalisman = "Fenumal Talisman (100% increased Aspect of the Spider Buff Effect)"
        },
        new Beast
        {
            DisplayName = "Vivid Watcher",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestSquidT3_",
            Crafts =
            [
                "Sacrifice an Exceptional Support Gem: for 3 high Level, high Quality Support Gems"
            ]
        },
        new Beast
        {
            DisplayName = "Farrul, First of the Plains",
            Path = "Metadata/Monsters/LeagueBestiary/TigerBestiarySpiritBoss",
            Crafts =
            [
                "Craft an Aspect Skill onto an Item: Aspect of the Cat skill",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Cat skill (with Black Mórrigan)"
            ],
            DropsTalisman = "Farric Talisman (100% increased Aspect of the Cat Buff Effect)"
        },
        new Beast
        {
            DisplayName = "Saqawal, First of the Sky",
            Path = "Metadata/Monsters/LeagueBestiary/MarakethBirdSpiritBoss",
            Crafts =
            [
                "Craft an Aspect Skill onto an Item: Aspect of the Avian skill",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Avian skill (with Black Mórrigan)"
            ],
            DropsTalisman = "Saqawine Talisman (100% increased Aspect of the Avian Buff Effect)"
        },
        new Beast
        {
            DisplayName = "Farric Frost Hellion Alpha",
            Path = "Metadata/Monsters/LeagueBestiary/HellionBestiary2",
            Crafts =
            [
                "Create Currency Items: A Stack of 3 Orbs of Unmaking (with Farric Gargantuan)",
                "Fracture a Modifier: On a Rare Talisman with at least 4 modifiers"
            ],
            DropsTalisman = "Frost Hellion Talisman (+4% to maximum Cold Resistance)"
        },
        new Beast
        {
            DisplayName = "Craiceann, First of the Deep",
            Path = "Metadata/Monsters/LeagueBestiary/NessaCrabBestiarySpiritBoss",
            Crafts =
            [
                "Craft an Aspect Skill onto an Item: Aspect of the Crab skill",
                "Craft an Aspect Skill onto an Item: Level 30 Aspect of the Crab skill (with Black Mórrigan)"
            ],
            DropsTalisman = "Craicic Talisman (100% increased Aspect of the Crab Buff Effect)"
        },
        new Beast
        {
            DisplayName = "Wild Brambleback",
            Path = "Metadata/Monsters/LeagueHarvest/Red/HarvestBrambleHulkT3",
            Crafts =
            [
                "Transform an Item: Adds 500m Experience to an Exceptional Support Gem"
            ]
        },
        new Beast
        {
            DisplayName = "Primal Rhex Matriarch",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestRhexT3",
            Crafts =
            [
                "Create an Item: Synthesis Unique Map"
            ]
        },
        new Beast
        {
            DisplayName = "Farric Tiger Alpha",
            Path = "Metadata/Monsters/LeagueBestiary/TigerBestiary",
            Crafts =
            [
                "Open a Portal: to Farrul's Den"
            ],
            DropsTalisman = "Tiger Talisman (8% increased Action Speed)"
        },
        new Beast
        {
            DisplayName = "Fenumal Plagued Arachnid",
            Path = "Metadata/Monsters/LeagueBestiary/SpiderPlagueBestiary",
            Crafts =
            [
                "Create an Imprint: Of a Rare Talisman",
                "Fracture two Modifiers: On a Rare Talisman with at least 6 modifiers (with Black Mórrigan)"
            ],
            DropsTalisman = "Plagued Arachnid Talisman (35% increased Effect of Withered)"
        },
        new Beast
        {
            DisplayName = "Fenumal Hybrid Arachnid",
            Path = "Metadata/Monsters/LeagueBestiary/SpiderPlatedBestiary",
            Crafts =
            [
                "Open a Portal: to Fenumus' Lair"
            ],
            DropsTalisman = "Hybrid Arachnid Talisman (Minions have +30% to Damage over Time Multiplier)"
        },
        new Beast
        {
            DisplayName = "Saqawine Rhex",
            Path = "Metadata/Monsters/LeagueBestiary/Avians/MarakethBirdBestiary",
            Crafts =
            [
                "Open a Portal: to Saqawal's Roost"
            ],
            DropsTalisman = "Rhex Talisman (100% of Cold and Lightning Damage from Hits taken as Fire Damage)"
        },
        new Beast
        {
            DisplayName = "Farric Wolf Alpha",
            Path = "Metadata/Monsters/LeagueBestiary/WolfBestiary",
            Crafts =
            [
                "Modify Mods on an Item: Add a Prefix, Remove a Random Suffix"
            ],
            DropsTalisman = "Wolf Alpha Talisman (+40% to Global Critical Strike Multiplier)"
        },
        new Beast
        {
            DisplayName = "Craicic Spider Crab",
            Path = "Metadata/Monsters/LeagueBestiary/CrabSpiderBestiary",
            Crafts =
            [
                "Open a Portal: to Craiceann's Cove"
            ],
            DropsTalisman = "Spider Crab Talisman (+15% to Quality of all Skill Gems)"
        },
        new Beast
        {
            DisplayName = "Primal Crushclaw",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestNessaCrabT3",
            Crafts =
            [
                "Create an Item: A valuable Scarab"
            ]
        },
        new Beast
        {
            DisplayName = "Craicic Vassal",
            Path = "Metadata/Monsters/LeagueBestiary/ParasiticSquidBestiary",
            Crafts =
            [
                "Corrupt a Map: To have an Implicit Modifier",
                "Corrupt a Map: Twice (with Fenumal Scorpion)"
            ],
            DropsTalisman = "Octopus Talisman (20% chance to Freeze Enemies for 1 second when they Hit you)"
        },
        new Beast
        {
            DisplayName = "Vivid Abberarach",
            Path = "Metadata/Monsters/LeagueHarvest/Green/HarvestPlatedScorpionT3",
            Crafts =
            [
                "Create an Item: Shaper Guardian, Elder Guardian or Conqueror Map"
            ]
        },
        new Beast
        {
            DisplayName = "Farric Lynx Alpha",
            Path = "Metadata/Monsters/LeagueBestiary/LynxBestiary",
            Crafts =
            [
                "Modify Mods on an Item: Add a Suffix, Remove a Random Prefix"
            ],
            DropsTalisman = "Lynx Talisman (+4% to maximum Lightning Resistance)"
        },
        new Beast
        {
            DisplayName = "Craicic Maw",
            Path = "Metadata/Monsters/LeagueBestiary/FrogBestiary",
            Crafts =
            [
                "Modify Mods on an Item: Add a Mod to a Shaper Item (with Fenumal Devourer)",
                "Modify Mods on an Item: Add a Mod to an Elder Item (with Saqawine Blood Viper)",
                "Modify Mods on an Item: Add a Mod to a Redeemer Item (with Fenumal Queen)",
                "Modify Mods on an Item: Add a Mod to a Hunter Item (with Craicic Watcher)",
                "Modify Mods on an Item: Add a Mod to a Crusader Item (with Farric Goliath)",
                "Modify Mods on an Item: Add a Mod to a Warlord Item (with Fenumal Scrabbler)"
            ],
            DropsTalisman = "Great Maw Talisman (15% increased Attributes)"
        },
        new Beast
        {
            DisplayName = "Farric Ape",
            Path = "Metadata/Monsters/LeagueBestiary/MonkeyBloodBestiary",
            Crafts =
            [
                "Create a Unique: Belt"
            ],
            DropsTalisman = "Ape Talisman (+1 to Minimum Endurance, Frenzy and Power Charges)"
        },
        new Beast
        {
            DisplayName = "Primal Cystcaller",
            Path = "Metadata/Monsters/LeagueHarvest/Blue/HarvestGoatmanT3",
            Crafts =
            [
                "Create an Item: Nightmare Map"
            ]
        },
        new Beast
        {
            DisplayName = "Craicic Savage Crab",
            Path = "Metadata/Monsters/LeagueBestiary/CrabParasiteLargeBestiary_",
            Crafts =
            [
                "Create a Unique: Item",
                "Modify Mods on an Item: Add a Mod to a Rare Map (with Saqawine Cobra)"
            ],
            DropsTalisman = "Savage Crab Talisman (Damage Penetrates 15% Cold Resistance)"
        },
        new Beast
        {
            DisplayName = "Saqawine Vulture",
            Path = "Metadata/Monsters/LeagueBestiary/VultureBestiary",
            Crafts =
            [
                "Create an Item: Fully-linked Six-socket Rare"
            ],
            DropsTalisman = "Vulture Talisman (Skills fire an additional Projectile)"
        },
        new Beast
        {
            DisplayName = "Saqawine Cobra",
            Path = "Metadata/Monsters/LeagueBestiary/SnakeBestiary1",
            Crafts =
            [
                "Create a Unique: Mace or Sceptre",
                "Modify Mods on an Item: Add a Mod to a Rare Map (with Craicic Savage Crab)"
            ],
            DropsTalisman = "Cobra Talisman (+1 to Maximum Frenzy Charges)"
        },
        new Beast
        {
            DisplayName = "Farric Pit Hound",
            Path = "Metadata/Monsters/LeagueBestiary/PitbullBestiary",
            Crafts =
            [
                "Create an Item: Level 21 Corrupted Gem"
            ],
            DropsTalisman = "Pitbull Talisman (Warcries Exert 1 additional Attack)"
        },
        new Beast
        {
            DisplayName = "Farric Ursa",
            Path = "Metadata/Monsters/LeagueBestiary/DropBearBestiary",
            Crafts =
            [
                "Create a Unique: Body Armour"
            ],
            DropsTalisman = "Ursa Talisman (30% increased Effect of your Marks)"
        },
        new Beast
        {
            DisplayName = "Fenumal Queen",
            Path = "Metadata/Monsters/LeagueBestiary/InsectSpawnerBestiary",
            Crafts =
            [
                "Create a Unique: Staff",
                "Modify Mods on an Item: Add a Mod to a Redeemer Item (with Craicic Maw)"
            ],
            DropsTalisman = "Carrion Queen Talisman (+1 to maximum number of Spectres)"
        },
        new Beast
        {
            DisplayName = "Farric Goliath",
            Path = "Metadata/Monsters/LeagueBestiary/BestiarySpiker",
            Crafts =
            [
                "Create a Unique: Bow",
                "Modify Mods on an Item: Add a Mod to a Crusader Item (with Craicic Maw)"
            ],
            DropsTalisman = "Goliath Talisman (Projectiles Pierce 3 additional Targets)"
        },
        new Beast
        {
            DisplayName = "Fenumal Devourer",
            Path = "Metadata/Monsters/LeagueBestiary/RootSpiderBestiary_",
            Crafts =
            [
                "Create a Unique: Shield or Quiver",
                "Modify Mods on an Item: Add a Mod to a Shaper Item (with Craicic Maw)"
            ],
            DropsTalisman = "Devourer Talisman (Damage Penetrates 15% Fire Resistance)"
        },
        new Beast
        {
            DisplayName = "Craicic Watcher",
            Path = "Metadata/Monsters/LeagueBestiary/SquidBestiary",
            Crafts =
            [
                "Create a Unique: Claw or Dagger",
                "Modify Mods on an Item: Add a Mod to a Hunter Item (with Craicic Maw)"
            ],
            DropsTalisman = "Watcher Talisman (Damage Penetrates 15% Lightning Resistance)"
        },
        new Beast
        {
            DisplayName = "Saqawine Chimeral",
            Path = "Metadata/Monsters/LeagueBestiary/IguanaBestiary",
            Crafts =
            [
                "Create Currency Items: A Stack of 10 Random Currency"
            ],
            DropsTalisman = "Chimeral Talisman (30% increased Projectile Speed)"
        },
        new Beast
        {
            DisplayName = "Farric Magma Hound",
            Path = "Metadata/Monsters/LeagueBestiary/HoundBestiary",
            Crafts =
            [
                "Create an Item: 23% Quality Corrupted Gem"
            ],
            DropsTalisman = "Magma Hound Talisman (Unaffected by Ignite)"
        },
        new Beast
        {
            DisplayName = "Farric Taurus",
            Path = "Metadata/Monsters/LeagueBestiary/BestiaryBull",
            Crafts =
            [
                "Create a Unique: Map"
            ],
            DropsTalisman = "Taurus Talisman (+1 to Maximum Endurance Charges)"
        },
        new Beast
        {
            DisplayName = "Craicic Squid",
            Path = "Metadata/Monsters/LeagueBestiary/SeaWitchSpawnBestiary",
            Crafts =
            [
                "Transform an Item: Reroll a Talisman Base Type"
            ],
            DropsTalisman = "Squid Talisman (30% increased maximum Mana)"
        },
        new Beast
        {
            DisplayName = "Craicic Shield Crab",
            Path = "Metadata/Monsters/LeagueBestiary/ShieldCrabBestiary",
            Crafts =
            [
                "Create Currency Items: A Stack of 4 Jeweller's Orbs",
                "Create Currency Items: A Stack of 2 Orbs of Binding (with Craicic Sand Spitter)",
                "Modify an Item: to Have Maximum Possible Number of Sockets (with Black Mórrigan)"
            ],
            DropsTalisman = "Shield Crab Talisman (30% increased Global Defences)"
        },
        new Beast
        {
            DisplayName = "Craicic Sand Spitter",
            Path = "Metadata/Monsters/LeagueBestiary/SandSpitterBestiary",
            Crafts =
            [
                "Create Currency Items: Orb of Fusing",
                "Create Currency Items: A Stack of 2 Orbs of Binding (with Craicic Shield Crab)",
                "Modify an Item: to Have Maximum Possible Links (with Black Mórrigan)"
            ],
            DropsTalisman = "Sand Spitter Talisman (12% increased Movement Speed)"
        },
        new Beast
        {
            DisplayName = "Fenumal Widow",
            Path = "Metadata/Monsters/LeagueBestiary/Spider5Bestiary",
            Crafts =
            [
                "Create a Unique: Gloves"
            ],
            DropsTalisman = "Black Widow Talisman (Utility Flasks gain 2 Charges every 3 seconds)"
        },
        new Beast
        {
            DisplayName = "Saqawine Rhoa",
            Path = "Metadata/Monsters/LeagueBestiary/RhoaBestiary",
            Crafts =
            [
                "Create a Rare: Four-Linked Helmet"
            ],
            DropsTalisman = "Rhoa Talisman (Gain 15% of Maximum Life as Extra Armour)"
        },
        new Beast
        {
            DisplayName = "Fenumal Scorpion",
            Path = "Metadata/Monsters/LeagueBestiary/BlackScorpionBestiary",
            Crafts =
            [
                "Convert this Unique Item: Into Another Random Unique Item",
                "Corrupt a Map: Twice (with Craicic Vassal)"
            ],
            DropsTalisman = "Scorpion Talisman (+20% to Damage over Time Multiplier)"
        },
        new Beast
        {
            DisplayName = "Farric Flame Hellion Alpha",
            Path = "Metadata/Monsters/LeagueBestiary/HellionBestiary",
            Crafts =
            [
                "Create a Unique: Ring"
            ],
            DropsTalisman = "Flame Hellion Talisman (+4% to maximum Fire Resistance)"
        },
        new Beast
        {
            DisplayName = "Farric Chieftain",
            Path = "Metadata/Monsters/LeagueBestiary/BestiaryMonkeyChiefBlood",
            Crafts =
            [
                "Create a Unique: Amulet"
            ],
            DropsTalisman = "Chieftain Talisman (16% increased Area of Effect)"
        },
        new Beast
        {
            DisplayName = "Saqawine Retch",
            Path = "Metadata/Monsters/LeagueBestiary/KiwethBestiary",
            Crafts =
            [
                "Create a Unique: Boots"
            ],
            DropsTalisman = "Retch Talisman (+1 to Maximum Power Charges)"
        },
        new Beast
        {
            DisplayName = "Farric Goatman",
            Path = "Metadata/Monsters/LeagueBestiary/GoatmanLeapSlamBestiary",
            Crafts =
            [
                "Create a Unique: Flask"
            ],
            DropsTalisman = "Goatman Talisman (Hits ignore Enemy Physical Damage Reduction)"
        },
        new Beast
        {
            DisplayName = "Saqawine Blood Viper",
            Path = "Metadata/Monsters/LeagueBestiary/SnakeBestiary2",
            Crafts =
            [
                "Create a Unique: Sword or Axe",
                "Modify Mods on an Item: Add a Mod to an Elder Item (with Craicic Maw)"
            ],
            DropsTalisman = "Blood Viper Talisman (20% increased Cooldown Recovery Rate)"
        },
        new Beast
        {
            DisplayName = "Fenumal Scrabbler",
            Path = "Metadata/Monsters/LeagueBestiary/SandLeaperBestiary",
            Crafts =
            [
                "Create a Unique: Wand",
                "Modify Mods on an Item: Add a Mod to a Warlord Item (with Craicic Maw)"
            ],
            DropsTalisman = "Scrabbler Talisman (+2 to Level of all Herald Skill Gems)"
        },
        new Beast
        {
            DisplayName = "Farric Gargantuan",
            Path = "Metadata/Monsters/LeagueBestiary/BeastCaveBestiary",
            Crafts =
            [
                "Create Currency Items: A Stack of 3 Orbs of Unmaking (with Farric Frost Hellion Alpha)",
                "Create a Unique: Helmet"
            ],
            DropsTalisman = "Gargantuan Talisman (15% increased maximum Life)"
        }
    };

}
