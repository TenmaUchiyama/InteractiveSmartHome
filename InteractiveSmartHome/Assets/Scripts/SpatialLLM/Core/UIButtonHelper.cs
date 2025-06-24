using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

namespace SpatialLLM.Core
{
    public enum UIButtonLabelType
    {
        Y,
        X,
        A,
        B,
        LeftThumbstick,
        RightThumbstick,
        LeftTrigger,
        RightTrigger,
        LeftGrip,
        RightGrip,
        Menu,
        None
    }

    public class UIButtonHelper : MonoBehaviour
    {
        [System.Serializable]
        public class ButtonLabelEntry
        {
            public UIButtonLabelType key;
            public UIButtonLabel label;
        }

        [Header("ボタンヘルパー一覧")]
        [SerializeField]
        private List<ButtonLabelEntry> buttonLabels = new();

        private Dictionary<UIButtonLabelType, UIButtonLabel> labelDict;


        private UIButtonLabelType[] rightControllerLabels = new[]
        {
            UIButtonLabelType.A,
            UIButtonLabelType.B,
            UIButtonLabelType.RightThumbstick,
            UIButtonLabelType.RightTrigger,
            UIButtonLabelType.RightGrip
        };

        private UIButtonLabelType[] leftControllerLabels = new[]
        {
            UIButtonLabelType.Y,
            UIButtonLabelType.X,
            UIButtonLabelType.LeftThumbstick,
            UIButtonLabelType.LeftTrigger,
            UIButtonLabelType.LeftGrip
        };

        void Awake()
        {
            labelDict = new Dictionary<UIButtonLabelType, UIButtonLabel>();

        

            foreach (var entry in buttonLabels)
            {
                if (entry.label != null)
                {
                    labelDict[entry.key] = entry.label;
                }
            }
        }

        /// <summary>
        /// 指定したキーのラベルを表示・非表示に切り替え
        /// </summary>
        public void SetLabelVisible(UIButtonLabelType key, bool visible)
        {
            if (labelDict.TryGetValue(key, out var label))
            {
                label.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// 指定キーのラベルテキストを更新
        /// </summary>
        public void SetLabelText(UIButtonLabelType key, string text)
        {
            if (labelDict.TryGetValue(key, out var label))
            {
                label.SetLabel(text);
            }
        }

        /// <summary>
        /// ラベルの色を変更
        /// </summary>
        public void SetLabelColor(UIButtonLabelType key, Color color)
        {


            if (labelDict.TryGetValue(key, out var label))
            {
                label.SetLabelColor(color);
            }
        }


        public void SetLabel(UIButtonLabelType key, string text, Color labelColor, Color bgColor )
        {
            if (labelDict.TryGetValue(key, out var label))
            {
                label.SetVisible(true);
                label.SetLabel(text);
                label.SetLabelColor(labelColor);
                label.SetBackgroundColor(bgColor);
            }
        }


        public void ShowAllRightLabels()
        {

            // やり方としては、右のコントローラーのUIButtonLabelTypeを配列で管理し、containsでチェックする方法が考えられます。
            foreach (var key in rightControllerLabels)
            {
                if (labelDict.TryGetValue(key, out var label))
                {
                    label.SetVisible(true);
                }
            }

        }


        public void ShowAllLeftLabels()
        {
            foreach (var key in leftControllerLabels)
            {
                if (labelDict.TryGetValue(key, out var label))
                {
                    label.SetVisible(true);
                }
            }
        }



        public void SetBackGroundColor(UIButtonLabelType key, Color color)
        {
            if (labelDict.TryGetValue(key, out var label))
            {
                label.SetBackgroundColor(color);
            }
        }


        public void DisableAllLabels()
        {
            foreach (var entry in buttonLabels)
            {
                if (entry.label != null)
                {
                    entry.label.SetVisible(false);
                }
            }
        }
    

        public void ShowAllLabels()
        {
            foreach (var entry in buttonLabels)
            {
                if (entry.label != null)
                {
                    entry.label.SetVisible(true);
                }
            }
        }
    }





}
