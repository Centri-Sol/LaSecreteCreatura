namespace SecretCreaturas;

public class SecretCreaturaGraphics : GraphicsModule
{
    public SecretCreatura Creatura => owner as SecretCreatura;
    public SecretCreaturaAI CreaturaAI => Creatura?.AI;

    public virtual int TubeSprite => 0;
    public virtual int FirstSegmentSprite => TubeSprite + 1;
    public virtual int FirstShellSprite => FirstSegmentSprite + SegmentCount;

    public virtual int FirstSpikeSprite => FirstShellSprite + SegmentCount * ShellSpriteLayers;
    public virtual int FirstLegSprite => FirstSpikeSprite + spikes * SpikeSpriteLayers;
    public virtual int FirstSecondarySegmentSprite => FirstLegSprite + legs.Length * LegSpriteLayers;

    public virtual int FirstAntennaSprite => FirstSecondarySegmentSprite + SecondarySegmentCount;
    public virtual int TotalSprites => FirstAntennaSprite + antennae.Length;


    public virtual int SegmentCount => Creatura.bodyChunks.Length;
    public virtual int SecondarySegmentCount => SegmentCount - 1;
    public virtual int ShellSpriteLayers => 2;
    public virtual int SpikeSpriteLayers => 2;
    public virtual int LegSpriteLayers => 2;

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual Color TopShellColor => Creatura.ShellColor;
    public Color BottomShellColor;
    public Color BodyColor;
    public Color PaletteBlack;

    public float[] darknesses;
    public float[] lastDarknesses;

    public Limb[,] legs;
    public float[] legLengths;

    public GenericBodyPart[] antennae;

    public int spikes;

    public float walkCycle;

    public float defaultRotat;
    public Vector2[,] bodyRotations;

    public ChunkDynamicSoundLoop soundLoop;

    public SecretCreaturaGraphics(PhysicalObject owner) : base(owner, false)
    {
        cullRange = 200f + Creatura.Size * 200f;

        Random.State state = Random.state;
        Random.InitState(Creatura.abstractCreature.ID.RandomSeed);
        defaultRotat = Mathf.Lerp(-5, 5, Random.value);

        bodyRotations = new Vector2[3, 2];
        for (int i = 0; i < bodyRotations.GetLength(0); i++)
        {
            bodyRotations[i, 0] = Custom.DegToVec(defaultRotat);
            bodyRotations[i, 1] = Custom.DegToVec(defaultRotat);
        }

        bodyParts = new BodyPart[SegmentCount * 2 + 8];
        int bodyPartCount = 0;

        legs = new Limb[SegmentCount, 2];
        legLengths = new float[SegmentCount];
        for (int s = 0; s < SegmentCount; s++)
        {
            float segmentProgression = s / (float)(SegmentCount - 1);
            legLengths[s] = Mathf.Lerp(10, 25, Mathf.Sin(segmentProgression * (float)Math.PI));
            legLengths[s] *= Mathf.Lerp(0.5f, 1.5f, Creatura.Size);
            for (int side = 0; side < legs.GetLength(1); side++)
            {
                legs[s, side] = new Limb(this, Creatura.bodyChunks[s], s * 2 + side, 2f, 0.5f, 0.9f, 7f, 0.8f);
                bodyParts[bodyPartCount] = legs[s, side];
                bodyPartCount++;
            }
        }

        antennae = new GenericBodyPart[2];
        for (int side = 0; side < antennae.Length; side++)
        {
            antennae[side] = new GenericBodyPart(this, 1f, 0.5f, 0.9f, Creatura.Head);
            bodyParts[bodyPartCount] = antennae[side];
            bodyPartCount++;
        }

        if (Creatura.Secretest)
        {
            spikes = SegmentCount;
        }

        darknesses = new float[1 + SegmentCount];

        soundLoop = new ChunkDynamicSoundLoop(Creatura.mainBodyChunk);
        Random.state = state;
    }

    //--------------------------------------------------------------------------------

    public override void Update()
    {
        base.Update();

        SoundloopUpdate();

        MovementCycleUpdate();

        BodyRotationsUpdate();

        LegsUpdate();

        WhiskersUpdate();

        lastCulled = culled;
        culled = ShouldBeCulled;
        if (!culled && lastCulled)
        {
            Reset();
        }
    }

