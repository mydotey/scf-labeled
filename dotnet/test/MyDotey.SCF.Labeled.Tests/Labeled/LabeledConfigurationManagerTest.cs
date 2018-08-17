using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

using Xunit;

using MyDotey.SCF.Facade;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class LabeledConfigurationManagerTest : ConfigurationManagerTest
    {
        protected override IConfigurationManager CreateManager(Dictionary<int, IConfigurationSource> sources)
        {
            Dictionary<int, IConfigurationSource> sourceList = new Dictionary<int, IConfigurationSource>(sources);
            ConfigurationSourceConfig config = ConfigurationSources.NewConfig("labeled-source");
            TestLabeledConfigurationSource source = CreateLabeledSource(config);
            sourceList[int.MaxValue - 1] = source;
            config = ConfigurationSources.NewConfig("dynamic-labeled-source");
            source = CreateDynamicLabeledSource(config);
            sourceList[int.MaxValue] = source;

            return CreateLabeledManager(sourceList);
        }

        protected virtual ILabeledConfigurationManager CreateLabeledManager(Dictionary<int, IConfigurationSource> sources)
        {
            ConfigurationManagerConfig managerConfig = ConfigurationManagers.NewConfigBuilder().SetName("test")
                    .AddSources(sources).Build();
            Console.WriteLine("manager config: " + managerConfig + "\n");
            return LabeledConfigurationManagers.NewManager(managerConfig);
        }

        protected virtual TestLabeledConfigurationSource CreateLabeledSource(ConfigurationSourceConfig config)
        {
            TestDataCenterSetting Setting = new TestDataCenterSetting("labeled-key-1", "v-0", null, null);
            TestDataCenterSetting Setting1 = new TestDataCenterSetting("labeled-key-1", "v-1", "sh-1", "app-1");
            TestDataCenterSetting Setting2 = new TestDataCenterSetting("labeled-key-1", "v-2", "sh-2", "app-1");
            TestDataCenterSetting Setting3 = new TestDataCenterSetting("labeled-key-1", "v-3", "sh-1", "app-2");
            return new TestLabeledConfigurationSource(config, new List<TestDataCenterSetting>() { Setting, Setting1, Setting2, Setting3 });
        }

        protected virtual TestDynamicLabeledConfigurationSource CreateDynamicLabeledSource(ConfigurationSourceConfig config)
        {
            TestDataCenterSetting Setting = new TestDataCenterSetting("labeled-key-1", "v-0-2", null, null);
            TestDataCenterSetting Setting1 = new TestDataCenterSetting("labeled-key-1", "v-1-2", "sh-1", "app-1");
            TestDataCenterSetting Setting2 = new TestDataCenterSetting("labeled-key-1", "v-2-2", "sh-2", "app-1");
            TestDataCenterSetting Setting3 = new TestDataCenterSetting("labeled-key-1", "v-3-2", "sh-1", "app-2");
            return new TestDynamicLabeledConfigurationSource(config,
                new List<TestDataCenterSetting>() { Setting, Setting1, Setting2, Setting3 });
        }

        [Fact]
        public virtual void TestGetLabeledProperty()
        {
            ConfigurationSourceConfig config = ConfigurationSources.NewConfig("labeled-source");
            TestLabeledConfigurationSource labeledSource = CreateLabeledSource(config);
            config = ConfigurationSources.NewConfig("dynamic-labeled-source");
            TestDynamicLabeledConfigurationSource dynamicLabeledSource = CreateDynamicLabeledSource(config);
            ILabeledConfigurationManager manager = CreateLabeledManager(
                    new Dictionary<int, IConfigurationSource>() { { 1, labeledSource }, { 2, dynamicLabeledSource } });

            List<IPropertyLabel> labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            PropertyLabels propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            LabeledKey<string> key = LabeledConfigurationProperties.NewKeyBuilder<string>().SetKey("labeled-key-1")
                    .SetPropertyLabels(propertyLabels).Build();
            PropertyConfig<LabeledKey<string>, string> propertyConfig = ConfigurationProperties
                    .NewConfigBuilder<LabeledKey<string>, string>().SetKey(key).SetDefaultValue("default-value-1").Build();
            IProperty<LabeledKey<string>, string> property = manager.GetProperty(propertyConfig);
            Console.WriteLine(property);
            Assert.Equal("v-1-2", property.Value);

            labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1-not-exist"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            propertyLabels = LabeledConfigurationProperties.NewLabels(labels);
            key = LabeledConfigurationProperties.NewKeyBuilder<String>().SetKey("labeled-key-1")
                        .SetPropertyLabels(propertyLabels).Build();
            propertyConfig = ConfigurationProperties.NewConfigBuilder<LabeledKey<string>, String>().SetKey(key)
                         .SetDefaultValue("default-value-1").Build();
            property = manager.GetProperty(propertyConfig);
            Console.WriteLine(property);
            Assert.Equal("default-value-1", property.Value);

            propertyLabels = LabeledConfigurationProperties.NewLabels(
                    LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1"),
                    LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1-not-exist"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            propertyLabels = LabeledConfigurationProperties.NewLabels(labels, propertyLabels);
            key = LabeledConfigurationProperties.NewKeyBuilder<string>().SetKey("labeled-key-1")
                        .SetPropertyLabels(propertyLabels).Build();
            propertyConfig = ConfigurationProperties.NewConfigBuilder<LabeledKey<string>, string>().SetKey(key)
                        .SetDefaultValue("default-value-1").Build();
            property = manager.GetProperty(propertyConfig);
            Console.WriteLine(property);
            Assert.Equal("v-1-2", property.Value);

            TestDataCenterSetting Setting = new TestDataCenterSetting("labeled-key-1", "v-4-2", "sh-1-not-exist", "app-1");
            dynamicLabeledSource.updateSetting(Setting);
            Thread.Sleep(10);
            Console.WriteLine(property);
            Assert.Equal("v-4-2", property.Value);

            dynamicLabeledSource.removeSetting(Setting);
            Thread.Sleep(10);
            Console.WriteLine(property);
            Assert.Equal("v-1-2", property.Value);

            labels = new List<IPropertyLabel>();
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.DC_KEY, "sh-1-not-exist"));
            labels.Add(LabeledConfigurationProperties.NewLabel(TestDataCenterSetting.APP_KEY, "app-1"));
            propertyLabels = LabeledConfigurationProperties.NewLabels(labels, PropertyLabels.EMPTY);
            key = LabeledConfigurationProperties.NewKeyBuilder<string>().SetKey("labeled-key-1")
                        .SetPropertyLabels(propertyLabels).Build();
            propertyConfig = ConfigurationProperties.NewConfigBuilder<LabeledKey<string>, string>().SetKey(key)
                        .SetDefaultValue("default-value-1").Build();
            property = manager.GetProperty(propertyConfig);
            Console.WriteLine(property);
            Assert.Equal("v-0-2", property.Value);
        }
    }
}