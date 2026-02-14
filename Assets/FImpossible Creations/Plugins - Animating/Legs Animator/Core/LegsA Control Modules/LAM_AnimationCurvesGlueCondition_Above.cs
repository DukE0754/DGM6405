using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

namespace FIMSpace.FProceduralAnimation
{
	[CreateAssetMenu(fileName = "LAM_AnimCurveGlueCondition_Above", menuName = "FImpossible Creations/Legs Animator/Control Module - Animation Curves Glue Condition Above", order = 5)]
    public class LAM_AnimationCurvesGlueCondition_Above : LegsAnimatorControlModuleBase
    {
        LegsAnimatorCustomModuleHelper _useHelper = null;

        Variable FloorValueAboveVar { get { return _useHelper.RequestVariable("Floor Value Above", 0.99f); } }
        Variable _play_FloorValueAbove = null;

        Variable IgnoreMidConditionsVar { get { return _useHelper.RequestVariable("Ignore Mid Conditions", false); } }
        Variable _play_IgnoreMidConditions = null;

        Variable AllowHeightGlueOnLevelVar { get { return _useHelper.RequestVariable("Allow Height Glue On Level", -1f); } }
        Variable _play_AllowHeightGlueOnLevels = null;

        private List<int> animatorHashes = null;
        private bool initialized = false;

        public override void OnInit(LegsAnimatorCustomModuleHelper helper)
        {
            if (LA.Mecanim == null) return;
            if (helper.customStringList == null) return;

            _useHelper = helper;

            _play_FloorValueAbove = FloorValueAboveVar;
            _play_IgnoreMidConditions = IgnoreMidConditionsVar;
            _play_AllowHeightGlueOnLevels = AllowHeightGlueOnLevelVar;

            animatorHashes = new List<int>();

            for (int l = 0; l < LA.Legs.Count; l++)
            {
                if (l >= helper.customStringList.Count) break;
                animatorHashes.Add(Animator.StringToHash(helper.customStringList[l]));
            }

            initialized = true;
        }

        public override void Leg_LateUpdate(LegsAnimatorCustomModuleHelper helper, Leg leg)
        {
            if (!initialized) return;
            if (leg.G_CustomForceAttach) return;

            float value = LA.Mecanim.GetFloat(animatorHashes[leg.PlaymodeIndex]);

            if (value <= _play_AllowHeightGlueOnLevels.GetFloat())
            {
                if (leg.A_PreWasAligning)
                {
                    value = _play_FloorValueAbove.GetFloat() + 0.01f;
                }
            }

            // 🔁 INVERTED LOGIC
            if (value >= _play_FloorValueAbove.GetFloat())
            {
                // FOOT GROUNDED
                leg.G_CustomForceAttach = LA.GroundedTime > 0.2f;

                if (_play_IgnoreMidConditions.GetBool())
                {
                    leg.G_CustomForceNOTDetach = true;
                }
            }
            else
            {
                // FOOT UNGROUNDED
                leg.G_CustomForceNOTAttach = true;

                if (_play_IgnoreMidConditions.GetBool())
                {
                    leg.G_CustomForceDetach = true;
                }
            }
        }

#if UNITY_EDITOR
        public override void Editor_InspectorGUI(LegsAnimator legsAnimator, LegsAnimatorCustomModuleHelper helper)
        {
            _useHelper = helper;

            if (legsAnimator.Mecanim == null)
            {
                EditorGUILayout.HelpBox("No Animator found!", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox("Grounds when animation curve value is ABOVE threshold.", MessageType.Info);
            GUILayout.Space(5);

            var floorValV = FloorValueAboveVar;
            if (!floorValV.TooltipAssigned)
                floorValV.AssignTooltip("Gluing condition based on animation curve being ABOVE threshold.");
            floorValV.Editor_DisplayVariableGUI();

            var ignMidV = IgnoreMidConditionsVar;
            ignMidV.Editor_DisplayVariableGUI();

            GUILayout.Space(5);

            if (helper.customStringList == null) helper.customStringList = new List<string>();
            var list = helper.customStringList;
            int targetCount = legsAnimator.Legs.Count;

            while (list.Count < targetCount) list.Add("");
            while (list.Count > targetCount) list.RemoveAt(list.Count - 1);

            EditorGUILayout.LabelField("Mecanim parameters per leg", EditorStyles.helpBox);

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = EditorGUILayout.TextField(
                    new GUIContent("Leg [" + i + "] Curve Parameter:",
                    "Leg = " + legsAnimator.Legs[i].BoneStart.name),
                    list[i]);
            }

            if (!initialized) return;

            GUILayout.Space(5);

            for (int l = 0; l < animatorHashes.Count; l++)
            {
                EditorGUILayout.BeginHorizontal();
                float val = LA.Mecanim.GetFloat(animatorHashes[l]);

                EditorGUILayout.LabelField("[" + l + "] " + val, GUILayout.Width(100));
                EditorGUILayout.LabelField(LA.Legs[l].Side.ToString(), GUILayout.Width(50));
                EditorGUILayout.LabelField(
                    val >= _play_FloorValueAbove.GetFloat()
                    ? "FOOT GROUNDED"
                    : "FOOT MOVING");

                EditorGUILayout.EndHorizontal();
            }
        }
#endif
    }
}
