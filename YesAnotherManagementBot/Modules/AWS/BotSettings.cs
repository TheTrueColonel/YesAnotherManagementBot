using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Extensions.Caching;
using Newtonsoft.Json;

namespace YAMB.Modules.AWS; 

public sealed class BotSettings : IDisposable {
    private static BotSettings? _instance;
    private static readonly object Lock = new();
    
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly SecretsManagerCache _cache;

    private BotSettings() {
        _secretsManager = new AmazonSecretsManagerClient(new BasicAWSCredentials(GlobalSettings.Instance.AccessKeyId, GlobalSettings.Instance.SecretAccessKey), RegionEndpoint.USEast1);
        _cache = new SecretsManagerCache(_secretsManager);
    }
    
    public static BotSettings Instance {
        get {
            lock (Lock) {
                return _instance ??= new BotSettings();
            }
        }
    }

    public void Dispose() {
        _secretsManager.Dispose();
        _cache.Dispose();
    }

    public async Task<string> GetToken() {
        var json = await _cache.GetSecretString("Production/YAMB");
        var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;

        return map["OAuthToken"];
    }
}