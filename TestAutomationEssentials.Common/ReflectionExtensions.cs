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

		public static bool DerivesFrom(this Type t1, Type t2)
		{
			return t2.IsAssignableFrom(t1);
		}

		public static object GetDefaultValue(this Type t)
		{
			return t.IsValueType ? Activator.CreateInstance(t) : null;
		}
	}
}
