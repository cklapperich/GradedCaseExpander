using System;
using System.IO;
using System.Text;

// Copy of EMonsterType enum from references/EMonsterType.cs
public enum EMonsterType
{
    EarlyPlayer = -1,
    None = 0,
    PiggyA = 1,
    PiggyB = 2,
    PiggyC = 3,
    PiggyD = 4,
    FoxA = 5,
    FoxB = 6,
    FoxC = 7,
    FoxD = 8,
    GolemA = 9,
    GolemB = 10,
    GolemC = 11,
    GolemD = 12,
    TreeA = 13,
    TreeB = 14,
    TreeC = 15,
    TreeD = 16,
    StarfishA = 17,
    StarfishB = 18,
    StarfishC = 19,
    StarfishD = 20,
    ShellyA = 21,
    ShellyB = 22,
    ShellyC = 23,
    ShellyD = 24,
    BugA = 25,
    BugB = 26,
    BugC = 27,
    BugD = 28,
    BatA = 29,
    BatB = 30,
    BatC = 31,
    BatD = 32,
    Skull = 33,
    Beetle = 34,
    Bear = 35,
    Jellyfish = 36,
    Wisp = 37,
    MummyMan = 38,
    FlowerA = 39,
    FlowerB = 40,
    FlowerC = 41,
    FlowerD = 42,
    WeirdBirdA = 43,
    FireSpirit = 44,
    Angez = 45,
    Mosquito = 46,
    HydraA = 47,
    HydraB = 48,
    HydraC = 49,
    HydraD = 50,
    DragonFire = 51,
    DragonEarth = 52,
    DragonWater = 53,
    DragonThunder = 54,
    Turtle = 55,
    FireWolfA = 56,
    FireWolfB = 57,
    FireWolfC = 58,
    FireWolfD = 59,
    FishA = 60,
    FishB = 61,
    FishC = 62,
    FishD = 63,
    HalloweenA = 64,
    HalloweenB = 65,
    HalloweenC = 66,
    HalloweenD = 67,
    TronA = 68,
    TronB = 69,
    TronC = 70,
    TronD = 71,
    LobsterA = 72,
    LobsterB = 73,
    LobsterC = 74,
    LobsterD = 75,
    FireBirdA = 76,
    FireBirdB = 77,
    FireBirdC = 78,
    SerpentA = 79,
    SerpentB = 80,
    SerpentC = 81,
    CloudA = 82,
    CloudB = 83,
    CloudC = 84,
    EmeraldA = 85,
    EmeraldB = 86,
    EmeraldC = 87,
    Crystalmon = 88,
    ElecDragon = 89,
    CrabA = 90,
    CrabB = 91,
    FireUmbrellaDragon = 92,
    Lanternmon = 93,
    SeedBugA = 94,
    SeedBugB = 95,
    SeedBugC = 96,
    NinjaBirdA = 97,
    NinjaBirdB = 98,
    NinjaBirdC = 99,
    NinjaBirdD = 100,
    NinjaCrowC = 101,
    NinjaCrowD = 102,
    MuffinTreeA = 103,
    MuffinTreeB = 104,
    MuffinTreeC = 105,
    SharkFishA = 106,
    SharkFishB = 107,
    SharkFishC = 108,
    FireGeckoA = 109,
    FireGeckoB = 110,
    EarthDino = 111,
    FireChickenA = 112,
    FireChickenB = 113,
    SeahorseA = 114,
    SeahorseB = 115,
    SeahorseC = 116,
    SeahorseD = 117,
    SlimeA = 118,
    SlimeB = 119,
    SlimeC = 120,
    SlimeD = 121,
    MAX = 122,
    START_MEGABOT = -1000,
    Alpha = 1000,
    Beta = 1001,
    Gamma = 1002,
    Ronin = 1003,
    Bumblebee = 1004,
    Orca = 1005,
    Garuda = 1006,
    Viper = 1007,
    Blitz = 1008,
    Cylops = 1009,
    Kabuto = 1010,
    Minotaur = 1011,
    Spectre = 1012,
    Wolf = 1013,
    Bolt = 1014,
    Hawk = 1015,
    Cyber = 1016,
    Space = 1017,
    Hangar = 1018,
    Arena = 1019,
    UndergroundDark = 1020,
    UndergroundLight = 1021,
    RoninBoss = 1022,
    OrcaBoss = 1023,
    GarudaBoss = 1024,
    ViperBoss = 1025,
    Max = 1026,
    Shockwave = 1027,
    Tremor = 1028,
    Rhino = 1029,
    RoninArt = 1030,
    GarudaArt = 1031,
    MaxArt = 1032,
    MinotaurArt = 1033,
    WolfArt = 1034,
    SkullArt = 1035,
    OrcaAlt = 1036,
    GarudaAlt = 1037,
    ViperAlt = 1038,
    KabutoAlt = 1039,
    Neon = 1040,
    Pulse = 1041,
    Raptor = 1042,
    AncientHammer = 1043,
    ArcMissile = 1044,
    Axe = 1045,
    BarrageMissle = 1046,
    BarrelHammer = 1047,
    BattleChip = 1048,
    BoxingGlove = 1049,
    ChainGun = 1050,
    ChillMissile = 1051,
    ColdShoulder = 1052,
    CrescentClaw = 1053,
    CrescentMachete = 1054,
    CrimsonWheel = 1055,
    CryoBlaster = 1056,
    Drill = 1057,
    DualMissile = 1058,
    ElecBroadsword = 1059,
    ElectricChainsaw = 1060,
    ElectricSpike = 1061,
    EnergyShield = 1062,
    FanBlade = 1063,
    FeatherBlade = 1064,
    FireAxe = 1065,
    FireBroadsword = 1066,
    FireKatana = 1067,
    FireRocket = 1068,
    FireShield = 1069,
    FireTwinAxe = 1070,
    Flamethrower = 1071,
    FreezeBomb = 1072,
    GigaBlade = 1073,
    GigaCannon = 1074,
    GreatCleaver = 1075,
    HeatKnife = 1076,
    HeatMissile = 1077,
    IceBroadsword = 1078,
    IceMortar = 1079,
    IcePike = 1080,
    IceSpinner = 1081,
    IceTwinSpear = 1082,
    InfernoBoost = 1083,
    IronBall = 1084,
    JetBurner = 1085,
    Katana = 1086,
    KnightShield = 1087,
    KnightSword = 1088,
    LightSaber = 1089,
    LionShield = 1090,
    MagneticCoil = 1091,
    MaulerMace = 1092,
    MegavoltBeam = 1093,
    MiniMissile = 1094,
    MissileCannon = 1095,
    MoonlightBlade = 1096,
    MorningStar = 1097,
    Mortar = 1098,
    PulseCannon = 1099,
    RailCannon = 1100,
    Ravager = 1101,
    RocketMissile = 1102,
    RollerSpike = 1103,
    ShurikenBlade = 1104,
    SonicBlaster = 1105,
    SpikeBall = 1106,
    SpikedWarhammer = 1107,
    Sword = 1108,
    ThorHammer = 1109,
    Thunderblade = 1110,
    ThunderTwinBlade = 1111,
    WingBooster = 1112,
    MAX_MEGABOT = 1113,
    START_FANTASYRPG = -2000,
    Archer = 2000,
    ArmoredSlime = 2001,
    AxeWarrior = 2002,
    Basilisk = 2003,
    Blacksmith = 2004,
    CatMage = 2005,
    CatPirate1 = 2006,
    CatThief = 2007,
    Cook = 2008,
    DarkKnight = 2009,
    Dragon = 2010,
    DragonKnight = 2011,
    EldritchCreature = 2012,
    Farmer = 2013,
    Fencer = 2014,
    Florist = 2015,
    GoblinMage = 2016,
    GoblinThief = 2017,
    GoblinWarrior = 2018,
    Golem = 2019,
    Grandma = 2020,
    Harpy1 = 2021,
    Harpy2 = 2022,
    InkKnight = 2023,
    Jester1 = 2024,
    Jester2 = 2025,
    King = 2026,
    Knight = 2027,
    Lamia = 2028,
    Mage = 2029,
    Mimic1 = 2030,
    Mimic2 = 2031,
    MushroomMonster = 2032,
    Noble1 = 2033,
    Noble2 = 2034,
    Peasant1 = 2035,
    Peasant2 = 2036,
    Phoenix = 2037,
    Queen = 2038,
    Reaper = 2039,
    Schoolboy1 = 2040,
    SchoolBoy2 = 2041,
    Slime = 2042,
    Snake = 2043,
    Traveller = 2044,
    TreeMonster = 2045,
    Vampire = 2046,
    Witch = 2047,
    Wizard = 2048,
    WolfFantasy = 2049,
    MAX_FANTASYRPG = 2050,
    START_CATJOB = -3000,
    EX0Teacher = 3000,
    EX0Detective = 3001,
    EX0Woodworker = 3002,
    EX0Plumber = 3003,
    EX0Electrician = 3004,
    EX0Soldier = 3005,
    EX0General = 3006,
    EX0Police = 3007,
    EX0Fireman = 3008,
    EX0Farmer = 3009,
    EX0Architect = 3010,
    EX0Construction = 3011,
    EX0Bodybuilder = 3012,
    EX0Archer = 3013,
    EX0Explorer = 3014,
    EX0Hiker = 3015,
    EX0Programmer = 3016,
    EX0Librarian = 3017,
    EX0Laundry = 3018,
    EX0Racer = 3019,
    EX0Florist = 3020,
    EX0Geologist = 3021,
    EX0Gamer = 3022,
    EX0Maid = 3023,
    EX0Barber = 3024,
    EX0Bartender = 3025,
    EX0Bouncer = 3026,
    EX0Composer = 3027,
    EX0Director = 3028,
    EX0Investor = 3029,
    EX0Singer = 3030,
    EX0Musician = 3031,
    EX0Artist = 3032,
    EX0Photographer = 3033,
    EX0Lawyer = 3034,
    EX0Janitor = 3035,
    EX0Psychic = 3036,
    EX0Astronaut = 3037,
    EX0Scout = 3038,
    EX0Pirate = 3039,
    MAX_CATJOB = 3040
}

