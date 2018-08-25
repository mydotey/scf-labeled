using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 15, 2018
     */
    public abstract class PropertyLabels
    {
        /**
         * empty labels and no altenative
         */
        public static readonly PropertyLabels Empty = new EmptyPropertyLabels();

        /**
         * labels
         */
        public abstract ICollection<IPropertyLabel> Labels { get; }

        /**
         * if not configured for @see {@link PropertyLabels#getLabels()}, use the alternative to have a try
         * <p>
         * default to null
         */
        public abstract PropertyLabels Alternative { get; }

        private class EmptyPropertyLabels : PropertyLabels
        {
            private ICollection<IPropertyLabel> _labels = ImmutableList.CreateBuilder<IPropertyLabel>().ToImmutable();

            public override ICollection<IPropertyLabel> Labels { get { return _labels; } }

            public override PropertyLabels Alternative { get { return null; } }

            public override string ToString()
            {
                return "EMPTY";
            }
        }
    }
}