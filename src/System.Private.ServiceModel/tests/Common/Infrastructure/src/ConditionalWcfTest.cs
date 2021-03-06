﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Infrastructure.Common
{
    // ConditionalWcfTest is expected to be the base class of any test
    // class that includes [ConditionalFact] or [ConditionalTheory]. This
    // is necessary because the conditional attributes are expected to
    // refer to members within their test class or its base classes.
    public class ConditionalWcfTest
    {
        private static Dictionary<string, bool> _evaluatedConditions = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private static object _evaluationLock = new object();

        // Returns 'true' or 'false' for the given condition name.
        // There are several levels of precedence to evaluate the condition.
        // The value is cached after being evaluated the first time.
        // Precedence order for evaluation is:
        //  1. Environment variable, if set and is 'true' or 'false'.
        //  2. TestProperties, if set and is 'true' or 'false'
        //  3. detection func, if specified
        //  4. If none of the above, 'false'
        private static bool GetConditionValue(string conditionName, Func<bool> detectFunc = null)
        {
            // Lock to evaluate once only
            lock (_evaluationLock)
            {
                bool result = false;
                if (_evaluatedConditions.TryGetValue(conditionName, out result))
                {
                    return result;
                }

                bool evaluatedResult = false;

                // Highest precedence: environment variable if set and can be parsed
                string value = Environment.GetEnvironmentVariable(conditionName);
                bool parsedValue = false;
                if (!String.IsNullOrWhiteSpace(value) && bool.TryParse(value, out parsedValue))
                {
                    result = parsedValue;
                    evaluatedResult = true;
                }

                // Next precedence: TestProperties if present and can be parsed
                else if (TestProperties.PropertyNames.Contains(conditionName))
                {
                    value = TestProperties.GetProperty(conditionName);
                    if (!String.IsNullOrWhiteSpace(value) && bool.TryParse(value, out parsedValue))
                    {
                        result = parsedValue;
                        evaluatedResult = true;
                    }
                }

                // Next precedence: optional runtime detection func
                if (!evaluatedResult && detectFunc != null)
                {
                    result = detectFunc();
                    evaluatedResult = true;
                }

                // Final precedence: false is default
                if (!evaluatedResult)
                {
                    result = false;
                }

                _evaluatedConditions[conditionName] = result;
                return result;
            }
        }

        private static bool Is_Server_Local()
        {
            // Temporary workaround to detect whether this test is using localhost
            // for either new or old test infrastructure.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            // to use centralized helper method.
            string host = null;
            if (TestProperties.PropertyNames.Contains("ServiceUri"))
            {
                host = TestProperties.GetProperty("ServiceUri");
            }
            else if (TestProperties.PropertyNames.Contains("BridgeHost"))
            {
                host = TestProperties.GetProperty("BridgeHost");
            }

            if (String.IsNullOrWhiteSpace(host))
            {
                return false;
            }

            int index = host.IndexOf("localhost", 0, StringComparison.OrdinalIgnoreCase);
            return index >= 0;
        }
        
        private static bool Is_Windows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        public static bool Domain_Joined()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(Domain_Joined),
                                     Is_Server_Local);
        }

        public static bool Root_Certificate_Installed()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this to test only when solving https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(Root_Certificate_Installed),
                                     Is_Server_Local);
        }

        public static bool Client_Certificate_Installed()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this to test only when solving https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(Client_Certificate_Installed),
                                     Is_Server_Local);
        }

        public static bool SPN_Available()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(SPN_Available),
                                     Is_Server_Local);
        }

        public static bool Kerberos_Available()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(Kerberos_Available),
                                     Is_Server_Local);
        }

        public static bool Windows_Authentication_Available()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(Windows_Authentication_Available),
                                     Is_Server_Local);
        }

        public static bool NTLM_Available()
        {
            // Temporarily use the simple heuristic that if we are running the services locally, it is.
            // Refactor this after integration to address https://github.com/dotnet/wcf/issues/1024 
            return GetConditionValue(nameof(NTLM_Available),
                                     Is_Server_Local);
        }
    }
}
