//--------------------------------------------------------------------
// FILENAME: API.cs
//
// Copyright © 2011 Motorola Solutions, Inc. All rights reserved.
//
// DESCRIPTION: Implements the code which communicates with the 
//					EMDK for .NET scanner API - Symbol.Barcode2.
//
// NOTES:
//
// 
//--------------------------------------------------------------------

using System;
using System.Windows.Forms;
using Symbol.Barcode2;

/// <summary>
/// The namespace for CS_Barcode2Sample1.
/// </summary>
namespace CS_Barcode2Sample1
{

	/// <summary>
	/// The class which communicates with the EMDK for .NET scanner API 
	/// - Symbol.Barcode2. 
	/// </summary>
	class API
	{
		private Barcode2 myBarcode2 = null;

		/// <summary>
		/// Initialize the Barcode2 object.
		/// </summary>
		public bool InitBarcode()
		{
			// If the Barcode2 object is already initialized then fail the initialization.
            if (myBarcode2 != null)
			{
				return false;
			}
			else // Else initialize the reader.
			{
				try
				{
					// Get the device selected by the user.
					Device MyDevice =
						SelectDevice.Select(
						"Barcode",
						Symbol.Barcode2.Devices.SupportedDevices);

					if (MyDevice == null)
					{
						MessageBox.Show(Resources.GetString("NoDeviceSelected"), Resources.GetString("SelectDevice"));
						return false;
					}

					// Create the reader, based on selected device.
                    myBarcode2 = new Barcode2(MyDevice);
                    
                    // In this sample, we are setting the aim type to trigger. 
                    switch (myBarcode2.Config.Reader.ReaderType)
                    {
                        case READER_TYPES.READER_TYPE_IMAGER:
                            myBarcode2.Config.Reader.ReaderSpecific.ImagerSpecific.AimType = AIM_TYPE.AIM_TYPE_TRIGGER;
                            break;
                        case READER_TYPES.READER_TYPE_LASER:
                            myBarcode2.Config.Reader.ReaderSpecific.LaserSpecific.AimType = AIM_TYPE.AIM_TYPE_TRIGGER;
                            break;
                    }
                    myBarcode2.Config.Reader.Set();
				}

				catch (OperationFailureException ex)
				{
					MessageBox.Show(Resources.GetString("InitBarcode")+"\n" +
						Resources.GetString("OperationFailure") + "\n" + ex.Message +
						"\n" +
						Resources.GetString("Result") +" = " + (Results)((uint)ex.Result)
						);

					return false;
				}
				catch (InvalidRequestException ex)
				{
                    MessageBox.Show(Resources.GetString("InitBarcode") + "\n" +
						Resources.GetString("InvalidRequest") + "\n" +
						ex.Message);

					return false;
				}
				catch (InvalidIndexerException ex)
				{
                    MessageBox.Show(Resources.GetString("InitBarcode") + "\n" +
                        Resources.GetString("InvalidIndexer") + "\n" +
						ex.Message);

					return false;
				};

				return true;
			}
		}

		/// <summary>
		/// Stop reading and disable/close the Barcode2 object.
		/// </summary>
		public void TermBarcode()
		{
			// If we have a reader
			if (myBarcode2 != null)
			{
				try
				{
					// stop all the notifications.
					StopScan();

					// Free it up.
					myBarcode2.Dispose();

					// Make the reference null.
                    myBarcode2 = null;
				}

				catch (OperationFailureException ex)
				{
					MessageBox.Show(Resources.GetString("TermBarcode") + "\n" +
                        Resources.GetString("OperationFailure") + "\n" + ex.Message +
						"\n" +
                        Resources.GetString("Result") + " = " + (Results)((uint)ex.Result)
						);
				}
				catch (InvalidRequestException ex)
				{
                    MessageBox.Show(Resources.GetString("TermBarcode") + "\n" +
                        Resources.GetString("InvalidRequest") + "\n" +
						ex.Message);
				}
				catch (InvalidIndexerException ex)
				{
                    MessageBox.Show(Resources.GetString("TermBarcode") + "\n" +
                        Resources.GetString("InvalidIndexer") + "\n" +
						ex.Message);
				};
			}
		}

