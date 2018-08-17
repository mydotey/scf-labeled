using System;

using MyDotey.SCF.Labeled;

namespace MyDotey.SCF.Facade
{
    /**
     * @author koqizhao
     *
     * May 21, 2018
     */
    public class LabeledStringProperties : StringValueProperties<LabeledKey<String>, ILabeledConfigurationManager>
    {
        public LabeledStringProperties(ILabeledConfigurationManager manager)
            : base(manager)
        {

        }
    }
}