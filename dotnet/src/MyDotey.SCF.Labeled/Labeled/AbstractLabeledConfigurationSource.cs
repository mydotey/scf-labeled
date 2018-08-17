using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
using sType = System.Type;

using MyDotey.SCF.Facade;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     * 
     * May 16, 2018
     */
    public abstract class AbstractLabeledConfigurationSource<C> : AbstractConfigurationSource<C>, ILabeledConfigurationSource
            where C : ConfigurationSourceConfig
    {
        private MethodInfo GetNoLabelPropertyValueMethod;

        public AbstractLabeledConfigurationSource(C config)
            : base(config)
        {
            GetNoLabelPropertyValueMethod = GetType().GetMethods().Where(m => m.Name == "GetPropertyValue"
                && m.IsGenericMethod && m.GetGenericArguments().Length == 2
                && m.GetParameters().Length == 1).Single();
        }

        public override V GetPropertyValue<K, V>(PropertyConfig<K, V> config)
        {
            if (!(config.Key is ILabeledKey))
                return base.GetPropertyValue(config);

            IPropertyConfig noLabelConfig = AbstractLabeledConfigurationSource.MakeNoLabelConfig(config);
            return (V)GetNoLabelPropertyValue(noLabelConfig);
        }

        protected override object GetPropertyValue(object key)
        {
            return GetPropertyValue(key, AbstractLabeledConfigurationSource.Empty);
        }

        public virtual V GetPropertyValue<K, V>(PropertyConfig<K, V> noLabelConfig, ICollection<IPropertyLabel> labels)
        {
            object value = GetPropertyValue(noLabelConfig.Key, labels);
            return Convert(noLabelConfig, value);
        }

        protected abstract object GetPropertyValue(object key, ICollection<IPropertyLabel> labels);

        protected virtual object GetNoLabelPropertyValue(IPropertyConfig noLabelPropertyConfig)
        {
            MethodInfo genericMethod = GetNoLabelPropertyValueMethod
                .MakeGenericMethod(noLabelPropertyConfig.GetType().GetGenericArguments());
            return genericMethod.Invoke(this, new object[] { noLabelPropertyConfig });
        }
    }

    internal static class AbstractLabeledConfigurationSource
    {
        public static readonly ICollection<IPropertyLabel> Empty = ImmutableList.CreateBuilder<IPropertyLabel>()
            .ToImmutable();

        private static readonly MethodInfo RemoveLabelsMethod = typeof(AbstractLabeledConfigurationSource)
            .GetMethod("RemoveLabels", BindingFlags.Static | BindingFlags.NonPublic);

        static AbstractLabeledConfigurationSource()
        {
        }

        private static PropertyConfig<K, V> RemoveLabels<K, V>(PropertyConfig<LabeledKey<K>, V> config)
        {
            return ConfigurationProperties.NewConfigBuilder<K, V>().SetKey(config.Key.Key)
                     .SetDefaultValue(config.DefaultValue)
                     .AddValueConverters(config.ValueConverters).SetValueFilter(config.ValueFilter).Build();
        }

        public static IPropertyConfig MakeNoLabelConfig(IPropertyConfig labeledPropertyConfig)
        {
            sType kType = labeledPropertyConfig.Key.GetType().GetGenericArguments()[0];
            sType vType = labeledPropertyConfig.ValueType;
            MethodInfo genericMethod = RemoveLabelsMethod.MakeGenericMethod(kType, vType);
            return (IPropertyConfig)genericMethod.Invoke(null, new object[] { labeledPropertyConfig });
        }
    }
}