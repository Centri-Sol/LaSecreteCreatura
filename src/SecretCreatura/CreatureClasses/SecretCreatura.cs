using System.Globalization;

namespace SecretCreaturas;

public class SecretCreatura : InsectoidCreature
{
    public SecretCreaturaState SecretCreaturaState => abstractCreature.state as SecretCreaturaState;

    public SecretCreaturaAI AI;
    public virtual bool Secreter => this is SecreterCreatura;
    public virtual bool Secretest => this is SecretestCreatura;

    // - - - - - - - - - - - - - - - - - - - -

    public Color ShellColor;

    public bool wantToFly;
    public int flyModeCounter;

    public bool outsideLevel;

    public int shockGiveUpCounter;

    public Rope[] connectionRopes;

    public Vector2 moveToPos;

    public int noFollowConCounter;

    public bool moving;

    public float bodyWave;

    public float doubleGrabCharge;

    public float Size;

    public BodyChunk Head => bodyChunks[0];
    public BodyChunk Butt => bodyChunks[bodyChunks.Length - 1];
    public Grasp Hold => grasps?[0];
    public override Vector2 VisionPoint => Head.pos;

    public bool ShouldSwim
    {
        get
        {
            if (Submersion > 0.5f)
            {
                return true;
            }
            return false;
        }
    }

    public bool CurledUp;

    public float curlUp;
    public float CurlUpRate = 1/60f;
    public float CurlDownRate = 1/120f;

