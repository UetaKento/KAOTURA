using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine.XR.ARKit;
#endif
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine.SceneManagement;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Populates the action unit coefficients for an <see cref="ARFace"/>.
    /// </summary>
    /// <remarks>
    /// If this <c>GameObject</c> has a <c>SkinnedMeshRenderer</c>,
    /// this component will generate the blend shape coefficients from the underlying <c>ARFace</c>.
    ///
    /// </remarks>
    [RequireComponent(typeof(ARFace))]
    public class BlendShapeController : MonoBehaviour
    {
        [SerializeField]
        private float m_CoefficientScale = 100.0f;

        private int goalWeight;
        private int topFaceID, bottomFaceID;
        private TextMeshProUGUI goalScoreText, weightScoreText, clearText;
        public TextMeshProUGUI blendShapesHint;
        private Button buttonToTitle;
        private float time = 3.0f;
        private bool clearFlag = true;
        bool isActive = false;
        private int max = 0;
        private string topFaceID_Description, bottomFaceID_Description;

        public float coefficientScale
        {
            get { return m_CoefficientScale; }
            set { m_CoefficientScale = value; }
        }

        [SerializeField]
        SkinnedMeshRenderer m_SkinnedMeshRenderer;

        public SkinnedMeshRenderer skinnedMeshRenderer
        {
            get
            {
                return m_SkinnedMeshRenderer;
            }
            set
            {
                m_SkinnedMeshRenderer = value;
                CreateFeatureBlendMapping();
            }
        }

#if UNITY_IOS && !UNITY_EDITOR
    ARKitFaceSubsystem m_ARKitFaceSubsystem;

    Dictionary<ARKitBlendShapeLocation, int> m_FaceArkitBlendShapeIndexMap;
#endif

        ARFace m_Face;

        void Awake()
        {
            m_Face = GetComponent<ARFace>();
            CreateFeatureBlendMapping();
            var topFaceDescriptDic = new Dictionary<int, string>()
            {
                {1, "ID:1 眉を上に持ちあげる"},
                {2, "ID:2 眉を顔の中心に集める"},
                {3, "ID:3 まぶたを閉じる"},
                {4, "ID:4 目を大きく見開く"},
                {5, "ID:5 右側をみる"},
                {6, "ID:6 左側をみる"},
                {7, "ID:7 上をみる"},
                {8, "ID:8 下をみる"}
            };
            var bottomFaceDescriptDic = new Dictionary<int, string>()
            {
                {1, "ID:1 頬を膨らませる"},
                {2, "ID:2 口を開く"},
                {3, "ID:3 あごを前に出す"},
                {4, "ID:4 口全体を右に寄せる"},
                {5, "ID:5 口全体を左に寄せる"},
                {6, "ID:6 唇を前に突き出す"},
                {7, "ID:7 上の唇を噛む"},
                {8, "ID:8 下の唇を噛む"},
                {9, "ID:9 笑顔を作る"},
                {10, "ID:10 口角を下げる"},
                {11, "ID:11 歯を剥き出しにする"}
            };

            topFaceID = Random.Range(1, 5); //ID:5~6は難しいので使わない。
            bottomFaceID = Random.Range(1, 12);
            switch (topFaceID)
            {
                case 1:
                    max += 2;
                    //max += 3;
                    break;
                case 2:
                    max += 3;
                    //max += 4;
                    break;
                default:
                    max += 2;
                    break;
            }
            switch (bottomFaceID)
            {
                case 4:
                    max += 2;
                    break;
                case 5:
                    max += 2;
                    break;
                case 9:
                    max += 3;
                    //max += 4;
                    break;
                case 10:
                    max += 2;
                    //max += 4;
                    break;
                case 11:
                    max += 3;
                    //max += 4;
                    break;
                default:
                    max += 1;
                    break;
            }
            goalWeight = (int)Random.Range((max * 80) - 150, (max * 80) - 50);
            goalScoreText = GameObject.Find("GoalScoreText").GetComponent<TextMeshProUGUI>();
            weightScoreText = GameObject.Find("WeightScoreText").GetComponent<TextMeshProUGUI>();
            clearText = GameObject.Find("ClearText").GetComponent<TextMeshProUGUI>();
            blendShapesHint = GameObject.Find("BlendShapesHint").GetComponent<TextMeshProUGUI>();
            topFaceDescriptDic.TryGetValue(topFaceID, out topFaceID_Description);
            bottomFaceDescriptDic.TryGetValue(bottomFaceID, out bottomFaceID_Description);
            blendShapesHint.text += topFaceID_Description;
            blendShapesHint.text += "\n" + bottomFaceID_Description;
            blendShapesHint.gameObject.SetActive(false);
            buttonToTitle = GameObject.Find("ToTitle").GetComponent<Button>();
            buttonToTitle.gameObject.SetActive(false);
            goalScoreText.text = "目標の点数は " + goalWeight.ToString();

        }

        void CreateFeatureBlendMapping()
        {
            if (skinnedMeshRenderer == null || skinnedMeshRenderer.sharedMesh == null)
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
        const string strPrefix = "blendShape2.";
        m_FaceArkitBlendShapeIndexMap = new Dictionary<ARKitBlendShapeLocation, int>();

        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowDownLeft        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "browDown_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowDownRight       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "browDown_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowInnerUp         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "browInnerUp");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowOuterUpLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "browOuterUp_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.BrowOuterUpRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "browOuterUp_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekPuff           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekPuff");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekSquintLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekSquint_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.CheekSquintRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "cheekSquint_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeBlinkLeft        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeBlink_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeBlinkRight       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeBlink_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookDownLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookDown_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookDownRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookDown_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookInLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookIn_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookInRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookIn_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookOutLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookOut_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookOutRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookOut_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookUpLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookUp_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeLookUpRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeLookUp_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeSquintLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeSquint_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeSquintRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeSquint_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeWideLeft         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeWide_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.EyeWideRight        ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "eyeWide_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawForward          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawForward");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawLeft             ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawLeft");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawOpen             ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawOpen");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.JawRight            ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "jawRight");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthClose          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthClose");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthDimpleLeft     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthDimple_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthDimpleRight    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthDimple_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFrownLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthFrown_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFrownRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthFrown_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthFunnel         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthFunnel");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLeft           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthLeft");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLowerDownLeft  ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthLowerDown_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthLowerDownRight ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthLowerDown_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPressLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthPress_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPressRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthPress_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthPucker         ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthPucker");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRight          ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRight");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRollLower      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRollLower");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthRollUpper      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthRollUpper");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthShrugLower     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthShrugLower");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthShrugUpper     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthShrugUpper");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthSmileLeft      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthSmile_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthSmileRight     ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthSmile_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthStretchLeft    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthStretch_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthStretchRight   ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthStretch_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthUpperUpLeft    ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthUpperUp_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.MouthUpperUpRight   ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "mouthUpperUp_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.NoseSneerLeft       ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "noseSneer_L");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.NoseSneerRight      ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "noseSneer_R");
        m_FaceArkitBlendShapeIndexMap[ARKitBlendShapeLocation.TongueOut           ]   = skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex(strPrefix + "tongueOut");
