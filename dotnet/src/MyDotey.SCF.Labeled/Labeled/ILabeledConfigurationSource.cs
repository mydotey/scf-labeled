using System;
using System.Collections.Generic;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 15, 2018
     */
    public interface ILabeledConfigurationSource : IConfigurationSource
    {
        /**
         * @param noLabelConfig normal config with plain key (non-labeled-key)
         */
        V GetPropertyValue<K, V>(PropertyConfig<K, V> noLabelConfig, ICollection<IPropertyLabel> labels);
    }
}