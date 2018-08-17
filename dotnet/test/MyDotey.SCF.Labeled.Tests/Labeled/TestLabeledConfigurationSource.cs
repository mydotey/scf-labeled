using System;
using System.Collections.Generic;
using System.Linq;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class TestLabeledConfigurationSource : AbstractLabeledConfigurationSource<ConfigurationSourceConfig>
    {
        protected IDictionary<TestDataCenterSetting, TestDataCenterSetting> _settings;

        public TestLabeledConfigurationSource(ConfigurationSourceConfig config,
                ICollection<TestDataCenterSetting> dataCenterSettings)
            : base(config)
        {
            if (dataCenterSettings == null)
                throw new ArgumentNullException("dataCenterSettings is null");

            Init();

            dataCenterSettings.ToList().ForEach(s =>
            {
                if (s != null)
                {
                    TestDataCenterSetting copy = (TestDataCenterSetting)s.Clone();
                    _settings[copy] = copy;
                }
            });
        }

        protected virtual void Init()
        {
            _settings = new Dictionary<TestDataCenterSetting, TestDataCenterSetting>();
        }

        protected override object GetPropertyValue(object key, ICollection<IPropertyLabel> labels)
        {
            if (key.GetType() != typeof(string))
                return null;

            TestDataCenterSetting setting = new TestDataCenterSetting((string)key, null, null, null);
            if (labels != null)
            {
                labels.ToList().ForEach(l =>
                {
                    if (l == null)
                        return;

                    if (object.Equals(l.Key, TestDataCenterSetting.DC_KEY))
                        setting.setDc((string)l.Value);

                    if (object.Equals(l.Key, TestDataCenterSetting.APP_KEY))
                        setting.setApp((string)l.Value);
                });
            }

            _settings.TryGetValue(setting, out TestDataCenterSetting labeledSetting);
            return labeledSetting == null ? null : labeledSetting.getValue();
        }
    }
}