    public int CrunchCooldown = 25;
    public int crunchCooldownTimer;


    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void GetSpawndata(out List<string> ActiveFlags)
    {
        ActiveFlags = new();
        if (abstractCreature.spawnData is null ||
            abstractCreature.spawnData[0] != '{')
        {
            return;
        }
        string[] flags = abstractCreature.spawnData.Substring(1, abstractCreature.spawnData.Length - 2).Split(',');
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i].Length < 1)
            {
                continue;
            }

            string[] flagParts = flags[i].Split(':');

            if (flagParts.Length < 1 ||
                flagParts[0].Length < 1)
            {
                continue;
            }

            switch (flagParts[0])
            {
                case "Size":
                    if (flagParts.Length < 2 ||
                        flagParts[1].Length < 1)
                    {
                        continue;
                    }
                    if (float.TryParse(flagParts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out Size))
                    {
                        ActiveFlags.Add(flagParts[0]);
                    }
                    break;

                case "Meat":
                    if (SecretCreaturaState.meatInitated ||
                        flagParts.Length < 2 ||
                        flagParts[1].Length < 1)
                    {
                        continue;
                    }
                    if (int.TryParse(flagParts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out SecretCreaturaState.meatLeft))
                    {
                        SecretCreaturaState.meatInitated = true;
                        ActiveFlags.Add(flagParts[0]);
                    }
                    break;

                case "Color":
                    HSLColor shellColor = new(0, 0, 0);
                    if (flagParts.Length < 4)
                    {
                        continue;
                    }
                    for (int v = 1; v < flagParts.Length && v < 3; v++)
                    {
                        if (flagParts[v].Length < 1 ||
                            !float.TryParse(flagParts[v], NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                        {
                            continue;
                        }
                        value = Mathf.Max(value, 0.05f);
                        if (v == 0)
                        {
                            shellColor.hue = value;
                        }
                        else if (v == 1)
                        {
                            shellColor.saturation = value;
                        }
                        else if (v == 2)
                        {
                            shellColor.lightness = value;
                        }
                    }
                    if (shellColor.rgb != Color.black)
                    {
                        ShellColor = shellColor.rgb;
                        ActiveFlags.Add(flagParts[0]);
                    }
                    break;
            }
        }
    }
    public SecretCreatura(AbstractCreature absSC, World world) : base(absSC, world)
    {
        GetSpawndata(out List<string> ActiveFlags);

        if (!ActiveFlags.Contains("Size"))
        {
            GenerateSize(absSC);
        }

        bodyChunks = new BodyChunk[Mathf.RoundToInt(Mathf.Lerp(7, 9.4f, Size))];
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            float bodyProgress = b / (float)(bodyChunks.Length - 1);
            float chunkRad =
                Mathf.Lerp(
                    Mathf.Lerp(2.5f, 5, Size),
                    Mathf.Lerp(5, 8.5f, Size),
                    Mathf.Pow(Mathf.Clamp(Mathf.Sin((float)Math.PI * bodyProgress), 0, 1), Mathf.Lerp(0.7f, 0.3f, Size)));
            float chunkMass = chunkRad/10f;

            bodyChunks[b] = new(this, b, default, chunkRad, chunkMass);
            bodyChunks[b].loudness = 0.3f;
        }
        bodyChunkConnections = new BodyChunkConnection[0];

        if (!ActiveFlags.Contains("Meat"))
        {
            InitiateMeatpoints(absSC);
        }

        mainBodyChunkIndex = bodyChunks.Length/2;

        bodyChunkConnections = new BodyChunkConnection[bodyChunks.Length * (bodyChunks.Length - 1) / 2];
        int c = 0;
        for (int conn1 = 0; conn1 < bodyChunks.Length; conn1++)
        {
            for (int conn2 = conn1 + 1; conn2 < bodyChunks.Length; conn2++)
            {
                bodyChunkConnections[c] = new BodyChunkConnection(bodyChunks[conn1], bodyChunks[conn2], bodyChunks[conn1].rad + bodyChunks[conn2].rad, BodyChunkConnection.Type.Push, 1, -1);
                c++;
            }
        }

        if (SecretCreaturaState.shells is null ||
            SecretCreaturaState.shells.Length != bodyChunks.Length)
        {
            SecretCreaturaState.shells = new bool[bodyChunks.Length];
            for (int s = 0; s < SecretCreaturaState.shells.Length; s++)
            {
                SecretCreaturaState.shells[s] = true;
            }
        }

        airFriction = 0.9975f;
        gravity = 0.9f;
        bounce = 0.6f;
        surfaceFriction = 0.5f;
        collisionLayer = 1;
        waterFriction = 0.9975f;
        buoyancy = 1;
        collisionRange = 150f;

        if (!ActiveFlags.Contains("Color"))
        {
            Random.State state = Random.state;
            Random.InitState(absSC.ID.RandomSeed);
            SetShellColor();
            Random.state = state;
        }
    }
    public virtual void InitiateMeatpoints(AbstractCreature absSC)
    {
        if (SecretCreaturaState.meatInitated)
        {
            return;
        }

        if (Template.meatPoints > -1)
        {
            SecretCreaturaState.meatLeft = Template.meatPoints;
        }
        else
        {
            SecretCreaturaState.meatLeft = bodyChunks.Length;
        }
    }
    public virtual void GenerateSize(AbstractCreature absSC)
    {
        bool spawndataSized = false;
        if (absSC.spawnData is not null &&
            absSC.spawnData[0] == '{')
        {
            string[] flags = absSC.spawnData.Substring(1, absSC.spawnData.Length - 2).Split(new char[1] { ',' });
            for (int i = 0; i < flags.Length; i++)
            {
                if (flags[i].Length <= 0 || !float.TryParse(flags[i].Split(new char[1] { ':' })[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float size))
                {
                    continue;
                }

                if (flags[i].Split(new char[1] { ':' })[0] == "BodySize")
                {
                    spawndataSized = true;
                    Size = size;
                }

                break;
            }
        }

        if (!spawndataSized)
        {
            Random.State state = Random.state;
            Random.InitState(absSC.ID.RandomSeed);
            Size = Random.value;
            Random.state = state;
        }
    }
    public virtual void SetShellColor()
    {
        if (Random.value < 0.025f)
        {
            ShellColor = new HSLColor(220 / 360f, 0.6f, 0.5f).rgb;
        }
        else if (!abstractCreature.IsVoided())
        {
            ShellColor = new HSLColor(24 / 360f, 0.16f, 0.16f).rgb;
        }
        else
        {
            ShellColor = RainWorld.SaturatedGold;
        }
    }

    // - - - - - - - - - - - - - - - - - - - -
    public override void InitiateGraphicsModule()
    {
        if (graphicsModule is null)
        {
            graphicsModule = new SecretCreaturaGraphics(this);
        }
    }
    public override void NewRoom(Room newRoom)
    {
        base.NewRoom(newRoom);
        connectionRopes = new Rope[bodyChunks.Length - 1];
        for (int i = 0; i < connectionRopes.Length; i++)
        {
            connectionRopes[i] = new Rope(newRoom, bodyChunks[i].pos, bodyChunks[i + 1].pos, 1f);
        }
    }

    //--------------------------------------------------------------------------------

    public override void Update(bool eu)
    {
        base.Update(eu);

        if (AI is null ||
            room is null)
        {
            return;
        }

        UpdateGrasp();

        ManageChunkRopes();

        UpdateGrasp();

        if (room.game.devToolsActive &&
            Input.GetKey("b") && room.game.cameras[0].room == room)
        {
            bodyChunks[0].vel += Custom.DirVec(bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 10f * (1 + Size);
            Stun(12);
        }

        ChunkMovementWhenDamaged();

        if (Consious)
        {
            Act();
        }
        curlUp = Mathf.Max(0f, curlUp - CurlDownRate);

    }
    public void UpdateGrasp()
    {
        if (Hold is null)
        {
            return;
        }

        if (!safariControlled &&
            Random.value < 0.025f)
        {
            if (!AI.DoIWantToGrabObject(Hold.grabbed.abstractPhysicalObject) ||
                !Consious)
            {
                ReleaseGrasp(0);
                return;
            }
        }

        float distFromHeld = Vector2.Distance(Head.pos, Hold.grabbedChunk.pos);
        if (distFromHeld > 50f + Hold.grabbedChunk.rad)
        {
            ReleaseGrasp(0);
            return;
        }
        Vector2 heldPullDir = Custom.DirVec(Head.pos, Hold.grabbedChunk.pos);
        float heldRad = Hold.grabbedChunk.rad;
        float pullMult = 0.95f;
        float chunkMassFac = Hold.grabbedChunk.mass / (Hold.grabbedChunk.mass + Head.mass);
        Head.pos -= (heldRad - distFromHeld) * heldPullDir * chunkMassFac * pullMult;
        Head.vel -= (heldRad - distFromHeld) * heldPullDir * chunkMassFac * pullMult;
        Hold.grabbedChunk.pos += (heldRad - distFromHeld) * heldPullDir * (1f - chunkMassFac) * pullMult;
        Hold.grabbedChunk.vel += (heldRad - distFromHeld) * heldPullDir * (1f - chunkMassFac) * pullMult;
        for (int i = 0; i < Hold.grabbed.bodyChunks.Length; i++)
        {
            if (Custom.DistLess(Butt.pos, Hold.grabbed.bodyChunks[i].pos, Butt.rad + Hold.grabbed.bodyChunks[i].rad + 10f))
            {
                BodyChunk bodyChunk3 = Hold.grabbed.bodyChunks[i];
                heldPullDir = Custom.DirVec(Butt.pos, bodyChunk3.pos);
                heldRad = bodyChunk3.rad;
                pullMult = 0.95f;
                chunkMassFac = bodyChunk3.mass / (Hold.grabbedChunk.mass + Butt.mass);
                Butt.pos -= (heldRad - distFromHeld) * heldPullDir * chunkMassFac * pullMult;
                Butt.vel -= (heldRad - distFromHeld) * heldPullDir * chunkMassFac * pullMult;
                bodyChunk3.pos += (heldRad - distFromHeld) * heldPullDir * (1f - chunkMassFac) * pullMult;
                bodyChunk3.vel += (heldRad - distFromHeld) * heldPullDir * (1f - chunkMassFac) * pullMult;
                curlUp += CurlUpRate;
                if (!safariControlled &&
                    CurlUpRate >= 1f)
                {
                    Crunch(Hold.grabbed);
                }
                break;
            }
        }
    }
    public virtual void ManageChunkRopes()
    {
        if (!enteringShortCut.HasValue)
        {
            for (int r = 0; r < connectionRopes.Length; r++)
            {
                connectionRopes[r].Update(bodyChunks[r].pos, bodyChunks[r + 1].pos);
                float totalLength = connectionRopes[r].totalLength;
                float combinedRad = bodyChunks[r].rad + bodyChunks[r + 1].rad;
                if (totalLength > combinedRad)
                {
                    float chunkMassFac = bodyChunks[r].mass / (bodyChunks[r].mass + bodyChunks[r + 1].mass);
                    float pushMult = 1f;
                    Vector2 pushDir = Custom.DirVec(bodyChunks[r].pos, connectionRopes[r].AConnect);
                    bodyChunks[r].vel += pushDir * (totalLength - combinedRad) * pushMult * chunkMassFac;
                    bodyChunks[r].pos += pushDir * (totalLength - combinedRad) * pushMult * chunkMassFac;
                    pushDir = Custom.DirVec(bodyChunks[r + 1].pos, connectionRopes[r].BConnect);
                    bodyChunks[r + 1].vel += pushDir * (totalLength - combinedRad) * pushMult * (1f - chunkMassFac);
                    bodyChunks[r + 1].pos += pushDir * (totalLength - combinedRad) * pushMult * (1f - chunkMassFac);
                }
            }
            for (int r = connectionRopes.Length - 2; r >= 0; r--)
            {
                connectionRopes[r].Update(bodyChunks[r].pos, bodyChunks[r + 1].pos);
                float totalLength2 = connectionRopes[r].totalLength;
                float combinedRad = bodyChunks[r].rad + bodyChunks[r + 1].rad;
                if (totalLength2 > combinedRad)
                {
                    float chunkMassFac = bodyChunks[r].mass / (bodyChunks[r].mass + bodyChunks[r + 1].mass);
                    float pushMult = 1f;
                    Vector2 pushDir = Custom.DirVec(bodyChunks[r].pos, connectionRopes[r].AConnect);
                    bodyChunks[r].vel += pushDir * (totalLength2 - combinedRad) * pushMult * chunkMassFac;
                    bodyChunks[r].pos += pushDir * (totalLength2 - combinedRad) * pushMult * chunkMassFac;
                    pushDir = Custom.DirVec(bodyChunks[r + 1].pos, connectionRopes[r].BConnect);
                    bodyChunks[r + 1].vel += pushDir * (totalLength2 - combinedRad) * pushMult * (1f - chunkMassFac);
                    bodyChunks[r + 1].pos += pushDir * (totalLength2 - combinedRad) * pushMult * (1f - chunkMassFac);
                }
            }
            for (int b = 0; b < bodyChunks.Length - 2; b++)
            {
                bodyChunks[b].vel += Custom.DirVec(bodyChunks[b + 2].pos, bodyChunks[b].pos) * Mathf.Lerp(1f, 6f, curlUp) * (1 + Size);
                bodyChunks[b + 2].vel += Custom.DirVec(bodyChunks[b].pos, bodyChunks[b + 2].pos) * Mathf.Lerp(1f, 6f, curlUp) * (1 + Size);
            }
        }
    }
    public virtual void ChunkMovementWhenDamaged()
    {
        if (dead &&
            grabbedBy.Count == 0)
        {
            for (int b = 0; b < bodyChunks.Length; b++)
            {
                if (bodyChunks[b].ContactPoint.y != 0)
                {
                    bodyChunks[b].vel.x *= 0.1f;
                }
            }
        }
        else if (SecretCreaturaState.health < 0.75f)
        {
            if (Random.value * 0.75f > SecretCreaturaState.health && stun > 0)
            {
                stun--;
            }
            if (Random.value > SecretCreaturaState.health &&
                Random.value < 1 / 3f)
            {
                Stun(4);
                if (SecretCreaturaState.health <= 0f &&
                    Random.value < 1 / Mathf.Lerp(500, 10, 0 - SecretCreaturaState.health))
                {
                    Die();
                }
            }
            if (!dead)
            {
                for (int b = 0; b < bodyChunks.Length; b++)
                {
                    if (Random.value > SecretCreaturaState.health * 2f)
                    {
                        bodyChunks[b].vel += Custom.RNV() * Mathf.Pow(Random.value, Custom.LerpMap(SecretCreaturaState.health, 0.75f, 0, 3, 0.1f, 2)) * 4f * Mathf.InverseLerp(0.75f, 0, SecretCreaturaState.health);
                    }
                }
            }
        }
    }

    public void Crunch(PhysicalObject crunchObj)
    {
        BodyChunk CrunchChunk = crunchObj.bodyChunks[crunchObj.bodyChunks.Length/2];

        room.PlaySound(SoundID.Big_Needle_Worm_Impale_Terrain, CrunchChunk, false, 1.7f, Random.Range(0.5f, 0.75f));

        Vector2 center = Head.pos;
        for (int b = 1; b < bodyChunks.Length; b++)
        {
            center = Vector2.Lerp(center, bodyChunks[b].pos, 0.5f);
        }

        Vector2 centerDir;
        for (int b = 0; b < bodyChunks.Length; b++)
        {
            centerDir = Custom.DirVec(bodyChunks[b].pos, center);
            bodyChunks[b].pos += centerDir * 5f;
            bodyChunks[b].vel += centerDir * 5f;
        }

        for (int b = 0; b < crunchObj.bodyChunks.Length; b++)
        {
            centerDir = Custom.DirVec(crunchObj.bodyChunks[b].pos, center);
            crunchObj.bodyChunks[b].vel += centerDir * 7.5f;
            crunchObj.bodyChunks[b].pos += centerDir * 7.5f;
        }

        float CreaturaMass = TotalMass * (1 - (Submersion * 0.25f));
        CreaturaMass = Mathf.InverseLerp(CreaturaMass, CreaturaMass * 2f, crunchObj.TotalMass);

        if (crunchObj is Creature target)
        {
            target.Violence(Head, default, CrunchChunk, null, DamageType.Blunt, Mathf.Lerp(target.Template.baseDamageResistance, 0, CreaturaMass), 200);
            room.PlaySound(SoundID.Spear_Stick_In_Creature, CrunchChunk, false, 1.7f, Random.Range(0.5f, 0.75f));
        }
        else
        {

            if (CreaturaMass == 0)
            {
                crunchObj.Destroy();
                room.PlaySound(SoundID.Spear_Fragment_Bounce, CrunchChunk, false, 1.2f, 1 - 0.5f * Mathf.InverseLerp(0, CreaturaMass, crunchObj.TotalMass));
            }
        }

        crunchCooldownTimer = CrunchCooldown;
        AI.annoyingCollisions = Math.Min(AI.annoyingCollisions / 2, 150);

    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void Act()
    {
        if (ShouldSwim)
        {
            flyModeCounter = 100;
        }

        AI.Update();

        moving = AI.run > 0f && Custom.ManhattanDistance(room.GetWorldCoordinate(Head.pos), AI.pathFinder.GetDestination) > 2;

        if (ShouldSwim)
        {
            moving = true;
        }

        SafariCrunchControls();

        if (moving)
        {
            FigureOutDestination();
        }

        if (Hold is null)
        {
            if (ShouldSwim)
            {
                Swim();
            }
            else
            {
                Crawl();
            }
            doubleGrabCharge = Mathf.Max(0f, doubleGrabCharge - 0.025f);
            shockGiveUpCounter = Math.Max(0, shockGiveUpCounter - 2);
        }
        else
        {
            TryMovetoCrunchPos();
            Head.vel += Custom.DirVec(Head.pos, moveToPos) * Mathf.Pow(doubleGrabCharge, 2f) * 6f * Mathf.Lerp(0.7f, 1.3f, Size);
            doubleGrabCharge = Mathf.Min(1f, doubleGrabCharge + 0.0125f);
            if (doubleGrabCharge > 0.9f)
            {
                shockGiveUpCounter++;
                if (shockGiveUpCounter >= 110)
                {
                    Stun(12);
                    shockGiveUpCounter = 30;
                    LoseAllGrasps();
                }
            }
        }

        if (AI.CurrentPrey?.realizedObject is null ||
            AI.CurrentPrey.realizedObject.collisionLayer == collisionLayer)
        {
            return;
        }
        PhysicalObject prey = AI.CurrentPrey.realizedObject;

        for (int c = 0; c < prey.bodyChunks.Length; c++)
        {
            for (int b = 0; b < bodyChunks.Length; b++)
            {
                if (Custom.DistLess(prey.bodyChunks[c].pos, bodyChunks[b].pos, prey.bodyChunks[c].rad + bodyChunks[b].rad))
                {
                    Collide(prey, b, c);
                }
            }
        }
    }
    public virtual void SafariCrunchControls()
    {
        if (safariControlled)
        {
            moving = inputWithoutDiagonals.HasValue && (inputWithoutDiagonals.Value.x != 0 || inputWithoutDiagonals.Value.y != 0);

            if ((!inputWithoutDiagonals.Value.pckp || grabbedBy.Count > 0) && Hold is not null)
            {
                ReleaseGrasp(0);
            }
            else
            if (inputWithoutDiagonals.Value.pckp &&
                grabbedBy.Count == 0 &&
                Consious)
            {
                if (Hold is null)
                {
                    for (int c = 0; c < abstractCreature.Room.creatures.Count; c++)
                    {
                        if (abstractCreature.Room.creatures[c].realizedCreature is null)
                        {
                            continue;
                        }
                        Creature ctr = abstractCreature.Room.creatures[c].realizedCreature;

                        for (int b = 0; b < ctr.bodyChunks.Length; b++)
                        {
                            if (ctr.abstractCreature != abstractCreature &&
                               Custom.DistLess(Head.pos, ctr.bodyChunks[b].pos, 50f + ctr.bodyChunks[b].rad) &&
                               !ctr.dead)
                            {
                                Grab(ctr, 0, b, Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: false, pacifying: false);
                                room.PlaySound(SoundID.Centipede_Attach, Head, false, 1, 0.7f);
                                break;
                            }
                        }

                    }
                }
                if (Hold is not null &&
                    Custom.DistLess(Head.pos, Butt.pos, 15f))
                {
                    Crunch(Hold.grabbed);
                    ReleaseGrasp(0);
                }
            }
        }
    }
    public virtual void FigureOutDestination()
    {
        MovementConnection connection = (AI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(Head.pos), actuallyFollowingThisPath: true);
        if (safariControlled && (connection == default || !AllowableControlledAIOverride(connection.type)) && inputWithoutDiagonals.HasValue)
        {
            MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
            if (room.GetTile(mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
            {
                type = MovementConnection.MovementType.ShortCut;
            }
            if (inputWithoutDiagonals.Value.x != 0 ||
                inputWithoutDiagonals.Value.y != 0)
            {
                connection = new MovementConnection(type, room.GetWorldCoordinate(mainBodyChunk.pos), room.GetWorldCoordinate(mainBodyChunk.pos + new Vector2(inputWithoutDiagonals.Value.x, inputWithoutDiagonals.Value.y) * Mathf.Max(80f, Size * 240f)), 2);
            }

            GoThroughFloors = inputWithoutDiagonals.Value.y < 0;

        }
        moving = connection != default;
        if (moving)
        {
            if (shortcutDelay < 1 && (
                    connection.type == MovementConnection.MovementType.ShortCut ||
                    connection.type == MovementConnection.MovementType.NPCTransportation))
            {
                enteringShortCut = connection.StartTile;
                FindNPCTransportPath(connection);
                return;
            }
            if (connection.destinationCoord.TileDefined)
            {
                GoThroughFloors = connection.DestTile.y < connection.StartTile.y;
                moveToPos = room.MiddleOfTile(connection.DestTile);
                if (connection.DestTile.x != connection.StartTile.x)
                {
                    moveToPos.y += VerticalSitSurface(moveToPos) * 5f;
                }
                if (connection.DestTile.y != connection.StartTile.y)
                {
                    moveToPos.x += HorizontalSitSurface(moveToPos) * 5f;
                }
            }
            noFollowConCounter = 0;
        }
        else
        {
            noFollowConCounter++;
            if (noFollowConCounter > 40 && bodyChunks.Length != 0)
            {
                int randomChunk = Random.Range(0, bodyChunks.Length);
                if (AccessibleTile(room.GetTilePosition(bodyChunks[randomChunk].pos)))
                {
                    moveToPos = bodyChunks[randomChunk].pos;
                }
            }
        }
    }
    public virtual void FindNPCTransportPath(MovementConnection connection)
    {
        if (safariControlled)
        {
            bool foundEntrance = false;
            List<IntVector2> validExits = new List<IntVector2>();
            ShortcutData[] shortcuts = room.shortcuts;
            for (int s = 0; s < shortcuts.Length; s++)
            {
                ShortcutData shortcutData = shortcuts[s];
                if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation &&
                    shortcutData.StartTile != connection.StartTile)
                {
                    validExits.Add(shortcutData.StartTile);
                }
                if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation &&
                    shortcutData.StartTile == connection.StartTile)
                {
                    foundEntrance = true;
                }
            }
            if (foundEntrance)
            {
                if (validExits.Count > 0)
                {
                    validExits.Shuffle();
                    NPCTransportationDestination = room.GetWorldCoordinate(validExits[0]);
                }
                else
                {
                    NPCTransportationDestination = connection.destinationCoord;
                }
            }
        }
        else if (connection.type == MovementConnection.MovementType.NPCTransportation)
        {
            NPCTransportationDestination = connection.destinationCoord;
        }
    }
    public virtual void TryMovetoCrunchPos()
    {
        if (Hold is null ||
            !AI.DoIWantToGrabObject(Hold.grabbed.abstractPhysicalObject))
        {
            return;
        }
        moveToPos = Hold.grabbedChunk.pos;

        if (room.VisualContact(Head.pos, Hold.grabbedChunk.pos))
        {
            return;
        }

        for (int b = bodyChunks.Length - 1; b >= 0; b--)
        {
            if (room.VisualContact(bodyChunks[b].pos, Head.pos))
            {
                moveToPos = bodyChunks[b].pos;
                break;
            }
        }
    }

    public virtual int VerticalSitSurface(Vector2 pos)
    {
        if (room.GetTile(pos + new Vector2(0f, -20f)).Solid)
        {
            return -1;
        }
        if (room.GetTile(pos + new Vector2(0f, 20f)).Solid)
        {
            return 1;
        }
        return 0;
    }
    public virtual int HorizontalSitSurface(Vector2 pos)
    {
        if (room.GetTile(pos + new Vector2(-20f, 0f)).Solid &&
            !room.GetTile(pos + new Vector2(20f, 0f)).Solid)
        {
            return -1;
        }
        if (room.GetTile(pos + new Vector2(20f, 0f)).Solid &&
            !room.GetTile(pos + new Vector2(-20f, 0f)).Solid)
        {
            return 1;
        }
        return 0;
    }
    public virtual bool AccessibleTile(Vector2 testPos) => AccessibleTile(room.GetTilePosition(testPos));
    public virtual bool AccessibleTile(IntVector2 testPos)
    {
        if (testPos.y != room.defaultWaterLevel)
        {
            return room.aimap.TileAccessibleToCreature(testPos, Template);
        }
        return ClimbableTile(testPos);
    }
    public bool ClimbableTile(Vector2 testPos) => ClimbableTile(room.GetTilePosition(testPos));
    public bool ClimbableTile(IntVector2 testPos)
    {
        if (!room.GetTile(testPos).wallbehind && !room.GetTile(testPos).verticalBeam && !room.GetTile(testPos).horizontalBeam)
        {
            return room.aimap.getTerrainProximity(testPos) < 2;
        }
        return true;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual void Crawl()
    {
        int SegmentsMoved = 0;
        for (int i = 0; i < bodyChunks.Length; i++)
        {
            if (!AccessibleTile(room.GetTilePosition(bodyChunks[i].pos)))
            {
                continue;
            }
            SegmentsMoved++;
            bodyChunks[i].vel *= 0.7f;
            bodyChunks[i].vel.y += gravity * Mathf.Pow(Mathf.Max(0, Mathf.Lerp(SecretCreaturaState.ClampedHealth, 1, Random.value)), 0.25f);

            if (i > 0 && !AccessibleTile(room.GetTilePosition(bodyChunks[i - 1].pos)))
            {
                bodyChunks[i].vel *= 0.3f;
                bodyChunks[i].vel.y += gravity * Mathf.Pow(Mathf.Max(0, Mathf.Lerp(SecretCreaturaState.ClampedHealth, 1, Random.value)), 0.25f);
            }
            if (i < bodyChunks.Length - 1 && !AccessibleTile(room.GetTilePosition(bodyChunks[i + 1].pos)))
            {
                bodyChunks[i].vel *= 0.3f;
                bodyChunks[i].vel.y += gravity * Mathf.Pow(Mathf.Max(0, Mathf.Lerp(SecretCreaturaState.ClampedHealth, 1, Random.value)), 0.25f);
            }

            if (i <= 0 ||
                i >= bodyChunks.Length - 1)
            {
                continue;
            }

            if (moving)
            {
                if (AccessibleTile(room.GetTilePosition(bodyChunks[i - 1].pos)))
                {
                    bodyChunks[i].vel += Custom.DirVec(bodyChunks[i].pos, bodyChunks[i + 1].pos) * 1.5f * Mathf.Lerp(0.5f, 1.5f, Size * SecretCreaturaState.ClampedHealth);
                }
                bodyChunks[i].vel -= Custom.DirVec(bodyChunks[i].pos, bodyChunks[i + 1].pos) * 0.8f * Mathf.Lerp(0.7f, 1.3f, Size * SecretCreaturaState.ClampedHealth);
                continue;
            }

            Vector2 chunkDiff = bodyChunks[i].pos - bodyChunks[i - 1].pos;
            Vector2 chunksDir = chunkDiff.normalized;
            chunkDiff = bodyChunks[i + 1].pos - bodyChunks[i].pos;
            Vector2 finalChunkDir = (chunksDir + chunkDiff.normalized) / 2f;
            if (Mathf.Abs(finalChunkDir.x) > 0.5f)
            {
                bodyChunks[i].vel.y -= (bodyChunks[i].pos.y - (room.MiddleOfTile(bodyChunks[i].pos).y + VerticalSitSurface(bodyChunks[i].pos)   * (10 - bodyChunks[i].rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(Size * SecretCreaturaState.ClampedHealth, 1.2f));
            }
            if (Mathf.Abs(finalChunkDir.y) > 0.5f)
            {
                bodyChunks[i].vel.x -= (bodyChunks[i].pos.x - (room.MiddleOfTile(bodyChunks[i].pos).x + HorizontalSitSurface(bodyChunks[i].pos) * (10 - bodyChunks[i].rad))) * Mathf.Lerp(0.01f, 0.6f, Mathf.Pow(Size * SecretCreaturaState.ClampedHealth, 1.2f));
            }
        }

        if (SegmentsMoved > 0 &&
            !Custom.DistLess(Head.pos, moveToPos, 10f))
        {
            Head.vel += Custom.DirVec(Head.pos, moveToPos) * Custom.LerpMap(SegmentsMoved, 0, bodyChunks.Length, 6, 3) * Mathf.Lerp(0.7f, 1.3f, Size * SecretCreaturaState.health);
        }

    }
    public virtual void Swim()
    {
        bodyWave += Mathf.Clamp(Vector2.Distance(Head.pos, AI.tempIdlePos.Tile.ToVector2()) / 80f, 0.1f, 1f);

        for (int b = 0; b < bodyChunks.Length; b++)
        {
            float bodyProgress = b / (float)(bodyChunks.Length - 1);
            float bodyWaveFac = Mathf.Sin((bodyWave - bodyProgress * Mathf.Lerp(12, 28, Size)) * (float)Math.PI * 0.11f);
            bodyChunks[b].vel *= 0.9f;
            bodyChunks[b].vel.y += gravity;

            if (b <= 0 ||
                b >= bodyChunks.Length - 1)
            {
                continue;
            }

            Vector2 moveDir = Custom.DirVec(bodyChunks[b].pos, bodyChunks[b - 1].pos);
            Vector2 perpDir = Custom.PerpendicularVector(moveDir);
            bodyChunks[b].vel += moveDir * 0.5f * Mathf.Lerp(0.5f, 1.5f, Size);
            if (AI.behavior == SecretCreaturaAI.Behavior.Idle)
            {
                bodyChunks[b].vel *= Mathf.Clamp(Vector2.Distance(Head.pos, AI.tempIdlePos.Tile.ToVector2()) / 40f, 0.02f, 1f);
                if (Vector2.Distance(Head.pos, AI.tempIdlePos.Tile.ToVector2()) < 20f)
                {
                    bodyChunks[b].vel *= 0.28f;
                }
            }
            bodyChunks[b].pos += perpDir * 2.5f * bodyWaveFac;
        }

        if (room.aimap.getTerrainProximity(moveToPos) > 2)
        {
            if (ShouldSwim)
            {
                Head.vel += Custom.DirVec(Head.pos, moveToPos + Custom.DegToVec(bodyWave *  5) * 10) * 5f * Mathf.Lerp(0.7f, 1.3f, Size);
            }
            else
            {
                Head.vel += Custom.DirVec(Head.pos, moveToPos + Custom.DegToVec(bodyWave * 10) * 60) * 4f * Mathf.Lerp(0.7f, 1.3f, Size);
            }
        }
        else if (ShouldSwim)
        {
            Head.vel += Custom.DirVec(Head.pos, moveToPos) * 2f * Mathf.Lerp(0.2f, 0.8f, Size);
        }
        else
        {
            Head.vel += Custom.DirVec(Head.pos, moveToPos) * 4f * Mathf.Lerp(0.7f, 1.3f, Size);
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual bool IsShellIntact(BodyChunk chunk)
    {
        if (chunk is not null &&
            chunk.index >= 0 &&
            chunk.index < SecretCreaturaState.shells.Length &&
            SecretCreaturaState.shells[chunk.index])
        {
            return true;
        }
        return false;
    }
    public override bool SpearStick(Weapon source, float dmg, BodyChunk hitChunk, Appendage.Pos appPos, Vector2 direction)
    {
        if (IsShellIntact(hitChunk) && CurledUp)
        {
            return false;
        }
        return base.SpearStick(source, dmg, hitChunk, appPos, direction);
    }
    public override void Violence(BodyChunk source, Vector2? attackForce, BodyChunk hitChunk, Appendage.Pos hitAppendage, DamageType type, float damage, float stunBonus)
    {
        attackForce ??= default;

        if (!SpearStick(source.owner is Weapon wpn ? wpn : default, damage, hitChunk, hitAppendage, (Vector2)attackForce))
        {
            Color ParticleColor = Color.Lerp(ShellColor, Color.white, 0.9f);
            int sparks = (int)Math.Min(25f, Math.Max(
                            (damage * 15f + stunBonus) / Template.baseStunResistance / 2f,
                            (int)(damage / Template.baseDamageResistance * 15f)));
            while (sparks > 0)
            {
                sparks--;
                room.AddObject(new Spark(
                    pos: source.pos + Custom.DegToVec(Random.value * 360f) * 5f * Random.value,
                    vel: source.vel * -0.1f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.2f, 0.4f, Random.value) * attackForce.Value.magnitude,
                    color: ParticleColor,
                    lizard: null,
                    standardLifeTime: 10, 
                    exceptionalLifeTime: 120));
            }
            room.AddObject(new StationaryEffect(source.pos, ParticleColor, null, StationaryEffect.EffectType.FlashingOrb));
            room.PlaySound(SoundID.Lizard_Head_Shield_Deflect, hitChunk);

            if (damage >= 0.25f)
            {
                BreakShell(hitChunk.index, (Vector2)attackForce);
            }

            damage *= 0.05f;
        }
        else
        {
            base.LoseAllGrasps();

            if (IsShellIntact(hitChunk) && damage >= 2)
            {
                BreakShell(hitChunk.index, (Vector2)attackForce);
            }

            if (type == DamageType.Electric ||
                type == DamageType.Water ||
                type == DamageType.Bite)
            {
                damage *= 0.125f;
            }
            else
            {
                damage *= 0.250f;
            }
        }

        if (hitChunk == Head)
        {
            damage *= 0.5f;
        }

        stunBonus *= 1 - (SecretCreaturaState.health * 0.5f);

        if (Template.baseDamageResistance == 1)
        {
            damage /= Mathf.Lerp(1, 6, Mathf.Pow(Size, 0.5f));
        }

        base.Violence(source, attackForce, hitChunk, hitAppendage, type, damage, stunBonus);

    }
    public virtual void BreakShell(int chunkIndex, Vector2 breakVel)
    {
        SecretCreaturaState.shells[chunkIndex] = false;
        if (room is not null &&
            graphicsModule is not null)
        {
            for (int s = 0; s < bodyChunks.Length; s++)
            {
                Vector2 vel = breakVel + (Custom.RNV() * Random.value * (s % 2 == 0 ? 0.5f : 1f) * breakVel.magnitude);
                float shardSize = Size + Random.Range(0.2f, 0.4f);
                float shardVol = 0.8f + (Size * 0.5f);
                float shardPitch = 1.2f - (Size * 0.5f);
                SecretCreaturaShard shellShard = new(bodyChunks[chunkIndex].pos, vel, shardSize, shardVol, shardPitch, ShellColor);
                room.AddObject(shellShard);
            }
        }
        room.PlaySound(SoundID.Red_Centipede_Shield_Falloff, bodyChunks[chunkIndex]);
    }

    public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        base.Collide(otherObject, myChunk, otherChunk);

        if (!Consious ||
            safariControlled)
        {
            return;
        }

        AI.AnnoyingCollision(otherObject.abstractPhysicalObject);

        if (otherObject is Creature target)
        {
            AI.tracker.SeeCreature(target.abstractCreature);
        }

        if (AI.DoIWantToGrabObject(otherObject.abstractPhysicalObject) &&
            myChunk == 0 &&
            Hold is null)
        {
            bool grab = true;
            for (int b = 0; b < grabbedBy.Count && grab; b++)
            {
                if (grabbedBy[b].grabber == otherObject)
                {
                    grab = false;
                }
            }
            if (shockGiveUpCounter > 0)
            {
                grab = false;
            }
            if (grab)
            {
                room.PlaySound(SoundID.Centipede_Attach, bodyChunks[myChunk]);
                Grab(otherObject, 0, otherChunk, Grasp.Shareability.NonExclusive, 1f, overrideEquallyDominant: false, pacifying: false);
            }
        }

        if (otherObject is SecretCreatura sc &&
            sc.Size > Size)
        {
            AI.CheckRandomIdlePos();
        }
    }
    public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
    {
        base.TerrainImpact(chunk, direction, speed, firstContact);
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public override Color ShortCutColor() => ShellColor;

}
