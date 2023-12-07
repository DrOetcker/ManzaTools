
using ManzaTools.Models;
using ManzaTools.Utils;

namespace ManzaTools.Services
{
    public class CfgShipper
    {
        
        public void InitDefaultCfgs(string modulePath)
        {
            if (!Directory.Exists(Statics.CfgPath))
                Directory.CreateDirectory(Statics.CfgPath);

            InitCfg(modulePath, GameModeEnum.Practice);
            InitCfg(modulePath, GameModeEnum.PracticeMatch);
            InitCfg(modulePath, GameModeEnum.Deathmatch);
        }

        private void InitCfg(string modulePath, GameModeEnum gameMode)
        {
            var cfgPath = Path.Combine(Statics.CfgPath, Statics.GameModeCfgs[gameMode]);
            //If config already exists dont override it!
            if (File.Exists(cfgPath))
                return;


            var initCfgDirectory = Path.Combine(modulePath, "initCfgs");
            var initCfgPath = Path.Combine(initCfgDirectory, Statics.GameModeCfgs[gameMode]);

            using (StreamReader fileReader = File.OpenText(initCfgPath))
            {
                var cfgContent = fileReader.ReadToEnd();
                fileReader.ReadToEnd();
                using(StreamWriter fileWriter = File.CreateText(cfgPath))
                    fileWriter.Write(cfgContent);
            }
            Directory.Delete(initCfgDirectory, true);
            Responses.ReplyToServer($"Init of cfg {Statics.GameModeCfgs[gameMode]} done", false, true);
        }
    }
}
