using System;
using System.Linq;
using ET;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    [RequireComponent(typeof(Image))]
    [LabelText("Image 图片多语言Format")]
    [AddComponentMenu("YIUIBind/Data/图片多语言 【ImageI2】 UIDataBindImageI2Format")]
    public sealed class UIDataBindImageI2Format : UIDataBindSelectBase
    {
        [SerializeField]
        [ReadOnly]
        [Required("必须有此组件")]
        [LabelText("图片")]
        private Image m_Image;

        [SerializeField]
        [LabelText("自动调整图像大小")]
        private bool m_SetNativeSize = false;

        [SerializeField]
        [LabelText("可修改Enabled")]
        private bool m_ChangeEnabled = true;

        [NonSerialized]
        private string m_LastSpriteName;

        [SerializeField]
        [LabelText("多语言Key (字符串填充{0}{1}形式)")]
        private string m_I2Key;

        protected override int Mask()
        {
            return 1 << (int)EUIBindDataType.Int | 1 << (int)EUIBindDataType.String;
        }

        protected override int SelectMax()
        {
            return 1;
        }

        protected override void OnRefreshData()
        {
            base.OnRefreshData();
            m_Image = GetComponent<Image>();
            if (!m_ChangeEnabled && !m_Image.enabled)
            {
                Logger.LogError($"{name} 当前禁止修改Enabled 且当前处于隐藏状态 可能会出现问题 请检查");
            }
        }

        private void SetEnabled(bool set)
        {
            if (!m_ChangeEnabled) return;

            if (m_Image == null) return;

            m_Image.enabled = set;
        }

        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            var data = DataSelectDic.Count >= 1 ? DataSelectDic.First().Value : null;

            if (data == null)
            {
                SetEnabled(false);
                return;
            }

            var dataValue = GetDataToString(data);

            if (string.IsNullOrEmpty(dataValue))
            {
                SetEnabled(false);
                return;
            }

            ChangeSprite(dataValue).NoContext();
        }

        private string GetDataToString(UIDataSelect dataSelect)
        {
            var dataValue = dataSelect?.Data?.DataValue;
            if (dataValue == null) return "";
            var i2Content = GetI2Content(m_I2Key);
            if (string.IsNullOrEmpty(i2Content))
            {
                return "";
            }

            var dataString = "";

            try
            {
                switch (dataValue.UIBindDataType)
                {
                    case EUIBindDataType.Int:
                        var intValue = dataValue.GetValue<int>();
                        dataString = intValue < 0 ? "" : intValue.ToString();
                        break;
                    case EUIBindDataType.String:
                        dataString = dataValue.GetValue<string>();
                        break;
                    default:
                        Logger.LogError($"{name} 不支持此类型 {dataValue.UIBindDataType}", this);
                        break;
                }

                if (string.IsNullOrEmpty(dataString))
                {
                    return "";
                }

                if (!string.IsNullOrEmpty(i2Content))
                {
                    return string.Format(i2Content, dataString);
                }
            }
            catch (FormatException exp)
            {
                Logger.LogError($"{name} 字符串拼接Format 出错请检查是否有拼写错误  {i2Content} , {dataString}");
                Logger.LogError(exp.Message, this);
            }

            return "";
        }

        [NonSerialized]
        private string m_LastI2Key;

        [NonSerialized]
        private string m_I2Content;

        private string GetI2Content(string key)
        {
            //TODO 无法实施切换语言
            if (key != m_LastI2Key)
            {
                m_I2Content = LocalizationManager.GetTranslation(m_I2Key);
                if (string.IsNullOrEmpty(m_I2Content))
                {
                    Logger.LogErrorContext(this, $"{this.gameObject.name} 未找到多语言资源:[{m_I2Key}]");
                }
                else
                {
                    m_LastI2Key = key;
                }
            }

            return m_I2Content;
        }

        private async ETTask ChangeSprite(string resName)
        {
            using var coroutineLock = await EventSystem.Instance?.YIUIInvokeAsync<YIUIInvokeCoroutineLock, ETTask<Entity>>(new YIUIInvokeCoroutineLock { Lock = this.GetHashCode() });

            if (m_LastSpriteName == resName)
            {
                if (m_Image != null && m_Image.sprite != null)
                {
                    SetEnabled(true);
                }
                else
                {
                    SetEnabled(false);
                }

                return;
            }

            var sprite = await EventSystem.Instance?.YIUIInvokeAsync<YIUIInvokeLoadSprite, ETTask<Sprite>>(new YIUIInvokeLoadSprite { ResName = resName });

            if (sprite == null)
            {
                m_LastSpriteName = "";
                SetEnabled(false);
                return;
            }

            ReleaseLastSprite();

            if (gameObject == null || m_Image == null)
            {
                EventSystem.Instance?.YIUIInvokeSync(new YIUIInvokeRelease { obj = sprite });
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 gameObject == null || m_Image == null");
                return;
            }

            m_LastSprite   = sprite;
            m_Image.sprite = sprite;
            if (m_SetNativeSize)
                m_Image.SetNativeSize();

            SetEnabled(true);
            m_LastSpriteName = resName;
        }

        protected override void UnBindData()
        {
            base.UnBindData();
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            ReleaseLastSprite();
        }

        private Sprite m_LastSprite;

        private void ReleaseLastSprite()
        {
            if (m_LastSprite != null)
            {
                EventSystem.Instance?.YIUIInvokeSync(new YIUIInvokeRelease { obj = m_LastSprite });
                m_LastSprite = null;
            }
        }
    }
}