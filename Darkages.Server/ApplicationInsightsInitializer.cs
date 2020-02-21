// *****************************************************************************
//Project Lorule: A Dark Ages Server (http://darkages.creatorlink.net/index/)
//Copyright(C) 2018 TrippyInc Pty Ltd
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
// *************************************************************************


using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace Darkages
{
    public class ApplicationInsightsInitializer : ITelemetryInitializer
    {
        public string InstrumentationKey { get; private set; }

        public ApplicationInsightsInitializer(string instrumentationKey)
        {
            InstrumentationKey = instrumentationKey;
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.InstrumentationKey = InstrumentationKey;
            telemetry.Context.Cloud.RoleName     = ServerContext.Config.SERVER_TITLE + " (" + ServerContext.IPADDR.ToString()  + ")";

            if (!string.IsNullOrWhiteSpace(Environment.UserName))
                telemetry.Context.User.Id = Environment.UserName;
        }
    }
}