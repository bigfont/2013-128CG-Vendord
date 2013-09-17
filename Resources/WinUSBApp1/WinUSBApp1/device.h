//
// Define below GUIDs
//
#include <initguid.h>

//
// Device Interface GUID.
// Used by all WinUsb devices that this application talks to.
// Must match "DeviceInterfaceGUIDs" registry value specified in the INF file.
// 964142b9-5a2d-46f3-99b0-23a150878ef6
//
DEFINE_GUID(GUID_DEVINTERFACE_WinUSBApp1,
    0x964142b9,0x5a2d,0x46f3,0x99,0xb0,0x23,0xa1,0x50,0x87,0x8e,0xf6);

typedef struct _DEVICE_DATA {

    BOOL                    HandlesOpen;
    WINUSB_INTERFACE_HANDLE WinusbHandle;
    HANDLE                  DeviceHandle;
    TCHAR                   DevicePath[MAX_PATH];

} DEVICE_DATA, *PDEVICE_DATA;

HRESULT
OpenDevice(
    _Out_     PDEVICE_DATA DeviceData,
    _Out_opt_ PBOOL        FailureDeviceNotFound
    );

VOID
CloseDevice(
    _Inout_ PDEVICE_DATA DeviceData
    );
