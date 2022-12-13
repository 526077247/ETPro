﻿using HybridCLR.Editor.Link;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HybridCLR.Editor.Commands
{

    public static class Il2CppDefGeneratorCommand
    {

        // [MenuItem("HybridCLR/Generate/Il2CppDef", priority = 104)]
        public static void GenerateIl2CppDef()
        {
            var options = new Il2CppDef.Il2CppDefGenerator.Options()
            {
                UnityVersion = UnityEngine.Application.unityVersion,
                OutputFile = $"{SettingsUtil.LocalIl2CppDir}/libil2cpp/il2cpp-config.h",
            };

            var g = new Il2CppDef.Il2CppDefGenerator(options);
            g.Generate();
        }
    }
}
