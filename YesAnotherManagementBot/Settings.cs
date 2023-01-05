using System.Reflection;
using Newtonsoft.Json;

namespace YAMB; 

// ReSharper disable UnassignedGetOnlyAutoProperty
public sealed class GlobalSettings {
    private static GlobalSettings? _instance;
    private static readonly object Lock = new();

    private GlobalSettings() {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(x => x.EndsWith("settings.json"));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
                        
        var json = reader.ReadToEnd();
                    
        var map = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;

        AccessKeyId = map["AccessKeyID"];
        SecretAccessKey = map["SecretAccessKey"];
        EncryptionKey = map["EncryptionKey"];
        SignatureKey = map["SignatureKey"];
        ConnectionString = map["ConnectionString"];
    }
    
    public static GlobalSettings Instance {
        get {
            lock (Lock) {
                return _instance ??= new GlobalSettings();
            }
        }
    }
    
    public string AccessKeyId { get; }
    public string SecretAccessKey { get; }
    public string EncryptionKey { get; }
    public string SignatureKey { get; }
    public string ConnectionString { get; }
}