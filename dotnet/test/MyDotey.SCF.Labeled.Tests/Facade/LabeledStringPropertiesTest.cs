using System;
using System.Collections.Generic;

using Xunit;

using MyDotey.SCF.Type;
using MyDotey.SCF.Type.String;
using MyDotey.SCF.Source.StringProperty.PropertiesFile;
using MyDotey.SCF.Labeled;

namespace MyDotey.SCF.Facade
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class LabeledStringPropertiesTest : StringPropertiesTest
    {
        protected override IConfigurationManager CreateManager(String fileName)
        {
            PropertiesFileConfigurationSourceConfig sourceConfig = StringPropertySources
                    .NewPropertiesFileSourceConfigBuilder().SetName("properties-source").SetFileName(fileName).Build();
            Console.WriteLine("source config: " + sourceConfig + "\n");
            ConfigurationManagerConfig managerConfig = ConfigurationManagers.NewConfigBuilder().SetName("test")
                    .AddSource(1, StringPropertySources.NewPropertiesFileSource(sourceConfig)).Build();
            Console.WriteLine("manager config: " + managerConfig + "\n");
            return LabeledConfigurationManagers.NewManager(managerConfig);
        }

        protected override StringProperties CreateStringProperties(String fileName)
        {
            IConfigurationManager manager = CreateManager(fileName);
            return new StringProperties(manager);
        }

        protected virtual LabeledStringProperties CreateLabeledStringProperties()
        {
            ILabeledConfigurationManager manager = CreateLabeledManager();
            return new LabeledStringProperties(manager);
        }

        protected virtual ILabeledConfigurationManager CreateLabeledManager()
        {
            ConfigurationSourceConfig sourceConfig = ConfigurationSources.NewConfig("labeled-source");
            Console.WriteLine("source config: " + sourceConfig + "\n");
            TestDataCenterSetting Setting1 = new TestDataCenterSetting("exist", "ok", "sh-1", "app-1");
            TestDataCenterSetting Setting2 = new TestDataCenterSetting("int-value", "1", "sh-1", "app-1");
            TestDataCenterSetting Setting3 = new TestDataCenterSetting("list-value", "s1, s2, s3", "sh-1", "app-1");
            TestDataCenterSetting Setting4 = new TestDataCenterSetting("map-value", "k1: v1, k2: v2, k3: v3", "sh-1",
                    "app-1");
            TestDataCenterSetting Setting5 = new TestDataCenterSetting("int-list-value", "1, 2, 3", "sh-1", "app-1");
            TestDataCenterSetting Setting6 = new TestDataCenterSetting("int-long-map-value", "1: 2, 3: 4, 5: 6", "sh-1",
                    "app-1");
            TestLabeledConfigurationSource source = new TestLabeledConfigurationSource(sourceConfig,
                    new List<TestDataCenterSetting>() { Setting1, Setting2, Setting3, Setting4, Setting5, Setting6 });
            ConfigurationManagerConfig managerConfig = ConfigurationManagers.NewConfigBuilder().SetName("test")
                    .AddSource(1, source).Build();
            Console.WriteLine("manager config: " + managerConfig + "\n");
            return LabeledConfigurationManagers.NewManager(managerConfig);
        }

        [Fact]
        public virtual void TestGetLabeledProperties()
        {
            LabeledStringProperties labeledStringProperties = CreateLabeledStringProperties();

            List<IPropertyLabel> labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            PropertyLabels propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            LabeledKey<string> key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("not-exist")
                    .SetPropertyLabels(propertyLabels).Build();

            IProperty<LabeledKey<string>, string> property = labeledStringProperties.GetStringProperty(key);
            Console.WriteLine("property: " + property + "\n");
            Assert.Null(property.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("not-exist2")
                    .SetPropertyLabels(propertyLabels).Build();
            property = labeledStringProperties.GetStringProperty(key, "default");
            Console.WriteLine("property: " + property + "\n");
            Assert.Equal("default", property.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("exist").SetPropertyLabels(propertyLabels)
                    .Build();
            property = labeledStringProperties.GetStringProperty(key, "default");
            Console.WriteLine("property: " + property + "\n");
            Assert.Equal("ok", property.Value);
        }

        [Fact]
        public virtual void TestGetTypedLabeledProperties()
        {
            LabeledStringProperties labeledStringProperties = CreateLabeledStringProperties();

            List<IPropertyLabel> labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            PropertyLabels propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            LabeledKey<string> key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("int-value")
                    .SetPropertyLabels(propertyLabels).Build();

            IProperty<LabeledKey<string>, int?> property = labeledStringProperties.GetIntProperty(key);
            Console.WriteLine("property: " + property + "\n");
            int? expected = 1;
            Assert.Equal(expected, property.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("list-value")
                    .SetPropertyLabels(propertyLabels).Build();
            IProperty<LabeledKey<string>, List<string>> property2 = labeledStringProperties.GetListProperty(key);
            Console.WriteLine("property: " + property2 + "\n");
            List<string> expected2 = new List<string>() { "s1", "s2", "s3" };
            Assert.Equal(expected2, property2.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("map-value")
                    .SetPropertyLabels(propertyLabels).Build();
            IProperty<LabeledKey<string>, Dictionary<String, string>> property3 = labeledStringProperties.GetDictionaryProperty(key);
            Console.WriteLine("property: " + property3 + "\n");
            Dictionary<String, string> expected3 = new Dictionary<string, string>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected3, property3.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("int-list-value")
                    .SetPropertyLabels(propertyLabels).Build();
            IProperty<LabeledKey<string>, List<int?>> property4 = labeledStringProperties.GetListProperty(key,
                    StringToIntConverter.Default);
            Console.WriteLine("property: " + property4 + "\n");
            List<int?> expected4 = new List<int?>() { 1, 2, 3 };
            Assert.Equal(expected4, property4.Value);

            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("int-long-map-value")
                    .SetPropertyLabels(propertyLabels).Build();
            IProperty<LabeledKey<string>, Dictionary<int?, long?>> property5 = labeledStringProperties.GetDictionaryProperty(key,
                    StringToIntConverter.Default, StringToLongConverter.Default);
            Console.WriteLine("property: " + property5 + "\n");
            Dictionary<int?, long?> expected5 = new Dictionary<int?, long?>()
            {
                { 1, 2L },
                { 3, 4L },
                { 5, 6L }
            };
            Assert.Equal(expected5, property5.Value);
        }

        [Fact]
        public virtual void TestSameKeyDifferentConfigForLabeledProperties()
        {
            LabeledStringProperties labeledStringProperties = CreateLabeledStringProperties();

            List<IPropertyLabel> labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            PropertyLabels propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            LabeledKey<string> key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("map-value")
                    .SetPropertyLabels(propertyLabels).Build();

            IProperty<LabeledKey<string>, Dictionary<String, string>> property = labeledStringProperties.GetDictionaryProperty(key);
            Dictionary<String, string> expected = new Dictionary<string, string>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected, property.Value);

            Assert.Throws<ArgumentException>(() => labeledStringProperties.GetDictionaryProperty(
                key, StringToIntConverter.Default, StringToLongConverter.Default));
        }

        [Fact]
        public virtual void TestSameConfigSameLabeledProperty()
        {
            LabeledStringProperties labeledStringProperties = CreateLabeledStringProperties();

            List<IPropertyLabel> labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            PropertyLabels propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            LabeledKey<string> key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("map-value")
                    .SetPropertyLabels(propertyLabels).Build();

            IProperty<LabeledKey<string>, Dictionary<String, string>> property = labeledStringProperties.GetDictionaryProperty(key);
            Dictionary<String, string> expected = new Dictionary<string, string>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected, property.Value);

            IProperty<LabeledKey<string>, Dictionary<String, string>> property2 = labeledStringProperties.GetDictionaryProperty(key);
            Console.WriteLine("property2: " + property + "\n");
            Assert.True(object.ReferenceEquals(property, property2));
        }
    }
}