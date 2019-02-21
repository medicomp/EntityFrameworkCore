// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.Data.Sqlite.Utilities
{
    internal static class BundleInitializer
    {
        public static void Initialize()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.Load(new AssemblyName("SQLitePCLRaw.batteries_v2"));
            }
            catch
            {
                return;
            }
            
#if NET40
            assembly.GetType("SQLitePCL.Batteries_V2").GetMethod("Init")
#else
             assembly.GetType("SQLitePCL.Batteries_V2").GetTypeInfo().GetDeclaredMethod("Init")
#endif
                .Invoke(null, null);
        }
    }
}
