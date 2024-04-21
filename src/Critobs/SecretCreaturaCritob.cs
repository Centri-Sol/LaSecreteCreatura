using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using DevInterface;
using static PathCost.Legality;


namespace SecretCreaturas;

//----------------------------------------------------------------------------------
//----------------------------------------------------------------------------------

public class SecretCreaturaCritob : Critob
{
    public HSLColor SecretCreaturaColor => new(21/360f, 0.11f, 0.25f);

    internal SecretCreaturaCritob() : base(SCEnums.CreaturaTypes.SecretCreatura)
    {
        Icon = new SimpleIcon("Kill_SecretCreatura", SecretCreaturaColor.rgb);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.9f, 0.6f);
        ShelterDanger = ShelterDanger.Hostile;
        RegisterUnlock(KillScore.Configurable(16), SCEnums.CreaturaUnlocks.SecretCreatura);
    }
    public override int ExpeditionScore() => 16;

    public override Color DevtoolsMapColor(AbstractCreature absSC) => SecretCreaturaColor.rgb;

    public override string DevtoolsMapName(AbstractCreature absSC) => "SC";
    public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
    {
        return new[]
        {
            RoomAttractivenessPanel.Category.Dark,
            RoomAttractivenessPanel.Category.LikesInside,
            RoomAttractivenessPanel.Category.LikesWater,
            RoomAttractivenessPanel.Category.Swimming
        };
    }
    public override IEnumerable<string> WorldFileAliases() => new[] { "secretcreatura", "SecretCreatura" };

    public override CreatureTemplate CreateTemplate()
    {
        CreatureTemplate SecretCreatura = new CreatureFormula(null, Type, "Secret Creatura")
        {
            TileResistances = new()
            {
                Floor = new(1.1f, Allowed),
                Climb = new(5, Allowed),
                Wall = new(1, Allowed),
                Ceiling = new(50, Unwanted),
                Corridor = new(500, Unwanted),
            },
            ConnectionResistances = new()
            {
                Standard = new(1, Allowed),
                Slope = new(3, Allowed),
                CeilingSlope = new(5, Allowed),
                OpenDiagonal = new(5, Allowed),
                SemiDiagonalReach = new(5, Allowed),
                ReachOverGap = new(5, Allowed),
                ReachDown = new(1.1f, Allowed),
                ReachUp = new(1.5f, Allowed),
                DoubleReachUp = new(3, Allowed),
                ShortCut = new(10, Allowed),
                NPCTransportation = new(50, Allowed),
                DropToFloor = new(100, Allowed),
                DropToClimb = new(10, Allowed),
                DropToWater = new(10, Allowed),
                OffScreenMovement = new(1, Allowed),
                BigCreatureShortCutSqueeze = new(10, Allowed),
                BetweenRooms = new(5, Allowed)
            },
            DamageResistances = new() { Base = 3, Water = 2 },
            StunResistances =   new() { Base = 1, Water = 1.25f },
            InstantDeathDamage = 6,
            Pathing = PreBakedPathing.Ancestral(CreatureTemplate.Type.RedCentipede),
            HasAI = true
        }.IntoTemplate();

        SecretCreatura.bodySize = 7;
        SecretCreatura.shortcutSegments = 4;
        SecretCreatura.dangerousToPlayer = 0.35f;
        SecretCreatura.communityInfluence = 0.7f;
        SecretCreatura.communityID = CreatureCommunities.CommunityID.None;
        SecretCreatura.quickDeath = false;
        SecretCreatura.meatPoints = 9;
        SecretCreatura.grasps = 2;
        SecretCreatura.socialMemory = true;

        SecretCreatura.visualRadius = 600f;
        SecretCreatura.movementBasedVision = 1;
        SecretCreatura.throughSurfaceVision = 0.85f;
        SecretCreatura.waterVision = 1.5f;
        SecretCreatura.lungCapacity = 72000;
        SecretCreatura.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        SecretCreatura.canSwim = true;

        SecretCreatura.offScreenSpeed = 0.3f;
        SecretCreatura.abstractedLaziness = 200;
        SecretCreatura.roamInRoomChance = 0.15f;
        SecretCreatura.roamBetweenRoomsChance = 0.05f;
        SecretCreatura.interestInOtherAncestorsCatches = 0;
        SecretCreatura.interestInOtherCreaturesCatches = 0;
        SecretCreatura.usesCreatureHoles = true;
        SecretCreatura.usesNPCTransportation = true;
        SecretCreatura.stowFoodInDen = false;
        SecretCreatura.hibernateOffScreen = true;

        SecretCreatura.BlizzardAdapted = true;
        SecretCreatura.BlizzardWanderer = true;
        SecretCreatura.wormGrassImmune = true;
        SecretCreatura.wormgrassTilesIgnored = true;

        SecretCreatura.scaryness = 0.2f;
        SecretCreatura.deliciousness = 0.5f;

        SecretCreatura.jumpAction = "Curl Up / Uncurl";
        SecretCreatura.pickupAction = "Grab / Crunch";
        SecretCreatura.throwAction = "Release Grasp";

        return SecretCreatura;
    }
    public override void EstablishRelationships()
    {

        Relationships SecretCreatura = new(SCEnums.CreaturaTypes.SecretCreatura);
        for (int i = 0; i < ExtEnum<CreatureTemplate.Type>.values.entries.Count; i++)
        {
            SecretCreatura.Ignores(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.entries[i])); // Default relationship
        }

        // HasDynamicRelationship (SocialDependent) - Has specific interactions with the given creature type.
        SecretCreatura.HasDynamicRelationship(CreatureTemplate.Type.Scavenger, 1); // Applies to all Scavenger types
        SecretCreatura.HasDynamicRelationship(CreatureTemplate.Type.Centipede, 1); // Applies to most Centipede types
        SecretCreatura.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 0.6f);
        SecretCreatura.HasDynamicRelationship(CreatureTemplate.Type.LizardTemplate, 0.6f); // Applies to most Lizard types

        // Eats - Grabs the target and eats them right them and there
        SecretCreatura.Eats(CreatureTemplate.Type.VultureGrub, 0.4f);
        SecretCreatura.Eats(CreatureTemplate.Type.Leech, 0.2f);
        SecretCreatura.Eats(CreatureTemplate.Type.SeaLeech, 0.1f);
        SecretCreatura.Eats(CreatureTemplate.Type.Spider, 0.1f);
        SecretCreatura.Eats(CreatureTemplate.Type.Fly, 0.1f);
        SecretCreatura.Eats(CreatureTemplate.Type.SmallCentipede, 0.05f);

        // Attacks - Grabs the target and curls up around them to crush them
        SecretCreatura.Attacks(CreatureTemplate.Type.TubeWorm, 0.9f);
        SecretCreatura.Attacks(CreatureTemplate.Type.Snail, 0.8f);
        SecretCreatura.Attacks(CreatureTemplate.Type.BigSpider, 0.7f); // Applies to all larger spiders
        SecretCreatura.Attacks(CreatureTemplate.Type.Vulture, 0.6f); // Applies to most Vultures
        SecretCreatura.Attacks(CreatureTemplate.Type.EggBug, 0.5f);
        SecretCreatura.Attacks(CreatureTemplate.Type.DropBug, 0.4f);
        SecretCreatura.Attacks(CreatureTemplate.Type.CicadaA, 0.2f); // Applies to both Squidcada types

        // UncomfortableAround (Uncomfortable) - Becomes a bit more antsy around the creature, moving around more
        SecretCreatura.UncomfortableAround(CreatureTemplate.Type.TempleGuard, 0.5f);

        // IntimidatedBy (StayOutOfWay) - Runs away if the creature gets too close
        // Unused.

        // Fears (Afraid) - Avoids being anywhere near the creature
        SecretCreatura.Fears(CreatureTemplate.Type.BigEel, 1);
        SecretCreatura.Fears(CreatureTemplate.Type.KingVulture, 0.8f);
        SecretCreatura.Fears(CreatureTemplate.Type.DaddyLongLegs, 1); // Applies to all rot types

        // Rivals (AggressiveRival) - Curls up into a ball and rolls towards the target to bludgeon them
        SecretCreatura.Rivals(CreatureTemplate.Type.RedCentipede, 1);
        SecretCreatura.Rivals(CreatureTemplate.Type.MirosBird, 0.9f);
        SecretCreatura.Rivals(CreatureTemplate.Type.BrotherLongLegs, 0.8f);
        SecretCreatura.Rivals(CreatureTemplate.Type.JetFish, 0.7f);
        SecretCreatura.Rivals(SCEnums.CreaturaTypes.SecretCreatura, 0.6f);
        SecretCreatura.Rivals(CreatureTemplate.Type.GreenLizard, 0.6f);
        SecretCreatura.Rivals(CreatureTemplate.Type.Deer, 0.4f);

        // Antagonizes - Combines Attacks and Rivals behaviors
        // Used only for dynamic relationships in SecretCreaturaAI.

        // Along with all these behaviors, there are certain things that creatures can do that will cause their usual relationship to be overridden with something else.

        //----------------------------------------

        SecretCreatura.EatenBy(CreatureTemplate.Type.MirosBird, 0.8f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.BrotherLongLegs, 0.65f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.BigSpider, 0.6f); // Applies to all larger spiders
        SecretCreatura.EatenBy(CreatureTemplate.Type.DropBug, 0.5f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.Vulture, 0.4f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.KingVulture, 0.6f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.Slugcat, 0.25f);
        SecretCreatura.EatenBy(CreatureTemplate.Type.Leech, 0.15f); // Applies to all Leech types

        SecretCreatura.FearedBy(CreatureTemplate.Type.Fly, 0.7f);
        SecretCreatura.FearedBy(CreatureTemplate.Type.Snail, 0.7f);
        SecretCreatura.FearedBy(CreatureTemplate.Type.Scavenger, 0.6f); // Applies to all scavenger types
        SecretCreatura.FearedBy(CreatureTemplate.Type.EggBug, 0.35f);
        SecretCreatura.FearedBy(CreatureTemplate.Type.CicadaA, 0.2f); // Applies to both Squidcada types
        SecretCreatura.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.2f);

        SecretCreatura.AntagonizedBy(CreatureTemplate.Type.JetFish, 0.5f);

        //----------------------------------------

        if (ModManager.MSC)
        {
            // Eats - Grabs the target and eats them right them and there
            SecretCreatura.Eats(MoreSlugcatsEnums.CreatureTemplateType.JungleLeech, 0.15f);

            // UncomfortableAround (Uncomfortable) - Becomes a bit more antsy around the creature, moving around more
            SecretCreatura.UncomfortableAround(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug, 0.75f);

            // IntimidatedBy (StayOutOfWay) - Runs away if the creature gets too close
            SecretCreatura.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 0.8f);
            SecretCreatura.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.BigJelly, 0.7f);
            SecretCreatura.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);

            // Fears (Afraid) - Avoids being anywhere near the creature
            SecretCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 0.6f);
            SecretCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.5f);

            // Rivals (AggressiveRival) - Curls up into a ball and rolls towards the target to bludgeon them
            SecretCreatura.Rivals(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard, 1);
            SecretCreatura.Rivals(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard, 0.8f);

            //----------------------------------------

            SecretCreatura.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider, 0.7f);
            SecretCreatura.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.MirosVulture, 0.6f);

            SecretCreatura.FearedBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.5f);

            SecretCreatura.AntagonizedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.5f);
        }

    }
    public override Creature CreateRealizedCreature(AbstractCreature absSC) => new SecretCreatura(absSC, absSC.world);
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absSC) => new SecretCreaturaAI(absSC, absSC.world);
    //public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absSC) => new SecretCreaturaAbstractAI(absSC.world, absSC);
    public override CreatureState CreateState(AbstractCreature absSC) => new SecretCreaturaState(absSC);
    public override CreatureTemplate.Type ArenaFallback() => CreatureTemplate.Type.Centipede;
}