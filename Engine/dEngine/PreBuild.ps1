param([Parameter()]$commitId, [Parameter()]$enginePath)

$Source = @”
using System;
using System.IO;

namespace dEngine.PreBuild
{
    public static class PreBuildHelper
    {
        private static string GetAssemblyValue(ref string assemblyInfo, string name)
        {
            var start = assemblyInfo.IndexOf(name) + name.Length + 2;
            var end = assemblyInfo.IndexOf('"', start);
            var value = assemblyInfo.Substring(start, end - start);
            return value;
        }

        private static void ReplaceAssemblyValue(ref string assemblyInfo, string name, string value)
        {
            var fileStart = assemblyInfo.IndexOf(name) + name.Length + 2;
            var fileEnd = assemblyInfo.IndexOf('"', fileStart);
            assemblyInfo = assemblyInfo.Remove(fileStart, fileEnd - fileStart);
            assemblyInfo = assemblyInfo.Insert(fileStart, value);
        }

        public static void UpdateVersion(string projectPath, string gitCommitId) 
        {
		System.Debug.WriteLine(projectPath);
            var assemblyPath = Path.Combine(projectPath, "Properties", "AssemblyInfo.cs");
            var assemblyInfo = File.ReadAllText(assemblyPath);
            
            int buildNumber = int.Parse(GetAssemblyValue(ref assemblyInfo, "BuildNumber"));
			buildNumber++;
			ReplaceAssemblyValue(ref assemblyInfo, "BuildNumber", buildNumber.ToString());

            var buildType = GetAssemblyValue(ref assemblyInfo, "BuildType");
            var version = GetAssemblyValue(ref assemblyInfo, "AssemblyVersion");
            var versionWithBuildNumber = version.Substring(0, version.IndexOf('.', version.IndexOf('.', version.IndexOf('.')+1)+1)) + "." + buildNumber;

            buildType = buildType.ToLower();
            buildType = buildType == "release" ? "" : "-" + buildType;
			
			ReplaceAssemblyValue(ref assemblyInfo, "CommitId", gitCommitId);
            ReplaceAssemblyValue(ref assemblyInfo, "AssemblyFileVersion", versionWithBuildNumber);
            ReplaceAssemblyValue(ref assemblyInfo, "AssemblyInformationalVersion", version + buildType + "+sha." + gitCommitId);
            
            File.WriteAllText(assemblyPath, assemblyInfo);
        }
    } 
} 
“@

Add-Type -TypeDefinition $Source -Language CSharp  

[dEngine.PreBuild.PreBuildHelper]::UpdateVersion($enginePath, $commitId)