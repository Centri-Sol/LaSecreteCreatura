namespace SecretCreaturas;

public class SecretCreaturaAI : ArtificialIntelligence, IUseARelationshipTracker, IAINoiseReaction, IUseItemTracker, ITrackItemRelationships
{
    public class Behavior : ExtEnum<Behavior>
    {
        public static readonly Behavior Idle = new Behavior("Idle", register: true);

        public static readonly Behavior Flee = new Behavior("Flee", register: true);

        public static readonly Behavior Hunt = new Behavior("Hunt", register: true);

        public static readonly Behavior Injured = new Behavior("Injured", register: true);

        public static readonly Behavior EscapeRain = new Behavior("EscapeRain", register: true);

        public static readonly Behavior InvestigateSound = new Behavior("InvestigateSound", register: true);

        public static readonly Behavior Fighting = new Behavior("Fighting", register: true);

        public static readonly Behavior ReturnPrey = new Behavior("ReturnPrey", register: true);


        public Behavior(string value, bool register = false)
            : base(value, register)
        {
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public SecretCreatura Creatura => creature.realizedCreature as SecretCreatura;
    public SecretCreaturaState SecretCreaturaState => creature.state as SecretCreaturaState;

    public virtual AbstractPhysicalObject CurrentPrey
    {
        get
        {
            if (agressionTracker?.AgressionTarget()?.representedCreature is not null &&
                    (preyTracker?.MostAttractivePrey?.representedCreature is null || ObjectRelationship(agressionTracker.AgressionTarget().representedCreature).intensity > ObjectRelationship(preyTracker.MostAttractivePrey.representedCreature).intensity) &&
                    (itemFoodTracker?.MostAttractiveItem?.representedItem is null || ObjectRelationship(agressionTracker.AgressionTarget().representedCreature).intensity > ObjectRelationship(itemFoodTracker.MostAttractiveItem.representedItem).intensity))
            {
                return agressionTracker.AgressionTarget().representedCreature;
            }
            if (preyTracker?.MostAttractivePrey?.representedCreature is null || 
                    itemFoodTracker?.MostAttractiveItem?.representedItem is not null &&
                    ObjectRelationship(itemFoodTracker.MostAttractiveItem.representedItem).intensity > ObjectRelationship(preyTracker.MostAttractivePrey.representedCreature).intensity)
            {
                return itemFoodTracker?.MostAttractiveItem?.representedItem;
            }
            return preyTracker?.MostAttractivePrey?.representedCreature;
        }
    }
    public virtual AbstractPhysicalObject CurrentThreat
    {
        get
        {
            if (threatTracker?.mostThreateningCreature?.representedCreature is null || 
                    itemThreatTracker?.mostThreateningItem?.representedItem is not null &&
                    ObjectRelationship(itemThreatTracker.mostThreateningItem.representedItem).intensity > ObjectRelationship(threatTracker.mostThreateningCreature.representedCreature).intensity)
            {
                return itemThreatTracker?.mostThreateningItem?.representedItem;
            }
            return threatTracker?.mostThreateningCreature?.representedCreature;
        }
    }
    public virtual bool PreyVisual
    {
        get
        {
            PhysicalObject prey = CurrentPrey?.realizedObject;
            if (prey is not null)
            {
                if (prey is Creature && preyTracker.MostAttractivePrey.VisualContact)
                {
                    return true;
                }
                if (prey is not Creature && itemFoodTracker.MostAttractiveItem.VisualContact)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public virtual bool Panic
    {
        get
        {
            PhysicalObject threat = CurrentThreat?.realizedObject;
            if (threat is not null)
            {
                bool afraid =
                    ObjectRelationship(CurrentThreat).type == CreatureTemplate.Relationship.Type.Afraid ||
                    ObjectRelationship(CurrentThreat).type == CreatureTemplate.Relationship.Type.StayOutOfWay;
                float panicRange = Creatura.Template.visualRadius * 1 / 3f * ObjectRelationship(CurrentThreat).intensity;
                if (afraid)
                {
                    panicRange += Creatura.Template.visualRadius * 1 / 3f;
                }
                Vector2 threatPos = threat is Creature ctr ? ctr.DangerPos : threat.firstChunk.pos;
                if (Custom.DistLess(Creatura.Head.pos, threatPos, panicRange))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public virtual float currentUtility => Mathf.Clamp01(utilityComparer?.HighestUtility() ?? 0);

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public Behavior behavior;

    public bool onValidTile;

    public int annoyingCollisions;

    public float excitement;
    public float run;

    public WorldCoordinate forbiddenIdlePos;
    public WorldCoordinate tempIdlePos;
    public int idleCounter;

    public List<PlacedObject> foodObjects;

    public virtual float CrunchMassLimit => Creatura.TotalMass * 1.5f;

    public int noiseRectionDelay;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public ItemFoodTracker itemFoodTracker;
    public ItemThreatTracker itemThreatTracker;

    public SecretCreaturaAI(AbstractCreature absMite, World world) : base(absMite, world)
    {
        int seeAroundCorners = 15;
        int giveupTime = 1200;
        AddModule(new StandardPather(this, world, creature));
        pathFinder.accessibilityStepsPerFrame = 40;
        pathFinder.stepsPerFrame = 15;

        AddModule(new Tracker(this, seeAroundCorners, 10, 1200, 0.5f, 5, 5, 24));
        AddModule(new PreyTracker(this, 5, 1, 52.5f, 210, 0.075f));
        AddModule(new ThreatTracker(this, 3));
        AddModule(new RelationshipTracker(this, tracker));
        preyTracker.giveUpOnUnreachablePrey = giveupTime;

        AddModule(new ItemTracker(this, seeAroundCorners, 6, 800, 50, true));
        itemFoodTracker = new ItemFoodTracker(this, 3, 1f, 50, 100, 0);
        itemThreatTracker = new ItemThreatTracker(this, 3);
        AddModule(itemFoodTracker);
        AddModule(itemThreatTracker);
        itemFoodTracker.giveUpOnUnreachablePrey = giveupTime;

        AddModule(new AgressionTracker(this, 0.00075f, 0.001f));
        AddModule(new NoiseTracker(this, tracker));
        AddModule(new RainTracker(this));
        AddModule(new DenFinder(this, creature));
        AddModule(new InjuryTracker(this, 0.6f));

        AddModule(new UtilityComparer(this));
        utilityComparer.AddComparedModule(preyTracker, null, 0.9f, 1.05f);
        utilityComparer.AddComparedModule(itemFoodTracker, null, 0.9f, 1.05f);
        utilityComparer.AddComparedModule(threatTracker, null, 1, 1.1f);
        utilityComparer.AddComparedModule(itemThreatTracker, null, 1, 1.1f);
        utilityComparer.AddComparedModule(agressionTracker, null, 0.5f, 1.2f);
        utilityComparer.AddComparedModule(rainTracker, null, 1, 1.1f);
        utilityComparer.AddComparedModule(injuryTracker, null, 0.9f, 1.15f);
        utilityComparer.AddComparedModule(noiseTracker, null, 0.45f, 1.25f);

        behavior = Behavior.Idle;
    }
    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        forbiddenIdlePos = creature.pos;
        tempIdlePos = creature.pos;
        foodObjects = new List<PlacedObject>();
        for (int i = 0; i < newRoom.roomSettings.placedObjects.Count; i++)
        {
            if (newRoom.roomSettings.placedObjects[i].active && (
                    newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.DangleFruit ||
                    newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.WaterNut ||
                    newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.SlimeMold ||
                    newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.SporePlant ||
                    newRoom.roomSettings.placedObjects[i].type == PlacedObject.Type.KarmaFlower ||
                    newRoom.roomSettings.placedObjects[i].type == MoreSlugcatsEnums.PlacedObjectType.DandelionPeach ||
                    newRoom.roomSettings.placedObjects[i].type == MoreSlugcatsEnums.PlacedObjectType.GooieDuck ||
                    newRoom.roomSettings.placedObjects[i].type == MoreSlugcatsEnums.PlacedObjectType.LillyPuck ||
                    newRoom.roomSettings.placedObjects[i].type == MoreSlugcatsEnums.PlacedObjectType.GlowWeed))
            {
                foodObjects.Add(newRoom.roomSettings.placedObjects[i]);
            }
        }
    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        UpdateTimers();

        UpdateTrackedItems();

        UpdateAnnoyingCollisions();

        UpdateTrackerWeights();

        UpdateBehavior();

        UpdateDestinationAndExcitement();

        UpdateRunning();

    }
    public virtual void UpdateTimers()
    {
        if (noiseRectionDelay > 0)
        {
            noiseRectionDelay--;
        }
    }
    public virtual void UpdateTrackedItems()
    {
        for (int i = itemTracker.ItemCount - 1; i >= 0; i--)
        {
            AIModule tracker = (this as ITrackItemRelationships).ModuleToTrackItemRelationship(itemTracker.GetRep(i).representedItem);
            if (tracker is not null)
            {
                if (tracker is ItemFoodTracker f)
                {
                    f.AddFood(itemTracker.GetRep(i));
                }
                else if (tracker is ItemThreatTracker t)
                {
                    t.AddThreatItem(itemTracker.GetRep(i));
                }
                continue;
            }
        }

    }
    public virtual void UpdateAnnoyingCollisions()
    {
        if (ModManager.MSC && Creatura.LickedByPlayer is not null)
        {
            tracker.SeeCreature(Creatura.LickedByPlayer.abstractCreature);
            AnnoyingCollision(Creatura.LickedByPlayer.abstractCreature);
        }
        if (annoyingCollisions > 0)
        {
            annoyingCollisions--;
        }
    }
    public virtual void UpdateTrackerWeights()
    {
        if (noiseTracker is not null)
        {
            noiseTracker.hearingSkill = Creatura.moving ? 1.25f : 0.25f;
        }
        if (preyTracker.MostAttractivePrey is not null)
        {
            utilityComparer.GetUtilityTracker(preyTracker).weight = Mathf.InverseLerp(50, 10, preyTracker.MostAttractivePrey.TicksSinceSeen);
        }
        if (threatTracker.mostThreateningCreature is not null)
        {
            utilityComparer.GetUtilityTracker(threatTracker).weight = Mathf.InverseLerp(500, 100, threatTracker.mostThreateningCreature.TicksSinceSeen);
        }
        utilityComparer.GetUtilityTracker(agressionTracker).weight = (creature.world.game.IsStorySession ? 0.2f : 0.1f) * SecretCreaturaState.health;
    }
    public virtual void UpdateBehavior()
    {
        AIModule AIModule = utilityComparer.HighestUtilityModule();
        if (currentUtility < 0.1f)
        {
            behavior = Behavior.Idle;
        }
        else if (AIModule is not null)
        {
            if (AIModule is ThreatTracker)
            {
                behavior = Behavior.Flee;
            }
            else if (AIModule is RainTracker)
            {
                behavior = Behavior.EscapeRain;
            }
            else if (AIModule is PreyTracker)
            {
                behavior = Behavior.Hunt;
            }
            else if (AIModule is AgressionTracker)
            {
                behavior = Behavior.Fighting;
            }
            else if (AIModule is NoiseTracker)
            {
                behavior = Behavior.InvestigateSound;
            }
            else if (AIModule is InjuryTracker)
            {
                behavior = Behavior.Injured;
            }
        }
    }
    public virtual void UpdateDestinationAndExcitement()
    {
        float exciteGoal = 0f;
        if (behavior == Behavior.Idle)
        {
            IdleBehavior();
        }
        else if (behavior == Behavior.Flee)
        {
            exciteGoal = 1f;
            WorldCoordinate destination = threatTracker.FleeTo(creature.pos, 1, 30, currentUtility > 0.3f);
            creature.abstractAI.SetDestination(destination);
        }
        else if (behavior == Behavior.EscapeRain)
        {
            exciteGoal = 0.5f;
            if (denFinder.GetDenPosition().HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
            }
        }
        else if (behavior == Behavior.Injured)
        {
            exciteGoal = 1f;
            if (denFinder.GetDenPosition().HasValue)
            {
                creature.abstractAI.SetDestination(denFinder.GetDenPosition().Value);
            }
        }
        else if (behavior == Behavior.Hunt)
        {
            exciteGoal = ObjectRelationship(CurrentPrey).intensity;
            creature.abstractAI.SetDestination(preyTracker.MostAttractivePrey.BestGuessForPosition());
        }
        else if (behavior == Behavior.Fighting)
        {
            exciteGoal = ObjectRelationship(CurrentPrey).intensity;
            creature.abstractAI.SetDestination(agressionTracker.AgressionTarget().BestGuessForPosition());
        }
        else if (behavior == Behavior.InvestigateSound)
        {
            exciteGoal = 0.2f;
            creature.abstractAI.SetDestination(noiseTracker.ExaminePos);
        }
        excitement = Mathf.Lerp(excitement, exciteGoal, 0.1f);
    }
    public virtual void UpdateRunning()
    {
        if (behavior == Behavior.Hunt)
        {
            run = 500f;
            return;
        }
        run -= 1f;
        if (run < Mathf.Lerp(-50, -5, excitement))
        {
            run = Mathf.Lerp(30, 50, excitement);
        }
        int SecretCreaturaCount = 0;
        float GroupRun = 0f;
        for (int i = 0; i < tracker.CreaturesCount; i++)
        {
            if (tracker.GetRep(i).representedCreature.realizedCreature is not null and SecretCreatura otherSC &&
                tracker.GetRep(i).representedCreature.Room == creature.Room &&
                otherSC.AI.run > 0 == run > 0)
            {
                GroupRun += otherSC.AI.run;
                SecretCreaturaCount++;
            }
        }
        if (SecretCreaturaCount > 0)
        {
            run = Mathf.Lerp(run, GroupRun / SecretCreaturaCount, 0.1f);
        }
    }

    public void AnnoyingCollision(AbstractPhysicalObject absObj)
    {
        if (absObj?.realizedObject is null)
        {
            return;
        }
        if (absObj is AbstractCreature &&
            (absObj as AbstractCreature).state.dead)
        {
            return;
        }

        annoyingCollisions += 10;
        if (annoyingCollisions < 150)
        {
            return;
        }

        if (absObj is AbstractCreature absCtr &&
            tracker.RepresentationForCreature(absCtr, addIfMissing: false) is not null)
        {
            (tracker.RepresentationForCreature(absCtr, addIfMissing: false).dynamicRelationship.state as SecretCreaturaTrackCreatureState).annoyingCollisions++;
        }

    }
    public virtual void IdleBehavior()
    {
        WorldCoordinate testPos = creature.pos + new IntVector2(Random.Range(-10, 11), Random.Range(-10, 11));
        if (Random.value < 0.01f)
        {
            testPos = new WorldCoordinate(creature.pos.room, Random.Range(0, Creatura.room.TileWidth), Random.Range(0, Creatura.room.TileHeight), -1);
        }
        else if (foodObjects.Count > 0 && Random.value < 0.025f)
        {
            PlacedObject placedObject = foodObjects[Random.Range(0, foodObjects.Count)];
            testPos = Creatura.room.GetWorldCoordinate(placedObject.pos + Custom.RNV() * 50f * Random.value);
        }
        if (IdleScore(testPos) > IdleScore(tempIdlePos))
        {
            tempIdlePos = testPos;
            idleCounter = 0;
        }
        else
        {
            idleCounter++;
            if (creature.pos.room == tempIdlePos.room && creature.pos.Tile.FloatDist(tempIdlePos.Tile) < 10f)
            {
                idleCounter += 2;
            }
            if (idleCounter > 1000 || Creatura.outsideLevel)
            {
                idleCounter = 0;
                forbiddenIdlePos = tempIdlePos;
            }
        }
        if (tempIdlePos != pathFinder.GetDestination &&
            IdleScore(tempIdlePos) > IdleScore(pathFinder.GetDestination) + 100f)
        {
            creature.abstractAI.SetDestination(tempIdlePos);
        }
    }
    public float IdleScore(WorldCoordinate testPos)
    {
        if (!testPos.TileDefined)
        {
            return float.MinValue;
        }
        if (testPos.room != creature.pos.room)
        {
            return float.MinValue;
        }
        if (!pathFinder.CoordinateReachableAndGetbackable(testPos))
        {
            return float.MinValue;
        }
        float score = 1000f / Mathf.Max(1f, Creatura.room.aimap.getTerrainProximity(testPos) - 1f);
        score -= Custom.LerpMap(testPos.Tile.FloatDist(forbiddenIdlePos.Tile), 0, 10, 1000, 0);
        for (int c = 0; c < tracker.CreaturesCount; c++)
        {
            if (tracker.GetRep(c).representedCreature.creatureTemplate.type == creature.creatureTemplate.type &&
                tracker.GetRep(c).representedCreature.realizedCreature is not null &&
                tracker.GetRep(c).representedCreature.realizedCreature is SecretCreatura otherSC &&
                otherSC.Size > Creatura.Size &&
                tracker.GetRep(c).BestGuessForPosition().room == creature.pos.room &&
                Creatura.AI.behavior == Behavior.Idle)
            {
                score -= Custom.LerpMap(testPos.Tile.FloatDist(otherSC.AI.tempIdlePos.Tile), 0, 20, 1000, 0) * Mathf.InverseLerp(Creatura.Size, 1, Creatura.Size);
                score -= Custom.LerpMap(testPos.Tile.FloatDist(otherSC.AI.pathFinder.GetDestination.Tile), 0, 20, 1000, 0) * Mathf.InverseLerp(Creatura.Size, 1, Creatura.Size);
            }
        }
        if (Creatura.room.aimap.getAItile(testPos).fallRiskTile.y < 0)
        {
            score -= Custom.LerpMap(testPos.y, 10f, 30f, 1000f, 0f);
        }
        for (int f = 0; f < foodObjects.Count; f++)
        {
            if (Custom.DistLess(Creatura.room.MiddleOfTile(testPos), foodObjects[f].pos, 50))
            {
                score += 1000f;
                break;
            }
        }
        return score;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public void CheckRandomIdlePos()
    {
        WorldCoordinate testPos = new WorldCoordinate(creature.pos.room, Random.Range(0, Creatura.room.TileWidth), Random.Range(0, Creatura.room.TileHeight), -1);
        if (IdleScore(testPos) > IdleScore(tempIdlePos))
        {
            tempIdlePos = testPos;
            idleCounter = 0;
        }
    }
    public virtual bool DoIWantToGrabObject(AbstractPhysicalObject absObj)
    {
        if (Creatura.safariControlled)
        {
            return true;
        }
        if (annoyingCollisions < 150 && (behavior == Behavior.Flee || behavior == Behavior.EscapeRain) && currentUtility > 0.1f)
        {
            return false;
        }
        if (absObj is AbstractCreature &&
            (absObj as AbstractCreature).state.dead)
        {
            return false;
        }

        if (absObj is AbstractCreature absCtr)
        {
            Tracker.CreatureRepresentation ctrRep = tracker.RepresentationForCreature(absCtr, addIfMissing: false);

            if (annoyingCollisions > 150 && (ctrRep is null || (ctrRep.dynamicRelationship.state as SecretCreaturaTrackCreatureState).annoyingCollisions > (int)(Mathf.Lerp(120, 60, Creatura.Size) * SecretCreaturaState.health)))
            {
                return true;
            }
        }

        CreatureTemplate.Relationship relat = ObjectRelationship(absObj);

        if (relat.type == CreatureTemplate.Relationship.Type.Eats ||
            relat.type == CreatureTemplate.Relationship.Type.Attacks ||
            relat.type == CreatureTemplate.Relationship.Type.Antagonizes && !Creatura.CurledUp)
        {
            return true;
        }

        return false;
    }
    public override float VisualScore(Vector2 lookAtPoint, float bonus)
    {
        Vector2 headPos = Creatura.Head.pos;
        float visScore = base.VisualScore(lookAtPoint, bonus);
        Vector2 chunkDiff = Creatura.bodyChunks[1].pos - headPos;
        Vector2 lookDir = chunkDiff.normalized;
        chunkDiff = headPos - lookAtPoint;
        return visScore - Mathf.InverseLerp(1, 0.2f, Vector2.Dot(lookDir, chunkDiff.normalized));
    }
    public override bool WantToStayInDenUntilEndOfCycle()
    {
        return rainTracker.Utility() > 0.01f;
    }
    public float OverChasm(IntVector2 testPos)
    {
        float score = Creatura.room.aimap.getAItile(testPos).fallRiskTile.y < 0 ? 1 : 0;
        for (int i = -1; i < 2; i += 2)
        {
            for (int j = 1; j < 7 && !Creatura.room.GetTile(testPos + new IntVector2(j * i, 0)).Solid; j++)
            {
                if (Creatura.room.aimap.getAItile(testPos + new IntVector2(j * i, 0)).fallRiskTile.y < 0)
                {
                    score += 1 / (float)j;
                }
            }
        }
        return Mathf.InverseLerp(0f, 5.9f, score);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Creature relationships
    RelationshipTracker.TrackedCreatureState IUseARelationshipTracker.CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel) => new SecretCreaturaTrackCreatureState();
    public override Tracker.CreatureRepresentation CreateTrackerRepresentationForCreature(AbstractCreature otherCreature)
    {
        if (otherCreature.creatureTemplate.smallCreature)
        {
            return new Tracker.SimpleCreatureRepresentation(tracker, otherCreature, 0.4f, forgetWhenNotVisible: false);
        }
        return new Tracker.ElaborateCreatureRepresentation(tracker, otherCreature, 0.6f, 3);
    }
    AIModule IUseARelationshipTracker.ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
    {
        if (relationship.type == CreatureTemplate.Relationship.Type.StayOutOfWay ||
            relationship.type == CreatureTemplate.Relationship.Type.Afraid ||
            relationship.type == CreatureTemplate.Relationship.Type.Uncomfortable)
        {
            return threatTracker;
        }
        if (relationship.type == CreatureTemplate.Relationship.Type.Eats ||
            relationship.type == CreatureTemplate.Relationship.Type.Attacks)
        {
            return preyTracker;
        }
        if (relationship.type == CreatureTemplate.Relationship.Type.AgressiveRival ||
            relationship.type == CreatureTemplate.Relationship.Type.Antagonizes)
        {
            return agressionTracker;
        }
        return null;
    }
    CreatureTemplate.Relationship IUseARelationshipTracker.UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dynamRelat)
    {
        CreatureTemplate.Relationship relationship = StaticRelationship(dynamRelat.trackerRep.representedCreature);

        if (relationship.type == CreatureTemplate.Relationship.Type.Ignores ||
            (this as IUseARelationshipTracker).ModuleToTrackRelationship(relationship) is ThreatTracker)
        {
            return relationship;
        }

        if (dynamRelat.trackerRep.representedCreature.realizedCreature is not null)
        {
            Creature ctr = dynamRelat.trackerRep.representedCreature.realizedCreature;
            float intensity;

            if (ctr.dead)
            {
                return new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0);
            }

            if ((this as IUseARelationshipTracker).ModuleToTrackRelationship(relationship) is AgressionTracker)
            {
                relationship.intensity *= 1 - OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                return relationship;
            }

            if ((this as IUseARelationshipTracker).ModuleToTrackRelationship(relationship) is PreyTracker &&
                ctr.TotalMass < CrunchMassLimit)
            {
                intensity = Mathf.Pow(
                    Mathf.InverseLerp(0, CrunchMassLimit, ctr.TotalMass),
                    Custom.LerpMap(ctr.TotalMass, 0.2f, 0.7f, 3, 0.1f));
                intensity *= 1 - OverChasm(dynamRelat.trackerRep.BestGuessForPosition().Tile);
                relationship.intensity *= intensity;
                return relationship;
            }

            intensity = Mathf.InverseLerp(CrunchMassLimit, CrunchMassLimit * 1.5f, ctr.TotalMass);
            intensity = 0.2f + 0.8f * intensity;
            if (ctr.Template.CreatureRelationship(Creatura).type == CreatureTemplate.Relationship.Type.Eats)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, intensity);
            }
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.StayOutOfWay, intensity);
        }
        return relationship;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Item relationships
    public virtual CreatureTemplate.Relationship ObjectRelationship(AbstractPhysicalObject absObj)
    {

        if (absObj is AbstractCreature absCtr)
        {
            return DynamicRelationship(absCtr);
        }
        else if (annoyingCollisions >= Mathf.Lerp(6000, 12000, Creatura.Size) * SecretCreaturaState.health)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Attacks, 1);
        }

        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.WaterNut)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Eats, 1);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.DangleFruit ||
            absObj.type == AbstractPhysicalObject.AbstractObjectType.EggBugEgg)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Eats, 0.6f);
        }
        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.SlimeMold ||
            absObj.type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Eats, 0.2f);
        }

        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.JellyFish ||
            absObj.type == AbstractPhysicalObject.AbstractObjectType.SporePlant ||
            absObj.type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.StayOutOfWay, 0.1f);
        }

        if (absObj.type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
        {
            return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Uncomfortable, 0.75f);
        }

        if (ModManager.MSC)
        {
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.LillyPuck)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, 0.9f);
            }
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.GlowWeed)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, 0.7f);
            }
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.Seed)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Eats, 0.5f);
            }

            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.FireEgg)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Uncomfortable, 1);
            }
            if (absObj.type == MoreSlugcatsEnums.AbstractObjectType.SingularityBomb)
            {
                return new CreatureTemplate.Relationship
                    (CreatureTemplate.Relationship.Type.Afraid, 0.25f);
            }
        }

        return new CreatureTemplate.Relationship
                (CreatureTemplate.Relationship.Type.Ignores, 0);
    }
    bool IUseItemTracker.TrackItem(AbstractPhysicalObject obj)
    {
        if (obj is not null &&
            obj.type != AbstractPhysicalObject.AbstractObjectType.Creature &&
            ObjectRelationship(obj).type != CreatureTemplate.Relationship.Type.Ignores &&
            ObjectRelationship(obj).type != CreatureTemplate.Relationship.Type.DoesntTrack &&
            ObjectRelationship(obj).intensity > 0)
        {
            return true;
        }
        return false;
    }
    void IUseItemTracker.SeeThrownWeapon(PhysicalObject obj, Creature thrower) { }
    AIModule ITrackItemRelationships.ModuleToTrackItemRelationship(AbstractPhysicalObject obj)
    {
        if (obj is null)
        {
            return null;
        }
        if (ObjectRelationship(obj).type == CreatureTemplate.Relationship.Type.Eats ||
            ObjectRelationship(obj).type == CreatureTemplate.Relationship.Type.Attacks)
        {
            return itemFoodTracker;
        }
        if (ObjectRelationship(obj).type == CreatureTemplate.Relationship.Type.Afraid ||
            ObjectRelationship(obj).type == CreatureTemplate.Relationship.Type.StayOutOfWay ||
            ObjectRelationship(obj).type == CreatureTemplate.Relationship.Type.Uncomfortable)
        {
            return itemThreatTracker;
        }
        return null;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
    // Noise-tracking
    public void ReactToNoise(NoiseTracker.TheorizedSource source, Noise.InGameNoise noise)
    {
        if (noiseRectionDelay > noise.strength / 50f)
        {
            return;
        }

        noiseRectionDelay = 30;
    }

}
