using System;
using System.Reflection;

namespace TestAutomationEssentials.Common
{
	public static class ReflectionExtensions
	{
		public static bool HasAttribute<TAttribute>(this MemberInfo member) 
			where TAttribute : Attribute
		{
			return member.GetCustomAttribute<TAttribute>(false) != null;
		}
	}
}
