using System;
using System.Text.RegularExpressions;

namespace LostPolygon.uLiveWallpaper.Editor.Internal {
    internal class UnityVersionParser {
        private readonly int _versionMajor;
        private readonly int _versionMinor;
        private readonly int _versionPatch;
        private readonly UnityBuildType? _versionBuildType = UnityBuildType.Unknown;
        private readonly int? _versionReleaseNumber;

        public UnityVersionParser(string unityVersion) {
            Match versionMatch = Regex.Match(unityVersion, @"(\d+)\.(\d+)\.(\d+)([abpf])?(\d+)?");
            _versionMajor = TryParseInt(versionMatch.Groups[1].Value, 0);
            _versionMinor = TryParseInt(versionMatch.Groups[2].Value, 0);
            _versionPatch = TryParseInt(versionMatch.Groups[3].Value, 0);
            if (versionMatch.Groups.Count <= 4)
                return;

            string versionBuildType = versionMatch.Groups[4].Value;
            switch (versionBuildType) {
                case "f":
                    _versionBuildType = UnityBuildType.Final;
                    break;
                case "p":
                    _versionBuildType = UnityBuildType.Patch;
                    break;
                case "a":
                    _versionBuildType = UnityBuildType.Alpha;
                    break;
                case "b":
                    _versionBuildType = UnityBuildType.Beta;
                    break;
                default:
                    _versionBuildType = UnityBuildType.Unknown;
                    break;
            }

            _versionReleaseNumber = TryParseInt(versionMatch.Groups[5].Value);
        }

        public int GetUnityVersionNumeric() {
            return _versionMajor * 100 + _versionMinor * 10 + _versionPatch;
        }

        public int VersionMajor {
            get {
                return _versionMajor;
            }
        }

        public int VersionMinor {
            get {
                return _versionMinor;
            }
        }

        public int VersionPatch {
            get {
                return _versionPatch;
            }
        }

        public UnityBuildType? VersionBuildType {
            get {
                return _versionBuildType;
            }
        }

        public int? VersionReleaseNumber {
            get {
                return _versionReleaseNumber;
            }
        }

        public override string ToString() {
            return
                string.Format(
                    "{0}.{1}.{2}{3}{4}",
                    _versionMajor,
                    _versionMinor,
                    _versionPatch,
                    _versionBuildType != null ? BuildTypeShortName(_versionBuildType.Value) : "",
                    _versionReleaseNumber != null ? _versionReleaseNumber.Value.ToString() : ""
                    );
        }

        public static string BuildTypeShortName(UnityBuildType buildType) {
            switch (buildType) {
                case UnityBuildType.Final:
                    return "f";
                case UnityBuildType.Alpha:
                    return "a";
                case UnityBuildType.Beta:
                    return "b";
                case UnityBuildType.Patch:
                    return "p";
                case UnityBuildType.Unknown:
                    return "";
                default:
                    throw new ArgumentOutOfRangeException("buildType", buildType, null);
            }
        }

        private static int TryParseInt(string s, int defaultValue) {
            int val;
            return Int32.TryParse(s, out val) ? val : defaultValue;
        }

        private static int? TryParseInt(string s) {
            int val;
            if (!Int32.TryParse(s, out val))
                return null;

            return val;
        }

        public enum UnityBuildType {
            Unknown,
            Final,
            Alpha,
            Beta,
            Patch
        }
    }
}
