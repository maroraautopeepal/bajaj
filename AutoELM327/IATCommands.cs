using AutoELM327.Enums;
using AutoELM327.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AutoELM327
{
    internal interface IATCommands
    {

       Task<object> Dongle_Reset();

        Task<object> Dongle_SetProtocol(string protocolEnum);

        Task<object> Dongle_GetProtocol();

        Task<object> Dongle_GetFimrwareVersion();

        Task<object> Dongle_ReadVoltageLevels();

        //-------------------------------------

        Task<object> CAN_SetTxHeader(string setTxHeader);

        Task<object> CAN_SetRxHeaderMask(string setRxHeader);

        Task<object> CAN_StartPadding();

        Task<object> CAN_StopPadding();

        Task<object> CAN_TxData();

        Task<object> CAN_RxData();

        Task<ResponseArrayStatus> CAN_TxRx(int frameLength, string txrx);

    }
}