﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HybridCLR.Generators.MethodBridge
{
    public interface IPlatformAdaptor
    {
        bool IsArch32 { get; }

        TypeInfo CreateTypeInfo(Type type, bool returnValue);

        IEnumerable<MethodBridgeSig> GetPreserveMethods();

        void GenerateCall(MethodBridgeSig method, List<string> outputLines);

        void GenCallStub(List<MethodBridgeSig> methods, List<string> lines);
    }
}
