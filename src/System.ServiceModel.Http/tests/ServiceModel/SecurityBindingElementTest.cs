﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Xunit;

public static class SecurityBindingElementTest
{
    [Theory]
    [InlineData(BasicHttpSecurityMode.TransportCredentialOnly)]
    [InlineData(BasicHttpSecurityMode.Transport)]
    [InlineData(BasicHttpSecurityMode.None)]
    public static void Create_HttpBinding_SecurityMode_Without_SecurityBindingElement(BasicHttpSecurityMode securityMode)
    {
        BasicHttpBinding binding = new BasicHttpBinding(securityMode);
        var bindingElements = binding.CreateBindingElements();

        var securityBindingElement = bindingElements.FirstOrDefault(x => x is SecurityBindingElement) as SecurityBindingElement;
        Assert.True(securityBindingElement == null, string.Format("securityBindingElement should be null when BasicHttpSecurityMode is '{0}'", securityMode));

        Assert.True(binding.CanBuildChannelFactory<IRequestChannel>(), string.Format("CanBuildChannelFactory should return true for BasicHttpSecurityMode:'{0}'", securityMode));
        binding.BuildChannelFactory<IRequestChannel>();
    }

    [Theory]
    [InlineData(BasicHttpSecurityMode.Message)]
    [InlineData(BasicHttpSecurityMode.TransportWithMessageCredential)]
    // BasicHttpSecurityMode.Message is not supported
    public static void Create_HttpBinding_SecurityMode_Message_Throws_NotSupported(BasicHttpSecurityMode securityMode)
    {
        Assert.Throws<PlatformNotSupportedException>(() => {
            new BasicHttpBinding(securityMode);
        });
    }
}
