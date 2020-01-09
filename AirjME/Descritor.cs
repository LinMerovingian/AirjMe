using System;
using System.Collections.Generic;
using System.Text;

namespace AirjME
{
    class Descritor
    {
        private const int CGObjectDataEnd = (int) CGObjectData.CGObjectDataEnd;
        private const int CGItemDataEnd = (int) CGItemData.CGItemDataEnd;
        private const int CGUnitDataEnd = (int) CGUnitData.CGUnitDataEnd;
        private const int CGPlayerDataEnd = (int) CGPlayerData.CGPlayerDataEnd;

        enum CGObjectData
        {
            Guid = 0, // size 4
            EntryID = 4, // size 1
            DynamicFlags = 5, // size 1
            Scale = 6, // size 1
            CGObjectDataEnd = 7
        }


        enum CGItemData
        {
            Owner = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            ContainedIn = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            Creator = CGObjectDataEnd + 8, // size 4 flags: MIRROR_ALL
            GiftCreator = CGObjectDataEnd + 12, // size 4 flags: MIRROR_ALL
            StackCount = CGObjectDataEnd + 16, // size 1 flags: MIRROR_OWNER
            Expiration = CGObjectDataEnd + 17, // size 1 flags: MIRROR_OWNER
            SpellCharges = CGObjectDataEnd + 18, // size 5 flags: MIRROR_OWNER
            DynamicFlags = CGObjectDataEnd + 23, // size 1 flags: MIRROR_ALL
            Enchantment = CGObjectDataEnd + 24, // size 39 flags: MIRROR_ALL
            PropertySeed = CGObjectDataEnd + 63, // size 1 flags: MIRROR_ALL
            RandomPropertiesID = CGObjectDataEnd + 64, // size 1 flags: MIRROR_ALL
            Durability = CGObjectDataEnd + 65, // size 1 flags: MIRROR_OWNER
            MaxDurability = CGObjectDataEnd + 66, // size 1 flags: MIRROR_OWNER
            CreatePlayedTime = CGObjectDataEnd + 67, // size 1 flags: MIRROR_ALL
            ModifiersMask = CGObjectDataEnd + 68, // size 1 flags: MIRROR_OWNER
            Context = CGObjectDataEnd + 69, // size 1 flags: MIRROR_ALL
            ArtifactXP = CGObjectDataEnd + 70, // size 2 flags: MIRROR_OWNER
            ItemAppearanceModID = CGObjectDataEnd + 72, // size 1 flags: MIRROR_OWNER
            CGItemDataEnd = CGObjectDataEnd + 73
        }

        enum CGContainerData
        {
            Slots = CGItemDataEnd + 0, // size 144 flags: MIRROR_ALL
            NumSlots = CGItemDataEnd + 144, // size 1 flags: MIRROR_ALL
            CGContainerDataEnd = CGItemDataEnd + 145
        }


        enum CGAzeriteEmpoweredItemData
        {
            Selections = 0, // size 4
            CGAzeriteEmpoweredItemDataEnd = 4
        }

        enum CGAzeriteItemData
        {
            Xp = CGItemDataEnd + 0, // size 2 flags: MIRROR_ALL
            Level = CGItemDataEnd + 2, // size 1 flags: MIRROR_ALL
            AuraLevel = CGItemDataEnd + 3, // size 1 flags: MIRROR_ALL
            KnowledgeLevel = CGItemDataEnd + 4, // size 1 flags: MIRROR_OWNER
            DEBUGknowledgeWeek = CGItemDataEnd + 5, // size 1 flags: MIRROR_OWNER
            CGAzeriteItemDataEnd = CGItemDataEnd + 6
        }

