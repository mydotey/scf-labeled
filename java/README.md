# scf-labeled: scf library for labeled key scenario

## Maven Dependency

```xml
    <dependencyManagement>
        <dependencies>
            <dependency>
                <groupId>org.mydotey.scf</groupId>
                <artifactId>scf-bom</artifactId>
                <version>1.5.3</version>
                <type>pom</type>
                <scope>import</scope>
            </dependency>
        </dependencies>
    </dependencyManagement>

    <dependencies>
        <dependency>
            <groupId>org.mydotey.scf</groupId>
            <artifactId>scf-core</artifactId>
        </dependency>
        <dependency>
            <groupId>org.mydotey.scf</groupId>
            <artifactId>scf-simple</artifactId>
        </dependency>
        <dependency>
            <groupId>org.mydotey.scf</groupId>
            <artifactId>scf-labeled</artifactId>
        </dependency>
    </dependencies>
```

## Usage

Code comes from [scf-best-practice](https://github.com/mydotey/scf-best-practice):

```java
scf-bp-componnet/org.mydotey.scf.bp.component.labeled
```

### Sample Labeled Pojo

```java
public class DcSetting implements Cloneable {

    public static final String APP_KEY = "app";
    public static final String DC_KEY = "dc";

    private String key;
    private String app;
    private String dc;
    private String value;

    public DcSetting() {

    }

    public DcSetting(String key, String app, String dc, String value) {
        super();
        this.key = key;
        this.app = app;
        this.dc = dc;
        this.value = value;
    }

    public String getKey() {
        return key;
    }

    public String getApp() {
        return app;
    }

    public String getDc() {
        return dc;
    }

    public String getValue() {
        return value;
    }

    @Override
    public int hashCode() {
        final int prime = 31;
        int result = 1;
        result = prime * result + ((app == null) ? 0 : app.hashCode());
        result = prime * result + ((dc == null) ? 0 : dc.hashCode());
        result = prime * result + ((key == null) ? 0 : key.hashCode());
        return result;
    }

    @Override
    public boolean equals(Object obj) {
        if (this == obj)
            return true;
        if (obj == null)
            return false;
        if (getClass() != obj.getClass())
            return false;
        DcSetting other = (DcSetting) obj;
        if (app == null) {
            if (other.app != null)
                return false;
        } else if (!app.equals(other.app))
            return false;
        if (dc == null) {
            if (other.dc != null)
                return false;
        } else if (!dc.equals(other.dc))
            return false;
        if (key == null) {
            if (other.key != null)
                return false;
        } else if (!key.equals(other.key)) {
            return false;
        }

        return true;
    }

    @Override
    public String toString() {
        return String.format("%s { key: %s, app: %s, dc: %s, value: %s }",
            getClass().getSimpleName(), key, app, dc, value);
    }

    @Override
    public DcSetting clone() {
        try {
            return (DcSetting) super.clone();
        } catch (CloneNotSupportedException e) {
            e.printStackTrace();
            return null;
        }
    }

}
```

### Sample Labeled Source

```java
public abstract class AbstractDcConfigurationSource extends AbstractLabeledConfigurationSource {

    public AbstractDcConfigurationSource(ConfigurationSourceConfig config) {
        super(config);
    }

    @Override
    protected Object getPropertyValue(Object key, Collection<PropertyLabel> labels) {
        if (key.getClass() != String.class)
            return null;

        String app = null;
        String dc = null;
        if (labels != null) {
            for (PropertyLabel l : labels) {
                if (Objects.equals(l.getKey(), DcSetting.APP_KEY))
                    app = (String) l.getValue();

                if (Objects.equals(l.getKey(), DcSetting.DC_KEY))
                    dc = (String) l.getValue();
            }
        }

        return getPropertyValue((String) key, app, dc);
    }

    protected String getPropertyValue(String key, String app, String dc) {
        DcSetting dcSetting = getPropertyValue(new DcSetting(key, app, dc, null));
        return dcSetting == null ? null : dcSetting.getValue();
    }

    protected abstract DcSetting getPropertyValue(DcSetting setting);

}
```

```java
public class DynamicDcConfigurationSource extends AbstractDcConfigurationSource {

    private Map<DcSetting, DcSetting> _dcSettings = new ConcurrentHashMap<>();

    public DynamicDcConfigurationSource(ConfigurationSourceConfig config) {
        super(config);
    }

    public void updateSetting(DcSetting setting) {
        Objects.requireNonNull(setting, "setting is null");
        Objects.requireNonNull(setting.getKey(), "setting.key is null");

        DcSetting oldValue = _dcSettings.get(setting);
        if (oldValue != null) {
            if (Objects.equals(oldValue.getValue(), setting.getValue()))
                return;
        }

        setting = setting.clone();
        _dcSettings.put(setting, setting);

        raiseChangeEvent();
    }

    public void removeSetting(DcSetting setting) {
        Objects.requireNonNull(setting, "setting is null");
        Objects.requireNonNull(setting.getKey(), "setting.key is null");

        DcSetting oldValue = _dcSettings.remove(setting);

        if (oldValue == null)
            return;

        raiseChangeEvent();
    }

    @Override
    protected DcSetting getPropertyValue(DcSetting setting) {
        return _dcSettings.get(setting);
    }

}
```

```java
public class YamlDcConfigurationSource extends AbstractDcConfigurationSource {

    private static final Logger LOGGER = LoggerFactory.getLogger(YamlDcConfigurationSource.class);

    private Map<DcSetting, DcSetting> _dcSettings;

    @SuppressWarnings("unchecked")
    public YamlDcConfigurationSource(YamlFileConfigurationSourceConfig config) {
        super(config);

        _dcSettings = new HashMap<>();
        try (InputStream is = Thread.currentThread().getContextClassLoader()
                .getResourceAsStream(config.getFileName())) {
            if (is == null) {
                LOGGER.warn("file not found: {}", config.getFileName());
                return;
            }

            Object properties = new Yaml().load(is);
            if (properties == null || !(properties instanceof List)) {
                LOGGER.error(YamlDcConfigurationSource.class.getSimpleName()
                    + " only accepts YAML file with Sequence root. Current is Mapping or Scala."
                    + " YAML file: " + config.getFileName());
                return;
            }

            List<Map<Object, Object>> listProperties = (List<Map<Object, Object>>) properties;
            ObjectMapper objectMapper = new ObjectMapper();
            listProperties.forEach(m -> {
                DcSetting dcSetting = objectMapper.convertValue(m, DcSetting.class);
                _dcSettings.put(dcSetting, dcSetting);
            });

        } catch (Exception e) {
            LOGGER.warn("failed to load yaml file: " + config.getFileName(), e);
        }
    }

    @Override
    protected DcSetting getPropertyValue(DcSetting setting) {
        return _dcSettings.get(setting);
    }

}
```

### Sample Usage

```java
public class LabeledPropertyComponent {

    private PropertyLabels _propertyLabels;

    private LabeledStringProperties _properties;

    private Property<LabeledKey<String>, Integer> _requestTimeout;

    public LabeledPropertyComponent(LabeledConfigurationManager manager) {
        _properties = new LabeledStringProperties(manager);
        initLabels();

        _requestTimeout = _properties.getIntProperty(toLabeledKey("request.timeout"), 1000);
    }

    // priority: (key, app, dc) > (key, app) > (key, dc) > key
    private void initLabels() {
        PropertyLabel dcLabel = LabeledConfigurationProperties.newLabel("dc", "shanghai");
        PropertyLabel appLabel = LabeledConfigurationProperties.newLabel("app", "10000");
        ArrayList<PropertyLabel> dcLabels = new ArrayList<>();
        dcLabels.add(dcLabel);
        ArrayList<PropertyLabel> appLabels = new ArrayList<>();
        appLabels.add(appLabel);
        ArrayList<PropertyLabel> allLabels = new ArrayList<>();
        allLabels.add(dcLabel);
        allLabels.add(appLabel);

        PropertyLabels alternative1 =
            LabeledConfigurationProperties.newLabels(dcLabels, PropertyLabels.EMPTY);
        PropertyLabels alternative2 = LabeledConfigurationProperties.newLabels(appLabels, alternative1);
        _propertyLabels = LabeledConfigurationProperties.newLabels(allLabels, alternative2);
    }

    protected LabeledKey<String> toLabeledKey(String key) {
        return LabeledConfigurationProperties.<String> newKeyBuilder().setKey(key)
            .setPropertyLabels(_propertyLabels).build();
    }

    /**
     * use the following methods to get your properties / property values
     *      LabeledKey&lt;String&gt; labeledKey = toLabeledKey(key);
     *      getProperties().getSomeProperty
     *      getProperties().getSomePropertyValue
     *      getProperties().getManager()
     */
    protected LabeledStringProperties getProperties() {
        return _properties;
    }

    public void yourComponentApi() {
        System.out.println("OuterManagerComponent is doing something");
        System.out.println("request.timeout is: " + _requestTimeout.getValue());
    }

}
```

```java
// LabeledPropertyComponent only uses config, let the component's user to define how to config
// each user can define different config way
YamlFileConfigurationSourceConfig labeledSourceConfig = new YamlFileConfigurationSourceConfig.Builder()
        .setName("yaml-file").setFileName("labeled-property-component.yaml").build();
ConfigurationSource labeledSource = new YamlDcConfigurationSource(labeledSourceConfig);
DynamicDcConfigurationSource dynamicLabeledSource = new DynamicDcConfigurationSource(
        ConfigurationSources.newConfig("dynamic-labeled-source"));
ConfigurationManagerConfig managerConfig = ConfigurationManagers.newConfigBuilder()
        .setName("labeled-property-component").addSource(1, labeledSource)
        .addSource(2, dynamicLabeledSource).build();
LabeledConfigurationManager manager = LabeledConfigurationManagers.newManager(managerConfig);
LabeledPropertyComponent labeledPropertyComponent = new LabeledPropertyComponent(manager);
```
