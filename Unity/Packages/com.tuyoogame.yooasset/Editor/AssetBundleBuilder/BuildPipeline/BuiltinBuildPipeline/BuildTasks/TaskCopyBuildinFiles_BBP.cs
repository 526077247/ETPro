﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YooAsset.Editor
{
    public class TaskCopyBuildinFiles_BBP : TaskCopyBuildinFiles, IBuildTask
    {
        void IBuildTask.Run(BuildContext context)
        {
            var buildParametersContext = context.GetContextObject<BuildParametersContext>();
            var manifestContext = context.GetContextObject<ManifestContext>();
            if (buildParametersContext.Parameters.BuildinFileCopyOption != EBuildinFileCopyOption.None)
            {
                CopyBuildinFilesToStreaming(buildParametersContext, manifestContext.Manifest);
            }
        }
    }
}