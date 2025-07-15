﻿using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Pipeline.Interfaces;

namespace YooAsset.Editor
{
    public class TaskVerifyBuildResult_SBP : IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var buildParameters = buildParametersContext.Parameters as ScriptableBuildParameters;

            // 验证构建结果
            if (buildParameters.VerifyBuildingResult)
            {
                var buildResultContext = context.GetContextObject<TaskBuilding_SBP.BuildResultContext>();
                VerifyingBuildingResult(context, buildResultContext.Results);
            }
        }

        /// <summary>
        /// 验证构建结果
        /// </summary>
        private void VerifyingBuildingResult(BuildContext context, IBundleBuildResults buildResults)
        {
            var buildMapContext = context.GetContextObject<BuildMapContext>();
            List<string> unityBuildContent = buildResults.BundleInfos.Keys.ToList();

            // 1. 计划内容
            List<string> planningContent = buildMapContext.Collection.Select(t => t.BundleName).ToList();

            // 2. 验证差异
            List<string> exceptBundleList1 = unityBuildContent.Except(planningContent).ToList();
            if (exceptBundleList1.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList1)
                {
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle, $"Found unintended build bundle : {exceptBundle}");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, $"Unintended build, See the detailed warnings !");
                throw new Exception(exception);
            }

            // 3. 验证差异
            List<string> exceptBundleList2 = planningContent.Except(unityBuildContent).ToList();
            if (exceptBundleList2.Count > 0)
            {
                foreach (var exceptBundle in exceptBundleList2)
                {
                    string warning = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildBundle, $"Found unintended build bundle : {exceptBundle}");
                    BuildLogger.Warning(warning);
                }

                string exception = BuildLogger.GetErrorMessage(ErrorCode.UnintendedBuildResult, $"Unintended build, See the detailed warnings !");
                throw new Exception(exception);
            }

            BuildLogger.Log("Build results verify success!");
        }
    }
}