    public virtual void SoundloopUpdate()
    {
        soundLoop.Update();
        if (Creatura.curlUp > 0f)
        {
            //soundLoop.sound = SoundID.Centipede_Electric_Charge_LOOP;
            //soundLoop.Volume = Mathf.InverseLerp(0f, 0.1f, centipede.shockCharge);
            //soundLoop.Pitch = Mathf.Lerp(0.5f, 1.5f, centipede.shockCharge);
        }
        else if (Creatura.dead)
        {
            soundLoop.Volume = 0f;
        }
        else
        {
            soundLoop.sound = SoundID.Centipede_Crawl_LOOP;
            soundLoop.Volume = (!Creatura.moving ? 0 : (Mathf.InverseLerp(Vector2.Distance(Creatura.mainBodyChunk.lastPos, Creatura.mainBodyChunk.pos), 0.5f, 2) * Mathf.Lerp(0.5f, 1, Creatura.Size)));
            soundLoop.Pitch = Mathf.Lerp(1.2f, 0.8f, Creatura.Size);
        }
    }
    public virtual void MovementCycleUpdate()
    {
        if (Creatura.moving &&
            Creatura.Consious)
        {
            walkCycle -= 0.1f;
        }
    }
    public virtual void BodyRotationsUpdate()
    {
        for (int i = 0; i < bodyRotations.GetLength(0); i++)
        {
            bodyRotations[i, 1] = bodyRotations[i, 0];
            int chunkIndex = i switch
            {
                0 => 0,
                1 => Creatura.bodyChunks.Length / 2,
                _ => Creatura.bodyChunks.Length - 1,
            };
            bodyRotations[i, 0] = Vector3.Slerp(bodyRotations[i, 0], BestBodyRotatAtChunk(chunkIndex), Creatura.moving ? 0.4f : 0.01f);
        }
    }
    public virtual void LegsUpdate()
    {
        Vector2 frontOfFirstChunk = Creatura.Head.pos + Custom.DirVec(Creatura.bodyChunks[1].pos, Creatura.Head.pos);

        for (int segment = 0; segment < SegmentCount; segment++)
        {
            float segmentProgression = segment / (float)(SegmentCount - 1);
            Vector2 chunkPos = Creatura.bodyChunks[segment].pos;
            Vector2 dirToChunkFromFirstChunk = Custom.DirVec(frontOfFirstChunk, chunkPos);
            Vector2 perpDir = Custom.PerpendicularVector(dirToChunkFromFirstChunk);
            Vector2 rotat = RotatAtChunk(segment, 1f);
            float legMoveFac = 0.5f + 0.5f * Mathf.Sin((walkCycle + segment / 10f) * (float)Math.PI * 2f);
            for (int side = 0; side < legs.GetLength(1); side++)
            {
                legs[segment, side].Update();

                Vector2 legAnchor = Creatura.bodyChunks[segment].pos + perpDir * (side == 0 ? -1f : 1f) * rotat.y * Creatura.bodyChunks[segment].rad;
                float legDirFac = Mathf.Lerp(-1, 1, segmentProgression);
                legDirFac = Mathf.Lerp(legDirFac, -1, Mathf.Abs(legMoveFac - 0.5f));
                Vector2 legDir = Vector3.Slerp(perpDir * (side == 0 ? -1f : 1f) * rotat.y, perpDir * rotat.x, Mathf.Abs(rotat.x));
                legDir = Vector3.Slerp(dirToChunkFromFirstChunk * legDirFac, legDir, Mathf.Lerp(0.5f + 0.5f * Mathf.Sin(segmentProgression * (float)Math.PI), 0f, Mathf.Abs(legMoveFac - 0.5f) * 2f)).normalized;

                Vector2 legGoalPos = legAnchor + legDir * legLengths[segment];

                legs[segment, side].ConnectToPoint(legAnchor, legLengths[segment], push: false, 0f, Creatura.bodyChunks[segment].vel, 0.1f, 0f);

                if (Creatura.Consious &&
                    !legs[segment, side].reachedSnapPosition)
                {
                    legs[segment, side].FindGrip(Creatura.room, legAnchor, legAnchor, legLengths[segment] * 1.5f, legGoalPos, -2, -2, behindWalls: true);
                }

                if (!Creatura.Consious ||
                    !Custom.DistLess(
                        legs[segment, side].pos,
                        legs[segment, side].absoluteHuntPos,
                        legLengths[segment] * 1.5f))
                {
                    legs[segment, side].mode = Limb.Mode.Dangle;
                    legs[segment, side].vel += legDir * 13f;
                    legs[segment, side].vel = Vector2.Lerp(legs[segment, side].vel, legGoalPos - legs[segment, side].pos, 0.5f);
                }
                else
                {
                    legs[segment, side].vel += legDir * 5f;
                }
            }
        }
    }
    public virtual void WhiskersUpdate()
    {
        for (int side = 0; side < antennae.GetLength(0); side++)
        {
            Vector2 HeadPos = Creatura.Head.pos;
            antennae[side].Update();
            antennae[side].ConnectToPoint(HeadPos, WhiskerLength(side), push: false, 0f, new Vector2(0f, 0f), 0f, 0f);
            antennae[side].vel += (HeadPos + WhiskerDir(side, 1f) * WhiskerLength(side) - antennae[side].pos) / 30f;
            antennae[side].vel += WhiskerDir(side, 1f);
            antennae[side].vel.y -= 0.3f;

            if (Creatura.Consious &&
                !Creatura.moving)
            {
                antennae[side].pos += Custom.RNV() * Mathf.Lerp(0.5f, 1.5f, Creatura.Size) * 2f;
            }
        }
    }

