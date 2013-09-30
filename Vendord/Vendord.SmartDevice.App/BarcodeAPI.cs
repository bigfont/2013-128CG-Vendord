using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Symbol.Barcode2;

namespace Vendord.SmartDevice.App
{
    public class BarcodeAPI
    {
        private const int SCAN_TIMEOUT_MILLISECONDS = 5 * 60 * 1000; // five minutes in milliseconds.

        private Barcode2 myScannerAPI = null;

        public BarcodeAPI(Barcode2.OnStatusHandler statusHandler, Barcode2.OnScanHandler scanHandler)
        {
            InstantiateScannerAPI();
            ConfigureTheScannerAPI();                        
            EnableTheReader();
            RegisterForStatusEvent(statusHandler);
            RegisterForScanEvent(scanHandler);            
        }

        public void Scan()
        {
            if (myScannerAPI != null)
            {
                // Issue an asynchronous scan meaning the trigger is live until the Scan completes.
                // Fire the OnStatus event...
                // immediately or 
                // if the trigger fires with no barcode.
                // Fire the OnScan event...
                // if the trigger fires with a barcode,
                // if the timeout completes, or
                // if ScanCancel is called.
                // Triggers include the hardware trigger AND/OR a software trigger.                
                myScannerAPI.Scan(SCAN_TIMEOUT_MILLISECONDS);
            }
        }

        public void StopScan()
        {

        }

        private void ConfigureTheScannerAPI()
        {
            // aim type
            // remains idle until the user presses the trigger
            // pressing the trigger starts a decoding session
            // a session ends when 1/ decoding completes 2/ BeamTimer expires or 3/ user releases trigger.
            myScannerAPI.Config.Reader.ReaderSpecific.LaserSpecific.AimType =
                AIM_TYPE.AIM_TYPE_TRIGGER;

            // trigger mode
            // aim the user must manually press the trigger
            myScannerAPI.Config.TriggerMode =
                TRIGGERMODES.HARD;

            
            myScannerAPI.Config.Reader.Set();
        }

        private void InstantiateScannerAPI()
        {
            if (myScannerAPI == null)
            {
                // get the currently available devices
                Device[] supportedDevices = Devices.SupportedDevices;

                // use the first available laser device
                Device myDevice =
                    supportedDevices.First<Device>(d => d.DeviceType == DEVICETYPES.INTERNAL_LASER1);

                // create the barcode reader
                myScannerAPI = new Barcode2(myDevice);                                
            }            
        }

        private void EnableTheReader()
        {
            if (myScannerAPI != null)
            {
                // This method does NOT make the scanner scan nor turn on the laser.
                myScannerAPI.Enable();
            }
        }

        private void RegisterForStatusEvent(Barcode2.OnStatusHandler eventHandler)
        {
            myScannerAPI.OnStatus += eventHandler;
        }

        private void RegisterForScanEvent(Barcode2.OnScanHandler eventHandler)
        {
            myScannerAPI.OnScan += eventHandler;
        }
    }


}
