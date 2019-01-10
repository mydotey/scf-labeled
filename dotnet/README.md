# scf-labeled: scf library for labeled key scenario

## NuGet Package

```sh
dotnet add package MyDotey.SCF.Labeled -v 1.5.2
```

Or use a single meta package

```sh
dotnet add package MyDotey.SCF.Bom -v 1.5.2
```

## Usage

Code comes from [scf-best-practice](https://github.com/mydotey/scf-best-practice):

```c#
dotnet/src/MyDotey.SCF.BP.Component/Labeled/
```

### Sample Labeled Pojo

```c#
public class DcSetting : ICloneable
{
    public static string APP_KEY = "App";
    public static string DC_KEY = "DC";

    public string Key { get; set; }
    public string App { get; set; }
    public string Dc { get; set; }
    public string Value { get; set; }

    public DcSetting()
    {

    }

    public DcSetting(string key, string app, string dc, string value)
    {
        Key = key;
        App = app;
        Dc = dc;
        Value = value;
    }

    public override int GetHashCode()
    {
        int prime = 31;
        int result = 1;
        result = prime * result + ((App == null) ? 0 : App.GetHashCode());
        result = prime * result + ((Dc == null) ? 0 : Dc.GetHashCode());
        result = prime * result + ((Key == null) ? 0 : Key.GetHashCode());
        return result;
    }

    public override bool Equals(object obj)
    {
        if (object.ReferenceEquals(this, obj))
            return true;
        if (obj == null)
            return false;
        if (GetType() != obj.GetType())
            return false;
        DcSetting other = (DcSetting)obj;
        if (App == null)
        {
            if (other.App != null)
                return false;
        }
        else if (!App.Equals(other.App))
            return false;
        if (Dc == null)
        {
            if (other.Dc != null)
                return false;
        }
        else if (!Dc.Equals(other.Dc))
            return false;
        if (Key == null)
        {
            if (other.Key != null)
                return false;
        }
        else if (!Key.Equals(other.Key))
        {
            return false;
        }

        return true;
    }

    public override string ToString()
    {
        return string.Format("{0} {{ Key: {1}, App: {2}, Dc: {3}, Value: {4} }}",
            GetType().Name, Key, App, Dc, Value);
    }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}
```

### Sample Labeled Source

```c#
public abstract class AbstractDcConfigurationSource
    : AbstractLabeledConfigurationSource<ConfigurationSourceConfig>
{
    public AbstractDcConfigurationSource(ConfigurationSourceConfig config)
        : base(config)
    {

    }

    protected override object GetPropertyValue(object key, ICollection<IPropertyLabel> labels)
    {
        if (!(key is string))
            return null;

        String app = null;
        String dc = null;
        if (labels != null)
        {
            foreach (IPropertyLabel l in labels)
            {
                if (object.Equals(l.Key, DcSetting.APP_KEY))
                    app = (String)l.Value;

                if (object.Equals(l.Key, DcSetting.DC_KEY))
                    dc = (String)l.Value;
            }
        }

        return GetPropertyValue((String)key, app, dc);
    }

    protected virtual String GetPropertyValue(String key, String app, String dc)
    {
        DcSetting dcSetting = GetPropertyValue(new DcSetting(key, app, dc, null));
        return dcSetting == null ? null : dcSetting.Value;
    }

    protected abstract DcSetting GetPropertyValue(DcSetting setting);
}
```

```c#
public class DynamicDcConfigurationSource : AbstractDcConfigurationSource
{
    private ConcurrentDictionary<DcSetting, DcSetting> _dcSettings =
        new ConcurrentDictionary<DcSetting, DcSetting>();

    public DynamicDcConfigurationSource(ConfigurationSourceConfig config)
        : base(config)
    {

    }

    public virtual void UpdateSetting(DcSetting setting)
    {
        if (setting == null)
            throw new ArgumentNullException("setting is null");

        if (string.IsNullOrWhiteSpace(setting.Key))
            throw new ArgumentNullException("setting.key is null");

        _dcSettings.TryGetValue(setting, out DcSetting oldValue);
        if (oldValue != null)
        {
            if (object.Equals(oldValue.Value, setting.Value))
                return;
        }

        setting = (DcSetting)setting.Clone();
        _dcSettings[setting] = setting;

        RaiseChangeEvent();
    }

    public virtual void RemoveSetting(DcSetting setting)
    {
        if (setting == null)
            throw new ArgumentNullException("setting is null");

        if (string.IsNullOrWhiteSpace(setting.Key))
            throw new ArgumentNullException("setting.key is null");

        _dcSettings.TryRemove(setting, out DcSetting oldValue);

        if (oldValue == null)
            return;

        RaiseChangeEvent();
    }

    protected override DcSetting GetPropertyValue(DcSetting setting)
    {
        _dcSettings.TryGetValue(setting, out DcSetting value);
        return value;
    }
}
```

```c#
public class YamlDcConfigurationSource : AbstractDcConfigurationSource
{
    private static Logger Logger = LogManager.GetCurrentClassLogger(typeof(YamlDcConfigurationSource));

    private Dictionary<DcSetting, DcSetting> _dcSettings;

    public YamlDcConfigurationSource(YamlFileConfigurationSourceConfig config)
        : base(config)
    {
        _dcSettings = new Dictionary<DcSetting, DcSetting>();
        try
        {
            using (Stream @is = new FileStream(config.FileName, FileMode.Open))
            {
                if (@is == null)
                {
                    Logger.Warn("file not found: {0}", config.FileName);
                    return;
                }

                StreamReader fileReader = new StreamReader(config.FileName, new UTF8Encoding(false));
                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(fileReader);
                if (!(yamlStream.Documents[0].RootNode is YamlSequenceNode))
                {
                    Logger.Error(typeof(YamlFileConfigurationSource).Name
                        + " only accepts YAML file with Sequence root."
                        + " Current is Mapping or Scala. YAML file: "
                        + config.FileName);
                    return;
                }

                YamlSequenceNode properties = (YamlSequenceNode)yamlStream.Documents[0].RootNode;
                properties.Children.ToList().ForEach(m =>
                {
                    DcSetting dcSetting = Convert<DcSetting>(m);
                    _dcSettings[dcSetting] = dcSetting;
                });
            }
        }
        catch (Exception e)
        {
            Logger.Warn(e, "failed to load yaml file: " + config.FileName);
        }
    }

    protected override DcSetting GetPropertyValue(DcSetting setting)
    {
        _dcSettings.TryGetValue(setting, out DcSetting value);
        return value;
    }

    public virtual V Convert<V>(YamlNode node)
    {
        if (node.NodeType == YamlNodeType.Alias)
            return default(V);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(new CamelCaseNamingConvention()).Build();

        if (node.NodeType == YamlNodeType.Scalar)
            return deserializer.Deserialize<V>(((YamlScalarNode)node).Value);

        YamlDocument yamlDocument = new YamlDocument(node);
        YamlStream yamlStream = new YamlStream();
        yamlStream.Add(yamlDocument);
        using (MemoryStream stream = new MemoryStream())
        {
            StreamWriter writer = new StreamWriter(stream);
            yamlStream.Save(writer);
            writer.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            return deserializer.Deserialize<V>(reader);
        }
    }
}
```

### Sample Usage

```c#
public class LabeledPropertyComponent
{
    private PropertyLabels _propertyLabels;

    private LabeledStringProperties _properties;

    private IProperty<LabeledKey<string>, int?> _requestTimeout;

    public LabeledPropertyComponent(ILabeledConfigurationManager manager)
    {
        _properties = new LabeledStringProperties(manager);
        InitLabels();

        _requestTimeout = _properties.GetIntProperty(ToLabeledKey("request.timeout"), 1000);
    }

    // priority: (key, app, dc) > (key, app) > (key, dc) > key
    private void InitLabels()
    {
        IPropertyLabel dcLabel = LabeledConfigurationProperties.NewLabel("dc", "shanghai");
        IPropertyLabel appLabel = LabeledConfigurationProperties.NewLabel("app", "10000");
        List<IPropertyLabel> dcLabels = new List<IPropertyLabel>();
        dcLabels.Add(dcLabel);
        List<IPropertyLabel> appLabels = new List<IPropertyLabel>();
        appLabels.Add(appLabel);
        List<IPropertyLabel> allLabels = new List<IPropertyLabel>();
        allLabels.Add(dcLabel);
        allLabels.Add(appLabel);

        PropertyLabels alternative1 =
            LabeledConfigurationProperties.NewLabels(dcLabels, PropertyLabels.Empty);
        PropertyLabels alternative2 = LabeledConfigurationProperties.NewLabels(appLabels, alternative1);
        _propertyLabels = LabeledConfigurationProperties.NewLabels(allLabels, alternative2);
    }

    protected LabeledKey<string> ToLabeledKey(string key)
    {
        return LabeledConfigurationProperties.NewKeyBuilder<string>()
            .SetKey(key).SetPropertyLabels(_propertyLabels).Build();
    }

    /**
        * use the following methods to get your properties / property values
        *      LabeledKey&lt;string&gt; labeledKey = ToLabeledKey(key);
        *      Properties.GetSomeProperty
        *      Properties.GetSomePropertyValue
        *      Properties.Manager
        */
    protected LabeledStringProperties Properties { get { return _properties; } }

    public void YourComponentApi()
    {
        Console.WriteLine("OuterManagerComponent is doing something");
        Console.WriteLine("request.timeout is: " + _requestTimeout.Value);
    }
}
```

```c#
// LabeledPropertyComponent only uses config, let the component's user to define how to config
// each user can define different config way
YamlFileConfigurationSourceConfig labeledSourceConfig = new YamlFileConfigurationSourceConfig.Builder()
    .SetName("yaml-file").SetFileName("labeled-property-component.yaml").Build();
IConfigurationSource labeledSource = new YamlDcConfigurationSource(labeledSourceConfig);
DynamicDcConfigurationSource dynamicLabeledSource = new DynamicDcConfigurationSource(
    ConfigurationSources.NewConfig("dynamic-labeled-source"));
ConfigurationManagerConfig managerConfig2 = ConfigurationManagers.NewConfigBuilder()
    .SetName("labeled-property-component").AddSource(1, labeledSource)
    .AddSource(2, dynamicLabeledSource).Build();
ILabeledConfigurationManager manager2 = LabeledConfigurationManagers.NewManager(managerConfig2);
LabeledPropertyComponent labeledPropertyComponent = new LabeledPropertyComponent(manager2);
```