class Program
{
    static void Main(string[] args)
    {
        string outputPath = "../monster_data.csv";
        GenerateCSV(outputPath);
    }

    static void GenerateCSV(string outputPath)
    {
        var csv = new StringBuilder();
        csv.AppendLine("MonsterType,EnumValue,Expansion,ImageFileName,CommonName,Rarity");

        foreach (EMonsterType monsterType in Enum.GetValues(typeof(EMonsterType)))
        {
            // Generate base entry
            var line = GenerateCSVLine(monsterType);
            csv.AppendLine(line);

            // If this is a Tetramon monster, also generate a Destiny version
            int enumValue = (int)monsterType;
            if (enumValue >= 1 && enumValue <= 121)
            {
                var destinyLine = GenerateCSVLineForExpansion(monsterType, "Destiny");
                csv.AppendLine(destinyLine);
            }

            // If this monster has a known Ghost version, add Ghost entry
            if (HasGhostVersion(monsterType))
            {
                var ghostLine = GenerateCSVLineForExpansion(monsterType, "Ghost");
                csv.AppendLine(ghostLine);
            }
        }

        File.WriteAllText(outputPath, csv.ToString());
        Console.WriteLine($"Generated CSV file: {outputPath}");
    }

    static string GenerateCSVLine(EMonsterType monsterType)
    {
        int enumValue = (int)monsterType;
        string expansion = GetExpansionType(enumValue);
        string imageFileName = monsterType.ToString();

        // Get Tetramon card info if available
        var cardInfo = GetTetramonCardInfo(monsterType);
        string commonName = cardInfo.commonName ?? "";
        string rarity = cardInfo.rarity ?? "Unknown";

        return $"{monsterType},{enumValue},{expansion},{imageFileName},{commonName},{rarity}";
    }

