using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 19, 2018
     */
    public class DefaultPropertyLabels : PropertyLabels
    {
        private Dictionary<object, IPropertyLabel> _labelMap;
        private ICollection<IPropertyLabel> _labels;
        private PropertyLabels _alternative;

        public DefaultPropertyLabels(ICollection<IPropertyLabel> labels, PropertyLabels alternative)
        {
            if (labels == null || labels.Count == 0)
                throw new ArgumentNullException("labels is null or empty");

            _labelMap = new Dictionary<object, IPropertyLabel>();
            labels.ToList().ForEach(l =>
            {
                if (l != null)
                    _labelMap[l.Key] = l;
            });

            if (_labelMap.Count == 0)
                throw new ArgumentNullException("all elements of labels are null");
            _labels = ImmutableList.CreateRange(_labelMap.Values);

            _alternative = alternative;
        }

        public override ICollection<IPropertyLabel> Labels { get { return _labels; } }

        public override PropertyLabels Alternative { get { return _alternative; } }

        public override int GetHashCode()
        {
            int prime = 31;
            int result = 1;
            result = prime * result + ((_alternative == null) ? 0 : _alternative.GetHashCode());
            result = prime * result + ((_labelMap == null) ? 0 : _labelMap.HashCode());
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

            DefaultPropertyLabels labels = (DefaultPropertyLabels)obj;

            if (!_labelMap.Equal(labels._labelMap))
                return false;

            if (!object.Equals(_alternative, labels._alternative))
                return false;

            return true;
        }

        public override string ToString()
        {
            return string.Format("{0} {{ labels: {1}, alternative: {2} }}", GetType().Name, _labels, _alternative);
        }
    }
}