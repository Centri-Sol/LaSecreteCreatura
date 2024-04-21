namespace SecretCreaturas;

public class HookTests
{
    public static bool IncanInit;
    public static bool DrainInit;
    public static void Init()
    {
        if(ModManager.ActiveMods.Any((ModManager.Mod mod) => mod.id == "theincandescent"))
        {
            On.StaticWorld.EstablishRelationship += StaticWorld_EstablishRelationship;
        }
        if (ModManager.ActiveMods.Any((ModManager.Mod mod) => mod.id == "drainmites"))
        {
            On.StaticWorld.EstablishRelationship += StaticWorld_EstablishRelationship1;
        }
    }

    private static void StaticWorld_EstablishRelationship1(On.StaticWorld.orig_EstablishRelationship orig, CreatureTemplate.Type a, CreatureTemplate.Type b, CreatureTemplate.Relationship relationship)
    {
        if(!DrainInit)
        {
            //Secret creature Drainmites relationships
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, DMEnums.TemplateType.DrainMite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.5f));
            StaticWorld.EstablishRelationship(DMEnums.TemplateType.DrainMite, SCEnums.CreaturaTypes.SecretCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.5f));

            //Secreter 
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, DMEnums.TemplateType.DrainMite, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Uncomfortable, 0.5f));

            DrainInit = true;
        }
        else
        {
            orig(a, b, relationship);
        }
    }

    private static void StaticWorld_EstablishRelationship(On.StaticWorld.orig_EstablishRelationship orig, CreatureTemplate.Type a, CreatureTemplate.Type b, CreatureTemplate.Relationship relationship)
    {
        if(!IncanInit)
        {
            //Secret creature HailStorm relationships
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.SnowcuttleTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.SocialDependent, 0.05f));

            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.Raven, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Attacks, 1f));

            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.IcyBlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.FreezerLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.GorditoGreenieLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.Chillipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 1f));

            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecretCreatura, HSEnums.CreatureType.Cyanwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));

            StaticWorld.EstablishRelationship(HSEnums.CreatureType.Cyanwing, SCEnums.CreaturaTypes.SecretCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.Chillipede, SCEnums.CreaturaTypes.SecretCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));

            //Secreter
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.Raven, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.IcyBlueLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.FreezerLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.Chillipede, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.Cyanwing, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 1f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.GorditoGreenieLizard, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Afraid, 0.9f));

            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.SnowcuttleTemplate, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.7f));
            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.SnowcuttleFemale, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.AgressiveRival, 0.9f));

            StaticWorld.EstablishRelationship(SCEnums.CreaturaTypes.SecreterCreatura, HSEnums.CreatureType.PeachSpider, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Antagonizes, 0.7f));

            StaticWorld.EstablishRelationship(HSEnums.CreatureType.IcyBlueLizard, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.FreezerLizard, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.Cyanwing, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.Chillipede, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 1f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.Raven, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.45f));
            StaticWorld.EstablishRelationship(HSEnums.CreatureType.PeachSpider, SCEnums.CreaturaTypes.SecreterCreatura, new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Eats, 0.3f));

            IncanInit = true;
        }
        else
        {
            orig(a, b, relationship);
        }
    }
}