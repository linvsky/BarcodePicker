using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utilites.BarcodePicker
{
    public class ContinuousBarcodePicker
    {
        #region Events

        /// <summary>
        /// Event hanlder for barcode picked event
        /// </summary>
        /// <param name="pickedBarcodes">Dictionary for stroing picked barcodes. The key is position index and the value is corresponding tube barcode.</param>
        public delegate void BarcodePickedEventHandler(List<BarcodeEntity> pickedBarcodes);

        /// <summary>
        /// Event fired after all barcodes have been picked or scanning action done
        /// </summary>
        public BarcodePickedEventHandler BarcodePicked;

        #endregion

        #region Private Fields

        /// <summary>
        /// Fixed position barcodes
        /// </summary>
        private Dictionary<string, int> m_PosBarcodes;

        /// <summary>
        /// Indicates whether allow duplicate barcodes
        /// </summary>
        private bool m_AllowDuplicateBarcodes;

        /// <summary>
        /// Background thread for picking barcodes
        /// </summary>
        private Thread m_WorkingThread;

        /// <summary>
        /// Flag indicates whether is working for picking
        /// </summary>
        private bool m_IsWorking;

        /// <summary>
        /// Cache unprocessed barcodes
        /// </summary>
        private Queue<string> m_UnprocessedBarcodes;

        #endregion

        #region Properties

        /// <summary>
        /// Flag indicates whether the picking procedure is finished
        /// </summary>
        public bool IsFinished { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="fixedPositionBarcodes">All positions barcodes</param>
        /// <param name="allowDuplicateBarcodes">Whether allow duplicate barcodes</param>
        public ContinuousBarcodePicker(List<string> fixedPositionBarcodes, bool allowDuplicateBarcodes)
        {
            m_PosBarcodes = new Dictionary<string, int>();
            for (int i = 0; i < fixedPositionBarcodes.Count; i++)
                m_PosBarcodes[fixedPositionBarcodes[i]] = i + 1;
            m_IsWorking = false;
            m_UnprocessedBarcodes = new Queue<string>();
            m_AllowDuplicateBarcodes = allowDuplicateBarcodes;
        }

        #endregion

        #region Publics        

        /// <summary>
        /// Begin picking asynchronous
        /// </summary>
        public void BeginPicking()
        {
            if (!m_IsWorking)
            {
                m_IsWorking = true;
                m_WorkingThread = new Thread(Picking);
                m_WorkingThread.Start();
            }
        }

        /// <summary>
        /// End picking
        /// </summary>
        public void EndPicking()
        {
            if (m_UnprocessedBarcodes.Count > 0)
                Thread.Sleep(100);

            if (m_WorkingThread != null && m_WorkingThread.IsAlive)
            {
                // Inform current thread to stop
                m_IsWorking = false;

                int maxWaitingTimes = 5;
                int waitingTimes = 0;
                while (m_WorkingThread.IsAlive && waitingTimes++ < maxWaitingTimes)
                    Thread.Sleep(100);

                // Force to abort current working thread if it is still alive
                if (m_WorkingThread.IsAlive)
                {
                    try
                    {
                        m_WorkingThread.Abort();
                    }
                    catch
                    { }
                }
            }
        }

        /// <summary>
        /// Client call this function to add current barcode to pick
        /// </summary>
        /// <param name="currentBarcode">Current Scanned Barcode</param>
        public void AddScannedBarcode(string scannedBarcode)
        {
            if (!string.IsNullOrEmpty(scannedBarcode))
                m_UnprocessedBarcodes.Enqueue(scannedBarcode);
        }

        /// <summary>
        /// Reset the picking produce
        /// </summary>
        public void Reset()
        {
            EndPicking();
            BeginPicking();
        }

        #endregion

        #region Privates

        /// <summary>
        /// Main picking procedure
        /// </summary>
        private void Picking()
        {
            int sectionStart = -1;
            int sectionEnd = -1;
            bool isForwardScanning = true;
            Queue<string> sectionBarcodes = new Queue<string>();
            Queue<string> unsolvedSectionBarcodes = new Queue<string>();

            // Endless loop to dequeue barcodes and try to pick sample barcodes from them
            while (m_IsWorking)
            {
                // If no barcodes to pick then wait for a while
                if (m_UnprocessedBarcodes.Count == 0)
                {
                    IsFinished = true;
                    Thread.Sleep(50);
                    continue;
                }
                else
                {
                    IsFinished = false;
                }

                // Dequeue to get barcode
                string currentBarcode = m_UnprocessedBarcodes.Dequeue();

                // If it is a position barcode
                if (m_PosBarcodes.Keys.Contains(currentBarcode))
                {
                    // Calculate section boundaries
                    if (sectionStart == -1 && sectionEnd == -1)
                        sectionStart = m_PosBarcodes[currentBarcode];
                    else if (sectionStart != -1 && sectionEnd == -1)
                        sectionEnd = m_PosBarcodes[currentBarcode];
                    else
                    {
                        sectionStart = sectionEnd;
                        sectionEnd = m_PosBarcodes[currentBarcode];
                    }

                    // If the section is completely determined
                    // e.g. Position n | barcode | Position n+1
                    if (sectionStart != -1 && sectionEnd != -1)
                    {
                        isForwardScanning = sectionStart < sectionEnd;
                        BarcodePossibility posibility;
                        List<BarcodeEntity> results;

                        // Scan order is confirmed then pick barcodes in unsoloved section
                        if (unsolvedSectionBarcodes.Count > 0)
                        {
                            int unsolvedSectionStart;
                            int unsolvedSectionEnd;
                            if (isForwardScanning)
                            {
                                unsolvedSectionStart = 1;
                                unsolvedSectionEnd = sectionStart;
                                posibility = unsolvedSectionBarcodes.Count == unsolvedSectionEnd - 1 ? BarcodePossibility.MostLikely : BarcodePossibility.Unreliable;
                            }
                            else
                            {
                                unsolvedSectionStart = m_PosBarcodes.Count + 1;
                                unsolvedSectionEnd = sectionStart;
                                posibility = unsolvedSectionBarcodes.Count == m_PosBarcodes.Count + 1 - unsolvedSectionEnd ? BarcodePossibility.MostLikely : BarcodePossibility.Unreliable;
                            }

                            results = GenerateBarcodeEntity(unsolvedSectionStart, unsolvedSectionEnd, ref unsolvedSectionBarcodes, posibility);
                            if (BarcodePicked != null)
                                BarcodePicked(results);
                        }

                        // Pick barcodes in current section
                        if (sectionStart != sectionEnd)
                            posibility = sectionBarcodes.Count == Math.Abs(sectionStart - sectionEnd) ? BarcodePossibility.Affirmative : BarcodePossibility.Unreliable;
                        else
                            posibility = sectionStart == m_PosBarcodes.Count ? BarcodePossibility.MostLikely : BarcodePossibility.Unreliable;
                        results = GenerateBarcodeEntity(sectionStart, sectionEnd, ref sectionBarcodes, posibility);
                        sectionStart = sectionEnd;
                        sectionEnd = -1;
                        if (BarcodePicked != null)
                            BarcodePicked(results);
                    }
                    // If the section has no start position then move them into unsolved section.
                    // Once the scanning order has been confirmed we can process the unsolved section
                    // e.g. barcode | Position n | barcode | Position n+1
                    else if (sectionBarcodes.Count > 0 && sectionStart != -1)
                    {
                        while (sectionBarcodes.Count > 0)
                            unsolvedSectionBarcodes.Enqueue(sectionBarcodes.Dequeue());
                    }
                }
                else
                {
                    // Add sample barcode (non-position barcode) into section cache
                    sectionBarcodes.Enqueue(currentBarcode);

                    // If the section has no end position AND unsolved section is empty
                    // e.g. barcode | Last Position | barcode
                    if (((isForwardScanning && m_PosBarcodes.Count - (sectionStart - 1) == sectionBarcodes.Count)
                            || (!isForwardScanning && sectionStart - 1 == sectionBarcodes.Count && sectionEnd == -1))
                        && unsolvedSectionBarcodes.Count <= 0 && m_UnprocessedBarcodes.Count == 0)
                    {
                        int tempSectionStart;
                        int tempSectionEnd;
                        if (isForwardScanning)
                        {
                            tempSectionStart = sectionStart;
                            tempSectionEnd = m_PosBarcodes.Count + 1;
                        }
                        else
                        {
                            tempSectionStart = sectionStart;
                            tempSectionEnd = 1;
                        }

                        List<BarcodeEntity> results = GenerateBarcodeEntity(tempSectionStart, tempSectionEnd, ref sectionBarcodes, BarcodePossibility.MostLikely);
                        if (BarcodePicked != null)
                            BarcodePicked(results);
                    }
                }
            }

            // There are still some unused barcodes in section cache then make them as unreliable and add to the end
            // e.g. barcode | barcode | barcode (no position barcodes)
            if (sectionBarcodes.Count > 0)
            {
                int tempSectionStart;
                int tempSectionEnd;
                if (isForwardScanning)
                {
                    tempSectionStart = sectionStart == -1 ? 1 : sectionStart;
                    tempSectionEnd = m_PosBarcodes.Count + 1;
                }
                else
                {
                    tempSectionStart = sectionStart == -1 ? m_PosBarcodes.Count + 1 : sectionStart;
                    tempSectionEnd = 1;
                }

                BarcodePossibility posibility = sectionBarcodes.Count == m_PosBarcodes.Count ? BarcodePossibility.MostLikely : BarcodePossibility.Unreliable;
                List<BarcodeEntity> results = GenerateBarcodeEntity(tempSectionStart, tempSectionEnd, ref sectionBarcodes, posibility);
                if (BarcodePicked != null)
                    BarcodePicked(results);
            }
        }

        /// <summary>
        /// Generate final barcode entity by given section start and end, as well as its posibility
        /// </summary>
        /// <param name="sectionStart">Section start of given barcodes</param>
        /// <param name="sectionEnd">Section end of given barcodes</param>
        /// <param name="sectionBarcodes">Barcodes in the section</param>
        /// <param name="posibility">Posibility of given barcodes</param>
        /// <returns></returns>
        private List<BarcodeEntity> GenerateBarcodeEntity(int sectionStart, int sectionEnd, ref Queue<string> sectionBarcodes, BarcodePossibility posibility)
        {
            List<BarcodeEntity> results = new List<BarcodeEntity>();

            // Calculate the length of section and swap start with end if necessary
            int sectionLength = Math.Abs(sectionStart - sectionEnd);
            if (sectionLength == 0 && sectionBarcodes.Count > 0)
            {
                BarcodeEntity entity = new BarcodeEntity(sectionStart, sectionBarcodes.Dequeue(), posibility);
                results.Add(entity);

                // Remove remaining duplicated barcodes
                sectionBarcodes.Clear();
            }
            else if (sectionLength <= sectionBarcodes.Count)
            {
                for (int i = 0; i < sectionLength; i++)
                {
                    BarcodeEntity entity = new BarcodeEntity(sectionStart < sectionEnd ? (sectionStart + i) : (sectionStart - 1 - i), sectionBarcodes.Dequeue(), posibility);
                    results.Add(entity);
                }

                // Clear remaining invalid barcodes in current section
                sectionBarcodes.Clear();
            }
            else if (sectionLength > sectionBarcodes.Count)
            {
                int length = sectionBarcodes.Count;
                for (int i = 0; i < length; i++)
                {
                    BarcodeEntity entity = new BarcodeEntity(sectionStart < sectionEnd ? (sectionStart + i) : (sectionStart - 1 - i), sectionBarcodes.Dequeue(), BarcodePossibility.Unreliable);
                    results.Add(entity);
                }
            }

            return results;
        }

        #endregion
    }
}
