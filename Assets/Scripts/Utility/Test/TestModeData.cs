#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Utility.Test
{
    [FilePath("TestModeData.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class TestModeData : ScriptableSingleton<TestModeData>
    {
        public bool testModeActive;

        public void Save()
        {
            Save(true);
        }
    }

    [InitializeOnLoad]
    public static class TestMode
    {
        private const string TestMenuItem = "Tools/Toggle Test Mode";
    
        public static bool IsTestModeActive => TestModeData.instance.testModeActive;
    
        static TestMode()
        {
            UpdateCheckMarks();
        }
        
        [MenuItem(TestMenuItem)]
        public static void ToggleTestMode()
        {
            SetTestMode(!IsTestModeActive);
        }
    
        private static void UpdateCheckMarks()
        {
            Menu.SetChecked(TestMenuItem, IsTestModeActive);
        }

        private static void SetTestMode(bool value)
        {
            TestModeData.instance.testModeActive = value;
            Menu.SetChecked(TestMenuItem, TestModeData.instance.testModeActive);
            Debug.Log(IsTestModeActive ? "Test mode enabled" : "Test mode disabled");
            TestModeData.instance.Save();
        }
    }
}
#endif
