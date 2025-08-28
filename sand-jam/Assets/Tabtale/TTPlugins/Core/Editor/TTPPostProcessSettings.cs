#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode.Extensions;

namespace Tabtale.TTPlugins
{
    public class TTPPostProcessSettings
    {
        private static PlistElementDict rootDict;

        private static PBXProject pbxProject;

        [PostProcessBuild(40005)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            var pbxProjectPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);
            pbxProject = new UnityEditor.iOS.Xcode.PBXProject();
            pbxProject.ReadFromString(System.IO.File.ReadAllText(pbxProjectPath));
            
            Debug.Log("TTPPostProcessSettings::Add swift support for mopub and ecpm");
            pbxProject.AddBuildProperty(GetTargetGUID(pbxProject), "LIBRARY_SEARCH_PATHS", "$(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)");
            pbxProject.AddBuildProperty(GetTargetGUID(pbxProject), "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift");
            pbxProject.SetBuildProperty(GetTargetGUID(pbxProject), "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift $(inherited) @executable_path/Frameworks");
            pbxProject.SetBuildProperty(GetTargetGUID(pbxProject), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            pbxProject.SetBuildProperty(GetTargetGUID(pbxProject), "SWIFT_VERSION", "5");
            pbxProject.SetBuildProperty(GetTargetGUID(pbxProject), "MARKETING_VERSION", Application.version);
#if UNITY_2019_3_OR_NEWER
            var unityFrameworkTarget = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LIBRARY_SEARCH_PATHS", "$(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift");
            pbxProject.SetBuildProperty(unityFrameworkTarget, "LD_RUNPATH_SEARCH_PATHS", "/usr/lib/swift $(inherited) @executable_path/Frameworks");
            pbxProject.SetBuildProperty(unityFrameworkTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            pbxProject.SetBuildProperty(unityFrameworkTarget, "SWIFT_VERSION", "5");
            
            // adding support for xcode 15 for -ld64
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1000", "");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1100", "");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1200", "");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1300", "");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1400", "");
            pbxProject.AddBuildProperty(unityFrameworkTarget, "LD_CLASSIC_1500", "-ld64");    
            var otherLinkerFlags = pbxProject.GetBuildPropertyForConfig(unityFrameworkTarget, "OTHER_LDFLAGS");
            otherLinkerFlags += " $(LD_CLASSIC_$(XCODE_VERSION_MAJOR))";
            pbxProject.AddBuildProperty(unityFrameworkTarget, "OTHER_LDFLAGS", otherLinkerFlags);

            var mainTargetLinkFrameworksId = pbxProject.GetFrameworksBuildPhaseByTarget(pbxProject.GetUnityMainTargetGuid());
            var unityFrameworkBuildProductId = pbxProject.GetTargetProductFileRef(pbxProject.GetUnityFrameworkTargetGuid());
            Debug.Log("Linking unity framework to main target to support unity 2020 - " + mainTargetLinkFrameworksId + ", " + unityFrameworkBuildProductId);
            pbxProject.AddFileToBuildSection(pbxProject.GetUnityMainTargetGuid(), mainTargetLinkFrameworksId, unityFrameworkBuildProductId);

#if !UNITY_2021_3_OR_NEWER
            string otherCFlags = pbxProject.GetBuildPropertyForAnyConfig(pbxProject.GetUnityMainTargetGuid(), "OTHER_CFLAGS");
            Debug.Log("TTPPostProcessSettings:: found other cFlags: " + otherCFlags);
            if (otherCFlags.Contains("-mno-thumb")) 
            { 
                otherCFlags = otherCFlags.Replace("-mno-thumb", "");
            } 
            Debug.Log("TTPPostProcessSettings:: saving other cFlags: " + otherCFlags);
            pbxProject.SetBuildProperty(pbxProject.GetUnityMainTargetGuid(), "OTHER_CFLAGS", otherCFlags); 
#endif
            
#endif

#if UNITY_2019_3_OR_NEWER
            string xcodeTarget = pbxProject.GetUnityMainTargetGuid();
#else
            string xcodeTarget = pbxProject.TargetGuidByName("Unity-iPhone");
#endif
            string xcodeTargetUnityFramework = pbxProject.TargetGuidByName("UnityFramework");
            
            if (pbxProject.ContainsFileByProjectPath("Libraries/Adjust/iOS/AdjustSigSdk.a"))
            {
                if (!string.IsNullOrEmpty(xcodeTargetUnityFramework))
                {
                    pbxProject.AddBuildProperty(xcodeTargetUnityFramework, "OTHER_LDFLAGS", "-force_load $(PROJECT_DIR)/Libraries/Adjust/iOS/AdjustSigSdk.a");
                }
                else
                {
                    pbxProject.AddBuildProperty(xcodeTarget, "OTHER_LDFLAGS", "-force_load $(PROJECT_DIR)/Libraries/Adjust/iOS/AdjustSigSdk.a");
                }
            }
            else 
            {
                Debug.Log("TTPPostProcessSettings::Can't find AdjustSigSdk.a lib in the project");
            }

            File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());

