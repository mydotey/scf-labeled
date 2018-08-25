using System;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public class DefaultPropertyLabel : IPropertyLabel
    {
        private object _key;
        private object _value;

        public DefaultPropertyLabel(object key, object value)
        {
            if (key == null)
                throw new ArgumentNullException("key is null");

            if (value == null)
                throw new ArgumentNullException("value is null");

            _key = key;
            _value = value;
        }

        public virtual object Key { get { return _key; } }

        public virtual object Value { get { return _value; } }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((_key == null) ? 0 : _key.GetHashCode());
            result = prime * result + ((_value == null) ? 0 : _value.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            DefaultPropertyLabel label = (DefaultPropertyLabel)obj;

            if (!object.Equals(_key, label._key))
                return false;

            if (!object.Equals(_value, label._value))
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ key: {1}, value: {2} }}", GetType().Name, _key, _value);
        }
    }
}