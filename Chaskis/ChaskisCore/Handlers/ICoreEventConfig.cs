//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    public interface ICoreEventConfig<TConfig>
    {
        /// <summary>
        /// Ensure the implementation is configured correctly.
        /// Throws a <see cref="SethCS.Exceptions.ListedValidationException"/> if not.
        /// </summary>
        void Validate();

        /// <summary>
        /// Makes a deep-copy of this object.
        /// </summary>
        TConfig Clone();
    }
}
