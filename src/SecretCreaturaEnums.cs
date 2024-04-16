namespace SecretCreaturas;

public static class SCEnums
{
    public static void Init()
    {
        RuntimeHelpers.RunClassConstructor(typeof(CreaturaTypes).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CreaturaUnlocks).TypeHandle);
    }
    public static void Unregister()
    {
        UnregisterEnums(typeof(CreaturaTypes));
        UnregisterEnums(typeof(CreaturaUnlocks));
        UnregisterEnums(typeof(ModdedCreatureTypes));
    }

    public static void UnregisterEnums(Type type)
    {
        IEnumerable<FieldInfo> extEnums = type.GetFields(Static | Public).Where(x => x.FieldType.IsSubclassOf(typeof(ExtEnumBase)));
        foreach ((FieldInfo extEnum, object obj) in from extEnum in extEnums
                                                    let obj = extEnum.GetValue(null)
                                                    where obj is not null
                                                    select (extEnum, obj))
        {
            obj.GetType().GetMethod("Unregister")!.Invoke(obj, null);
            extEnum.SetValue(null, null);
        }
    }

    public static class CreaturaTypes
    {
        public static CreatureTemplate.Type SecretCreatura = new(nameof(SecretCreatura), true);
        public static CreatureTemplate.Type SecreterCreatura = new(nameof(SecreterCreatura), true);
        public static CreatureTemplate.Type SecretestCreatura = new(nameof(SecretestCreatura), true);
    }

    public static class CreaturaUnlocks
    {
        public static MultiplayerUnlocks.SandboxUnlockID SecretCreatura = new(nameof(SecretCreatura), true);
        public static MultiplayerUnlocks.SandboxUnlockID SecreterCreatura = new(nameof(SecreterCreatura), true);
        public static MultiplayerUnlocks.SandboxUnlockID SecretestCreatura = new(nameof(SecretestCreatura), true);

        public static List<MultiplayerUnlocks.SandboxUnlockID> CreaturaUnlocksList = new()
        {
            SecretCreatura,
            SecreterCreatura,
            SecretestCreatura
        };
    }

    public static class ModdedCreatureTypes
    {
        public static CreatureTemplate.Type InfantAquapede = new("InfantAquapede");
        public static CreatureTemplate.Type Raven = new("Raven");
        public static CreatureTemplate.Type PeachSpider = new("PeachSpider");
        public static CreatureTemplate.Type IcyBlueLizard = new("IcyBlueLizard");
        public static CreatureTemplate.Type FreezerLizard = new("FreezerLizard");
        public static CreatureTemplate.Type SnowcuttleTemplate = new("SnowcuttleTemplate");
        public static CreatureTemplate.Type SnowcuttleFemale = new("SnowcuttleFemale");
        public static CreatureTemplate.Type SnowcuttleMale = new("SnowcuttleMale");
        public static CreatureTemplate.Type SnowcuttleLe = new("SnowcuttleLe");
        public static CreatureTemplate.Type Cyanwing = new("Cyanwing");
        public static CreatureTemplate.Type GorditoGreenieLizard = new("GorditoGreenieLizard");
        public static CreatureTemplate.Type Chillipede = new("Chillipede");
        public static CreatureTemplate.Type Luminescipede = new("Luminescipede");

        public static CreatureTemplate.Type DrainMite = new("DrainMite");
    }
}