using System;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     * 
     * can handle both propertyConfig with normal key or with @see LabeledKey
     * <p>
     * for normal key propertyConfig, the same as normal configuration manager
     * <p>
     * for PropertyConfig&lt;LabeledKey&gt;, calling {@link LabeledConfigurationSource#getPropertyValue(org.mydotey.scf.PropertyConfig, java.util.Collection)}
     * instead of @see {@link org.mydotey.scf.ConfigurationSource#getPropertyValue(org.mydotey.scf.PropertyConfig)}
     */
    public interface ILabeledConfigurationManager : IConfigurationManager
    {

    }
}