        enum CGUnitData
        {
            Charm = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            Summon = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            Critter = CGObjectDataEnd + 8, // size 4 flags: MIRROR_SELF
            CharmedBy = CGObjectDataEnd + 12, // size 4 flags: MIRROR_ALL
            SummonedBy = CGObjectDataEnd + 16, // size 4 flags: MIRROR_ALL
            CreatedBy = CGObjectDataEnd + 20, // size 4 flags: MIRROR_ALL
            DemonCreator = CGObjectDataEnd + 24, // size 4 flags: MIRROR_ALL
            LookAtControllerTarget = CGObjectDataEnd + 28, // size 4 flags: MIRROR_ALL
            Target = CGObjectDataEnd + 32, // size 4 flags: MIRROR_ALL
            BattlePetCompanionGUID = CGObjectDataEnd + 36, // size 4 flags: MIRROR_ALL
            BattlePetDBID = CGObjectDataEnd + 40, // size 2 flags: MIRROR_ALL
            ChannelData = CGObjectDataEnd + 42, // size 2 flags: 
            SummonedByHomeRealm = CGObjectDataEnd + 44, // size 1 flags: MIRROR_ALL
            Sex = CGObjectDataEnd + 45, // size 1 flags: MIRROR_ALL
            DisplayPower = CGObjectDataEnd + 46, // size 1 flags: MIRROR_ALL
            OverrideDisplayPowerID = CGObjectDataEnd + 47, // size 1 flags: MIRROR_ALL
            Health = CGObjectDataEnd + 48, // size 2 flags: MIRROR_ALL
            Power = CGObjectDataEnd + 50, // size 6 flags: 
            MaxHealth = CGObjectDataEnd + 56, // size 2 flags: MIRROR_ALL
            MaxPower = CGObjectDataEnd + 58, // size 6 flags: MIRROR_ALL
            PowerRegenFlatModifier = CGObjectDataEnd + 64, // size 6 flags: 
            PowerRegenInterruptedFlatModifier = CGObjectDataEnd + 70, // size 6 flags: 
            Level = CGObjectDataEnd + 76, // size 1 flags: MIRROR_ALL
            EffectiveLevel = CGObjectDataEnd + 77, // size 1 flags: MIRROR_ALL
            ContentTuningID = CGObjectDataEnd + 78, // size 1 flags: MIRROR_ALL
            ScalingLevelMin = CGObjectDataEnd + 79, // size 1 flags: MIRROR_ALL
            ScalingLevelMax = CGObjectDataEnd + 80, // size 1 flags: MIRROR_ALL
            ScalingLevelDelta = CGObjectDataEnd + 81, // size 1 flags: MIRROR_ALL
            ScalingFactionGroup = CGObjectDataEnd + 82, // size 1 flags: MIRROR_ALL
            ScalingHealthItemLevelCurveID = CGObjectDataEnd + 83, // size 1 flags: MIRROR_ALL
            ScalingDamageItemLevelCurveID = CGObjectDataEnd + 84, // size 1 flags: MIRROR_ALL
            FactionTemplate = CGObjectDataEnd + 85, // size 1 flags: MIRROR_ALL
            VirtualItems = CGObjectDataEnd + 86, // size 6 flags: MIRROR_ALL
            Flags = CGObjectDataEnd + 92, // size 1 flags: 
            Flags2 = CGObjectDataEnd + 93, // size 1 flags: 
            Flags3 = CGObjectDataEnd + 94, // size 1 flags: 
            AuraState = CGObjectDataEnd + 95, // size 1 flags: MIRROR_ALL
            AttackRoundBaseTime = CGObjectDataEnd + 96, // size 2 flags: MIRROR_ALL
            RangedAttackRoundBaseTime = CGObjectDataEnd + 98, // size 1 flags: MIRROR_SELF
            BoundingRadius = CGObjectDataEnd + 99, // size 1 flags: MIRROR_ALL
            CombatReach = CGObjectDataEnd + 100, // size 1 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 101, // size 1 flags: 
            DisplayScale = CGObjectDataEnd + 102, // size 1 flags: 
            NativeDisplayID = CGObjectDataEnd + 103, // size 1 flags: 
            NativeXDisplayScale = CGObjectDataEnd + 104, // size 1 flags: 
            MountDisplayID = CGObjectDataEnd + 105, // size 1 flags: 
            MinDamage = CGObjectDataEnd + 106, // size 1 flags: 
            MaxDamage = CGObjectDataEnd + 107, // size 1 flags: 
            MinOffHandDamage = CGObjectDataEnd + 108, // size 1 flags: 
            MaxOffHandDamage = CGObjectDataEnd + 109, // size 1 flags: 
            AnimTier = CGObjectDataEnd + 110, // size 1 flags: MIRROR_ALL
            PetNumber = CGObjectDataEnd + 111, // size 1 flags: MIRROR_ALL
            PetNameTimestamp = CGObjectDataEnd + 112, // size 1 flags: MIRROR_ALL
            PetExperience = CGObjectDataEnd + 113, // size 1 flags: MIRROR_OWNER
            PetNextLevelExperience = CGObjectDataEnd + 114, // size 1 flags: MIRROR_OWNER
            ModCastingSpeed = CGObjectDataEnd + 115, // size 1 flags: MIRROR_ALL
            ModSpellHaste = CGObjectDataEnd + 116, // size 1 flags: MIRROR_ALL
            ModHaste = CGObjectDataEnd + 117, // size 1 flags: MIRROR_ALL
            ModRangedHaste = CGObjectDataEnd + 118, // size 1 flags: MIRROR_ALL
            ModHasteRegen = CGObjectDataEnd + 119, // size 1 flags: MIRROR_ALL
            ModTimeRate = CGObjectDataEnd + 120, // size 1 flags: MIRROR_ALL
            CreatedBySpell = CGObjectDataEnd + 121, // size 1 flags: MIRROR_ALL
            NpcFlags = CGObjectDataEnd + 122, // size 2 flags: 
            EmoteState = CGObjectDataEnd + 124, // size 1 flags: MIRROR_ALL
            Stats = CGObjectDataEnd + 125, // size 4 flags: 
            StatPosBuff = CGObjectDataEnd + 129, // size 4 flags: 
            StatNegBuff = CGObjectDataEnd + 133, // size 4 flags: 
            Resistances = CGObjectDataEnd + 137, // size 7 flags: 
            BonusResistanceMods = CGObjectDataEnd + 144, // size 7 flags: 
            BaseMana = CGObjectDataEnd + 151, // size 1 flags: MIRROR_ALL
            BaseHealth = CGObjectDataEnd + 152, // size 1 flags: 
            ShapeshiftForm = CGObjectDataEnd + 153, // size 1 flags: MIRROR_ALL
            AttackPower = CGObjectDataEnd + 154, // size 1 flags: 
            AttackPowerModPos = CGObjectDataEnd + 155, // size 1 flags: 
            AttackPowerModNeg = CGObjectDataEnd + 156, // size 1 flags: 
            AttackPowerMultiplier = CGObjectDataEnd + 157, // size 1 flags: 
            RangedAttackPower = CGObjectDataEnd + 158, // size 1 flags: 
            RangedAttackPowerModPos = CGObjectDataEnd + 159, // size 1 flags: 
            RangedAttackPowerModNeg = CGObjectDataEnd + 160, // size 1 flags: 
            RangedAttackPowerMultiplier = CGObjectDataEnd + 161, // size 1 flags: 
            MainHandWeaponAttackPower = CGObjectDataEnd + 162, // size 1 flags: 
            OffHandWeaponAttackPower = CGObjectDataEnd + 163, // size 1 flags: 
            RangedWeaponAttackPower = CGObjectDataEnd + 164, // size 1 flags: 
            SetAttackSpeedAura = CGObjectDataEnd + 165, // size 1 flags: 
            Lifesteal = CGObjectDataEnd + 166, // size 1 flags: 
            MinRangedDamage = CGObjectDataEnd + 167, // size 1 flags: 
            MaxRangedDamage = CGObjectDataEnd + 168, // size 1 flags: 
            PowerCostModifier = CGObjectDataEnd + 169, // size 7 flags: 
            PowerCostMultiplier = CGObjectDataEnd + 176, // size 7 flags: 
            MaxHealthModifier = CGObjectDataEnd + 183, // size 1 flags: 
            HoverHeight = CGObjectDataEnd + 184, // size 1 flags: MIRROR_ALL
            MinItemLevelCutoff = CGObjectDataEnd + 185, // size 1 flags: MIRROR_ALL
            MinItemLevel = CGObjectDataEnd + 186, // size 1 flags: MIRROR_ALL
            MaxItemLevel = CGObjectDataEnd + 187, // size 1 flags: MIRROR_ALL
            WildBattlePetLevel = CGObjectDataEnd + 188, // size 1 flags: MIRROR_ALL
            BattlePetCompanionNameTimestamp = CGObjectDataEnd + 189, // size 1 flags: MIRROR_ALL
            InteractSpellID = CGObjectDataEnd + 190, // size 1 flags: MIRROR_ALL
            StateSpellVisualID = CGObjectDataEnd + 191, // size 1 flags: 
            StateAnimID = CGObjectDataEnd + 192, // size 1 flags: 
            StateAnimKitID = CGObjectDataEnd + 193, // size 1 flags: 
            StateWorldEffectID = CGObjectDataEnd + 194, // size 4 flags: 
            ScaleDuration = CGObjectDataEnd + 198, // size 1 flags: MIRROR_ALL
            LooksLikeMountID = CGObjectDataEnd + 199, // size 1 flags: MIRROR_ALL
            LooksLikeCreatureID = CGObjectDataEnd + 200, // size 1 flags: MIRROR_ALL
            LookAtControllerID = CGObjectDataEnd + 201, // size 1 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 202, // size 4 flags: MIRROR_ALL
            CGUnitDataEnd = CGObjectDataEnd + 206
        }

