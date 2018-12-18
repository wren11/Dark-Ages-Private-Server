///************************************************************************
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
//*************************************************************************/
using System;
using System.Reflection;

namespace Darkages.Common
{
    public static class Reflection
    {
        public static object Create(string typeAssemblyQualifiedName)
        {
            var targetType = ResolveType(typeAssemblyQualifiedName);
            if (targetType == null)
                throw new ArgumentException("Unable to resolve object type: " + typeAssemblyQualifiedName);

            return Create(targetType);
        }

        public static T Create<T>()
        {
            var targetType = typeof(T);
            return (T)Create(targetType);
        }


        public static object Create(Type targetType)
        {
            if (Type.GetTypeCode(targetType) == TypeCode.String)
                return string.Empty;

            var types = new Type[0];
            var info = targetType.GetConstructor(types);
            object targetObject = null;

            if (info == null)
                if (targetType.BaseType != null && targetType.BaseType.UnderlyingSystemType.FullName != null &&
                    targetType.BaseType != null && targetType.BaseType.UnderlyingSystemType.FullName.Contains("Enum"))
                    targetObject = Activator.CreateInstance(targetType);
                else
                    throw new ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName +
                                                " - Constructor not found");
            else
                targetObject = info.Invoke(null);

            if (targetObject == null)
                throw new ArgumentException("Unable to instantiate type: " + targetType.AssemblyQualifiedName +
                                            " - Unknown Error");
            return targetObject;
        }

        public static Type ResolveType(string typeAssemblyQualifiedName)
        {
            var commaIndex = typeAssemblyQualifiedName.IndexOf(",");
            var className = typeAssemblyQualifiedName.Substring(0, commaIndex).Trim();
            var assemblyName = typeAssemblyQualifiedName.Substring(commaIndex + 1).Trim();

            if (className.Contains("[]"))
                className.Remove(className.IndexOf("[]"), 2);

            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    throw new ArgumentException("Can't load assembly " + assemblyName);
                }
            }

            return assembly.GetType(className, false, false);
        }
    }
}
