using Insthync.UnityVivoxIntegration;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class MMOVivoxConfig : VivoxConfig
    {
        private ServerConfig _serverConfig;
        private ClientConfig _clientConfig;

        public override async Task LoadServer()
        {
            _serverConfig = ConfigManager.ReadServerConfig();
            if (!string.IsNullOrEmpty(_serverConfig.vivoxServer))
                _server = _serverConfig.vivoxServer;
            if (!string.IsNullOrEmpty(_serverConfig.vivoxDomain))
                _domain = _serverConfig.vivoxDomain;
            if (!string.IsNullOrEmpty(_serverConfig.vivoxIssuer))
                _issuer = _serverConfig.vivoxIssuer;
            if (!string.IsNullOrEmpty(_serverConfig.vivoxKey))
                _key = _serverConfig.vivoxKey;

            string envVal;
            envVal = System.Environment.GetEnvironmentVariable("vivoxServer");
            if (!string.IsNullOrEmpty(envVal))
                _server = envVal;
            envVal = System.Environment.GetEnvironmentVariable("vivoxDomain");
            if (!string.IsNullOrEmpty(envVal))
                _domain = envVal;
            envVal = System.Environment.GetEnvironmentVariable("vivoxIssuer");
            if (!string.IsNullOrEmpty(envVal))
                _issuer = envVal;
            envVal = System.Environment.GetEnvironmentVariable("vivoxKey");
            if (!string.IsNullOrEmpty(envVal))
                _key = envVal;

            await Task.Yield();
        }

        public override async Task LoadClient()
        {
            _clientConfig = await ConfigManager.ReadClientConfig();
            if (!string.IsNullOrEmpty(_clientConfig.vivoxServer))
                _server = _clientConfig.vivoxServer;
            if (!string.IsNullOrEmpty(_clientConfig.vivoxDomain))
                _domain = _clientConfig.vivoxDomain;
            if (!string.IsNullOrEmpty(_clientConfig.vivoxIssuer))
                _issuer = _clientConfig.vivoxIssuer;
        }
    }
}