        enum CGPlayerData
        {
            DuelArbiter = CGUnitDataEnd + 0, // size 4 flags: MIRROR_ALL
            WowAccount = CGUnitDataEnd + 4, // size 4 flags: MIRROR_ALL
            LootTargetGUID = CGUnitDataEnd + 8, // size 4 flags: MIRROR_ALL
            PlayerFlags = CGUnitDataEnd + 12, // size 1 flags: MIRROR_ALL
            PlayerFlagsEx = CGUnitDataEnd + 13, // size 1 flags: MIRROR_ALL
            GuildRankID = CGUnitDataEnd + 14, // size 1 flags: MIRROR_ALL
            GuildDeleteDate = CGUnitDataEnd + 15, // size 1 flags: MIRROR_ALL
            GuildLevel = CGUnitDataEnd + 16, // size 1 flags: MIRROR_ALL
            HairColorID = CGUnitDataEnd + 17, // size 1 flags: MIRROR_ALL
            CustomDisplayOption = CGUnitDataEnd + 18, // size 1 flags: MIRROR_ALL
            Inebriation = CGUnitDataEnd + 19, // size 1 flags: MIRROR_ALL
            ArenaFaction = CGUnitDataEnd + 20, // size 1 flags: MIRROR_ALL
            DuelTeam = CGUnitDataEnd + 21, // size 1 flags: MIRROR_ALL
            GuildTimeStamp = CGUnitDataEnd + 22, // size 1 flags: MIRROR_ALL
            QuestLog = CGUnitDataEnd + 23, // size 1600 flags: MIRROR_PARTY
            VisibleItems = CGUnitDataEnd + 1623, // size 38 flags: MIRROR_ALL
            PlayerTitle = CGUnitDataEnd + 1661, // size 1 flags: MIRROR_ALL
            FakeInebriation = CGUnitDataEnd + 1662, // size 1 flags: MIRROR_ALL
            VirtualPlayerRealm = CGUnitDataEnd + 1663, // size 1 flags: MIRROR_ALL
            CurrentSpecID = CGUnitDataEnd + 1664, // size 1 flags: MIRROR_ALL
            TaxiMountAnimKitID = CGUnitDataEnd + 1665, // size 1 flags: MIRROR_ALL
            AvgItemLevel = CGUnitDataEnd + 1666, // size 4 flags: MIRROR_ALL
            CurrentBattlePetBreedQuality = CGUnitDataEnd + 1670, // size 1 flags: MIRROR_ALL
            HonorLevel = CGUnitDataEnd + 1671, // size 1 flags: MIRROR_ALL
            CGPlayerDataEnd = CGUnitDataEnd + 1672
        }

