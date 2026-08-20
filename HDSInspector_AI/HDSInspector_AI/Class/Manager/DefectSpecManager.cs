using Common;
using ControlzEx.Behaviors;
using HDSInspector_AI.Class.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HDSInspector_AI.Class.GlobalFunctions.GlobalFunction;

namespace HDSInspector_AI.Class.Manager
{

    /// <summary>   불량에 대한 Spec 정의    </summary>
    /// <remarks>   hjkim, 2026-08-13.       </remarks>
    
    public class DefectSpecManager
    {
        private readonly Dictionary<string, DefectSpec> _specs;

        public DefectSpecManager()
        {
            _specs = new Dictionary<string, DefectSpec>();
        }

        private string GetKey(InspectionCameraType cameraType, DefectClass defectClass)
        {
            return $"{cameraType}_{defectClass}";
        }

        public DefectSpec GetSpec(InspectionCameraType cameraType, DefectClass defectClass)
        {
            DefectSpec spec;

            if(_specs.TryGetValue(GetKey(cameraType, defectClass), out spec)) { return spec; }

            return null;
        }

        public void LoadFromSetting()
        {
            _specs.Clear();

            AddFromSetting(InspectionCameraType.Top, DefectClass.Contaminant, "TOP_CONTAMINANT");
            AddFromSetting(InspectionCameraType.Top, DefectClass.ContaminantAg, "TOP_CONTAMINANT_AG");
            AddFromSetting(InspectionCameraType.Top, DefectClass.Particle,      "TOP_PARTICLE");
            AddFromSetting(InspectionCameraType.Top, DefectClass.UnderEtching,  "TOP_UNDERETCHING");
            AddFromSetting(InspectionCameraType.Top, DefectClass.Flash,         "TOP_FLASH");
            AddFromSetting(InspectionCameraType.Top, DefectClass.Void,          "TOP_VOID");

            AddFromSetting(InspectionCameraType.Bottom, DefectClass.Contaminant,  "BOTTOM_CONTAMINANT");
            AddFromSetting(InspectionCameraType.Bottom, DefectClass.Particle,       "BOTTOM_PARTICLE");
            AddFromSetting(InspectionCameraType.Bottom, DefectClass.UnderEtching,   "BOTTOM_UNDERETCHING");

            AddFromSetting(InspectionCameraType.Trans, DefectClass.Particle,     "TRANS_PARTICLE");
            AddFromSetting(InspectionCameraType.Trans, DefectClass.UnderEtching, "TRANS_UNDERETCHING");
            AddFromSetting(InspectionCameraType.Trans, DefectClass.Punch,        "TRANS_PUNCH");

        }

        private void AddFromSetting(InspectionCameraType cameraType, DefectClass defectClass, string settingKey)
        {
            DefectSpecSettingItem setting = GLB.Setting.DefectSpec.Get(settingKey);

            if (setting == null || !setting.Enable) return;

            DefectJudgeMethod judgeMethod;
            if (!System.Enum.TryParse(setting.JudgeMethod, true, out judgeMethod))
                judgeMethod = DefectJudgeMethod.Direct;

            AIJudgement directJudgement;
            if (!System.Enum.TryParse(setting.DirectJudgement, true, out directJudgement))
                directJudgement = AIJudgement.Unknown;

            Add(new DefectSpec
            {
                CameraType = cameraType,
                DefectClass = defectClass,
                ClassificationThreshold = (float)setting.ClassificationThreshold,
                ClassificationMargin = (float)setting.ClassificationMargin,
                JudgeMethod = judgeMethod,
                DirectJudgement = directJudgement,
                ThresholdUm = setting.ThresholdUm,
            });
        }

        private void Add(DefectSpec spec)
        {
            if(spec == null) return;

            string key = GetKey(spec.CameraType, spec.DefectClass);

            _specs[key] = spec;
        }
    }
}
