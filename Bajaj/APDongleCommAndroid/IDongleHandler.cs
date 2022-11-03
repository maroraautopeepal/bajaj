﻿using System.Threading.Tasks;

namespace APDongleCommAnroid
{
    internal interface IDongleHandler
    {
        Task<object> Dongle_Reset();
        Task<object> Dongle_SetProtocol(int protocol);

        Task<object> Dongle_GetProtocol();
        Task<object> Dongle_GetFimrwareVersion();
    }
}
