using System;

using MyDotey.SCF.Labeled;

namespace MyDotey.SCF.Facade
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public class LabeledConfigurationManagers
    {
        protected LabeledConfigurationManagers()
        {

        }

        public static ILabeledConfigurationManager NewManager(ConfigurationManagerConfig config)
        {
            return new DefaultLabeledConfigurationManager(config);
        }
    }
}