using System;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * May 17, 2018
     */
    public class DefaultLabeledKey<K> : LabeledKey<K>, ICloneable
    {
        private K _key;
        private PropertyLabels _labels;

        private volatile int _hashCode;

        protected DefaultLabeledKey()
        {

        }

        public override K Key { get { return _key; } }

        public override PropertyLabels Labels { get { return _labels; } }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("{0} {{ key: {1}, labels: {2} }}", GetType().Name, Key, Labels);
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
            {
                int prime = 31;
                int result = 1;
                result = prime * result + ((_key == null) ? 0 : _key.GetHashCode());
                result = prime * result + ((_labels == null) ? 0 : _labels.GetHashCode());
                _hashCode = result;
            }

            return _hashCode;
        }

        public override bool Equals(object other)
        {
            if (object.ReferenceEquals(this, other))
                return true;

            if (other == null)
                return false;

            if (GetType() != other.GetType())
                return false;

            if (GetHashCode() != other.GetHashCode())
                return false;

            DefaultLabeledKey<K> labeledKey = (DefaultLabeledKey<K>)other;

            if (!object.Equals(_key, labeledKey._key))
                return false;

            if (!object.Equals(_labels, labeledKey._labels))
                return false;

            return true;
        }

        public class Builder : DefaultAbstractBuilder<LabeledKey<K>.IBuilder, LabeledKey<K>>, LabeledKey<K>.IBuilder
        {

        }

        public abstract class DefaultAbstractBuilder<B, LK> : LabeledKey<K>.IAbstractBuilder<B, LK>
            where B : LabeledKey<K>.IAbstractBuilder<B, LK>
            where LK : LabeledKey<K>
        {
            private DefaultLabeledKey<K> _labeledKey;

            protected DefaultAbstractBuilder()
            {
                _labeledKey = NewLabeledKey();
            }

            protected virtual DefaultLabeledKey<K> NewLabeledKey()
            {
                return new DefaultLabeledKey<K>();
            }

            protected virtual DefaultLabeledKey<K> GetLabeledKey()
            {
                return _labeledKey;
            }

            public virtual B SetKey(K key)
            {
                GetLabeledKey()._key = key;
                return (B)(object)this;
            }

            public virtual B SetPropertyLabels(PropertyLabels labels)
            {
                GetLabeledKey()._labels = labels;
                return (B)(object)this;
            }

            public virtual LK Build()
            {
                if (GetLabeledKey()._key == null)
                    throw new ArgumentNullException("key is null");
                if (GetLabeledKey()._labels == null)
                    throw new ArgumentNullException("labels is null");

                return (LK)_labeledKey.Clone();
            }
        }
    }
}