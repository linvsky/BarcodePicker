using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilites.BarcodePicker
{
    /// <summary>
    /// Indicates barcode possibility of its position
    /// </summary>
    public enum BarcodePossibility
    {
        /// <summary>
        /// Description: If a barcode can be on serval positions then it is Unreliable.
        /// e.g. Position X Barcode | Barcode | Position X+2 Barcode
        /// </summary>
        Unreliable = 0,

        /// <summary>
        /// Description: If a barcode can be inferred to a unique position but not 100% then it is MostLikely.
        /// e.g. Last Position Barcode | Barcode
        /// </summary>
        MostLikely = 1,

        /// <summary>
        /// Description: If a barcode is between two contiguous positions then it is Affirmative.
        /// e.g. 1: Position n Barcode | Barcode | Position n+1 Barcode
        /// e.g. 2: Position n Barcode | Barcode | Barcode | Position n+2 Barcode
        /// </summary>
        Affirmative = 2,
    }
}
