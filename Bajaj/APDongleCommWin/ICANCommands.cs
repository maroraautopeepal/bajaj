using APDongleCommWin.Models;
using System.Threading.Tasks;

namespace APDongleCommWin
{
    internal interface ICANCommands
    {

        //-------------------------------------

        Task<object> CAN_SetTxHeader(string txHeader);

        Task<object> CAN_SetRxHeaderMask(string txHeaderMask);

        Task<object> CAN_GetRxHeaderMask();

        Task<object> CAN_SetP1Min(string p1min);

        Task<object> CAN_GetP1Min();

        Task<object> CAN_SetP2Max(string p2max);

        Task<object> CAN_GetP2Max();

        Task<object> CAN_StartTP();

        Task<object> CAN_StopTP();

        Task<object> CAN_StartPadding(string padding);

        Task<object> CAN_StopPadding();

        Task<object> CAN_TxData(string txData);

        //Task<object> CAN_RxData();

        Task<ResponseArrayStatus> CAN_TxRx(int frameLength, string txdata);

        Task<object> SetBlkSeqCntr(string blklen);
        Task<object> GetBlkSeqCntr();

        Task<object> SetSepTime(string septime);
        Task<object> GetSepTime();

    }
}

