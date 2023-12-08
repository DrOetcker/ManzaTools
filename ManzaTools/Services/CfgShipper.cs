
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

            var initCfgDirectory = Path.Combine(modulePath, "initCfgs");
            InitCfg(modulePath, initCfgDirectory, GameModeEnum.Practice);
            InitCfg(modulePath, initCfgDirectory, GameModeEnum.PracticeMatch);
            InitCfg(modulePath, initCfgDirectory, GameModeEnum.Deathmatch);
            if (Directory.Exists(initCfgDirectory))
                Directory.Delete(initCfgDirectory, true);
        }

        private void InitCfg(string modulePath, string initCfgDirectory, GameModeEnum gameMode)
        {
            var cfgPath = Path.Combine(Statics.CfgPath, Statics.GameModeCfgs[gameMode]);
            //If config already exists dont override it!
            if (File.Exists(cfgPath))
                return;


            var initCfgPath = Path.Combine(initCfgDirectory, Statics.GameModeCfgs[gameMode]);

            using (StreamReader fileReader = File.OpenText(initCfgPath))
            {
                var cfgContent = fileReader.ReadToEnd();
                fileReader.ReadToEnd();
                using(StreamWriter fileWriter = File.CreateText(cfgPath))
                    fileWriter.Write(cfgContent);
            }
            Responses.ReplyToServer($"Init of cfg {Statics.GameModeCfgs[gameMode]} done", false, true);
        }
    }
}