            var plistPath = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            rootDict = plist.root;

            if (ShouldBlockFirebaseAnalytics())
            {
                rootDict.SetBoolean("GOOGLE_ANALYTICS_DEFAULT_ALLOW_AD_PERSONALIZATION_SIGNALS", false);
            }
            rootDict.SetBoolean("CADisableMinimumFrameDurationOnPhone", true);
            
            // Put correct path to the site where for receiving copies of winning install-validation postbacks
            // url https://ttpsdk.info is only for testing
            // Full description in RST-1717
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://ttpsdk.info");

            var array = rootDict.CreateArray("SKAdNetworkItems");
            array.AddDict().SetString("SKAdNetworkIdentifier", "22mmun2rn5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "238da6jt44.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "24t9a8vw3c.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "24zw6aqk47.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "252b5q8x7y.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "275upjj5gd.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "294l99pt4k.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "2fnua5tdw4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "2q884k2j68.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "2rq3zucswp.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "2tdux39lx8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "2u9pt9hc89.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "32z4fx6l9h.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3cgn6rq224.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3l6bd9hu43.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3qcr597p9d.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3qy4746246.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3rd42ekr43.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "3sh42y64q3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "424m5254lk.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4468km3ulz.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "44jx6755aq.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "44n7hlldy6.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "47vhws6wlr.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "488r3q3dtq.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4dzt52r2t5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4fzdc2evr5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4mn522wn87.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4pfyvq9l8r.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "4w7y6s5ca2.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "523jb4fst2.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "52fl2v3hgk.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "53nm4hsx3w.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "54nzkqm89y.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "55644vm79v.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "55y65gfgn7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "577p5t736z.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "578prtvx9j.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5a6flpkh64.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5f5u5tfb26.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5ghnmfs3dh.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5l3tpt7t6e.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5lm9lj6jb7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5mv394q32t.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "5tjdwbrq8w.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "627r9wr2y5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "633vhxswh4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "67369282zy.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6964rsfnh4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6g9af3uyq4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6p4ks3rnbw.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6qx585k4p6.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6rd35atwn8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6v7lgmsu45.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6xzpu9s2p8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6yxyv74ff7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "737z793b9f.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "74b6s63p6l.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7953jerfzd.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "79pbpufp6p.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "79w64w269u.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7bxrt786m8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7fbxrn65az.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7fmhfwg9en.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7k3cvf297u.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7rz58n8ntl.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7tnzynbdc7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "7ug5zh24hu.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "84993kbrcf.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "866k9ut3g3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "88k8774x49.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "899vrgt9g8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "89z7zv988g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8c4e2ghe7u.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8m87ys6875.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8qiegk9qfv.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8r8llnkz5a.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8s468mfl3y.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "8w3np9l82g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "97r2b46745.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9b89h5y424.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9g2aggbj52.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9nlqeag3gk.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9rd848q2bz.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9t245vhmpl.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9vvzujtq5s.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9wsyqb3ku7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "9yg77x724h.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "a2p9lx4jpn.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "a7xqa6mtl2.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "a8cz6cu7e5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "au67k4efj4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "av6w8kgt66.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "axh5283zss.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "b55w3d8y8z.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "b9bk5wbcq9.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "bvpn9ufa9b.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "bxvub5ada5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "c3frkrj4fj.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "c6k4g5qg8m.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "c7g47wypnu.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cad8qz2s3j.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ce8ybjwass.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cg4yq2srnc.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cj5566h2ga.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cp8zw746q7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cs644xg564.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cstr6suwn9.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "cwn433xbcr.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "d7g9azk84q.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dbu4b84rxf.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dd3a75yxkv.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dkc879ngq3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dmv22haz9p.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dn942472g5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dr774724x4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dt3cjx1a9i.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dzg6xy7pwj.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "e5fvkxwrpn.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ecpz2srf59.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "eh6m2bh4zr.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ejvt5qm6ak.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "eqhxz8m8av.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "f38h382jlk.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "f73kdq92p3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "f7s53z58qe.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "feyaarzu9v.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "fkak3gfpt6.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "fz2k2k5tej.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "g28c52eehv.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "g2y4y55b64.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "g6gcrrvk4p.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "gfat3222tu.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ggvn48r87g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "glqzh8vgby.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "gta8lk7p23.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "gta9lk7p23.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "gvmwg8q7h5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "h5jmj969g5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "h65wbv5k3f.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "h8vml93bkz.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "hb56zgv37p.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "hdw39hrw9y.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "hjevpa356n.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "hs6bdukanm.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "jb7bn6koa5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "jk2fsx2rgz.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "k674qkevps.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "k6y4y55b64.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "kbd757ywx3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "kbmxgpxpgc.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "klf5c3l5u5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "krvm3zuq6h.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "l6nv3x923s.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "l93v5h6a4m.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ln5gz23vtd.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "lr83yxwka7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ludvb6z3bs.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "m297p6643m.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "m5mvw97r93.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "m8dbw4sv7c.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mj797d8u6f.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mlmmfzh3r3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mls7yz5dvl.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mp6xlyr22a.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mqn7fxpca7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "mtkv5xtk9e.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "n38lu8286q.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "n66cz3y3bx.SKAdNetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "n66cz3y3bx.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "n6fk4nfna4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "n9x2a789qt.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "nfqy3847ph.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "nrt9jy4kw9.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "nu4557a4je.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "nzq8sh4pbs.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "p78axxw29g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "pd25vrrwzn.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ppxm28t8ap.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "prcb7njmu6.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "pu4na253f3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "pwa73g5rt2.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "pwdxu55a5a.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "qlbq5gtkt8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "qqp299437r.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "qu637u8glc.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "r45fhb6rf7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "r8lj5b58b5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "rvh3l7un93.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "rx5hdcabgc.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "s39g8k73mm.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "s69wq72ugq.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "sczv5946wb.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "su67r6k2v3.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "t38b2kh725.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "t3b3f7n3x8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "t6d3zquu66.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "t7ky8fmwkd.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "tl55sbb4fm.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "tmhh9296z4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "tvvz7th9br.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "u679fj5vs4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "uw77j35x4d.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "uzqba5354d.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "v4nxqhlyqp.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "v72qych5uu.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "v7896pgt74.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "v79kvwwj4g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "v9wttpbfk9.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "vc83br9sjg.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "vcra2ehyfk.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "vhf287vqwu.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "vutu7akeur.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "w28pnjg2k4.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "w7jznl3r6g.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "w9q455wk68.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "wg4vff78zm.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "wzmmz9fp6w.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x44k69ngh6.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x5854y7y24.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x5l83yy675.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x8jxxk4ff5.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x8uqf25wch.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "xga6mpmplv.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "xmn954pzmp.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "xx9sdjej2w.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "xy9t38ct57.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "y45688jllp.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "y5ghdn5j9k.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "y755zyxw56.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "yclnxrl5pm.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ydx93a7ass.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "yrqqpx2mcb.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "z24wtl6j62.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "z4gj7hsk7h.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "z5b3gh5ugf.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "z959bm4gru.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "zh3b7bxvad.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "zmvfpc5aq8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "zq492l623r.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "g69uk9uh2b.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "33r6p7g8nc.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "6rd35atwn8.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "z959bm4gru.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ln5gz23vtd.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "qwpu75vrh2.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "ns5j362hk7.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "dticjx1a9i.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "r26jy69rpl.skadnetwork");
            array.AddDict().SetString("SKAdNetworkIdentifier", "x2jnk7ly8j.skadnetwork");

