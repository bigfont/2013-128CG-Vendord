[module:
    System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.DocumentationRules", "*",
        Justification = "Reviewed. Suppression of all documentation rules is OK here.")]

namespace Vendord.SmartDevice.App
{
    using System;
    using System.Collections.Generic;
    using System.Linq;    
    using System.Text;
    using Symbol.Barcode2;

    public class BarcodeAPI
    {
        public bool IsScanning { get; set; }

        private const int ScanTimeoutMilliseconds = 5 * 60 * 1000; // five minutes in milliseconds.

        private Barcode2 myScannerAPI = null;        

        public BarcodeAPI(Barcode2.OnStatusHandler statusHandler, Barcode2.OnScanHandler scanHandler)
        {
            this.InstantiateScannerAPI();
            this.ConfigureTheScannerAPI();
            this.EnableTheReader();
            this.RegisterForStatusEvent(statusHandler);
            this.RegisterForScanEvent(scanHandler);            
        }

        public void Scan()
        {
            if (this.myScannerAPI != null)
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
                this.myScannerAPI.Scan(ScanTimeoutMilliseconds);
                this.IsScanning = true;
            }
        }

        public void StopScan()
        {
            this.myScannerAPI.ScanCancel();
            this.IsScanning = false;
        }

        private void ConfigureTheScannerAPI()
        {
            // aim type
            // remains idle until the user presses the trigger
            // pressing the trigger starts a decoding session
            // a session ends when 1/ decoding completes 2/ BeamTimer expires or 3/ user releases trigger.
            this.myScannerAPI.Config.Reader.ReaderSpecific.LaserSpecific.AimType =
                AIM_TYPE.AIM_TYPE_TRIGGER;

            // trigger mode
            // aim the user must manually press the trigger
            this.myScannerAPI.Config.TriggerMode =
                TRIGGERMODES.HARD;

            this.myScannerAPI.Config.Reader.Set();
        }

        private void InstantiateScannerAPI()
        {
            if (this.myScannerAPI == null)
            {
                // get the currently available devices
                Device[] supportedDevices = Devices.SupportedDevices;

                // use the first available laser device
                Device myDevice =
                    supportedDevices.First<Device>(d => d.DeviceType == DEVICETYPES.INTERNAL_LASER1);

                // create the barcode reader
                this.myScannerAPI = new Barcode2(myDevice);                                
            }            
        }

        private void EnableTheReader()
        {
            if (this.myScannerAPI != null)
            {
                // This method does NOT make the scanner scan nor turn on the laser.
                this.myScannerAPI.Enable();
            }
        }

        private void RegisterForStatusEvent(Barcode2.OnStatusHandler eventHandler)
        {
            this.myScannerAPI.OnStatus += eventHandler;
        }

        private void RegisterForScanEvent(Barcode2.OnScanHandler eventHandler)
        {
            this.myScannerAPI.OnScan += eventHandler;
        }
    }
}
