using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilites.BarcodePicker
{
    public class BarcodeEntity
    {
        public int Position { get; set; }

        public string Barcode { get; set; }

        public BarcodePossibility Possibility { get; set; }

        public object Tag { get; set; }

        public BarcodeEntity(int position, string barcode, BarcodePossibility possibility)
        {
            Position = position;
            Barcode = barcode;
            Possibility = possibility;
            Tag = null;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2}) {3}", Position, Barcode, Possibility, Tag != null ? Tag.ToString() : string.Empty);
        }
    }
}
