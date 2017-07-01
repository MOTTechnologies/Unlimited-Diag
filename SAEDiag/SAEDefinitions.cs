using System;
using System.Collections.Generic;

namespace SAE
{
    public enum SAE_NETWORK
    {
//        NONE,
        ISO15765,
        J1850,
        ISO14230,
        ISO9141,
        SCI
    }


    [Flags]
    public enum SAE_DTC_status
    {
        IMMATURE = 0x01,    //0 Maturing/intermittent code - insufficient data to consider as a malfunction
        OCCURING_NOW = 0x02,    //Current code - present at time of request
        OEM_FLAG1 = 0x04,   //Manufacturer specific status
        OEM_FLAG2 = 0x08,   //Manufacturer specific status
        STORED = 0x10,  //Stored trouble code
        PASSING = 0x20, //Warning lamp was previously illuminated for this code, malfunction not currently detected, code not yet erased
        PENDING = 0x40, //Warning lamp pending for this code, not illuminate but malfunction was detected
        MIL_ON = 0x80   //Warning lamp illuminated for this code
    }

    public enum SuccessResponse:byte
    { 
        AFFIRMITIVE_RESPONSE = 0x00,
		SECURITY_ACCESS_ALLOWED = 0x34,
		READY_FOR_DOWNLOAD = 0x44,
		READY_FOR_UPLOAD = 0x54,
		TRANSFER_COMPLETE = 0x73,
		TRANSFER_RECEIVED = 0x78,
    }

    public enum SAE_responses
	{
		AFFIRMITIVE_RESPONSE = 0x00,
		GENERAL_REJECT = 0x10,
		MODE_NOT_SUPPORTED = 0x11,
		INVALID_SUBFUNCTION = 0x12,
		REPEAT_REQUEST = 0x21,
		CONDITIONS_NOT_CORRECT = 0x22,
		ROUTINE_NOT_COMPLETE = 0x23,
		REQUEST_OUT_OF_RANGE = 0x31,
		SECURITY_ACCESS_DENIED = 0x33,
		SECURITY_ACCESS_ALLOWED = 0x34,
		INVALID_KEY = 0x35,
		EXCEED_NUMBER_OF_ATTEMPTS = 0x36,
		TIME_DELAY_NOT_EXPIRED = 0x37,
		DOWNLOAD_NOT_ACCEPTED = 0x40,
		IMPROPER_DOWNLOAD_TYPE = 0x41,
		DOWNLOAD_ADDRESS_UNAVAILABLE = 0x42,
		IMPROPER_DOWNLOAD_LENGTH = 0x43,
		READY_FOR_DOWNLOAD = 0x44,
		UPLOAD_NOT_ACCEPTED = 0x50,
		IMPROPER_UPLOAD_TYPE = 0x51,
		UPLOAD_ADDRESS_UNAVAILABLE = 0x52,
		IMPROPER_UPLOAD_LENGTH = 0x53,
		READY_FOR_UPLOAD = 0x54,
		PASS_WITH_RESULTS = 0x61,
		PASS_WITHOUT_RESULTS = 0x62,
		FAIL_WITH_RESULTS = 0x63,
		FAIL_WITHOUT_RESULTS = 0x64,
		TRANSFER_SUSPENDED = 0x71,
		TRANSFER_ABORTED = 0x72,
		TRANSFER_COMPLETE = 0x73,
		TRANSFER_ADDRESS_UNAVAILABLE = 0x74,
		IMPROPER_TRANSFER_LENGTH = 0x75,
		IMPROPER_TRANSFER_TYPE = 0x76,
		TRANSFER_CHECKSUM_FAIL = 0x77,
		TRANSFER_RECEIVED = 0x78,
		TRANSFER_BYTE_COUNT_MISMATCH = 0x79,
        MANUFACTURER_SPECIFIC = 0x100,
        NONE = 0x101
    }	
}