#endif
        }

        void SetVisible(bool visible)
        {
            if (skinnedMeshRenderer == null) return;

            skinnedMeshRenderer.enabled = visible;
        }

        void UpdateVisibility()
        {
            var visible =
                enabled &&
                (m_Face.trackingState == TrackingState.Tracking) &&
                (ARSession.state > ARSessionState.Ready);

            SetVisible(visible);
        }

        void OnEnable()
        {
#if UNITY_IOS && !UNITY_EDITOR
        var faceManager = FindObjectOfType<ARFaceManager>();
        if (faceManager != null)
        {
            m_ARKitFaceSubsystem = (ARKitFaceSubsystem)faceManager.subsystem;
        }
#endif
            UpdateVisibility();
            m_Face.updated += OnUpdated;
            ARSession.stateChanged += OnSystemStateChanged;
        }

        void OnDisable()
        {
            m_Face.updated -= OnUpdated;
            ARSession.stateChanged -= OnSystemStateChanged;
        }

        void OnSystemStateChanged(ARSessionStateChangedEventArgs eventArgs)
        {
            UpdateVisibility();
        }

        void OnUpdated(ARFaceUpdatedEventArgs eventArgs)
        {
            UpdateVisibility();
            UpdateFaceFeatures();
        }

        //-----ScreenShot.csのコピー始まり-----
        [DllImport("__Internal")]
        private static extern void SaveToAlbum(string path);

        IEnumerator SaveToCameraroll(string path)
        {
            // ファイルが生成されるまで待つ
            while (true)
            {
                if (File.Exists(path))
                    break;
                yield return null;
            }

            SaveToAlbum(path);
        }
        //-----ScreenShot.csのコピー終わり-----


        void UpdateFaceFeatures()
        {
            if (skinnedMeshRenderer == null || !skinnedMeshRenderer.enabled || skinnedMeshRenderer.sharedMesh == null)
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
        var topFaceIndexWeightDic = new Dictionary<string, int>();
        var bottomFaceIndexWeightDic = new Dictionary<string, int>();

        int browInnerUpIndex, browOuterUpLeftIndex, browOuterUpRightIndex;
        int browDownLeftIndex, browDownRightIndex, noseSneerLeftIndex, noseSneerRightIndex;
        int eyeLookUpLeftIndex, eyeLookUpRightIndex;
        int eyeLookDownLeftIndex, eyeLookDownRightIndex;
        int eyeLookOutLeftIndex, eyeLookInRightIndex;
        int eyeLookInLeftIndex, eyeLookOutRightIndex;
        int eyeBlinkLeftIndex, eyeBlinkRightIndex;
        int eyeWideLeftIndex, eyeWideRightIndex;

        int cheekPuffIndex;
        int jawOpenIndex;
        int jawForwardIndex;
        int jawLeftIndex, mouthLeftIndex;
        int jawRightIndex, mouthRightIndex;
        int mouthPuckerIndex;
        int mouthRollUpperIndex;
        int mouthRollLowerIndex;
        int cheekSquintLeftIndex, cheekSquintRightIndex, mouthSmileLeftIndex, mouthSmileRightIndex;
        int mouthFrownLeftIndex, mouthFrownRightIndex, mouthStretchLeftIndex, mouthStretchRightIndex;
        int mouthUpperUpLeftIndex, mouthUpperUpRightIndex, mouthLowerDownLeftIndex, mouthLowerDownRightIndex;

        switch (topFaceID)
        {
            case 1:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.BrowInnerUp, out browInnerUpIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.BrowOuterUpLeft, out browOuterUpLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.BrowOuterUpRight, out browOuterUpRightIndex);
                topFaceIndexWeightDic.Add("browInnerUp", (int)skinnedMeshRenderer.GetBlendShapeWeight(browInnerUpIndex));
                topFaceIndexWeightDic.Add("browOuterUpLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(browOuterUpLeftIndex));
                topFaceIndexWeightDic.Add("browOuterUpRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(browOuterUpRightIndex));
                break;
            case 2:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.BrowDownLeft, out browDownLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.BrowDownRight, out browDownRightIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.NoseSneerLeft, out noseSneerLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.NoseSneerRight, out noseSneerRightIndex);
                topFaceIndexWeightDic.Add("browDownLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(browDownLeftIndex));
                topFaceIndexWeightDic.Add("browDownRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(browDownRightIndex));
                topFaceIndexWeightDic.Add("noseSneerLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(noseSneerLeftIndex));
                topFaceIndexWeightDic.Add("noseSneerRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(noseSneerRightIndex));
                break;
            case 3:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeBlinkLeft, out eyeBlinkLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeBlinkRight, out eyeBlinkRightIndex);
                topFaceIndexWeightDic.Add("eyeBlinkLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeBlinkLeftIndex));
                topFaceIndexWeightDic.Add("eyeBlinkRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeBlinkRightIndex));
                break;
            case 4:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeWideLeft, out eyeWideLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeWideRight, out eyeWideRightIndex);
                topFaceIndexWeightDic.Add("eyeWideLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeWideLeftIndex));
                topFaceIndexWeightDic.Add("eyeWideRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeWideRightIndex));
                break;
            case 5:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookOutLeft, out eyeLookOutLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookInRight, out eyeLookInRightIndex);
                topFaceIndexWeightDic.Add("eyeLookOutLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookOutLeftIndex));
                topFaceIndexWeightDic.Add("eyeLookInRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookInRightIndex));
                break;
            case 6:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookInLeft, out eyeLookInLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookOutRight, out eyeLookOutRightIndex);
                topFaceIndexWeightDic.Add("eyeLookInLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookInLeftIndex));
                topFaceIndexWeightDic.Add("eyeLookOutRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookOutRightIndex));
                break;
            case 7:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookUpLeft, out eyeLookUpLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookUpRight, out eyeLookUpRightIndex);
                topFaceIndexWeightDic.Add("eyeLookUpLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookUpLeftIndex));
                topFaceIndexWeightDic.Add("eyeLookUpRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookUpRightIndex));
                break;
            case 8:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookDownLeft, out eyeLookDownLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.EyeLookDownRight, out eyeLookDownRightIndex);
                topFaceIndexWeightDic.Add("eyeLookDownLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookDownLeftIndex));
                topFaceIndexWeightDic.Add("eyeLookDownRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(eyeLookDownRightIndex));
                break;
            default:
                break;
        }

        switch (bottomFaceID)
        {
            case 1:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.CheekPuff, out cheekPuffIndex);
                bottomFaceIndexWeightDic.Add("cheekPuff", (int)skinnedMeshRenderer.GetBlendShapeWeight(cheekPuffIndex));
                break;
            case 2:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.JawOpen, out jawOpenIndex);
                bottomFaceIndexWeightDic.Add("jawOpen", (int)skinnedMeshRenderer.GetBlendShapeWeight(jawOpenIndex));
                break;
            case 3:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.JawForward, out jawForwardIndex);
                bottomFaceIndexWeightDic.Add("jawForward", (int)skinnedMeshRenderer.GetBlendShapeWeight(jawForwardIndex));
                break;
            case 4:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.JawLeft, out jawLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthLeft, out mouthLeftIndex);
                bottomFaceIndexWeightDic.Add("jawLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(jawLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthLeftIndex));
                break;
            case 5:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.JawRight, out jawRightIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthRight, out mouthRightIndex);
                bottomFaceIndexWeightDic.Add("jawRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(jawRightIndex));
                bottomFaceIndexWeightDic.Add("mouthRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthRightIndex));
                break;
            case 6:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthPucker, out mouthPuckerIndex);
                bottomFaceIndexWeightDic.Add("mouthPucker", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthPuckerIndex));
                break;
            case 7:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthRollUpper, out mouthRollUpperIndex);
                bottomFaceIndexWeightDic.Add("mouthRollUpper", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthRollUpperIndex));
                break;
            case 8:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthRollLower, out mouthRollLowerIndex);
                bottomFaceIndexWeightDic.Add("mouthRollLower", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthRollLowerIndex));
                break;
            case 9:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.CheekSquintLeft, out cheekSquintLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.CheekSquintRight, out cheekSquintRightIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthSmileLeft, out mouthSmileLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthSmileRight, out mouthSmileRightIndex);
                bottomFaceIndexWeightDic.Add("cheekSquintLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(cheekSquintLeftIndex));
                bottomFaceIndexWeightDic.Add("cheekSquintRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(cheekSquintRightIndex));
                bottomFaceIndexWeightDic.Add("mouthSmileLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthSmileLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthSmileRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthSmileRightIndex));
                break;
            case 10:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthFrownLeft, out mouthFrownLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthFrownRight, out mouthFrownRightIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthStretchLeft, out mouthStretchLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthStretchRight, out mouthStretchRightIndex);
                bottomFaceIndexWeightDic.Add("mouthFrownLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthFrownLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthFrownRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthFrownRightIndex));
                bottomFaceIndexWeightDic.Add("mouthStretchLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthStretchLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthStretchRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthStretchRightIndex));
                break;
            case 11:
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthUpperUpLeft, out mouthUpperUpLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthUpperUpRight, out mouthUpperUpRightIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthLowerDownLeft, out mouthLowerDownLeftIndex);
                m_FaceArkitBlendShapeIndexMap.TryGetValue(ARKitBlendShapeLocation.MouthLowerDownRight, out mouthLowerDownRightIndex);
                bottomFaceIndexWeightDic.Add("mouthUpperUpLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthUpperUpLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthUpperUpRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthUpperUpRightIndex));
                bottomFaceIndexWeightDic.Add("mouthLowerDownLeft", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthLowerDownLeftIndex));
                bottomFaceIndexWeightDic.Add("mouthLowerDownRight", (int)skinnedMeshRenderer.GetBlendShapeWeight(mouthLowerDownRightIndex));
                break;
            default:
                break;
        }

        int weightSum = 0;
        foreach (KeyValuePair<string, int> item in topFaceIndexWeightDic){
            weightSum += item.Value;
            //topBlendShapes.text += item.Key;
        }
        foreach (KeyValuePair<string, int> item in bottomFaceIndexWeightDic){
            weightSum += item.Value;
            //topBlendShapes.text += item.Key;
        }

        weightScoreText.text = "あなたの点数は " + weightSum.ToString();

        if((goalWeight - 10) <= weightSum && weightSum <= (goalWeight + 10)){
            time -= Time.deltaTime;
            weightScoreText.text = time.ToString();
            if(time <= 0 && clearFlag){
                goalScoreText.gameObject.SetActive(false);
                weightScoreText.gameObject.SetActive(false);
                buttonToTitle.gameObject.SetActive(true);
                clearText.text = "クリア！！";
                skinnedMeshRenderer.enabled = false;

                //-----ScreenShot.csのコピー始まり-----
                string filename = "DigitalMiraiArt2023.png";
                string path = Application.persistentDataPath + "/" + filename;

                // 以前のスクリーンショットを削除する
                File.Delete(path);

                // スクリーンショットを撮影する
                ScreenCapture.CaptureScreenshot(filename);

                // カメラロールに保存する
                StartCoroutine(SaveToCameraroll(path));
                //-----ScreenShot.csのコピー終わり-----

                clearFlag = false;                
            }
        }

        using (var blendShapes = m_ARKitFaceSubsystem.GetBlendShapeCoefficients(m_Face.trackableId, Allocator.Temp))
        {
            foreach (var featureCoefficient in blendShapes)
            {
                int mappedBlendShapeIndex;
                if (m_FaceArkitBlendShapeIndexMap.TryGetValue(featureCoefficient.blendShapeLocation, out mappedBlendShapeIndex))
                {
                    if (mappedBlendShapeIndex >= 0)
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(mappedBlendShapeIndex, featureCoefficient.coefficient * coefficientScale);
                    }
                }
            }
        }
#endif
        }
    }
}
