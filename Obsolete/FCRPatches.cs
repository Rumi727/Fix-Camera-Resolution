using System;
using UnityEngine;

#pragma warning disable IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.
namespace Rumi.FixCameraResolutions
#pragma warning restore IDE0130 // 네임스페이스가 폴더 구조와 일치하지 않습니다.
{
    [Obsolete("Deprecated class name! Please use FCRResPatches")]
    public class FCRPatches
    {
        [Obsolete("Deprecated class name! Please use FCRResPatches.width")] public static int width => FCRResPatches.width;

        [Obsolete("Deprecated class name! Please use FCRResPatches.height")] public static int height => FCRResPatches.height;

        [Obsolete("Deprecated class name! Please use FCRResPatches.orgWidth")] public static int? orgWidth => FCRResPatches.orgWidth;
        [Obsolete("Deprecated class name! Please use FCRResPatches.orgHeight")] public static int? orgHeight => FCRResPatches.orgHeight;

        [Obsolete("Deprecated class name! Please use FCRResPatches.AllTerminalPatch")] public static void AllTerminalPatch() => FCRResPatches.UpdateAllTerminal();

        [Obsolete("Deprecated class name! Please use FCRResPatches.RenderTexturePatch")] public static void RenderTexturePatch(RenderTexture renderTexture) => FCRResPatches.UpdateRenderTexture(renderTexture);
    }
}
