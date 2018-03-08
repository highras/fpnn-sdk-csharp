#region -- License Terms --
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
using System.Text;
using MsgPack.Serialization.Reflection;

namespace MsgPack.Serialization
{
	/// <summary>
	///		Utilities related to member/type ID.
	/// </summary>
	internal static class IdentifierUtility
	{
		/// <summary>
		///		Builds the name of the generating method.
		/// </summary>
		/// <param name="operation">The operation.</param>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="targetMemberName">Name of the target member.</param>
		/// <returns>Name of the method.</returns>
		public static string BuildMethodName( string operation, Type targetType, string targetMemberName )
		{
#if !NETFX_35
			return String.Join( "_", operation, EscapeTypeName( targetType ), targetMemberName );
#else
			return String.Join( "_", new string[] { operation, EscapeTypeName( targetType ), targetMemberName } );
#endif
		}

		public static string EscapeTypeName( Type type )
		{
			var fullName = type.GetFullName();
			var result = new StringBuilder( fullName.Length );
			bool mayArray = false;
			foreach ( var c in fullName )
			{
				switch ( c )
				{
					case ' ':
					{
						mayArray = false;
						break;
					}
					case ',':
					{
						mayArray = false;
						result.Append( '_' );
						break;
					}
					case '[':
					{
						mayArray = true;
						break;
					}
					case ']':
					{
						if ( mayArray )
						{
							result.Append( "Array" );
							mayArray = false;
						}
						else
						{
							result.Append( '_' );
						}

						break;
					}
					case '*':
					{
						if ( !mayArray )
						{
							result.Append( "Pointer" );
						}

						break;
					}
					case '&':
					{
						if ( mayArray )
						{
							result.Append( '_' );
						}

						mayArray = false;

						result.Append( "Reference" );
						break;
					}
					case '.':
					case '`':
					{
						mayArray = false;
						result.Append( '_' );
						break;
					}
					default:
					{
						if ( mayArray )
						{
							result.Append( '_' );
						}

						mayArray = false;
						result.Append( c );
						break;
					}
				}
			}

			return result.ToString();
		}
	}
}
