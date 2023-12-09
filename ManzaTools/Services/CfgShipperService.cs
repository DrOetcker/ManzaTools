
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class CfgShipperService
    {
        
        public void InitDefaultCfgs(string modulePath)
        {
            if (!Directory.Exists(Statics.CfgPath))
                Directory.CreateDirectory(Statics.CfgPath);

            var initCfgDirectory = Path.Combine(modulePath, "initCfgs");
            InitCfg(modulePath, initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.Practice]);
            InitCfg(modulePath, initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.PracticeMatch]);
            InitCfg(modulePath, initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.Deathmatch]);
            InitCfg(modulePath, initCfgDirectory, "dmHsOnly.cfg");
            InitCfg(modulePath, initCfgDirectory, "dmNoBots.cfg");
            InitCfg(modulePath, initCfgDirectory, "dmPistolsOnly.cfg");
            InitCfg(modulePath, initCfgDirectory, "dmTeamDm.cfg");
            if (Directory.Exists(initCfgDirectory))
                Directory.Delete(initCfgDirectory, true);
        }

        private void InitCfg(string modulePath, string initCfgDirectory, string cfgFileName)
        {
            var cfgPath = Path.Combine(Statics.CfgPath, cfgFileName);
            //If config already exists dont override it!
            if (File.Exists(cfgPath))
                return;


            var initCfgPath = Path.Combine(initCfgDirectory, cfgFileName);
            try
            {
                using (StreamReader fileReader = File.OpenText(initCfgPath))
                {
                    var cfgContent = fileReader.ReadToEnd();
                    fileReader.ReadToEnd();
                    using(StreamWriter fileWriter = File.CreateText(cfgPath))
                        fileWriter.Write(cfgContent);
                }
            }
            catch(Exception ex)
            {
                Logging.Log($"Error {ex.Message}");
            }
            Responses.ReplyToServer($"Init of {cfgFileName} done", false, true);
        }
    }
}
