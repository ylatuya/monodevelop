﻿//
// From NuGet src/Core
//
// Copyright (c) 2010-2014 Outercurve Foundation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace MonoDevelop.Core.Web
{
	abstract class WIFTypeProvider
	{
		public abstract Type ChannelFactory { get; }

		public abstract Type RequestSecurityToken { get; }

		public abstract Type EndPoint { get; }

		public abstract Type RequestTypes { get; }

		public abstract Type KeyTypes { get; }

		protected abstract string AssemblyName { get; }

		public static WIFTypeProvider GetWIFTypes()
		{
			// First attempt to look up the 4.5 types
			WIFTypeProvider wifProvider = new WIFTypes45();
			if (wifProvider.ChannelFactory != null)
			{
				return wifProvider;
			}

			// We could be on 4.0 with the SDK \ WIF Runtime installed.
			wifProvider = new WIFTypes40();
			if (wifProvider.ChannelFactory != null)
			{
				return wifProvider;
			}
			return null;
		}

		protected string QualifyTypeName(string typeName)
		{
			return typeName + ',' + AssemblyName;
		}

		private sealed class WIFTypes40 : WIFTypeProvider
		{
			public override Type ChannelFactory
			{
				get
				{
					string typeName = QualifyTypeName("Microsoft.IdentityModel.Protocols.WSTrust.WSTrustChannelFactory");
					return Type.GetType(typeName);
				}
			}

			public override Type RequestSecurityToken
			{
				get
				{
					string typeName = QualifyTypeName("Microsoft.IdentityModel.Protocols.WSTrust.RequestSecurityToken");
					return Type.GetType(typeName);
				}
			}

			public override Type EndPoint
			{
				get
				{
					return typeof(System.ServiceModel.EndpointAddress);
				}
			}

			public override Type RequestTypes
			{
				get
				{
					string typeName = QualifyTypeName("Microsoft.IdentityModel.SecurityTokenService.RequestTypes");
					return Type.GetType(typeName);
				}
			}

			public override Type KeyTypes
			{
				get
				{
					string typeName = QualifyTypeName("Microsoft.IdentityModel.SecurityTokenService.KeyTypes");
					return Type.GetType(typeName);
				}
			}

			protected override string AssemblyName
			{
				get { return "Microsoft.IdentityModel, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"; }
			}
		}

		private sealed class WIFTypes45 : WIFTypeProvider
		{
			public override Type ChannelFactory
			{
				get
				{
					return Type.GetType("System.ServiceModel.Security.WSTrustChannelFactory, System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
				}
			}

			public override Type RequestSecurityToken
			{
				get
				{
					string typeName = QualifyTypeName("System.IdentityModel.Protocols.WSTrust.RequestSecurityToken");
					return Type.GetType(typeName);
				}
			}

			public override Type EndPoint
			{
				get
				{
					string typeName = QualifyTypeName("System.IdentityModel.Protocols.WSTrust.EndpointReference");
					return Type.GetType(typeName);
				}
			}

			public override Type RequestTypes
			{
				get
				{
					string typeName = QualifyTypeName("System.IdentityModel.Protocols.WSTrust.RequestTypes");
					return Type.GetType(typeName);
				}
			}

			public override Type KeyTypes
			{
				get
				{
					string typeName = QualifyTypeName("System.IdentityModel.Protocols.WSTrust.KeyTypes");
					return Type.GetType(typeName);
				}
			}

			protected override string AssemblyName
			{
				get { return "System.IdentityModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"; }
			}
		}
	}
}