    static string GenerateCSVLineForExpansion(EMonsterType monsterType, string expansionType)
    {
        int enumValue = (int)monsterType;
        string imageFileName = monsterType.ToString();

        // Get Tetramon card info if available (Destiny uses same names/rarities as Tetramon)
        var cardInfo = GetTetramonCardInfo(monsterType);
        string commonName = cardInfo.commonName ?? "";
        string rarity = cardInfo.rarity ?? "Unknown";

        return $"{monsterType},{enumValue},{expansionType},{imageFileName},{commonName},{rarity}";
    }

    static string GetExpansionType(int enumValue)
    {
        if (enumValue == -1) return "None";
        if (enumValue == 0) return "None";
        if (enumValue >= 1 && enumValue <= 121) return "Tetramon";
        if (enumValue >= 1000 && enumValue <= 1112) return "Megabot";
        if (enumValue >= 2000 && enumValue <= 2049) return "FantasyRPG";
        if (enumValue >= 3000 && enumValue <= 3039) return "CatJob";

        return "Unknown";
    }

    static bool HasGhostVersion(EMonsterType monsterType)
    {
        // Hardcoded list of monsters that have confirmed working Ghost versions
        // Based on empirically tested file paths from actual game installation
        switch (monsterType)
        {
            case EMonsterType.BatD:
            case EMonsterType.BugD:
            case EMonsterType.DragonEarth:
            case EMonsterType.DragonFire:
            case EMonsterType.DragonThunder:
            case EMonsterType.DragonWater:
            case EMonsterType.FireWolfD:
            case EMonsterType.FishD:
            case EMonsterType.FlowerD:
            case EMonsterType.FoxD:
            case EMonsterType.GolemD:
            case EMonsterType.HalloweenD:
            case EMonsterType.HydraD:
            case EMonsterType.LobsterD:
            case EMonsterType.NinjaCrowD:
            case EMonsterType.PiggyD:
            case EMonsterType.ShellyD:
            case EMonsterType.StarfishD:
            case EMonsterType.TreeD:
            case EMonsterType.TronD:
                return true;
            default:
                return false;
        }
    }