        enum CGActivePlayerData
        {
            InvSlots = CGPlayerDataEnd + 0, // size 780 flags: MIRROR_ALL
            FarsightObject = CGPlayerDataEnd + 780, // size 4 flags: MIRROR_ALL
            SummonedBattlePetGUID = CGPlayerDataEnd + 784, // size 4 flags: MIRROR_ALL
            KnownTitles = CGPlayerDataEnd + 788, // size 12 flags: MIRROR_ALL
            Coinage = CGPlayerDataEnd + 800, // size 2 flags: MIRROR_ALL
            XP = CGPlayerDataEnd + 802, // size 1 flags: MIRROR_ALL
            NextLevelXP = CGPlayerDataEnd + 803, // size 1 flags: MIRROR_ALL
            TrialXP = CGPlayerDataEnd + 804, // size 1 flags: MIRROR_ALL
            Skill = CGPlayerDataEnd + 805, // size 896 flags: MIRROR_ALL
            CharacterPoints = CGPlayerDataEnd + 1701, // size 1 flags: MIRROR_ALL
            MaxTalentTiers = CGPlayerDataEnd + 1702, // size 1 flags: MIRROR_ALL
            TrackCreatureMask = CGPlayerDataEnd + 1703, // size 1 flags: MIRROR_ALL
            TrackResourceMask = CGPlayerDataEnd + 1704, // size 2 flags: MIRROR_ALL
            MainhandExpertise = CGPlayerDataEnd + 1706, // size 1 flags: MIRROR_ALL
            OffhandExpertise = CGPlayerDataEnd + 1707, // size 1 flags: MIRROR_ALL
            RangedExpertise = CGPlayerDataEnd + 1708, // size 1 flags: MIRROR_ALL
            CombatRatingExpertise = CGPlayerDataEnd + 1709, // size 1 flags: MIRROR_ALL
            BlockPercentage = CGPlayerDataEnd + 1710, // size 1 flags: MIRROR_ALL
            DodgePercentage = CGPlayerDataEnd + 1711, // size 1 flags: MIRROR_ALL
            DodgePercentageFromAttribute = CGPlayerDataEnd + 1712, // size 1 flags: MIRROR_ALL
            ParryPercentage = CGPlayerDataEnd + 1713, // size 1 flags: MIRROR_ALL
            ParryPercentageFromAttribute = CGPlayerDataEnd + 1714, // size 1 flags: MIRROR_ALL
            CritPercentage = CGPlayerDataEnd + 1715, // size 1 flags: MIRROR_ALL
            RangedCritPercentage = CGPlayerDataEnd + 1716, // size 1 flags: MIRROR_ALL
            OffhandCritPercentage = CGPlayerDataEnd + 1717, // size 1 flags: MIRROR_ALL
            SpellCritPercentage = CGPlayerDataEnd + 1718, // size 1 flags: MIRROR_ALL
            ShieldBlock = CGPlayerDataEnd + 1719, // size 1 flags: MIRROR_ALL
            ShieldBlockCritPercentage = CGPlayerDataEnd + 1720, // size 1 flags: MIRROR_ALL
            Mastery = CGPlayerDataEnd + 1721, // size 1 flags: MIRROR_ALL
            Speed = CGPlayerDataEnd + 1722, // size 1 flags: MIRROR_ALL
            Avoidance = CGPlayerDataEnd + 1723, // size 1 flags: MIRROR_ALL
            Sturdiness = CGPlayerDataEnd + 1724, // size 1 flags: MIRROR_ALL
            Versatility = CGPlayerDataEnd + 1725, // size 1 flags: MIRROR_ALL
            VersatilityBonus = CGPlayerDataEnd + 1726, // size 1 flags: MIRROR_ALL
            PvpPowerDamage = CGPlayerDataEnd + 1727, // size 1 flags: MIRROR_ALL
            PvpPowerHealing = CGPlayerDataEnd + 1728, // size 1 flags: MIRROR_ALL
            ExploredZones = CGPlayerDataEnd + 1729, // size 320 flags: MIRROR_ALL
            RestInfo = CGPlayerDataEnd + 2049, // size 4 flags: MIRROR_ALL
            ModDamageDonePos = CGPlayerDataEnd + 2053, // size 7 flags: MIRROR_ALL
            ModDamageDoneNeg = CGPlayerDataEnd + 2060, // size 7 flags: MIRROR_ALL
            ModDamageDonePercent = CGPlayerDataEnd + 2067, // size 7 flags: MIRROR_ALL
            ModHealingDonePos = CGPlayerDataEnd + 2074, // size 1 flags: MIRROR_ALL
            ModHealingPercent = CGPlayerDataEnd + 2075, // size 1 flags: MIRROR_ALL
            ModHealingDonePercent = CGPlayerDataEnd + 2076, // size 1 flags: MIRROR_ALL
            ModPeriodicHealingDonePercent = CGPlayerDataEnd + 2077, // size 1 flags: MIRROR_ALL
            WeaponDmgMultipliers = CGPlayerDataEnd + 2078, // size 3 flags: MIRROR_ALL
            WeaponAtkSpeedMultipliers = CGPlayerDataEnd + 2081, // size 3 flags: MIRROR_ALL
            ModSpellPowerPercent = CGPlayerDataEnd + 2084, // size 1 flags: MIRROR_ALL
            ModResiliencePercent = CGPlayerDataEnd + 2085, // size 1 flags: MIRROR_ALL
            OverrideSpellPowerByAPPercent = CGPlayerDataEnd + 2086, // size 1 flags: MIRROR_ALL
            OverrideAPBySpellPowerPercent = CGPlayerDataEnd + 2087, // size 1 flags: MIRROR_ALL
            ModTargetResistance = CGPlayerDataEnd + 2088, // size 1 flags: MIRROR_ALL
            ModTargetPhysicalResistance = CGPlayerDataEnd + 2089, // size 1 flags: MIRROR_ALL
            LocalFlags = CGPlayerDataEnd + 2090, // size 1 flags: MIRROR_ALL
            PvpMedals = CGPlayerDataEnd + 2091, // size 1 flags: MIRROR_ALL
            BuybackPrice = CGPlayerDataEnd + 2092, // size 12 flags: MIRROR_ALL
            BuybackTimestamp = CGPlayerDataEnd + 2104, // size 12 flags: MIRROR_ALL
            YesterdayHonorableKills = CGPlayerDataEnd + 2116, // size 1 flags: MIRROR_ALL
            LifetimeHonorableKills = CGPlayerDataEnd + 2117, // size 1 flags: MIRROR_ALL
            WatchedFactionIndex = CGPlayerDataEnd + 2118, // size 1 flags: MIRROR_ALL
            CombatRatings = CGPlayerDataEnd + 2119, // size 32 flags: MIRROR_ALL
            PvpInfo = CGPlayerDataEnd + 2151, // size 54 flags: MIRROR_ALL
            MaxLevel = CGPlayerDataEnd + 2205, // size 1 flags: MIRROR_ALL
            ScalingPlayerLevelDelta = CGPlayerDataEnd + 2206, // size 1 flags: MIRROR_ALL
            MaxCreatureScalingLevel = CGPlayerDataEnd + 2207, // size 1 flags: MIRROR_ALL
            NoReagentCostMask = CGPlayerDataEnd + 2208, // size 4 flags: MIRROR_ALL
            PetSpellPower = CGPlayerDataEnd + 2212, // size 1 flags: MIRROR_ALL
            ProfessionSkillLine = CGPlayerDataEnd + 2213, // size 2 flags: MIRROR_ALL
            UiHitModifier = CGPlayerDataEnd + 2215, // size 1 flags: MIRROR_ALL
            UiSpellHitModifier = CGPlayerDataEnd + 2216, // size 1 flags: MIRROR_ALL
            HomeRealmTimeOffset = CGPlayerDataEnd + 2217, // size 1 flags: MIRROR_ALL
            ModPetHaste = CGPlayerDataEnd + 2218, // size 1 flags: MIRROR_ALL
            NumBackpackSlots = CGPlayerDataEnd + 2219, // size 1 flags: MIRROR_ALL
            OverrideSpellsID = CGPlayerDataEnd + 2220, // size 1 flags: 
            LfgBonusFactionID = CGPlayerDataEnd + 2221, // size 1 flags: MIRROR_ALL
            LootSpecID = CGPlayerDataEnd + 2222, // size 1 flags: MIRROR_ALL
            OverrideZonePVPType = CGPlayerDataEnd + 2223, // size 1 flags: 
            BagSlotFlags = CGPlayerDataEnd + 2224, // size 4 flags: MIRROR_ALL
            BankBagSlotFlags = CGPlayerDataEnd + 2228, // size 7 flags: MIRROR_ALL
            InsertItemsLeftToRight = CGPlayerDataEnd + 2235, // size 1 flags: MIRROR_ALL
            QuestCompleted = CGPlayerDataEnd + 2236, // size 1750 flags: MIRROR_ALL
            Honor = CGPlayerDataEnd + 3986, // size 1 flags: MIRROR_ALL
            HonorNextLevel = CGPlayerDataEnd + 3987, // size 1 flags: MIRROR_ALL
            PvpTierMaxFromWins = CGPlayerDataEnd + 3988, // size 1 flags: MIRROR_ALL
            PvpLastWeeksTierMaxFromWins = CGPlayerDataEnd + 3989, // size 1 flags: MIRROR_ALL
            CGActivePlayerDataEnd = CGPlayerDataEnd + 3990
        }

