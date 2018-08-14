using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilites.BarcodePicker
{
    public class BatchBarcodePicker
    {
        private readonly List<string> m_FixedPositionBarcode;

        private List<BarcodeEntity> m_PickedBarcode;

        public BatchBarcodePicker(List<string> fixedPositionBarcode)
        {
            m_FixedPositionBarcode = fixedPositionBarcode;
            m_PickedBarcode = new List<BarcodeEntity>();
        }

        public Dictionary<int, BarcodeEntity> Scan(List<string> scannedBarcode)
        {
            ContinuousBarcodePicker picker = new ContinuousBarcodePicker(m_FixedPositionBarcode, false);
            picker.BarcodePicked += new ContinuousBarcodePicker.BarcodePickedEventHandler(OnBarcodePicked);
            picker.BeginPicking();
            foreach (string barcode in scannedBarcode)
                picker.AddScannedBarcode(barcode);

            while (!picker.IsFinished)
                System.Threading.Thread.Sleep(10);

            picker.EndPicking();

            // Merge results
            Dictionary<int, BarcodeEntity> mergedResults = new Dictionary<int, BarcodeEntity>();
            foreach (BarcodeEntity entity in m_PickedBarcode)
            {
                if (!mergedResults.ContainsKey(entity.Position))
                {
                    mergedResults[entity.Position] = new BarcodeEntity(entity.Position, entity.Barcode, entity.Possibility);
                }
                else
                {
                    if (entity.Possibility == BarcodePossibility.Affirmative
                        || (entity.Position == m_FixedPositionBarcode.Count && entity.Possibility == BarcodePossibility.MostLikely))
                    {
                        mergedResults[entity.Position].Barcode = entity.Barcode;
                        mergedResults[entity.Position].Possibility = entity.Possibility;
                    }
                }
            }
            return mergedResults;
        }

        private void OnBarcodePicked(List<BarcodeEntity> pickedBarcodes)
        {
            m_PickedBarcode.AddRange(pickedBarcodes);
        }
    }
}
