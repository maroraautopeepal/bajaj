namespace APDiagnosticAndroid.Enums
{
    public enum ReadDTCIndex
    {
        UDS_3BYTE_DTC,
        UDS_2BYTE12_DTC,
        UDS_2BYTE13_DTC,
        KWP_2BYTE_DTC,
        GENERIC_OBD,
        UDS
        //UDS_3BYTE_DTC,
        //UDS_2BYTE12_DTC,
        //UDS_2BYTE13_DTC,
        //KWP_2BYTE_DTC,
        //GENERIC_OBD,
        //UDS_4BYTES,
        //UDS_3BYTES,
        //UDS_2BYTE_DTC
    }

    public enum ClearDTCIndex
    {
        UDS_4BYTES,
        UDS_3BYTES,
        GENERIC_OBD,
        KWP
    }
}