            // fix problem with statusbar on iOS 14
            if (!rootDict.values.ContainsKey("UIViewControllerBasedStatusBarAppearance"))
            {
                rootDict.SetBoolean("UIViewControllerBasedStatusBarAppearance", false);
            }

            File.WriteAllText(plistPath, plist.WriteToString());

#if UNITY_2019_4_OR_NEWER
            Debug.Log("TTPPostProcessSettings::Update UnityFramework include");
            string mainAppPath = Path.Combine(path, "MainApp", "main.mm");
            string mainContent = File.ReadAllText(mainAppPath);
            Debug.Log(mainContent);
            string newContent = mainContent.Replace("#include <UnityFramework/UnityFramework.h>", @"#include ""../UnityFramework/UnityFramework.h""");
            File.WriteAllText(mainAppPath, newContent);
#endif
        }

        private static bool ShouldBlockFirebaseAnalytics()
        {
            var analyticsConfigurationJson = ((TTPCore.ITTPCoreInternal)TTPCore.Impl).GetConfigurationJson("analytics");
            if (!string.IsNullOrEmpty(analyticsConfigurationJson))
            {
                var analyticsConfig = TTPJson.Deserialize(analyticsConfigurationJson) as Dictionary<string, object>;
                if (analyticsConfig != null && analyticsConfig.ContainsKey("firebaseEchoMode") && (bool)analyticsConfig["firebaseEchoMode"])
                {
                    return false;
                }
            }
            return true;
        }

        private static string GetTargetGUID(PBXProject proj)
        {
#if UNITY_2019_3_OR_NEWER
            return proj.GetUnityMainTargetGuid();
#else
            return proj.TargetGuidByName("Unity-iPhone");
#endif
        }
    }
}
#endif
