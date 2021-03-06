﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ServiceModel.Channels;
using System.Xml;

namespace System.ServiceModel
{
    public class NetHttpsBinding : HttpBindingBase
    {
        BinaryMessageEncodingBindingElement _binaryMessageEncodingBindingElement;
        NetHttpMessageEncoding _messageEncoding;
        BasicHttpsSecurity _basicHttpsSecurity;

        public NetHttpsBinding() : this(BasicHttpsSecurity.DefaultMode) { }

        public NetHttpsBinding(BasicHttpsSecurityMode securityMode)
        {
            if (securityMode == BasicHttpsSecurityMode.TransportWithMessageCredential)
            {
                throw ExceptionHelper.PlatformNotSupported(SR.Format(SR.UnsupportedSecuritySetting, "securityMode", securityMode));
            }

            Initialize();
            _basicHttpsSecurity.Mode = securityMode;
        }

        public NetHttpMessageEncoding MessageEncoding
        {
            get { return _messageEncoding; }
            set { _messageEncoding = value; }
        }

        public BasicHttpsSecurity Security
        {
            get
            {
                return _basicHttpsSecurity;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }

                _basicHttpsSecurity = value;
            }
        }

        internal override BasicHttpSecurity BasicHttpSecurity
        {
            get
            {
                return _basicHttpsSecurity.BasicHttpSecurity;
            }
        }

        public WebSocketTransportSettings WebSocketSettings
        {
            get
            {
                return InternalWebSocketSettings;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            if ((BasicHttpSecurity.Mode == BasicHttpSecurityMode.Transport ||
                BasicHttpSecurity.Mode == BasicHttpSecurityMode.TransportCredentialOnly) &&
                BasicHttpSecurity.Transport.ClientCredentialType == HttpClientCredentialType.InheritedFromHost)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.Format(SR.HttpClientCredentialTypeInvalid, BasicHttpSecurity.Transport.ClientCredentialType)));
            }

            return base.BuildChannelFactory<TChannel>(parameters);
        }


        public override BindingElementCollection CreateBindingElements()
        {
            CheckSettings();

            // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();

            // add security (*optional)
            SecurityBindingElement messageSecurity = BasicHttpSecurity.CreateMessageSecurity();
            if (messageSecurity != null)
            {
                bindingElements.Add(messageSecurity);
            }

            // add encoding
            switch (MessageEncoding)
            {
                case NetHttpMessageEncoding.Text:
                    bindingElements.Add(TextMessageEncodingBindingElement);
                    break;
                case NetHttpMessageEncoding.Mtom:
                    throw ExceptionHelper.PlatformNotSupported(SR.Format(SR.UnsupportedBindingProperty, "MessageEncoding", MessageEncoding));
                default:
                    bindingElements.Add(_binaryMessageEncodingBindingElement);
                    break;
            }

            // add transport (http or https)
            bindingElements.Add(GetTransport());

            return bindingElements.Clone();
        }

        internal override void SetReaderQuotas(XmlDictionaryReaderQuotas readerQuotas)
        {
            readerQuotas.CopyTo(_binaryMessageEncodingBindingElement.ReaderQuotas);
        }

        internal override EnvelopeVersion GetEnvelopeVersion()
        {
            return EnvelopeVersion.Soap12;
        }

        internal override void CheckSettings()
        {
            base.CheckSettings();

            // Mtom is not supported.
            if (MessageEncoding == NetHttpMessageEncoding.Mtom)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.Format(SR.UnsupportedBindingProperty, "MessageEncoding", MessageEncoding)));
            }
        }

        void Initialize()
        {
            _messageEncoding = NetHttpBindingDefaults.MessageEncoding;
            _binaryMessageEncodingBindingElement = new BinaryMessageEncodingBindingElement() { MessageVersion = MessageVersion.Soap12WSAddressing10 };
            TextMessageEncodingBindingElement.MessageVersion = MessageVersion.Soap12WSAddressing10;
            InternalWebSocketSettings.TransportUsage = NetHttpBindingDefaults.TransportUsage;
            InternalWebSocketSettings.SubProtocol = WebSocketTransportSettings.SoapSubProtocol;
            _basicHttpsSecurity = new BasicHttpsSecurity();
        }

    }
}