<Query Kind="Program">
  <Namespace>System.Collections.Concurrent</Namespace>
</Query>

void Main()
{ 
	var supportedVersions = new SupportedVersionProvider();
	supportedVersions.AddSupportedVersion("6.5");
	supportedVersions.AddSupportedVersion("7.5");
	supportedVersions.AddSupportedVersion("7.6");
	supportedVersions.AddSupportedVersion("7.7");

	var scenarioConfigs = new ScenarioConfigurationProvider();
	scenarioConfigs.AddConfiguration("6.5", "Basic");
	scenarioConfigs.AddConfiguration("7.5", "Basic");
	scenarioConfigs.AddConfiguration("7.6", "Basic");
	scenarioConfigs.AddConfiguration("7.7", "Basic");
	scenarioConfigs.AddConfiguration("7.7", "PubSubCompatibilityMode");
	scenarioConfigs.AddConfiguration("Current", "Basic");
	scenarioConfigs.AddConfiguration("Current", "PubSubCompatibilityMode");
	
	var scenarioGenerator = new ScenarioGenerator(
		supportedVersions,
		scenarioConfigs
	);

	scenarioGenerator.GetScenarios(new SendReplyTest())
		.Select(x => x.ToString())
		.Dump("Send-Reply Tests");
	
	scenarioGenerator.GetScenarios(new PubSubTest())
		.Select(x => x.ToString())
		.Dump("Pub-Sub Tests");
}

class PubSubTest : TestScenario
{
	public override string Name => "Pub-Sub";
	public override string[] Endpoints => new[] { "Publisher", "Subscriber" };
	public override void Verify(ScenarioConfiguration scenarioConfig)
	{
		var configurations = Endpoints.Select(x => scenarioConfig.Endpoint(x)).ToArray();
		
		bool SupportsMessageDrivenPubSub(ScenarioEndpointConfiguration endpointConfig)
		{
			return endpointConfig.Version.StartsWith("6")
				|| endpointConfig.Version == "7.5"
				|| endpointConfig.Version == "7.6"
				|| endpointConfig.Configuration == "PubSubCompatibilityMode";
		}
		
		bool SupportsNativePubSub(ScenarioEndpointConfiguration endpointConfig)
		{
			return endpointConfig.Version == "Current" || endpointConfig.Version == "7.7";
		}
		
		if(!configurations.All(SupportsNativePubSub))
		{
			if(!configurations.All(SupportsMessageDrivenPubSub))
			{
				scenarioConfig.Exclude("A subset of endpoints only support message driven pub-sub and the rest do not have pub-sub compatibility mode switched on");
			}
		}		
	}
}

class SendReplyTest : TestScenario
{
	public override string Name => "Send-Reply";
	public override string[] Endpoints => new[] { "Sender", "Receiver" };
}


abstract class TestScenario
{
	public virtual string Name => GetType().Name;
	public abstract string[] Endpoints { get; }
	public virtual void Verify(ScenarioConfiguration scenarioConfig) { }
}


class SupportedVersionProvider
{
	private HashSet<string> supportedVersions = new HashSet<string>();
	
	public void AddSupportedVersion(string supportedVersion)
	{
		supportedVersions.Add(supportedVersion);
	}
	
	public IEnumerable<string> GetSupportedVersions()
		=> supportedVersions;
}

class ScenarioConfigurationProvider
{
	public IEnumerable<string> GetConfigurations(string version)
		=> configurations.TryGetValue(version, out var configs)
			? configs
			: Enumerable.Empty<string>();
		
	public void AddConfiguration(string version, string configuration)
	{
		configurations.GetOrAdd(version, _ => new HashSet<string>())
			.Add(configuration);
	}

	private ConcurrentDictionary<string, HashSet<string>> configurations
		= new ConcurrentDictionary<string, HashSet<string>>();
}

class ScenarioGenerator
{
	private SupportedVersionProvider supportedVersionsProvider;
	private ScenarioConfigurationProvider scenarioConfigurationProvider;
	
	public ScenarioGenerator(SupportedVersionProvider supportedVersionsProvider,
		ScenarioConfigurationProvider scenarioConfigurationProvider)
	{
		this.supportedVersionsProvider = supportedVersionsProvider;
		this.scenarioConfigurationProvider = scenarioConfigurationProvider;
	}
	
	public IEnumerable<ScenarioConfiguration> GetScenarios(TestScenario testScenario)
	{
		var currentVersion = "Current";
		for (var i = 0; i < testScenario.Endpoints.Length; i++)
		{
			foreach (var supportedVersion in supportedVersionsProvider.GetSupportedVersions())
			{
				foreach(var supportedVersionConfiguration in scenarioConfigurationProvider.GetConfigurations(supportedVersion))
				{
					foreach(var currentVersionConfiguration in scenarioConfigurationProvider.GetConfigurations(currentVersion))
					{
						var scenario = new ScenarioConfiguration(testScenario.Name);
						for(var j = 0; j < testScenario.Endpoints.Length; j++)
						{
							scenario.AddEndpoint(testScenario.Endpoints[j], i != j 
								? new ScenarioEndpointConfiguration(supportedVersion, supportedVersionConfiguration)
								: new ScenarioEndpointConfiguration(currentVersion, currentVersionConfiguration)
							);
						}
						
						testScenario.Verify(scenario);
						yield return scenario;
					}
				}
			}
		}
	}
}

class ScenarioConfiguration
{
	private string scenarioName;
	
	public ScenarioConfiguration(string scenarioName)
	{
		this.scenarioName = scenarioName;
	}
	
	public void AddEndpoint(string endpointName, ScenarioEndpointConfiguration endpointConfiguration)
	{
		endpoints.Add(endpointName, endpointConfiguration);
	}
	
	public ScenarioEndpointConfiguration Endpoint(string name) => endpoints[name];
	
	private Dictionary<string, ScenarioEndpointConfiguration> endpoints { get; }
		= new Dictionary<string, ScenarioEndpointConfiguration>();
		
	private string excludeReason;
	
	public bool IsExcluded => excludeReason != null;
	
	public void Exclude(string reason) => excludeReason = reason;

	public override string ToString()
		=> scenarioName + ": "
		+ string.Join(" | ", from e in endpoints select $"{e.Key} ({e.Value}))")
		+ (IsExcluded ? $" EXCLUDED - {excludeReason}" : "");
}

class ScenarioEndpointConfiguration
{
	public ScenarioEndpointConfiguration(string version, string configuration)
	{
		Version = version;
		Configuration = configuration;
	}
	
	public string Version { get; }
	public string Configuration { get; }

	public override string ToString() => $"{Version}, {Configuration}";
}
