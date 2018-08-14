using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilites.BarcodePicker.UnitTest
{
    class ScanBarcodeSimulation
    {
        private readonly List<string> m_FixedPositionBarcode;

        private List<BarcodeEntity> m_PickedBarcode;

        public ScanBarcodeSimulation(List<string> fixedPositionBarcode)
        {
            m_FixedPositionBarcode = fixedPositionBarcode;
            m_PickedBarcode = new List<BarcodeEntity>();
        }

        public List<BarcodeEntity> Scan(List<string> scannedBarcode)
        {
            ContinuousBarcodePicker picker = new ContinuousBarcodePicker(m_FixedPositionBarcode, false);
            picker.BarcodePicked += new ContinuousBarcodePicker.BarcodePickedEventHandler(OnBarcodePicked);
            picker.BeginPicking();
            foreach (string barcode in scannedBarcode)
                picker.AddScannedBarcode(barcode);

            while (!picker.IsFinished)
                System.Threading.Thread.Sleep(10);

            picker.EndPicking();
            return m_PickedBarcode;
        }

        private void OnBarcodePicked(List<BarcodeEntity> pickedBarcodes)
        {
            m_PickedBarcode.AddRange(pickedBarcodes);
        }
    }
}
