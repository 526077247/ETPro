﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;

namespace YooAsset.Editor
{
    public class TaskBuilding_SBP : IBuildTask
    {
        public class BuildResultContext : IContextObject
        {
            public IBundleBuildResults Results;
        }

        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            var scriptableBuildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 构建内容
            var buildContent = new BundleBuildContent(buildMapContext.GetPipelineBuilds());

            // 开始构建
            IBundleBuildResults buildResults;
            var buildParameters = scriptableBuildParameters.GetBundleBuildParameters();
            string builtinShadersBundleName = scriptableBuildParameters.BuiltinShadersBundleName;
            string monoScriptsBundleName = scriptableBuildParameters.MonoScriptsBundleName;
            var taskList = SBPBuildTasks.Create(builtinShadersBundleName, monoScriptsBundleName);
            ReturnCode exitCode = ContentPipeline.BuildAssetBundles(buildParameters, buildContent, out buildResults, taskList);
            if (exitCode < 0)
            {
                string message = BuildLogger.GetErrorMessage(ErrorCode.UnityEngineBuildFailed, $"UnityEngine build failed ! ReturnCode : {exitCode}");
                throw new Exception(message);
            }

            // 说明：解决因为特殊资源包导致验证失败。
            // 例如：当项目里没有着色器，如果有依赖内置着色器就会验证失败。
            if (string.IsNullOrEmpty(builtinShadersBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(builtinShadersBundleName))
                    buildMapContext.CreateEmptyBundleInfo(builtinShadersBundleName);
            }
            if (string.IsNullOrEmpty(monoScriptsBundleName) == false)
            {
                if (buildResults.BundleInfos.ContainsKey(monoScriptsBundleName))
                    buildMapContext.CreateEmptyBundleInfo(monoScriptsBundleName);
            }

            BuildLogger.Log("UnityEngine build success!");
            BuildResultContext buildResultContext = new BuildResultContext();
            buildResultContext.Results = buildResults;
            context.SetContextObject(buildResultContext);
        }
    }
}