        enum CGGameObjectData
        {
            CreatedBy = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 8, // size 1 flags: 
            Flags = CGObjectDataEnd + 9, // size 1 flags: 
            ParentRotation = CGObjectDataEnd + 10, // size 4 flags: MIRROR_ALL
            FactionTemplate = CGObjectDataEnd + 14, // size 1 flags: MIRROR_ALL
            Level = CGObjectDataEnd + 15, // size 1 flags: MIRROR_ALL
            PercentHealth = CGObjectDataEnd + 16, // size 1 flags: 
            SpellVisualID = CGObjectDataEnd + 17, // size 1 flags: 
            StateSpellVisualID = CGObjectDataEnd + 18, // size 1 flags: 
            SpawnTrackingStateAnimID = CGObjectDataEnd + 19, // size 1 flags: 
            SpawnTrackingStateAnimKitID = CGObjectDataEnd + 20, // size 1 flags: 
            StateWorldEffectID = CGObjectDataEnd + 21, // size 4 flags: 
            CustomParam = CGObjectDataEnd + 25, // size 1 flags: 
            CGGameObjectDataEnd = CGObjectDataEnd + 26
        }

        enum CGDynamicObjectData
        {
            Caster = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            Type = CGObjectDataEnd + 4, // size 1 flags: MIRROR_ALL
            SpellXSpellVisualID = CGObjectDataEnd + 5, // size 1 flags: MIRROR_ALL
            SpellID = CGObjectDataEnd + 6, // size 1 flags: MIRROR_ALL
            Radius = CGObjectDataEnd + 7, // size 1 flags: MIRROR_ALL
            CastTime = CGObjectDataEnd + 8, // size 1 flags: MIRROR_ALL
            CGDynamicObjectDataEnd = CGObjectDataEnd + 9
        }

