using System;

namespace MyDotey.SCF.Labeled
{
    public interface ILabeledKey
    {
        /**
         * non-null
         */
        object Key { get; }

        /**
         * default to null
         */
        PropertyLabels Labels { get; }
    }

    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public abstract class LabeledKey<K> : ILabeledKey
    {
        object ILabeledKey.Key { get { return Key; } }

        /**
         * non-null
         */
        public abstract K Key { get; }

        /**
         * default to null
         */
        public abstract PropertyLabels Labels { get; }

        public interface IBuilder : IAbstractBuilder<IBuilder, LabeledKey<K>>
        {

        }

        public interface IAbstractBuilder<B, LK>
            where B : IAbstractBuilder<B, LK>
            where LK : LabeledKey<K>
        {
            /**
             * required
             * @see LabeledKey#getKey()
             */
            B SetKey(K key);

            /**
             * optional
             * @see LabeledKey#getLabels()
             */
            B SetPropertyLabels(PropertyLabels propertyLabels);

            LK Build();
        }
    }
}