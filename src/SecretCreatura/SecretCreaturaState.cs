using System.Text.RegularExpressions;

namespace SecretCreaturas;

public class SecretCreaturaState : HealthState
{
    public bool[] shells;

    public bool meatInitated;

    public SecretCreaturaState(AbstractCreature creature) : base(creature) { }

    public override string ToString()
    {
        bool flag = false;
        string text = HealthBaseSaveString();
        if (shells != null)
        {
            for (int i = 0; i < shells.Length; i++)
            {
                if (!shells[i])
                {
                    flag = true;
                }
            }
        }
        if (flag)
        {
            string text2 = "";
            for (int j = 0; j < shells.Length; j++)
            {
                text2 += (shells[j] ? "1" : "0");
            }
            text = text + "<cB>Shells<cC>" + text2;
        }
        if (meatInitated)
        {
            text += "<cB>MeatInit";
        }
        foreach (KeyValuePair<string, string> unrecognizedSaveString in unrecognizedSaveStrings)
        {
            text = text + "<cB>" + unrecognizedSaveString.Key + "<cC>" + unrecognizedSaveString.Value;
        }
        return text;
    }

    public override void LoadFromString(string[] saveData)
    {
        base.LoadFromString(saveData);
        for (int i = 0; i < saveData.Length; i++)
        {
            switch (Regex.Split(saveData[i], "<cC>")[0])
            {
                case "Shells":
                    {
                        string text = Regex.Split(saveData[i], "<cC>")[1];
                        shells = new bool[text.Length];
                        for (int j = 0; j < text.Length && j < shells.Length; j++)
                        {
                            shells[j] = text[j] == '1';
                        }
                        break;
                    }
                case "MeatInit":
                    meatInitated = true;
                    break;
            }
        }
        unrecognizedSaveStrings.Remove("Shells");
        unrecognizedSaveStrings.Remove("MeatInit");
    }

    public override void CycleTick()
    {
        base.CycleTick();
        if (!alive ||
            shells is null)
        {
            return;
        }

        for (int i = 0; i < shells.Length; i++)
        {
            if (Random.value < 0.2f)
            {
                shells[i] = Random.value < 0.985f;
            }
        }
    }
}
