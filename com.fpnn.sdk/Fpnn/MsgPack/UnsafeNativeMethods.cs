﻿#region -- License Terms --
//
// MessagePack for CLI
//
// Copyright (C) 2010-2012 FUJIWARA, Yusuke
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//
#endregion -- License Terms --

using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace MsgPack
{

#if !SILVERLIGHT && !NETFX_CORE
	[SuppressUnmanagedCodeSecurity]
#endif
#if !WINDOWS_PHONE
	[SecurityCritical]
	internal static class UnsafeNativeMethods
	{
		private static int _libCAvailability = 0;
		private const int _libCAvailability_Unknown = 0;
		private const int _libCAvailability_MSVCRT = 1;
		private const int _libCAvailability_LibC = 2;
		private const int _libCAvailability_None = -1;

		[DllImport( "msvcrt", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp", ExactSpelling = true, SetLastError = false )]
		private static extern int memcmpVC( byte[] s1, byte[] s2, /*SIZE_T*/UIntPtr size );

		[DllImport( "libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp", ExactSpelling = true, SetLastError = false )]
		private static extern int memcmpLibC( byte[] s1, byte[] s2, /*SIZE_T*/UIntPtr size );

		public static bool TryMemCmp( byte[] s1, byte[] s2, /*SIZE_T*/UIntPtr size, out int result )
		{
			if ( _libCAvailability < 0 )
			{
				result = 0;
				return false;
			}

			if ( _libCAvailability <= _libCAvailability_MSVCRT )
			{
				try
				{
					result = memcmpVC( s1, s2, size );
					return true;
				}
				catch ( DllNotFoundException )
				{
					Interlocked.Exchange( ref _libCAvailability, _libCAvailability_LibC );
				}
			}

			if ( _libCAvailability <= _libCAvailability_LibC )
			{
				try
				{
					result = memcmpLibC( s1, s2, size );
					return true;
				}
				catch ( DllNotFoundException )
				{
					Interlocked.Exchange( ref _libCAvailability, _libCAvailability_None );
				}
			}

			result = 0;
			return false;
		}
	}
#endif
}