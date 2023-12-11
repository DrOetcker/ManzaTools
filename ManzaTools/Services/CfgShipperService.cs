using ManzaTools.Interfaces;
using ManzaTools.Models;
using ManzaTools.Utils;

using Microsoft.Extensions.Logging;

namespace ManzaTools.Services
{
    public class CfgShipperService : BaseService, ICfgShipperService
    {
        protected CfgShipperService(ILogger<CfgShipperService> logger)
            : base(logger)
        {
        }

        public void InitDefaultCfgs(string modulePath)
        {
            if (!Directory.Exists(Statics.CfgPath))
                Directory.CreateDirectory(Statics.CfgPath);

            var initCfgDirectory = Path.Combine(modulePath, "initCfgs");
            InitCfg(initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.Practice]);
            InitCfg(initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.PracticeMatch]);
            InitCfg(initCfgDirectory, Statics.GameModeCfgs[GameModeEnum.Deathmatch]);
            InitCfg(initCfgDirectory, "dmHsOnly.cfg");
            InitCfg(initCfgDirectory, "dmNoBots.cfg");
            InitCfg(initCfgDirectory, "dmPistolsOnly.cfg");
            InitCfg(initCfgDirectory, "dmTeamDm.cfg");
            if (Directory.Exists(initCfgDirectory))
                Directory.Delete(initCfgDirectory, true);
        }

        private void InitCfg(string initCfgDirectory, string cfgFileName)
        {
            var cfgPath = Path.Combine(Statics.CfgPath, cfgFileName);
            //If config already exists dont override it!
            if (File.Exists(cfgPath))
                return;


            var initCfgPath = Path.Combine(initCfgDirectory, cfgFileName);
            try
            {
                using var fileReader = File.OpenText(initCfgPath);
                var cfgContent = fileReader.ReadToEnd();
                fileReader.ReadToEnd();
                using var fileWriter = File.CreateText(cfgPath);
                fileWriter.Write(cfgContent);
            }
            catch (Exception ex)
            {
                Logging.Fatal(ex, nameof(CfgShipperService), nameof(InitCfg));
            }
            Responses.ReplyToServer($"Init of {cfgFileName} done", false, true);
        }
    }
}
