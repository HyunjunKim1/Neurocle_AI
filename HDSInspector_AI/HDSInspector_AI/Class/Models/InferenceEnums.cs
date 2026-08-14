using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDSInspector_AI.Class.Models
{
    // 검사 카메라 타입
    public enum InspectionCameraType
    {
        Top = 0,
        Bottom = 1,
        Trans = 2
    }

    // AI 분류 Class
    public enum DefectClass
    {
        Unknown = 0,

        Particle,
        Contamination,
        UnderEtching,
        Flash,
        Void,
        Deformation,
        Punch
    }

    // 최종 AI 판정
    public enum AIJudgement
    {
        Unknown = 0,
        OK = 1,
        NG = 2
    }

    // 불량 판정 방식
    public enum DefectJudgeMethod
    {
        // Classification 결과로 바로 판정
        Direct = 0,

        // Seg 해서 크기 보고 판정
        Size = 1,

        // 정상 영역으로부터 벗어난 거리 측정
        OverFlowDistance = 2,

        // REF, DEF 측정
        ReferenceDiffer = 3
    }
}