		/// <summary>
		/// Start a scan.
		/// </summary>
		public void StartScan(bool TriggerSoftAlways)
		{
            if (myBarcode2 != null)
            {
                try
                {
                    if (TriggerSoftAlways)
                    {
                        if (myBarcode2.Config.TriggerMode != TRIGGERMODES.SOFT_ALWAYS)
                        {
                            myBarcode2.Config.TriggerMode = TRIGGERMODES.SOFT_ALWAYS;
                        }
                    }
                    else
                    {
                        myBarcode2.Config.TriggerMode = TRIGGERMODES.HARD;
                    }

                    // By default, the assemly Symbol.Barcode2 will decode the barcodes whose data length is less than or equal to 55.
                    // The user will have to increase ScanDataSize in order to get any longer lengths decoded.
                    myBarcode2.Config.ScanDataSize = (int)(Symbol.Barcode2.ReaderDataLengths.MaximumLabel);
                    // Submit a scan.
                    myBarcode2.Scan(5000);
                }

                catch (Symbol.Exceptions.OperationFailureException ex)
                {
                    MessageBox.Show(Resources.GetString("StartScan") + "\n" +
                        Resources.GetString("OperationFailure") + "\n" + ex.Message +
                        "\n" +
                        Resources.GetString("Result") + " = " + (Symbol.Results)((uint)ex.Result));
                }
                catch (Symbol.Exceptions.InvalidRequestException ex)
                {
                    MessageBox.Show(Resources.GetString("StartScan") + "\n" +
                        Resources.GetString("InvalidRequest") + "\n" +
                        ex.Message);

                }
                catch (Symbol.Exceptions.InvalidIndexerException ex)
                {
                    MessageBox.Show(Resources.GetString("StartScan") + "\n" +
                        Resources.GetString("InvalidIndexer") + "\n" +
                        ex.Message);

                }
            }
		}

		/// <summary>
		/// Stop all reads on the reader.
		/// </summary>
		public void StopScan()
		{
			//If we have a reader
            if (myBarcode2 != null)
			{
				try
				{
					// Flush (Cancel all pending reads).
                    myBarcode2.ScanCancel();
				}

				catch (OperationFailureException ex)
				{
					MessageBox.Show(Resources.GetString("StopScan") + "\n" +
                        Resources.GetString("OperationFailure") + "\n" + ex.Message +
						"\n" +
                        Resources.GetString("Result") + " = " + (Results)((uint)ex.Result)
						);
				}
				catch (InvalidRequestException ex)
				{
                    MessageBox.Show(Resources.GetString("StopScan") + "\n" +
                        Resources.GetString("InvalidRequest") + "\n" +
						ex.Message);
				}
				catch (InvalidIndexerException ex)
				{
                    MessageBox.Show(Resources.GetString("StopScan") + "\n" +
						Resources.GetString("InvalidIndexer") + "\n" +
						ex.Message);
				};
			}
		}

		/// <summary>
		/// Provides the access to the Symbol.Barcode.Reader reference.
		/// The user can use this reference for his additional Reader - related operations.
		/// </summary>
		public Barcode2 Barcode2 
		{
			get
			{
				return myBarcode2;
			}
		}

		/// <summary>
		/// Attach a ScanNotify handler.
		/// </summary>
		public void AttachScanNotify(Barcode2.OnScanHandler ScanNotifyHandler)
		{
			// If we have a reader
			if( myBarcode2 != null)
			{
				// Attach the read notification handler.
                myBarcode2.OnScan += ScanNotifyHandler;
			}

		}

		/// <summary>
		/// Detach the ScanNotify handler.
		/// </summary>
        public void DetachScanNotify(Barcode2.OnScanHandler ScanNotifyHandler)
		{
            if (myBarcode2 != null)
			{
				// Detach the read notification handler.
                myBarcode2.OnScan -= ScanNotifyHandler;
			}
		}

		/// <summary>
		/// Attach a StatusNotify handler.
		/// </summary>
		public void AttachStatusNotify(Barcode2.OnStatusHandler StatusNotifyHandler)
		{
			// If we have a reader
			if( myBarcode2 != null)
			{
				// Attach status notification handler.
				myBarcode2.OnStatus += StatusNotifyHandler;
			}
		}

		/// <summary>
		/// Detach a StatusNotify handler.
		/// </summary>
        public void DetachStatusNotify(Barcode2.OnStatusHandler StatusNotifyHandler)
		{
			// If we have a reader registered for receiving the status notifications
            if (myBarcode2 != null)
			{
				// Detach the status notification handler.
                myBarcode2.OnStatus -= StatusNotifyHandler;
			}
		}

	}

}
 