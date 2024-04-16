namespace SecretCreaturas;

[BepInPlugin(MOD_ID, "Secret Creaturas", "1.0.0")]

//--------------------------------------------------------------------------------

public class Plugin : BaseUnityPlugin
{
    private const string MOD_ID = "bry.secret-creaturas";

    public bool IsInit;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += InitiateSecretCreaturas;
        On.RainWorld.PostModsInit += ReorderUnlocks;
        On.RainWorld.OnModsDisabled += DisableCheck;
        On.RainWorld.UnloadResources += UnloadSprites;
    }

    // - - - - - - - - - - - - - - - - - - - - - -

    public void InitiateSecretCreaturas(On.RainWorld.orig_OnModsInit orig, RainWorld rw)
    {
        orig(rw);

        try
        {
            if (IsInit) return;
            IsInit = true;

            Futile.atlasManager.LoadAtlas("atlases/Kill_Icons");

            On.ArtificialIntelligence.VisualContact_BodyChunk += SecretCreaturaStillnessBasedVision;

            On.MoreSlugcats.SlugNPCAI.GetFoodType += SecretCreaturasForPups;

            Debug.LogWarning($"Secret Creaturas are on!");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogException(ex);
            throw;
        }

        Content.Register(
            new SecretCreaturaCritob(),
            new SecreterCreaturaCritob());
    }
    public void ReorderUnlocks(On.RainWorld.orig_PostModsInit orig, RainWorld rw)
    {
        orig(rw);
        OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID.JetFish, SCEnums.CreaturaUnlocks.SecreterCreatura);
        OrganizeUnlocks(SCEnums.CreaturaUnlocks.SecreterCreatura, SCEnums.CreaturaUnlocks.SecretCreatura);
        OrganizeUnlocks(SCEnums.CreaturaUnlocks.SecretCreatura, SCEnums.CreaturaUnlocks.SecretestCreatura);
    }
    public void DisableCheck(On.RainWorld.orig_OnModsDisabled orig, RainWorld rw, ModManager.Mod[] newlyDisabledMods)
    {
        orig(rw, newlyDisabledMods);
        for (int i = 0; i < newlyDisabledMods.Length; i++)
        {
            if (newlyDisabledMods[i].id == MOD_ID)
            {
                foreach (MultiplayerUnlocks.SandboxUnlockID unlock in SCEnums.CreaturaUnlocks.CreaturaUnlocksList)
                {
                    if (MultiplayerUnlocks.CreatureUnlockList.Contains(unlock))
                    {
                        MultiplayerUnlocks.CreatureUnlockList.Remove(unlock);
                    }
                }
                SCEnums.Unregister();
                break;
            }
        }
    }
    public void UnloadSprites(On.RainWorld.orig_UnloadResources orig, RainWorld rw)
    {
        orig(rw);
        if (Futile.atlasManager.DoesContainAtlas("Kill_Icons"))
        {
            Futile.atlasManager.UnloadAtlas("Kill_Icons");
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - -

    public void OrganizeUnlocks(MultiplayerUnlocks.SandboxUnlockID moveToBeforeThis, MultiplayerUnlocks.SandboxUnlockID unlockToMove)
    {
        if (MultiplayerUnlocks.CreatureUnlockList.Contains(unlockToMove) &&
            MultiplayerUnlocks.CreatureUnlockList.Contains(moveToBeforeThis))
        {
            MultiplayerUnlocks.CreatureUnlockList.Remove(unlockToMove);
            MultiplayerUnlocks.CreatureUnlockList.Insert(MultiplayerUnlocks.CreatureUnlockList.IndexOf(moveToBeforeThis), unlockToMove);
        }
    }

    // - - - - - - - - - - - - - - - - - - - - - -

    public static bool SecretCreaturaStillnessBasedVision(On.ArtificialIntelligence.orig_VisualContact_BodyChunk orig, ArtificialIntelligence ai, BodyChunk chunk)
    {
        if (ai?.creature?.realizedCreature is not null and SecretCreatura)
        {
            return ai.VisualContact(chunk.pos, -chunk.VisibilityBonus(ai.creature.creatureTemplate.movementBasedVision));
        }
        return orig(ai, chunk);
    }

    public SlugNPCAI.Food SecretCreaturasForPups(On.MoreSlugcats.SlugNPCAI.orig_GetFoodType orig, SlugNPCAI AI, PhysicalObject food)
    {
        if (ModManager.MSC &&
            food is SecreterCreatura)
        {
            return SlugNPCAI.Food.SmallCentipede;
        }
        return orig(AI, food);
    }

}

//--------------------------------------------------------------------------------