        enum CGCorpseData
        {
            Owner = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            PartyGUID = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 8, // size 4 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 12, // size 1 flags: MIRROR_ALL
            Items = CGObjectDataEnd + 13, // size 19 flags: MIRROR_ALL
            SkinID = CGObjectDataEnd + 32, // size 1 flags: MIRROR_ALL
            FacialHairStyleID = CGObjectDataEnd + 33, // size 1 flags: MIRROR_ALL
            Flags = CGObjectDataEnd + 34, // size 1 flags: MIRROR_ALL
            DynamicFlags = CGObjectDataEnd + 35, // size 1 flags: MIRROR_VIEWER_DEPENDENT
            FactionTemplate = CGObjectDataEnd + 36, // size 1 flags: MIRROR_ALL
            CustomDisplayOption = CGObjectDataEnd + 37, // size 1 flags: MIRROR_ALL
            CGCorpseDataEnd = CGObjectDataEnd + 38
        }

        enum CGAreaTriggerData
        {
            OverrideScaleCurve = CGObjectDataEnd + 0, // size 7 flags: 
            ExtraScaleCurve = CGObjectDataEnd + 7, // size 7 flags: 
            Caster = CGObjectDataEnd + 14, // size 4 flags: MIRROR_ALL
            Duration = CGObjectDataEnd + 18, // size 1 flags: MIRROR_ALL
            TimeToTarget = CGObjectDataEnd + 19, // size 1 flags: 
            TimeToTargetScale = CGObjectDataEnd + 20, // size 1 flags: 
            TimeToTargetExtraScale = CGObjectDataEnd + 21, // size 1 flags: 
            SpellID = CGObjectDataEnd + 22, // size 1 flags: MIRROR_ALL
            SpellForVisuals = CGObjectDataEnd + 23, // size 1 flags: MIRROR_ALL
            SpellXSpellVisualID = CGObjectDataEnd + 24, // size 1 flags: MIRROR_ALL
            BoundsRadius2D = CGObjectDataEnd + 25, // size 1 flags: 
            DecalPropertiesID = CGObjectDataEnd + 26, // size 1 flags: MIRROR_ALL
            CreatingEffectGUID = CGObjectDataEnd + 27, // size 4 flags: MIRROR_ALL
            CGAreaTriggerDataEnd = CGObjectDataEnd + 31
        }

