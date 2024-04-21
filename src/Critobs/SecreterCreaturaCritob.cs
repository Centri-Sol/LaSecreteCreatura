using static PathCost.Legality;

namespace SecretCreaturas;

public class SecreterCreaturaCritob : Critob
{
    public HSLColor SecreterCreaturaColor => new(20/360f, 0.1f, 0.32f);

    internal SecreterCreaturaCritob() : base(SCEnums.CreaturaTypes.SecreterCreatura)
    {
        Icon = new SimpleIcon("Kill_SecreterCreatura", SecreterCreaturaColor.rgb);
        LoadedPerformanceCost = 10f;
        SandboxPerformanceCost = new(0.9f, 0.6f);
        ShelterDanger = ShelterDanger.Safe;
        RegisterUnlock(KillScore.Configurable(1), SCEnums.CreaturaUnlocks.SecreterCreatura);
    }
    public override int ExpeditionScore() => 1;

    public override Color DevtoolsMapColor(AbstractCreature absSC) => SecreterCreaturaColor.rgb;

    public override string DevtoolsMapName(AbstractCreature absSC) => "sc";
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
        CreatureTemplate SecreterCreatura = new CreatureFormula(SCEnums.CreaturaTypes.SecretCreatura, Type, "Secreter Creatura")
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
            DamageResistances = new() { Base = 0.75f, Water = 1.5f },
            StunResistances =   new() { Base = 1 },
            InstantDeathDamage = 1.5f,
            Pathing = PreBakedPathing.Ancestral(SCEnums.CreaturaTypes.SecretCreatura),
            HasAI = true
        }.IntoTemplate();

        SecreterCreatura.bodySize = 0.75f;
        SecreterCreatura.shortcutSegments = 4;
        SecreterCreatura.dangerousToPlayer = 0.1f;
        SecreterCreatura.communityInfluence = 0.7f;
        SecreterCreatura.communityID = CreatureCommunities.CommunityID.None;
        SecreterCreatura.quickDeath = false;
        SecreterCreatura.meatPoints = 9;
        SecreterCreatura.grasps = 2;
        SecreterCreatura.socialMemory = true;

        SecreterCreatura.visualRadius = 600f;
        SecreterCreatura.movementBasedVision = 1;
        SecreterCreatura.throughSurfaceVision = 0.85f;
        SecreterCreatura.waterVision = 1.5f;
        SecreterCreatura.lungCapacity = 72000;
        SecreterCreatura.waterRelationship = CreatureTemplate.WaterRelationship.Amphibious;
        SecreterCreatura.canSwim = true;

        SecreterCreatura.offScreenSpeed = 0.3f;
        SecreterCreatura.abstractedLaziness = 200;
        SecreterCreatura.roamInRoomChance = 0.125f;
        SecreterCreatura.roamBetweenRoomsChance = 0.075f;
        SecreterCreatura.interestInOtherAncestorsCatches = 0.05f;
        SecreterCreatura.interestInOtherCreaturesCatches = 0.05f;
        SecreterCreatura.usesCreatureHoles = true;
        SecreterCreatura.usesNPCTransportation = true;
        SecreterCreatura.stowFoodInDen = false;
        SecreterCreatura.hibernateOffScreen = true;

        SecreterCreatura.scaryness = 0.05f;
        SecreterCreatura.deliciousness = 0.5f;

        SecreterCreatura.BlizzardAdapted = false;
        SecreterCreatura.BlizzardWanderer = false;
        SecreterCreatura.wormGrassImmune = false;
        SecreterCreatura.wormgrassTilesIgnored = false;

        return SecreterCreatura;
    }
    public override void EstablishRelationships()
    {

        Relationships SecreterCreatura = new(SCEnums.CreaturaTypes.SecretCreatura);
        for (int i = 0; i < ExtEnum<CreatureTemplate.Type>.values.entries.Count; i++)
        {
            SecreterCreatura.Ignores(new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.entries[i])); // Default relationship
        }
        
        // Eats - Grabs the target and eats them right them and there
        SecreterCreatura.Eats(CreatureTemplate.Type.Fly, 0.01f);

        // Attacks - Grabs the target and curls up around them to crush them
        SecreterCreatura.Attacks(CreatureTemplate.Type.VultureGrub, 0.8f);
        SecreterCreatura.Attacks(CreatureTemplate.Type.EggBug, 0.5f);
        SecreterCreatura.Attacks(CreatureTemplate.Type.DropBug, 0.4f);

        // UncomfortableAround (Uncomfortable) - Becomes a bit more antsy around the creature, moving around more
        SecreterCreatura.UncomfortableAround(CreatureTemplate.Type.TempleGuard, 1);
        SecreterCreatura.UncomfortableAround(CreatureTemplate.Type.GarbageWorm, 0.5f);
        SecreterCreatura.UncomfortableAround(CreatureTemplate.Type.EggBug, 0.25f);
        SecreterCreatura.UncomfortableAround(CreatureTemplate.Type.Hazer, 0.25f);
        SecreterCreatura.UncomfortableAround(CreatureTemplate.Type.LanternMouse, 0.25f);

        // IntimidatedBy (StayOutOfWay) - Runs away if the creature gets too close
        SecreterCreatura.IntimidatedBy(CreatureTemplate.Type.Centipede, 1); // Applies to most Centipede types
        SecreterCreatura.IntimidatedBy(CreatureTemplate.Type.BigNeedleWorm, 1);
        SecreterCreatura.IntimidatedBy(CreatureTemplate.Type.Deer, 0.75f);
        SecreterCreatura.IntimidatedBy(CreatureTemplate.Type.PoleMimic, 0.5f);

        // Fears (Afraid) - Avoids being anywhere near the creature
        SecreterCreatura.Fears(CreatureTemplate.Type.Vulture, 1); // Applies to all Vultures
        SecreterCreatura.Fears(CreatureTemplate.Type.JetFish, 1);
        SecreterCreatura.Fears(CreatureTemplate.Type.BigEel, 1);
        SecreterCreatura.Fears(CreatureTemplate.Type.MirosBird, 1);
        SecreterCreatura.Fears(CreatureTemplate.Type.DaddyLongLegs, 1); // Applies to all rot types
        SecreterCreatura.Fears(CreatureTemplate.Type.DropBug, 0.75f);
        SecreterCreatura.Fears(CreatureTemplate.Type.LizardTemplate, 0.6f); // Applies to all lizards
        SecreterCreatura.Fears(CreatureTemplate.Type.WhiteLizard, 1);
        SecreterCreatura.Fears(CreatureTemplate.Type.Salamander, 0.8f);
        SecreterCreatura.Fears(CreatureTemplate.Type.BlackLizard, 0.7f);
        SecreterCreatura.Fears(CreatureTemplate.Type.BigSpider, 0.5f); // Applies to all larger spiders
        SecreterCreatura.Fears(CreatureTemplate.Type.Leech, 0.25f); // Applies to all Leech types

        // Rivals (AggressiveRival) - Curls up into a ball and rolls towards the target to bludgeon them
        SecreterCreatura.Rivals(CreatureTemplate.Type.Snail, 1);
        SecreterCreatura.Rivals(CreatureTemplate.Type.TubeWorm, 1);
        SecreterCreatura.Rivals(SCEnums.CreaturaTypes.SecreterCreatura, 0.4f);

        // Antagonizes - Combines Attacks and Rivals behaviors
        SecreterCreatura.Antagonizes(CreatureTemplate.Type.CicadaA, 0.8f); // Applies to both Squidcada types

        // HasDynamicRelationship (SocialDependent) - Has specific interactions with the given creature type.
        SecreterCreatura.HasDynamicRelationship(CreatureTemplate.Type.SmallCentipede, 1);
        SecreterCreatura.HasDynamicRelationship(CreatureTemplate.Type.SmallNeedleWorm, 0.6f);
        SecreterCreatura.HasDynamicRelationship(CreatureTemplate.Type.Spider, 0.5f); // Eats by default | Flees if there are too many spiders

        // Along with all these behaviors, there are certain things that creatures can do that will cause their usual relationship to be overridden with something else.

        //----------------------------------------

        SecreterCreatura.EatenBy(CreatureTemplate.Type.LizardTemplate, 0.6f); // Applies to all lizards
        SecreterCreatura.EatenBy(CreatureTemplate.Type.GreenLizard, 1);
        SecreterCreatura.EatenBy(CreatureTemplate.Type.BlackLizard, 0.85f);
        SecreterCreatura.EatenBy(CreatureTemplate.Type.Salamander, 0.7f);
        SecreterCreatura.EatenBy(CreatureTemplate.Type.BigSpider, 0.7f); // Applies to all larger spiders
        SecreterCreatura.EatenBy(CreatureTemplate.Type.Leech, 0.6f); // Applies to all Leech types
        SecreterCreatura.EatenBy(CreatureTemplate.Type.MirosBird, 0.6f);
        SecreterCreatura.EatenBy(CreatureTemplate.Type.Vulture, 0.3f); // Applies to all Vulture types
        SecreterCreatura.EatenBy(CreatureTemplate.Type.Slugcat, 0.3f);

        SecreterCreatura.EatenBy(CreatureTemplate.Type.Scavenger, 0.2f); // Applies to all scavenger types

        SecreterCreatura.MakesUncomfortable(CreatureTemplate.Type.Snail, 0.8f);

        SecreterCreatura.FearedBy(CreatureTemplate.Type.Fly, 0.4f);
        SecreterCreatura.FearedBy(CreatureTemplate.Type.SmallNeedleWorm, 0.5f);

        SecreterCreatura.AntagonizedBy(CreatureTemplate.Type.CicadaA, 0.8f); // Applies to both Squidcada types
        SecreterCreatura.AntagonizedBy(CreatureTemplate.Type.JetFish, 0.8f);

        //----------------------------------------

        if (ModManager.MSC)
        {
            // Eats - Grabs the target and eats them right them and there
            // None in MSC

            // Attacks - Grabs the target and curls up around them to crush them
            // None in MSC

            // UncomfortableAround (Uncomfortable) - Becomes a bit more antsy around the creature, moving around more
            // None in MSC

            // IntimidatedBy (StayOutOfWay) - Runs away if the creature gets too close
            SecreterCreatura.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.Inspector, 1);
            SecreterCreatura.IntimidatedBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);

            // Fears (Afraid) - Avoids being anywhere near the creature
            SecreterCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 1);
            SecreterCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1);
            SecreterCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 1);
            SecreterCreatura.Fears(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.9f);

            // Rivals (AggressiveRival) - Curls up into a ball and rolls towards the target to bludgeon them
            // None in MSC

            // Antagonizes - Combines Attacks and Rivals behaviors
            // None in MSC

            //----------------------------------------

            SecreterCreatura.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.EelLizard, 0.85f);
            SecreterCreatura.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard, 0.7f);
            SecreterCreatura.EatenBy(MoreSlugcatsEnums.CreatureTemplateType.FireBug, 0.25f);
        }

    }

    public override Creature CreateRealizedCreature(AbstractCreature absSC) => new SecreterCreatura(absSC, absSC.world);
    public override ArtificialIntelligence CreateRealizedAI(AbstractCreature absSC) => new SecretCreaturaAI(absSC, absSC.world);
    //public override AbstractCreatureAI CreateAbstractAI(AbstractCreature absSC) => new SecretCreaturaAbstractAI(absSC.world, absSC);
    public override CreatureState CreateState(AbstractCreature absSC) => new SecretCreaturaState(absSC);

    public override CreatureTemplate.Type ArenaFallback() => SCEnums.CreaturaTypes.SecretCreatura;
}