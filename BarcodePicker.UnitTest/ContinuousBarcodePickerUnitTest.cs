using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Utilites.BarcodePicker.UnitTest
{
    [TestClass]
    public class ContinuousBarcodePickerUnitTest
    {
        #region Privates

        private readonly List<string> m_FixedPositionBarcode = new List<string>() { "Pos1", "Pos2", "Pos3", "Pos4", "Pos5", "Pos6" };

        private void CompareResults(Dictionary<int, BarcodeEntity> expectedResults, List<BarcodeEntity> pickedBarcodes)
        {
            Dictionary<int, BarcodeEntity> actualResults = new Dictionary<int, BarcodeEntity>();
            foreach (BarcodeEntity entity in pickedBarcodes)
            {
                if (!actualResults.ContainsKey(entity.Position))
                {
                    actualResults[entity.Position] = new BarcodeEntity(entity.Position, entity.Barcode, entity.Possibility);
                }
                else if (actualResults[entity.Position].Possibility < entity.Possibility || actualResults[entity.Position].Possibility == BarcodePossibility.MostLikely)
                {
                    actualResults[entity.Position].Barcode = entity.Barcode;
                    actualResults[entity.Position].Possibility = entity.Possibility;
                }
            }

            // Compare expected results with acutal results
            foreach (int position in expectedResults.Keys)
            {
                Assert.IsTrue(actualResults.ContainsKey(position),
                    string.Format("Existing Check   on position {0} expected {1} but actually is empty", position, expectedResults[position].Barcode));

                Assert.AreEqual(expectedResults[position].Barcode, actualResults[position].Barcode,
                    string.Format("Barcode Check    on position {0} expected {1} but actually is {2}", position, expectedResults[position], actualResults[position].Barcode));

                Assert.AreEqual(expectedResults[position].Possibility, actualResults[position].Possibility,
                    string.Format("Posibility Check on position {0} expected {1} but actually is {2}", position, expectedResults[position].Possibility, actualResults[position].Possibility));
            }
        }

        #endregion

        #region TestMethod - Forward Order

        [TestMethod]
        public void Test_Forward_FullBarcodes_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1], "Sample2",
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5], "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingFirstPositionBarcode_1MostLikely2to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                                           "Sample1",
                m_FixedPositionBarcode[1], "Sample2",
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5], "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.MostLikely) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingLastPositionBarcode_1to4Affirmative5to6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1], "Sample2",
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                                           "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.MostLikely) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingLastTwoBarcodes_1to4Affirmative5Unreliable6None()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1], "Sample2",
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Unreliable) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingAllPositionBarcodes_1to6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample1",
                "Sample2",
                "Sample3",
                "Sample4",
                "Sample5",
                "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.MostLikely) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.MostLikely) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.MostLikely) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.MostLikely) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.MostLikely) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingTwoMidContinuousPositionBarcodes_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                                           "Sample2",
                                           "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5], "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Forward_MissingThreeMidContinuousBarcodes_1to2Unreliable3None4to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                                           "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5], "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Unreliable) },
                { 2, new BarcodeEntity(2, "Sample3", BarcodePossibility.Unreliable) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        #endregion

        #region TestMethod - Reverse Order

        [TestMethod]
        public void Test_Reverse_FullBarcodes_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample5", m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample1", m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingFirstPositionBarcode_1to4Affirmative5to6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6",
                "Sample5", m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample1", m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.MostLikely) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingLastPositionBarcode_1MostLikely2to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample5", m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample1" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.MostLikely) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingLastTwoBarcodes_1None2to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample5", m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingLastThreeBarcodes_1None2Unreliable3to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample5", m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Unreliable) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingTwoMidContinuousBarcodes_1to3Affirmative4None5Unreliable6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample4", m_FixedPositionBarcode[3],
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample1", m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample4", BarcodePossibility.Unreliable) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_Reverse_MissingThreeMidContinuousBarcodes_1to2Affirmative4to5Unreliable6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                "Sample5",
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample1", m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample3", BarcodePossibility.Unreliable) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Unreliable) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        #endregion

        #region TestMethod - Mixed Order

        [TestMethod]
        public void Test_MixOrder_ForwardThreePositionsReverseOnePosition_1to2Affirmative3Unreliable()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2], "Sample3",
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Unreliable) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_MixOrder_ForwardThreePositionsReverseOnePositionForwardToEnd_1to2Affirmative3Unreliable()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2], "Sample3",
                "Sample3", m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                "Sample2",
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3], "Sample4",
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5], "Sample6" };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_MixOrder_ForwardOddReverseEven_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3],
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5],
                "Sample6", m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                           m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                           m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_MixOrder_ReverseEvenForwardOdd_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                "Sample6", m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                           m_FixedPositionBarcode[2],
                "Sample2", m_FixedPositionBarcode[1],
                           m_FixedPositionBarcode[0],
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3],
                m_FixedPositionBarcode[4], "Sample5",
                m_FixedPositionBarcode[5] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        [TestMethod]
        public void Test_MixOrder_ForwardSingleReverseSingle_1to5Affirmative6MostLikely()
        {
            List<string> scannedBarcode = new List<string>() {
                m_FixedPositionBarcode[0], "Sample1",
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2],
                m_FixedPositionBarcode[3],
                m_FixedPositionBarcode[4],
                m_FixedPositionBarcode[5],
                "Sample6", m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[4],
                           m_FixedPositionBarcode[3],
                           m_FixedPositionBarcode[2],
                           m_FixedPositionBarcode[1],
                           m_FixedPositionBarcode[0],
                m_FixedPositionBarcode[0],
                m_FixedPositionBarcode[1], "Sample2",
                m_FixedPositionBarcode[2],
                m_FixedPositionBarcode[3],
                m_FixedPositionBarcode[4],
                m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[5],
                "Sample5", m_FixedPositionBarcode[4],
                           m_FixedPositionBarcode[3],
                           m_FixedPositionBarcode[2],
                           m_FixedPositionBarcode[1],
                           m_FixedPositionBarcode[0],
                m_FixedPositionBarcode[0],
                m_FixedPositionBarcode[1],
                m_FixedPositionBarcode[2], "Sample3",
                m_FixedPositionBarcode[3],
                m_FixedPositionBarcode[4],
                m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[5],
                           m_FixedPositionBarcode[4],
                "Sample4", m_FixedPositionBarcode[3],
                           m_FixedPositionBarcode[2],
                           m_FixedPositionBarcode[1],
                           m_FixedPositionBarcode[0] };
            ScanBarcodeSimulation scanSimulation = new ScanBarcodeSimulation(m_FixedPositionBarcode);
            List<BarcodeEntity> pickedBarcodes = scanSimulation.Scan(scannedBarcode);

            Dictionary<int, BarcodeEntity> expectedResults = new Dictionary<int, BarcodeEntity>()
            {
                { 1, new BarcodeEntity(1, "Sample1", BarcodePossibility.Affirmative) },
                { 2, new BarcodeEntity(2, "Sample2", BarcodePossibility.Affirmative) },
                { 3, new BarcodeEntity(3, "Sample3", BarcodePossibility.Affirmative) },
                { 4, new BarcodeEntity(4, "Sample4", BarcodePossibility.Affirmative) },
                { 5, new BarcodeEntity(5, "Sample5", BarcodePossibility.Affirmative) },
                { 6, new BarcodeEntity(6, "Sample6", BarcodePossibility.MostLikely) },
            };
            CompareResults(expectedResults, pickedBarcodes);
        }

        #endregion
    }
}