        enum CGSceneObjectData
        {
            ScriptPackageID = CGObjectDataEnd + 0, // size 1 flags: MIRROR_ALL
            RndSeedVal = CGObjectDataEnd + 1, // size 1 flags: MIRROR_ALL
            CreatedBy = CGObjectDataEnd + 2, // size 4 flags: MIRROR_ALL
            SceneType = CGObjectDataEnd + 6, // size 1 flags: MIRROR_ALL
            CGSceneObjectDataEnd = CGObjectDataEnd + 7
        }

        enum CGConversationData
        {
            LastLineEndTime = CGObjectDataEnd + 0, // size 1 flags: MIRROR_VIEWER_DEPENDENT
            CGConversationDataEnd = CGObjectDataEnd + 1
        }

        enum CGItemDynamicData
        {
            Modifiers = CGObjectDataEnd + 0, // size 4 flags: MIRROR_NONE
            BonusListIDs = CGObjectDataEnd + 1, // size 260 flags: MIRROR_NONE
            ArtifactPowers = CGObjectDataEnd + 2, // size 4 flags: MIRROR_NONE
            Gems = CGObjectDataEnd + 3, // size 4 flags: MIRROR_NONE
            CGItemDynamicDataEnd = CGObjectDataEnd + 4
        }

        enum CGUnitDynamicData
        {
            PassiveSpells = CGObjectDataEnd + 0, // size 513 flags: MIRROR_NONE
            WorldEffects = CGObjectDataEnd + 1, // size 513 flags: MIRROR_NONE
            ChannelObjects = CGObjectDataEnd + 2, // size 513 flags: MIRROR_NONE
            CGUnitDynamicDataEnd = CGObjectDataEnd + 3
        }