    public virtual Vector2 BestBodyRotatAtChunk(int chunk)
    {
        Vector2 v =
            chunk == 0 ? Custom.DirVec(Creatura.bodyChunks[0].pos, Creatura.bodyChunks[1].pos) :
            chunk == Creatura.bodyChunks.Length - 1 ? Custom.DirVec(Creatura.bodyChunks[Creatura.bodyChunks.Length - 2].pos, Creatura.bodyChunks[Creatura.bodyChunks.Length - 1].pos) :
            Custom.DirVec(Creatura.bodyChunks[Creatura.bodyChunks.Length / 2 - 1].pos, Creatura.bodyChunks[Creatura.bodyChunks.Length / 2 + 1].pos);

        v = Custom.PerpendicularVector(v);
        float rotatFac = 0f;
        if (Creatura.room.GetTile(Creatura.bodyChunks[chunk].pos + v * 20f).Solid)
        {
            rotatFac += 1f;
        }
        if (Creatura.room.GetTile(Creatura.bodyChunks[chunk].pos - v * 20f).Solid)
        {
            rotatFac -= 1f;
        }
        Vector2 ChunkRotation;
        if (rotatFac == 0 &&
            Creatura.room.GetTile(Creatura.bodyChunks[chunk].pos).AnyBeam)
        {
            ChunkRotation = new Vector2(defaultRotat * 0.01f, -1f);
            return ChunkRotation.normalized;
        }
        ChunkRotation = new Vector2(-1f, 0.15f);
        Vector3 FirstChunkRot = ChunkRotation.normalized;
        ChunkRotation = new Vector2(1f, 0.15f);
        return Vector3.Slerp(FirstChunkRot, ChunkRotation.normalized, Mathf.InverseLerp(-1, 1, rotatFac + defaultRotat * 0.01f));
    }
    public virtual Vector2 RotatAtChunk(int chunk, float timeStacker)
    {
        if (chunk <= Creatura.bodyChunks.Length / 2)
        {
            return Vector3.Slerp(
                Vector3.Slerp(bodyRotations[0, 1], bodyRotations[0, 0], timeStacker),
                Vector3.Slerp(bodyRotations[1, 1], bodyRotations[1, 0], timeStacker),
                Mathf.InverseLerp(0, Creatura.bodyChunks.Length / 2f, chunk));
        }
        return Vector3.Slerp(
            Vector3.Slerp(bodyRotations[1, 1], bodyRotations[1, 0], timeStacker),
            Vector3.Slerp(bodyRotations[2, 1], bodyRotations[2, 0], timeStacker),
            Mathf.InverseLerp(Creatura.bodyChunks.Length / 2f, Creatura.bodyChunks.Length - 1, chunk));
    }
    public virtual Vector2 WhiskerDir(int side, float timeStacker)
    {
        Vector2 headDir = Custom.DirVec(
            Vector2.Lerp(Creatura.bodyChunks[1].lastPos, Creatura.bodyChunks[1].pos, timeStacker),
            Vector2.Lerp(Creatura.Head.lastPos, Creatura.Head.pos, timeStacker));

        Vector2 headRotation = RotatAtChunk(0, timeStacker);
        Vector2 perpHeadDir = Custom.PerpendicularVector(headDir) * -1f;
        Vector2 whiskerDir = Vector3.Slerp(
            perpHeadDir * (side == 0 ? -1f : 1f) * headRotation.y,
            perpHeadDir * headRotation.x * 0.25f,
            Mathf.Abs(headRotation.x));
        whiskerDir += headDir;
        return whiskerDir.normalized;
    }
    public virtual float WhiskerLength(int side)
    {
        float length = 16f * Mathf.Pow(Mathf.Lerp(1, 1.5f, Creatura.Size), 1 + (Creatura.Size * 0.6f));
        return length;
    }

    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    public virtual int ShellSprite(int segment, int layer)
    {
        return FirstShellSprite + (SegmentCount * layer) + segment;
    }
    public virtual int SpikeSprite(int segment, int layer)
    {
        return FirstSpikeSprite + (SegmentCount * layer) + segment;
    }
    public virtual int LegSprite(int segment, int side, int layer)
    {
        return FirstLegSprite + segment * legs.GetLength(1) * LegSpriteLayers + (side * legs.GetLength(1)) + layer;
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[TotalSprites];

        sLeaser.sprites[TubeSprite] = TriangleMesh.MakeLongMesh(Creatura.bodyChunks.Length, pointyTip: false, customColor: false);

        for (int s = 0; s < SegmentCount; s++)
        {
            sLeaser.sprites[FirstSegmentSprite + s] = new FSprite("CentipedeSegment")
            {
                scaleY = Creatura.bodyChunks[s].rad * 1.8f * (1 / 12f)
            };
            
            for (int l = 0; l < ShellSpriteLayers; l++)
            {
                sLeaser.sprites[ShellSprite(s, l)] = new FSprite("pixel");
                if (l == 1)
                {
                    sLeaser.sprites[ShellSprite(s, l)].shader = rCam.room.game.rainWorld.Shaders["AquapedeBody"];
                    sLeaser.sprites[ShellSprite(s, l)].alpha = Random.value;
                }
            }

            for (int l = 0; l < legs.GetLength(1); l++)
            {
                sLeaser.sprites[LegSprite(s, l, 0)] = new FSprite("CentipedeLegA");
                sLeaser.sprites[LegSprite(s, l, 1)] = new VertexColorSprite("CentipedeLegB");
                sLeaser.sprites[LegSprite(s, l, 0)].anchorY = 0.1f;
                sLeaser.sprites[LegSprite(s, l, 1)].anchorY = 0.1f;
            }

            if (s >= SecondarySegmentCount) continue;

            sLeaser.sprites[FirstSecondarySegmentSprite + s] = new FSprite("pixel");
            sLeaser.sprites[FirstSegmentSprite + s].scaleY = Mathf.Lerp(Creatura.bodyChunks[s].rad, Creatura.bodyChunks[s + 1].rad, 0.5f);
        }

        for (int w = FirstAntennaSprite; w < FirstAntennaSprite + antennae.GetLength(0); w++)
        {
            sLeaser.sprites[w] = TriangleMesh.MakeLongMesh(4, pointyTip: true, customColor: false);
        }

        for (int s = FirstSpikeSprite; s < FirstSpikeSprite + spikes; s++)
        {
            sLeaser.sprites[SpikeSprite(s, 1)] = new FSprite("CentipedeSegment");
            sLeaser.sprites[SpikeSprite(s, 0)] = new FSprite("Cicada8body");
            sLeaser.sprites[SpikeSprite(s, 0)].anchorY = 0.55f;
        }

        AddToContainer(sLeaser, rCam, null);
        base.InitiateSprites(sLeaser, rCam);
    }

