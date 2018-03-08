﻿#region -- License Terms --
//
// NLiblet
//
// Copyright (C) 2011 FUJIWARA, Yusuke
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
#if NETFX_CORE
using System.Reflection;
#endif

namespace MsgPack.Serialization.Reflection
{
	/// <summary>
	///		Define utility extension method for generic type.
	/// </summary>
	internal static class GenericTypeExtensions
	{
		// Type.EmptyTypes is not available on WinRT.
		private static readonly Type[] _emptyTypes = new Type[ 0 ];

		/// <summary>
		///		Determine whether the source type implements specified generic type or its built type.
		/// </summary>
		/// <param name="source">Target type.</param>
		/// <param name="genericType">Generic interface type.</param>
		/// <returns>
		///		<c>true</c> if <paramref name="source"/> implements <paramref name="genericType"/>,
		///		or built closed generic interface type;
		///		otherwise <c>false</c>.
		/// </returns>
		[Pure]
		public static bool Implements( this Type source, Type genericType )
		{
			Contract.Assert( source != null );
			Contract.Assert( genericType != null );
			Contract.Assert( genericType.GetIsInterface() );

			return EnumerateGenericIntefaces( source, genericType, false ).Any();
		}

		private static IEnumerable<Type> EnumerateGenericIntefaces( Type source, Type genericType, bool includesOwn )
		{
			return
				( includesOwn ? new[] { source }.Concat( source.GetInterfaces() ) : source.GetInterfaces() )
				.Where( @interface =>
					@interface.GetIsGenericType()
					&& ( genericType.GetIsGenericTypeDefinition()
						? @interface.GetGenericTypeDefinition() == genericType
						: @interface == genericType
					)
				).Select( @interface => // If source is GenericTypeDefinition, type def is only valid type (i.e. has name)
					source.GetIsGenericTypeDefinition() ? @interface.GetGenericTypeDefinition() : @interface
				);
		}

		/// <summary>
		///		Get name of type without namespace and assembly name of itself and its generic arguments.
		/// </summary>
		/// <param name="source">Target type.</param>
		/// <returns>Simple name of type.</returns>
		[Pure]
		public static string GetName( this Type source )
		{
			Contract.Assert( source != null );
			if ( !source.GetIsGenericType() )
			{
				return source.Name;
			}

			return
				String.Concat(
					source.Name,
					'[',
#if !NETFX_35
					String.Join( ", ", source.GetGenericArguments().Select( t => t.GetName() ) ),
#else
					String.Join( ", ", source.GetGenericArguments().Select( t => t.GetName() ).ToArray() ),
#endif
					']'
				);
		}

		/// <summary>
		///		Get full name of type including namespace and excluding assembly name of itself and its generic arguments.
		/// </summary>
		/// <param name="source">Target type.</param>
		/// <returns>Full name of type.</returns>
		[Pure]
		public static string GetFullName( this Type source )
		{
			Contract.Assert( source != null );

			if ( source.IsArray )
			{
				var elementType = source.GetElementType();
				if ( !elementType.GetIsGenericType() )
				{
					return source.FullName;
				}

				if ( 1 < source.GetArrayRank() )
				{
					return elementType.GetFullName() + "[*]";
				}
				else
				{
					return elementType.GetFullName() + "[]";
				}
			}

			if ( !source.GetIsGenericType() )
			{
				return source.FullName;
			}

			return
				String.Concat(
					source.Namespace,
					ReflectionAbstractions.TypeDelimiter,
					source.Name,
					'[',
#if !NETFX_35
					String.Join( ", ", source.GetGenericArguments().Select( t => t.GetFullName() ) ),
#else
					String.Join( ", ", source.GetGenericArguments().Select( t => t.GetFullName() ).ToArray() ),
#endif
					']'
				);
		}
	}
}
