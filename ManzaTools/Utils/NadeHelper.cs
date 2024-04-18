namespace ManzaTools.Utils
{
    public static class NadeHelper
    {
        public static string GetNadeType(string? cs2NadeName)
        {
            switch (cs2NadeName)
            {
                case Consts.FlashCs2:
                    return Consts.Flash;
                case Consts.SmokeCs2:
                    return Consts.Smoke;
                case Consts.NadeCs2:
                    return Consts.Nade;
                case Consts.MolotovCs2:
                case Consts.IncCs2:
                    return Consts.Molotov;
                default:
                    return "";
            }
        }
    }
}
