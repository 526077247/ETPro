﻿
namespace YooAsset
{
    internal class RawBundleLoadAllAssetsOperation : FSLoadAllAssetsOperation
    {
        internal override void InternalOnStart()
        {
            Error = $"{nameof(RawBundleLoadAllAssetsOperation)} not support load all assets !";
            Status = EOperationStatus.Failed;
        }
        internal override void InternalOnUpdate()
        {
        }
    }
}