        enum CGPlayerDynamicData
        {
            ArenaCooldowns = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            CGPlayerDynamicDataEnd = CGObjectDataEnd + 1
        }

        enum CGActivePlayerDynamicData
        {
            ResearchSites = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            ResearchSiteProgress = CGObjectDataEnd + 1, // size 1 flags: MIRROR_NONE
            DailyQuestsCompleted = CGObjectDataEnd + 2, // size 1 flags: MIRROR_NONE
            AvailableQuestLineXQuestIDs = CGObjectDataEnd + 3, // size 1 flags: MIRROR_NONE
            Heirlooms = CGObjectDataEnd + 4, // size 1 flags: MIRROR_NONE
            HeirloomFlags = CGObjectDataEnd + 5, // size 1 flags: MIRROR_NONE
            Toys = CGObjectDataEnd + 6, // size 1 flags: MIRROR_NONE
            Transmog = CGObjectDataEnd + 7, // size 1 flags: MIRROR_NONE
            ConditionalTransmog = CGObjectDataEnd + 8, // size 1 flags: MIRROR_NONE
            SelfResSpells = CGObjectDataEnd + 9, // size 1 flags: MIRROR_NONE
            CharacterRestrictions = CGObjectDataEnd + 10, // size 1 flags: MIRROR_NONE
            SpellPctModByLabel = CGObjectDataEnd + 11, // size 1 flags: MIRROR_NONE
            SpellFlatModByLabel = CGObjectDataEnd + 12, // size 1 flags: MIRROR_NONE
            Research = CGObjectDataEnd + 13, // size 1 flags: MIRROR_NONE
            CGActivePlayerDynamicDataEnd = CGObjectDataEnd + 14
        }

        enum CGGameObjectDynamicData
        {
            EnableDoodadSets = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            CGGameObjectDynamicDataEnd = CGObjectDataEnd + 1
        }

        enum CGConversationDynamicData
        {
            Actors = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            Lines = CGObjectDataEnd + 1, // size 256 flags: MIRROR_NONE
            CGConversationDynamicDataEnd = CGObjectDataEnd + 2
        }
    }
}