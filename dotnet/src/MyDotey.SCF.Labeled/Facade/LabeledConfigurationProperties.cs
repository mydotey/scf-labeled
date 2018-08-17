using System;
using System.Collections.Generic;
using System.Linq;

using MyDotey.SCF.Labeled;

namespace MyDotey.SCF.Facade
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public class LabeledConfigurationProperties
    {
        protected LabeledConfigurationProperties()
        {

        }

        public static LabeledKey<K>.IBuilder NewKeyBuilder<K>()
        {
            return new DefaultLabeledKey<K>.Builder();
        }

        public static IPropertyLabel NewLabel(Object key, Object value)
        {
            return new DefaultPropertyLabel(key, value);
        }

        public static PropertyLabels NewLabels(params IPropertyLabel[] labels)
        {
            return NewLabels(labels.ToList(), null);
        }

        public static PropertyLabels NewLabels(ICollection<IPropertyLabel> labels)
        {
            return NewLabels(labels, null);
        }

        public static PropertyLabels NewLabels(ICollection<IPropertyLabel> labels, PropertyLabels alternative)
        {
            return new DefaultPropertyLabels(labels, alternative);
        }
    }
}