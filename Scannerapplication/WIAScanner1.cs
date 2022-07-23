using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using WIA;
using System.Windows.Forms;
using Scannerapplication;

namespace WIATest
{
    class WIAScanner1
    {
        const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";
        class WIA_DPS_DOCUMENT_HANDLING_SELECT
        {
            public const uint FEEDER = 0x00000001;
            public const uint FLATBED = 0x00000002;
        }
        class WIA_DPS_DOCUMENT_HANDLING_STATUS
        {
            public const uint FEED_READY = 0x00000001;
        }
        class WIA_PROPERTIES
        {
            public const uint WIA_RESERVED_FOR_NEW_PROPS = 1024;
            public const uint WIA_DIP_FIRST = 2;
            public const uint WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            //
            // Scanner only device properties (DPS)
            //
            public const uint WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
            public const uint WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
        }
        /// <summary>
        /// Use scanner to scan an image (with user selecting the scanner from a dialog).
        /// </summary>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan()
        {
            WIA.ICommonDialog dialog = new WIA.CommonDialog();
            WIA.Device device = dialog.ShowSelectDevice(WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);
            if (device != null)
            {
                return Scan(device.DeviceID);
            }
            else
            {
                throw new Exception("Tarayıcı seçin.");
            }
        }

        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan(string scannerId)
        {
            try
            {
                // Create a DeviceManager instance
                var deviceManager = new DeviceManager();

                List<Image> ret = new List<Image>();

                WIA.CommonDialog dialog = new WIA.CommonDialog();
                WIA.Device device = dialog.ShowSelectDevice(WIA.WiaDeviceType.ScannerDeviceType);
                WIA.Item items = device.Items[1];
                //items.Properties["6146"].set_Value(2);
                //items.Properties["6147"].set_Value(150);
                //items.Properties["6148"].set_Value(150);
                //items.Properties["6151"].set_Value(150 * 8);
                //items.Properties["6152"].set_Value(150 * 3);

               
                    while (true)
                    {
                        try
                        {
                            WIA.ImageFile image = (WIA.ImageFile)dialog.ShowTransfer(items);
                            if (image != null && image.FileData != null)
                            {
                                var imageBytes = (byte[])image.FileData.get_BinaryData();
                                var ms = new MemoryStream(imageBytes);
                                Image img = null;
                                img = Image.FromStream(ms);

                                ret.Add(img);
                            }
                        }
                        catch
                        {
                            break;
                        }
                    }
                
                return ret;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the list of available WIA devices.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDevices()
        {
            List<string> devices = new List<string>();
            WIA.DeviceManager manager = new WIA.DeviceManager();
            foreach (WIA.DeviceInfo info in manager.DeviceInfos)
            {
                devices.Add(info.DeviceID);
            }
            return devices;
        }
    }
}
