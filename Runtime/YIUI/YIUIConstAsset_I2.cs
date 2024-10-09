using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YIUIFramework
{
    public partial class YIUIConstAsset
    {
        [BoxGroup("多语言设置", CenterLabel = true)]
        [LabelText("编辑器下模拟运行时")]
        public bool I2UseRuntimeModule = false;

        [BoxGroup("多语言设置", CenterLabel = true)]
        [LabelText("默认多语言")]
        public string I2DefaultLanguage = "Chinese";
    }
}