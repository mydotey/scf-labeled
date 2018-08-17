using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class TestDynamicLabeledConfigurationSource : TestLabeledConfigurationSource
    {
        private ConcurrentDictionary<TestDataCenterSetting, TestDataCenterSetting> _concurrentSettings;

        public TestDynamicLabeledConfigurationSource(ConfigurationSourceConfig config,
            ICollection<TestDataCenterSetting> dataCenterSettings)
            : base(config, dataCenterSettings)
        {
            _concurrentSettings = (ConcurrentDictionary<TestDataCenterSetting, TestDataCenterSetting>)_settings;
        }

        protected override void Init()
        {
            _settings = new ConcurrentDictionary<TestDataCenterSetting, TestDataCenterSetting>();
        }

        public void updateSetting(TestDataCenterSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting is null");

            if (setting.getKey() == null)
                throw new ArgumentNullException("setting.key is null");

            _settings.TryGetValue(setting, out TestDataCenterSetting oldValue);
            if (oldValue != null)
            {
                if (object.Equals(oldValue.getValue(), setting.getValue()))
                    return;
            }

            setting = (TestDataCenterSetting)setting.Clone();
            _settings[setting] = setting;

            RaiseChangeEvent();
        }

        public void removeSetting(TestDataCenterSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException("setting is null");

            if (setting.getKey() == null)
                throw new ArgumentNullException("setting.key is null");

            _concurrentSettings.TryRemove(setting, out TestDataCenterSetting oldValue);
            if (oldValue == null)
                return;

            RaiseChangeEvent();
        }
    }
}