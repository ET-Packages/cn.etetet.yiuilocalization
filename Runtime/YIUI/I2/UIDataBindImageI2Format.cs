using System;
using ET;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YIUIFramework
{
    [RequireComponent(typeof(Image))]
    [LabelText("Image 图片多语言")]
    [AddComponentMenu("YIUIBind/Data/图片多语言 【ImageI2】 UIDataBindImageI2")]
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
        [LabelText("多语言Key")]
        private string m_I2Key;

        [SerializeField]
        [InfoBox("如果有拼接则用拼接,否则多语言为拼接Key")]
        [LabelText("拼接 (多语言占位{0},没有就按顺序)")] //{1}{0}可以反过来 也可以自己加各种符号填充扩展性肯定比直接 拼接要强
        private string m_JointFormat;

        protected override int Mask()
        {
            return -1; //允许任何数据  只要你吧ToString写清楚就行
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

        private object[] m_ParamList;

        protected override void OnValueChanged()
        {
            if (!UIOperationHelper.IsPlaying())
            {
                return;
            }

            if (DataSelectDic == null || DataSelectDic.Count <= 0)
            {
                SetEnabled(false);
                return;
            }

            if (string.IsNullOrEmpty(m_I2Key))
            {
                SetEnabled(false);
                return;
            }

            var i2Content = GetI2Content(m_I2Key);
            if (string.IsNullOrEmpty(i2Content))
            {
                SetEnabled(false);
                return;
            }

            try
            {
                var dataValue = "";
                if (!string.IsNullOrEmpty(m_JointFormat))
                {
                    if (m_ParamList == null || m_ParamList.Length != DataSelectDic.Count + 1)
                    {
                        m_ParamList = new object[DataSelectDic.Count + 1];
                    }

                    var index = 0;
                    m_ParamList[index] = i2Content;
                    foreach (var dataSelect in DataSelectDic.Values)
                    {
                        index++;
                        m_ParamList[index] = GetDataToString(dataSelect);
                    }

                    dataValue = string.Format(m_JointFormat, m_ParamList);
                }
                else
                {
                    if (m_ParamList == null || m_ParamList.Length != DataSelectDic.Count)
                    {
                        m_ParamList = new object[DataSelectDic.Count];
                    }

                    var index = -1;
                    foreach (var dataSelect in DataSelectDic.Values)
                    {
                        index++;
                        m_ParamList[index] = GetDataToString(dataSelect);
                    }

                    dataValue = string.Format(i2Content, m_ParamList);
                }

                if (string.IsNullOrEmpty(dataValue))
                {
                    SetEnabled(false);
                    return;
                }

                ChangeSprite(dataValue).NoContext();
            }
            catch (FormatException exp)
            {
                Logger.LogError($"{name} 字符串拼接Format 出错请检查是否有拼写错误  {i2Content}");
                Logger.LogError(exp.Message, this);
            }
        }

        private string GetDataToString(UIDataSelect dataSelect)
        {
            var dataValue = dataSelect?.Data?.DataValue;
            if (dataValue == null) return "";

            switch (dataValue.UIBindDataType)
            {
                case EUIBindDataType.Float:
                    return dataValue.GetValue<float>().ToString();
                case EUIBindDataType.Double:
                    return dataValue.GetValue<double>().ToString();
                default:
                    return dataValue.GetValueToString();
            }
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
            using var coroutineLock = await EventSystem.Instance?.YIUIInvokeEntityAsync<YIUIInvokeEntity_CoroutineLock, ETTask<Entity>>(YIUISingletonHelper.YIUIMgr, new YIUIInvokeEntity_CoroutineLock { Lock = this.GetHashCode() });

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

            var sprite = await EventSystem.Instance?.YIUIInvokeEntityAsync<YIUIInvokeEntity_LoadSprite, ETTask<Sprite>>(YIUISingletonHelper.YIUIMgr, new YIUIInvokeEntity_LoadSprite { ResName = resName });

            if (sprite == null)
            {
                m_LastSpriteName = "";
                SetEnabled(false);
                return;
            }

            ReleaseLastSprite();

            if (gameObject == null || m_Image == null)
            {
                EventSystem.Instance?.YIUIInvokeEntitySync(YIUISingletonHelper.YIUIMgr, new YIUIInvokeEntity_Release { obj = sprite });
                Logger.LogError($"{resName} 加载过程中 对象被摧毁了 gameObject == null || m_Image == null");
                return;
            }

            m_LastSprite = sprite;
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
                EventSystem.Instance?.YIUIInvokeEntitySync(YIUISingletonHelper.YIUIMgr, new YIUIInvokeEntity_Release { obj = m_LastSprite });
                m_LastSprite = null;
            }
        }
    }
}