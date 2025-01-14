using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace IPTech.BuildTool.Processors {
	[Tooltip("This will add and IPTechBuildInfo.asset file to your Asset/Resources folder. It contains build information you can retrieve at runtime.")]
    public class AddBuildInfo : BuildProcessor {
        public override void PreprocessBuild(BuildReport report) {
			var buildInfo = IPTechBuildInfo.LoadFromResources() ?? CreateNewIPTechBuildInfo();
			string buildNumber = PlayerSettings.iOS.buildNumber;
			if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android) {
				buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
			}

			var assetPath = AssetDatabase.GetAssetPath(buildInfo);
			BuildToolLogger.Log($"Settings buildNumber to {buildNumber} on {assetPath}");
			buildInfo.BuildNumber = buildNumber;
			EditorUtility.SetDirty(buildInfo);
			AssetDatabase.SaveAssets();
		}

		IPTechBuildInfo CreateNewIPTechBuildInfo() {
			BuildToolLogger.Log("Created IPTechBuildInfo asset in resources");
			var buildInfo = ScriptableObject.CreateInstance<IPTechBuildInfo>();
			if(!Directory.Exists(Path.Combine("Assets", "Resources"))) {
				Directory.CreateDirectory(Path.Combine("Assets", "Resources"));
			}
			AssetDatabase.CreateAsset(buildInfo, Path.Combine("Assets", "Resources", nameof(IPTechBuildInfo) + ".asset"));
			return buildInfo;
		}
	}
}
