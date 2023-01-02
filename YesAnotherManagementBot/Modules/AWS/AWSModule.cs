using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace YAMB.Modules.AWS; 

public abstract class AwsModule {
    private static readonly AmazonSecretsManagerClient Client = new(new BasicAWSCredentials(GlobalSettings.Instance.AccessKeyId, GlobalSettings.Instance.SecretAccessKey), RegionEndpoint.USEast1);
    
    public static async Task<string> GetSecret(string secretName) {
        var request = new GetSecretValueRequest {
            SecretId = secretName
        };
        
        var response = await Client.GetSecretValueAsync(request);

        return response.SecretString;
    }
}