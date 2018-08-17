using System;
using System.Collections.Generic;

using Xunit;
using NLog;
using NLog.Config;
using NLog.Targets;
using MyDotey.SCF.Type;
using MyDotey.SCF.Type.String;
using MyDotey.SCF.Source.StringProperty.PropertiesFile;

namespace MyDotey.SCF.Facade
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class StringPropertiesTest
    {
        public StringPropertiesTest()
        {
            var config = new LoggingConfiguration();
            var logconsole = new ConsoleTarget() { Name = "logconsole" };
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", LogLevel.Trace, logconsole));
            NLog.LogManager.Configuration = config;
        }
 
        protected virtual IConfigurationManager CreateManager(String fileName)
        {
            PropertiesFileConfigurationSourceConfig sourceConfig = StringPropertySources
                    .NewPropertiesFileSourceConfigBuilder().SetName("properties-source").SetFileName(fileName).Build();
            Console.WriteLine("source config: " + sourceConfig + "\n");
            ConfigurationManagerConfig managerConfig = ConfigurationManagers.NewConfigBuilder().SetName("test")
                    .AddSource(1, StringPropertySources.NewPropertiesFileSource(sourceConfig)).Build();
            Console.WriteLine("manager config: " + managerConfig + "\n");
            return ConfigurationManagers.NewManager(managerConfig);
        }

        protected virtual StringProperties CreateStringProperties(String fileName)
        {
            IConfigurationManager manager = CreateManager(fileName);
            return new StringProperties(manager);
        }

        [Fact]
        public virtual void TestGetProperties()
        {
            StringProperties stringProperties = CreateStringProperties("test.properties");
            IProperty<String, String> property = stringProperties.GetStringProperty("not-exist");
            Console.WriteLine("property: " + property + "\n");
            Assert.Null(property.Value);

            property = stringProperties.GetStringProperty("not-exist2", "default");
            Console.WriteLine("property: " + property + "\n");
            Assert.Equal("default", property.Value);

            property = stringProperties.GetStringProperty("exist", "default");
            Console.WriteLine("property: " + property + "\n");
            Assert.Equal("ok", property.Value);
        }

        [Fact]
        public virtual void TestGetTypedProperties()
        {
            StringProperties stringProperties = CreateStringProperties("test.properties");
            IProperty<String, int?> property = stringProperties.GetIntProperty("int-value");
            Console.WriteLine("property: " + property + "\n");
            int? expected = 1;
            Assert.Equal(expected, property.Value);

            IProperty<String, List<String>> property2 = stringProperties.GetListProperty("list-value");
            Console.WriteLine("property: " + property2 + "\n");
            List<String> expected2 = new List<string>() { "s1", "s2", "s3" };
            Assert.Equal(expected2, property2.Value);

            IProperty<String, Dictionary<String, String>> property3 = stringProperties.GetDictionaryProperty("map-value");
            Console.WriteLine("property: " + property3 + "\n");
            Dictionary<String, String> expected3 = new Dictionary<String, String>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected3, property3.Value);

            IProperty<String, List<int?>> property4 = stringProperties.GetListProperty("int-list-value",
                    StringToIntConverter.Default);
            Console.WriteLine("property: " + property4 + "\n");
            List<int?> expected4 = new List<int?>() { 1, 2, 3 };
            Assert.Equal(expected4, property4.Value);

            IProperty<String, Dictionary<int?, long?>> property5 = stringProperties.GetDictionaryProperty("int-long-map-value",
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
        public virtual void TestSameKeyDifferentConfig()
        {
            StringProperties stringProperties = CreateStringProperties("test.properties");
            IProperty<String, Dictionary<String, String>> property = stringProperties.GetDictionaryProperty("map-value");
            Dictionary<String, String> expected = new Dictionary<String, String>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected, property.Value);

            Assert.Throws<ArgumentException>(
                () => stringProperties.GetDictionaryProperty("map-value", StringToIntConverter.Default, StringToLongConverter.Default));
        }

        [Fact]
        public virtual void TestSameConfigSameProperty()
        {
            StringProperties stringProperties = CreateStringProperties("test.properties");
            IProperty<String, Dictionary<String, String>> property = stringProperties.GetDictionaryProperty("map-value");
            Dictionary<String, String> expected = new Dictionary<String, String>()
            {
                { "k1", "v1" },
                { "k2", "v2" },
                { "k3", "v3" }
            };
            Assert.Equal(expected, property.Value);

            IProperty<String, Dictionary<String, String>> property2 = stringProperties.GetDictionaryProperty("map-value");
            Console.WriteLine("property2: " + property + "\n");
            Assert.True(property == property2);
        }
    }
}