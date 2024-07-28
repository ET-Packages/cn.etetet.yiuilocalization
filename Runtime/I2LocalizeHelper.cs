

namespace I2.Loc
{
    /// <summary>
    /// 文档: https://lib9kmxvq7k.feishu.cn/wiki/ZOKxwi5XsijdX8kPU9McSxs1nxd
    /// </summary>
    public static class I2LocalizeHelper
    {
        #if UNITY_EDITOR
        public const string I2GlobalSourcesEditorFile = "Packages/cn.etetet.yiuilocalization/Assets/Editor/I2Localization";
        public const string I2GlobalSourcesEditorPath = I2GlobalSourcesEditorFile + "/I2Languages.asset";
        #endif

        public const string I2ResAssetNamePrefix = "I2_";
    }
}