    static (string commonName, string rarity) GetTetramonCardInfo(EMonsterType monsterType)
    {
        // Data from https://tcgcardshopsimulator.wiki.gg/wiki/Tetramon_Base
        // Maps enum values 1-121 to display names and rarities
        return (int)monsterType switch
        {
            1 => ("Pigni", "Basic"),
            2 => ("Burpig", "Rare"),
            3 => ("Inferhog", "Epic"),
            4 => ("Blazoar", "Legendary"),
            5 => ("Kidsune", "Basic"),
            6 => ("Bonfiox", "Rare"),
            7 => ("Honobi", "Epic"),
            8 => ("Kyuenbi", "Legendary"),
            9 => ("Nanomite", "Basic"),
            10 => ("Decimite", "Rare"),
            11 => ("Meganite", "Epic"),
            12 => ("Giganite", "Legendary"),
            13 => ("Sapoling", "Basic"),
            14 => ("Forush", "Rare"),
            15 => ("Timbro", "Epic"),
            16 => ("Mammotree", "Legendary"),
            17 => ("Minstar", "Basic"),
            18 => ("Trickstar", "Rare"),
            19 => ("Princestar", "Epic"),
            20 => ("Kingstar", "Legendary"),
            21 => ("Shellow", "Basic"),
            22 => ("Clamigo", "Rare"),
            23 => ("Aquariff", "Epic"),
            24 => ("Fistronk", "Legendary"),
            25 => ("Wurmgle", "Basic"),
            26 => ("Pupazz", "Rare"),
            27 => ("Mothini", "Epic"),
            28 => ("Royalama", "Legendary"),
            29 => ("Nocti", "Basic"),
            30 => ("Lunight", "Rare"),
            31 => ("Vampicant", "Epic"),
            32 => ("Dracunix", "Legendary"),
            33 => ("Minotos", "Rare"),
            34 => ("Drilceros", "Epic"),
            35 => ("Grizzaw", "Epic"),
            36 => ("Jelicleen", "Rare"),
            37 => ("Wispo", "Rare"),
            38 => ("Mummog", "Rare"),
            39 => ("Helio", "Basic"),
            40 => ("Pixy", "Rare"),
            41 => ("Flory", "Epic"),
            42 => ("Magnoria", "Legendary"),
            43 => ("Werboo", "Basic"),
            44 => ("Flami", "Basic"),
            45 => ("Angez", "Rare"),
            46 => ("Moskit", "Epic"),
            47 => ("Kyrone", "Basic"),
            48 => ("Twofrost", "Rare"),
            49 => ("Threeze", "Epic"),
            50 => ("Hydroid", "Legendary"),
            51 => ("Drakon", "Legendary"),
            52 => ("Bogon", "Legendary"),
            53 => ("Hydron", "Legendary"),
            54 => ("Raizon", "Legendary"),
            55 => ("Tortugor", "Epic"),
            56 => ("Lupup", "Basic"),
            57 => ("Luphire", "Rare"),
            58 => ("Lucinder", "Epic"),
            59 => ("Lucadence", "Legendary"),
            60 => ("Gupi", "Basic"),
            61 => ("Sharfin", "Rare"),
            62 => ("Gilgabass", "Epic"),
            63 => ("Jigajawr", "Legendary"),
            64 => ("Batrang", "Basic"),
            65 => ("Dusko", "Rare"),
            66 => ("Wolgin", "Epic"),
            67 => ("Jacktern", "Legendary"),
            68 => ("Tetron", "Basic"),
            69 => ("Raxx", "Rare"),
            70 => ("Gannon", "Epic"),
            71 => ("GigatronX", "Legendary"),
            72 => ("Clawop", "Basic"),
            73 => ("Clawdos", "Rare"),
            74 => ("Clawaken", "Epic"),
            75 => ("Clawcifear", "Legendary"),
            76 => ("Sunflork", "Basic"),
            77 => ("Scarlios", "Rare"),
            78 => ("Scarkgorus", "Epic"),
            79 => ("Crobib", "Basic"),
            80 => ("Crosilisk", "Rare"),
            81 => ("Crorathian", "Epic"),
            82 => ("Nimblis", "Basic"),
            83 => ("Nimboculo", "Rare"),
            84 => ("Nimbustrike", "Epic"),
            85 => ("Esmeri", "Basic"),
            86 => ("Esmerock", "Rare"),
            87 => ("Esmerdios", "Epic"),
            88 => ("Litspire", "Epic"),
            89 => ("Voltrex", "Legendary"),
            90 => ("Crablox", "Rare"),
            91 => ("Clawvenger", "Epic"),
            92 => ("Flambrolly", "Legendary"),
            93 => ("Lumie", "Rare"),
            94 => ("Seedant", "Basic"),
            95 => ("Budwing", "Rare"),
            96 => ("Buzzeed", "Epic"),
            97 => ("Beakai", "Rare"),
            98 => ("Talontsu", "Epic"),
            99 => ("Talonika", "Epic"),
            100 => ("Talonryu", "Legendary"),
            101 => ("Kataryu", "Epic"),
            102 => ("Katengu", "Legendary"),
            103 => ("Mufflin", "Basic"),
            104 => ("Muffleur", "Rare"),
            105 => ("Mufflimax", "Epic"),
            106 => ("Anguifish", "Basic"),
            107 => ("Amneshark", "Rare"),
            108 => ("Amnesilla", "Epic"),
            109 => ("Frizard", "Rare"),
            110 => ("Gekoflare", "Epic"),
            111 => ("Terradrakon", "Legendary"),
            112 => ("Flamchik", "Basic"),
            113 => ("Pyropeck", "Rare"),
            114 => ("Poseia", "Basic"),
            115 => ("Posteed", "Rare"),
            116 => ("Poseigon", "Epic"),
            117 => ("Poseidrake", "Legendary"),
            118 => ("Sludglop", "Basic"),
            119 => ("Sludgetox", "Rare"),
            120 => ("Toxigoop", "Epic"),
            121 => ("Toximuck", "Legendary"),
            _ => (null, null)
        };
    }
}