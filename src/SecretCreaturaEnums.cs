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
        //public static CreatureTemplate.Type SecretestCreatura = new(nameof(SecretestCreatura), true);
    }

    public static class CreaturaUnlocks
    {
        public static MultiplayerUnlocks.SandboxUnlockID SecretCreatura = new(nameof(SecretCreatura), true);
        public static MultiplayerUnlocks.SandboxUnlockID SecreterCreatura = new(nameof(SecreterCreatura), true);
        //public static MultiplayerUnlocks.SandboxUnlockID SecretestCreatura = new(nameof(SecretestCreatura), true);
    }
}