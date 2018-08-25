using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using sType = System.Type;

using NLog;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 15, 2018
     */
    public class DefaultLabeledConfigurationManager : DefaultConfigurationManager, ILabeledConfigurationManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger(typeof(DefaultLabeledConfigurationManager));

        private MethodInfo NoLabelGetPropertyValueMethod = null;
        private MethodInfo LabeledGetPropertyValueMethod = null;

        public DefaultLabeledConfigurationManager(ConfigurationManagerConfig config)
            : base(config)
        {
            MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            NoLabelGetPropertyValueMethod = methods.Where(m => m.Name == "GetPropertyValue"
                && m.IsGenericMethod && m.GetGenericArguments().Length == 2
                && m.GetParameters().Length == 2).Single();

            methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            LabeledGetPropertyValueMethod = methods.Where(m => m.Name == "GetPropertyValue"
                && m.IsGenericMethod && m.GetGenericArguments().Length == 2
                && m.GetParameters().Length == 3).Single();
        }

        public override V GetPropertyValue<K, V>(PropertyConfig<K, V> propertyConfig)
        {
            if (propertyConfig == null)
                throw new ArgumentNullException("propertyConfig is null");

            if (!(propertyConfig.Key is ILabeledKey))
                return base.GetPropertyValue(propertyConfig);

            IPropertyConfig noLabelPropertyConfig =
                AbstractLabeledConfigurationSource.MakeNoLabelConfig(propertyConfig);
            for (PropertyLabels propertyLabels = ((ILabeledKey)propertyConfig.Key).Labels;
                propertyLabels != null; propertyLabels = propertyLabels.Alternative)
            {
                foreach (IConfigurationSource source in SortedSources.Values)
                {
                    if (!(source is ILabeledConfigurationSource) && propertyLabels.Labels.Count != 0)
                        continue;

                    V value = default(V);
                    if (propertyLabels.Labels.Count == 0)
                        value = (V)GetPropertyValue(source, noLabelPropertyConfig);
                    else
                        value = (V)GetPropertyValue(source, noLabelPropertyConfig, propertyLabels.Labels);

                    value = ApplyValueFilter(propertyConfig, value);

                    if (!object.Equals(value, default(V)))
                        return value;
                }
            }

            return propertyConfig.DefaultValue;
        }

        protected virtual V GetPropertyValue<K, V>(ILabeledConfigurationSource source,
            PropertyConfig<K, V> propertyConfig, ICollection<IPropertyLabel> labels)
        {
            V value = default(V);
            try
            {
                value = source.GetPropertyValue(propertyConfig, labels);
            }
            catch (Exception e)
            {
                string message = string.Format(
                        "error occurred when getting property value, ignore the source. source: {0}, propertyConfig: {1}",
                        source, propertyConfig);
                Logger.Error(e, message);
            }

            return value;
        }

        protected virtual object GetPropertyValue(object source, IPropertyConfig noLabelPropertyConfig)
        {
            MethodInfo genericMethod = NoLabelGetPropertyValueMethod.MakeGenericMethod(
                noLabelPropertyConfig.GetType().GetGenericArguments());
            return genericMethod.Invoke(this, new object[] { source, noLabelPropertyConfig });
        }

        protected virtual object GetPropertyValue(
            object source, IPropertyConfig noLabelPropertyConfig, object labels)
        {
            MethodInfo genericMethod = LabeledGetPropertyValueMethod.MakeGenericMethod(
                noLabelPropertyConfig.GetType().GetGenericArguments());
            return genericMethod.Invoke(this, new object[] { source, noLabelPropertyConfig, labels });
        }
    }
}