    public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        PaletteBlack = palette.blackColor;
        BottomShellColor = Color.Lerp(TopShellColor, PaletteBlack, 1/3f);
        BodyColor = Color.Lerp(TopShellColor, BottomShellColor, 0.5f);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        newContatiner ??= rCam.ReturnFContainer("Midground");

        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            newContatiner.AddChild(sLeaser.sprites[i]);
        }
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

        if (culled) return;

        lastDarknesses = darknesses;
        AntennaeDarkness(sLeaser, rCam, timeStacker);


        Vector2 lastSegDir = default;
        Vector2 HeadPos = Vector2.Lerp(Creatura.Head.lastPos, Creatura.Head.pos, timeStacker);
        HeadPos += Custom.DirVec(Vector2.Lerp(Creatura.bodyChunks[1].lastPos, Creatura.bodyChunks[1].pos, timeStacker), HeadPos) * 10f;
        Vector2 segRotation;

        for (int s = 0; s < SegmentCount; s++)
        {
            darknesses[s] = rCam.room.Darkness(Vector2.Lerp(Creatura.bodyChunks[s].lastPos, Creatura.bodyChunks[s].pos, timeStacker));
            darknesses[s] *= 1 - 0.5f * rCam.room.LightSourceExposure(Vector2.Lerp(Creatura.bodyChunks[s].lastPos, Creatura.bodyChunks[s].pos, timeStacker));

            for (int l = 0; l < ShellSpriteLayers; l++)
            {
                sLeaser.sprites[ShellSprite(s, l)].isVisible = Creatura.SecretCreaturaState.shells[s];
            }
            for (int l = 0; l < SpikeSpriteLayers; l++)
            {
                sLeaser.sprites[SpikeSprite(s, l)].isVisible = Creatura.SecretCreaturaState.shells[s];
            }
            float segmentProgress = s / (float)(SegmentCount - 1);
            segRotation = RotatAtChunk(s, timeStacker);
            Vector2 segRotNormalized = segRotation.normalized;
            Vector2 segPos = Vector2.Lerp(Creatura.bodyChunks[s].lastPos, Creatura.bodyChunks[s].pos, timeStacker);
            Vector2 nextSegPos = (s < SegmentCount - 1) ?
                Vector2.Lerp(Creatura.bodyChunks[s + 1].lastPos, Creatura.bodyChunks[s + 1].pos, timeStacker) :
                (segPos + Custom.DirVec(HeadPos, segPos) * 10f);
            segRotation = HeadPos - nextSegPos;
            Vector2 segDir = segRotation.normalized;
            Vector2 perpSegDir = Custom.PerpendicularVector(segDir);

            float tubeFrontLength = Vector2.Distance(segPos, HeadPos) / 4f;
            float tubeBackLength = Vector2.Distance(segPos, nextSegPos) / 4f;
            float tubeRightRad = 3f;
            float tubeLeftRad = 3f;
            if (s == 0 || Creatura.Secreter)
            {
                tubeRightRad = 1f;
            }
            else
            if (s == Creatura.Butt.index || Creatura.Secreter)
            {
                tubeLeftRad = 1f;
            }
            sLeaser.sprites[TubeSprite].color = Color.Lerp(BottomShellColor, PaletteBlack, darknesses[s]);
            TriangleMesh Tube = sLeaser.sprites[TubeSprite] as TriangleMesh;
            Tube.MoveVertice(s * 4, segPos - perpSegDir * tubeRightRad + segDir * tubeFrontLength - camPos);
            Tube.MoveVertice(s * 4 + 1, segPos + perpSegDir * tubeRightRad + segDir * tubeFrontLength - camPos);
            Tube.MoveVertice(s * 4 + 2, segPos - perpSegDir * tubeLeftRad - segDir * tubeBackLength - camPos);
            Tube.MoveVertice(s * 4 + 3, segPos + perpSegDir * tubeLeftRad - segDir * tubeBackLength - camPos);

            float segmentSize = Mathf.Clamp(Mathf.Sin(segmentProgress * (float)Math.PI), 0, 1);
            segmentSize *= Mathf.Lerp(0.9f, 0.3f, Creatura.Size);
            sLeaser.sprites[FirstSegmentSprite + s].x = segPos.x - camPos.x;
            sLeaser.sprites[FirstSegmentSprite + s].y = segPos.y - camPos.y;
            segRotation = HeadPos - nextSegPos;
            sLeaser.sprites[FirstSegmentSprite + s].rotation = Custom.VecToDeg(segRotation.normalized);
            sLeaser.sprites[FirstSegmentSprite + s].scaleX = Creatura.bodyChunks[s].rad * Mathf.Lerp(1, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(segRotNormalized.x)), segmentSize) * 2f * 0.0625f;
            sLeaser.sprites[FirstSegmentSprite + s].color = Color.Lerp(BodyColor, PaletteBlack, darknesses[s]);

            for (int l = 0; l < ShellSpriteLayers; l++)
            {
                if (segRotNormalized.y > 0f)
                {
                    sLeaser.sprites[ShellSprite(s, l)].scaleX = Creatura.bodyChunks[s].rad * Mathf.Lerp(1, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(segRotNormalized.x)), segmentSize) * 1.8f * segRotNormalized.y * (1/14f);
                    sLeaser.sprites[ShellSprite(s, l)].scaleY = Creatura.bodyChunks[s].rad * 1.7f * (1/11f);
                    float num7 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(segDir, Custom.DegToVec(30f) * segRotNormalized.x));
                    num7 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - segRotNormalized.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - segRotNormalized.x)));
                    num7 *= Mathf.Pow(1f - darknesses[s], 2f);
                    sLeaser.sprites[ShellSprite(s, l)].color = (l == 0) ?
                        Color.Lerp(TopShellColor, PaletteBlack, darknesses[s]) :
                        Color.Lerp(BottomShellColor, PaletteBlack, darknesses[s]);
                    sLeaser.sprites[ShellSprite(s, l)].element = Futile.atlasManager.GetElementWithName("CentipedeBackShell");
                    sLeaser.sprites[ShellSprite(s, l)].x = (segPos + Custom.PerpendicularVector(segDir) * segRotNormalized.x * Creatura.bodyChunks[s].rad * 1.1f).x - camPos.x;
                    sLeaser.sprites[ShellSprite(s, l)].y = (segPos + Custom.PerpendicularVector(segDir) * segRotNormalized.x * Creatura.bodyChunks[s].rad * 1.1f).y - camPos.y;
                }
                else
                {
                    sLeaser.sprites[ShellSprite(s, l)].scaleX = Creatura.bodyChunks[s].rad * -1.8f * segRotNormalized.y * (1f / 14f);
                    sLeaser.sprites[ShellSprite(s, l)].scaleY = Creatura.bodyChunks[s].rad * 1.3f * (1f / 11f);
                    sLeaser.sprites[ShellSprite(s, l)].color = (l == 0) ?
                        Color.Lerp(BottomShellColor, PaletteBlack, darknesses[s]) :
                        Color.Lerp(BottomShellColor, PaletteBlack, 0.5f + (darknesses[s] * 0.5f));
                    sLeaser.sprites[ShellSprite(s, l)].element = Futile.atlasManager.GetElementWithName("CentipedeBellyShell");
                    sLeaser.sprites[ShellSprite(s, l)].x = (segPos - Custom.PerpendicularVector(segDir) * segRotNormalized.x * Creatura.bodyChunks[s].rad).x - camPos.x;
                    sLeaser.sprites[ShellSprite(s, l)].y = (segPos - Custom.PerpendicularVector(segDir) * segRotNormalized.x * Creatura.bodyChunks[s].rad).y - camPos.y;
                }
                segRotation = HeadPos - nextSegPos;
                sLeaser.sprites[ShellSprite(s, l)].rotation = Custom.VecToDeg(segRotation.normalized);
            }

            if (s > 0)
            {
                sLeaser.sprites[FirstSecondarySegmentSprite + s - 1].x = Mathf.Lerp(HeadPos.x, segPos.x, 0.5f) - camPos.x;
                sLeaser.sprites[FirstSecondarySegmentSprite + s - 1].y = Mathf.Lerp(HeadPos.y, segPos.y, 0.5f) - camPos.y;
                sLeaser.sprites[FirstSecondarySegmentSprite + s - 1].rotation = Custom.VecToDeg(Vector3.Slerp(lastSegDir, segDir, 0.5f));
                sLeaser.sprites[FirstSecondarySegmentSprite + s - 1].scaleX = Creatura.bodyChunks[s].rad * Mathf.Lerp(0.9f, Mathf.Lerp(1.1f, 0.8f, Mathf.Abs(segRotNormalized.x)), segmentSize) * 2f;
                sLeaser.sprites[FirstSecondarySegmentSprite + s - 1].color = Color.Lerp(TopShellColor, BodyColor, Mathf.Lerp(0.4f, 1, Mathf.Lerp(darknesses[s - 1], darknesses[s], 0.5f)));
            }


            Vector2 spikeRotation = Custom.DegToVec(Custom.VecToDeg(segRotNormalized) + (segRotNormalized.x > 0f ? -90f : 90f));
            if (Creatura.SecretCreaturaState.shells[s])
            {
                float finalSegRot = Mathf.Pow(Mathf.Abs(segRotNormalized.x), 0.65f) * Mathf.Sign(segRotNormalized.x);
                sLeaser.sprites[SpikeSprite(s, 0)].x = (segPos + Custom.PerpendicularVector(segDir) * finalSegRot * Creatura.bodyChunks[s].rad * 1.1f).x - camPos.x;
                sLeaser.sprites[SpikeSprite(s, 0)].y = (segPos + Custom.PerpendicularVector(segDir) * finalSegRot * Creatura.bodyChunks[s].rad * 1.1f).y - camPos.y;
                sLeaser.sprites[SpikeSprite(s, 0)].rotation = Custom.VecToDeg(Vector3.Slerp(
                    (segmentProgress < 0.5f) ? segDir : -segDir,
                    Custom.PerpendicularVector(segDir) * Mathf.Sign(finalSegRot),
                    0.3f + 0.7f * Mathf.Sin(segmentProgress * (float)Math.PI)));
                float darknessFac1 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(segDir, Custom.DegToVec(30f) * segRotNormalized.x));
                darknessFac1 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - segRotNormalized.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - segRotNormalized.x)));
                darknessFac1 *= Mathf.Pow(1 - darknesses[s], 2);
                sLeaser.sprites[SpikeSprite(s, 0)].color = Color.Lerp(TopShellColor, PaletteBlack, 0.3f + 0.7f * darknesses[s] * (1 - darknessFac1));
                sLeaser.sprites[SpikeSprite(s, 0)].scaleX = Mathf.Lerp(0.15f, 0.25f, Mathf.Sin(segmentProgress * (float)Math.PI));
                sLeaser.sprites[SpikeSprite(s, 0)].scaleY = Mathf.Abs(finalSegRot) * Mathf.Lerp(-0.25f, -0.6f, Mathf.Sin(segmentProgress * (float)Math.PI));


                sLeaser.sprites[SpikeSprite(s, 1)].x = (segPos + Custom.PerpendicularVector(segDir) * spikeRotation.x * Creatura.bodyChunks[s].rad * 1.2f).x - camPos.x;
                sLeaser.sprites[SpikeSprite(s, 1)].y = (segPos + Custom.PerpendicularVector(segDir) * spikeRotation.x * Creatura.bodyChunks[s].rad * 1.2f).y - camPos.y;
                segRotation = HeadPos - nextSegPos;
                sLeaser.sprites[SpikeSprite(s, 1)].rotation = Custom.VecToDeg(segRotation.normalized);
                float darknessFac2 = Mathf.InverseLerp(-0.5f, 0.5f, Vector3.Dot(segDir, Custom.DegToVec(30f) * spikeRotation.x));
                darknessFac2 *= Mathf.Max(Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(-0.5f - spikeRotation.x)), Mathf.InverseLerp(0.3f, 0.05f, Mathf.Abs(0.5f - spikeRotation.x)));
                darknessFac2 *= Mathf.Pow(1 - darknesses[s], 2);
                sLeaser.sprites[SpikeSprite(s, 1)].color = Color.Lerp(TopShellColor, PaletteBlack, 0.3f + 0.7f * darknesses[s] * (1 - darknessFac2));
                sLeaser.sprites[SpikeSprite(s, 1)].scaleX = Creatura.bodyChunks[s].rad * Mathf.Lerp(1, Mathf.Lerp(1.5f, 0.9f, Mathf.Abs(spikeRotation.x)), segmentSize) * 0.7f * spikeRotation.y * (1 / 14f);
                sLeaser.sprites[SpikeSprite(s, 1)].scaleY = Creatura.bodyChunks[s].rad * 1.2f * (1/11f);

            }


            HeadPos = segPos;
            lastSegDir = segDir;
            for (int l = 0; l < legs.GetLength(1); l++)
            {
                Vector2 legAnchorPos = segPos - perpSegDir * (l == 0 ? -1f : 1f) * segRotNormalized.y * Creatura.bodyChunks[s].rad;
                Vector2 legEndPos = Vector2.Lerp(legs[s, l].lastPos, legs[s, l].pos, timeStacker);
                float igiveupihavenoideawhatmostoftheseare =
                    Mathf.Lerp(-1, 1, Mathf.Clamp(segmentProgress + 0.4f, 0, 1)) *
                    Mathf.Lerp(l == 0 ? 1 : -1, 0 - segRotNormalized.x, Mathf.Abs(segRotNormalized.x));
                igiveupihavenoideawhatmostoftheseare = Mathf.Pow(Mathf.Abs(igiveupihavenoideawhatmostoftheseare), 0.2f) * Mathf.Sign(igiveupihavenoideawhatmostoftheseare);
                Vector2 legOverlayAnchorPos = Custom.InverseKinematic(legAnchorPos, legEndPos, legLengths[s] / 2f, legLengths[s] / 2f, igiveupihavenoideawhatmostoftheseare);
                sLeaser.sprites[LegSprite(s, l, 0)].x = legAnchorPos.x - camPos.x;
                sLeaser.sprites[LegSprite(s, l, 0)].y = legAnchorPos.y - camPos.y;
                sLeaser.sprites[LegSprite(s, l, 0)].rotation = Custom.AimFromOneVectorToAnother(legAnchorPos, legOverlayAnchorPos);
                sLeaser.sprites[LegSprite(s, l, 0)].scaleY = Vector2.Distance(legAnchorPos, legOverlayAnchorPos) / 27f;

                sLeaser.sprites[LegSprite(s, l, 1)].x = legOverlayAnchorPos.x - camPos.x;
                sLeaser.sprites[LegSprite(s, l, 1)].y = legOverlayAnchorPos.y - camPos.y;
                sLeaser.sprites[LegSprite(s, l, 1)].rotation = Custom.AimFromOneVectorToAnother(legOverlayAnchorPos, legEndPos);
                sLeaser.sprites[LegSprite(s, l, 1)].scaleY = Vector2.Distance(legOverlayAnchorPos, legEndPos) / 25f;

                sLeaser.sprites[LegSprite(s, l, 1)].scaleX = sLeaser.sprites[LegSprite(s, l, 0)].scaleX = -Mathf.Sign(igiveupihavenoideawhatmostoftheseare) * 1.2f;

                VertexColorSprite LegOverlay = sLeaser.sprites[LegSprite(s, l, 1)] as VertexColorSprite;
                LegOverlay.verticeColors[0] = BottomShellColor;
                LegOverlay.verticeColors[1] = BottomShellColor;
                LegOverlay.verticeColors[2] = BodyColor;
                LegOverlay.verticeColors[3] = BodyColor;
            }
        }

        Vector2 head = Vector2.Lerp(Creatura.Head.lastPos, Creatura.Head.pos, timeStacker);
        Vector2 dirToHead = Custom.DirVec(Vector2.Lerp(Creatura.bodyChunks[1].lastPos, Creatura.bodyChunks[1].pos, timeStacker), head);
        for (int a = 0 ; a < FirstAntennaSprite + antennae.Length; a++)
        {
            Vector2 antennaEndPos = Vector2.Lerp(antennae[a].lastPos, antennae[a].pos, timeStacker);
            HeadPos = head;
            float antennaThickness = Mathf.Lerp(0.6f, 1f, Creatura.Size);

            sLeaser.sprites[a].color = BottomShellColor;
            TriangleMesh Antenna = sLeaser.sprites[a] as TriangleMesh;
            for (int v = 0; v < 4; v++)
            {
                Vector2 PosInAntennaBezier = Custom.Bezier(head, head + dirToHead * Vector2.Distance(head, antennaEndPos) * 0.7f, antennaEndPos, antennaEndPos, v / 3f);
                segRotation = PosInAntennaBezier - HeadPos;
                Vector2 dirToAntennaVertice = segRotation.normalized;
                Vector2 perpDirToVertice = Custom.PerpendicularVector(dirToAntennaVertice);
                float distToVertice = Vector2.Distance(PosInAntennaBezier, HeadPos) / (v == 0 ? 1f : 5f);

                Antenna.MoveVertice(v * 4, HeadPos - perpDirToVertice * antennaThickness + dirToAntennaVertice * distToVertice - camPos);
                Antenna.MoveVertice(v * 4 + 1, HeadPos + perpDirToVertice * antennaThickness + dirToAntennaVertice * distToVertice - camPos);
                if (v < 3)
                {
                    Antenna.MoveVertice(v * 4 + 2, PosInAntennaBezier - perpDirToVertice * antennaThickness - dirToAntennaVertice * distToVertice - camPos);
                    Antenna.MoveVertice(v * 4 + 3, PosInAntennaBezier + perpDirToVertice * antennaThickness - dirToAntennaVertice * distToVertice - camPos);
                }
                else
                {
                    Antenna.MoveVertice(v * 4 + 2, PosInAntennaBezier + dirToAntennaVertice * 2.1f - camPos);
                }

                HeadPos = PosInAntennaBezier;
            }
        }

    }
    public virtual void AntennaeDarkness(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker)
    {
        Vector2 dirToHead = Custom.DirVec(Creatura.bodyChunks[1].pos, Creatura.Head.pos);
        Vector2 antennaPos = Creatura.Head.pos + (dirToHead * Creatura.Head.rad * 1.5f);
        dirToHead = Custom.DirVec(Creatura.bodyChunks[1].lastPos, Creatura.Head.lastPos);
        dirToHead = Creatura.Head.lastPos + (dirToHead * Creatura.Head.rad * 1.5f);
        darknesses[SegmentCount] = rCam.room.Darkness(Vector2.Lerp(dirToHead, antennaPos, timeStacker));
        darknesses[SegmentCount] *= 1 - 0.5f * rCam.room.LightSourceExposure(Vector2.Lerp(dirToHead, antennaPos, timeStacker));
    }

}
