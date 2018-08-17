using System;

namespace MyDotey.SCF.Labeled
{
    /**
     * @author koqizhao
     *
     * Jun 15, 2018
     */
    public interface IPropertyLabel
    {
        /**
         * non-null
         */
        object Key { get; }

        /**
         * non-null
         */
        object Value { get; }
    }
}