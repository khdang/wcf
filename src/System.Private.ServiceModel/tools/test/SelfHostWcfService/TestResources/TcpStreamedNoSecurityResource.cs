﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ServiceModel;
using System.ServiceModel.Channels;

namespace WcfService.TestResources
{
    internal class TcpStreamedNoSecurityResource : TcpResource
    {
        protected override string Address { get { return "tcp-streamed-nosecurity"; } }

        protected override Binding GetBinding()
        {
            return new NetTcpBinding(SecurityMode.None)
            {
                PortSharingEnabled = false,
                TransferMode = TransferMode.Streamed
            };
        }
    }
}
