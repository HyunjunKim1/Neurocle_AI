using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Common;

namespace Common.Drawing
{
    /// <summary>   Definitions.  </summary>
    /// <remarks>   suoow2, 2014-11-28. </remarks>
    public static class Definitions
    {
        public static int MAX_LOCAL_ALIGN_COUNT = 4;
        public static int MAX_DOWNSET_ALIGN_COUNT = 3;
    }

    /// <summary>   Values that represent GraphicsRegionType.  </summary>
    /// <remarks>   suoow2, 2014-08-16. </remarks>
    public enum GraphicsRegionType
    {
        None = 0,
        Inspection = 1,     // 검사 영역
        UnitRegion = 2,     // 유닛 영역
        OuterRegion = 3,    // 외곽 영역
        Except = 4,         // 검사 제외 영역
        StripAlign = 5,     // 스트립 Align
        UnitAlign = 6,      // 유닛 Align
        LocalAlign = 7,     // 로컬 Align
        GuideLine = 8,      // Guide Line   
        CenterLine = 9,      // 중앙선 표시
        TapeLoaction = 10,      // 중앙선 표시
        InspectArea = 11,
        DownSetRegion = 12,     //다운셋 영역
        DownSetAlign = 13
    }

    public enum SymmetryType
    {
        Matrix = 0,          // 매트릭스 타입.
        XFlip = 1,           // X축 대칭
        YFlip = 2,           // Y축 대칭
        XYFlip = 3,          // 대각선 대칭
        Unknown = 4,          // 그 밖의 타입.
        ABCD = 5         //  그 밖의 타입.
    };

    /// <summary>   Values that represent ToolType.  </summary>
    public enum ToolType
    {
        Move = 0,
        Pointer = 1,
        Rectangle = 2,
        Outer = 3,   // Outer. (Equals Rectangle)
        Ellipse = 4,
        Line = 5,
        PolyLine = 6,
        AlignPattern = 7, // Equals Rectangle.
        GuideLine = 8,
        TapeLocation = 9,
        UnitPitch = 10,
        BlockGap = 11,
        RawMetrial = 12,
        StripAlign = 13,
        InnerBlockGapY = 14,
        InnerBlockGapX = 15,
        StripGap = 16,
        //2022.02.15 update
        inInnerBlockGapY = 17,
        inInnerBlockGapX = 18,
        Max = 19,
    };

    public enum FilpType
    {
        UPDOWN = 0,
        LEFTRIGHT = 1,
        STARTPOINT = 2
    }

    /// <summary>   Values that represent ContextMenuCommand.  </summary>
    public enum ContextMenuCommand
    {
        SetFiducialRegion = 0,
        UnloadFromSection = 1,
        RegisterSection = 2,
        ShowSelectedSectionGroup = 3,
        SetDownSetRegion = 4,
        UnSetDownSetRegion = 5,
        SetExceptInspectionRegion = 6,
        UnSetExceptInspectionRegion = 7,
        RetrySearchRegion = 8,
        Pointer = 9,
        Rectangle = 10,
        Ellipse = 11,
        PolyLine = 12,
        StripAlign = 13,
        UnitAlign = 14,
        SelectAll = 15,
        UnselectAll = 16,
        Delete = 17,
        DeleteAll = 18,
        Undo = 19,
        Redo = 20,
        MoveToFront = 21,
        MoveToBack = 22,
        SerProperties = 23,
        CopyROIToView11 = 24,
        CopyROIToView12 = 241,
        CopyROIToView21 = 25,
        CopyROIToView22 = 251,
        CopyROIToView31 = 26,
        CopyROIToView32 = 261,
        RotateROI = 27,
        RotateROI90 = 28,
        RotateROI180 = 29,
        SymmetryROI = 30,
        SymmetryROIUpDown = 31,
        SymmetryROILeftRight = 32,
        LocalAlign = 33,
        AddInspectItem = 34,
        CopySectionSetting = 35,
        PasteSectionSetting = 36,
        ShowFiducialRegion = 37,
        UnloadAndRegisterSection = 38,
        GuideLineSetting = 39,
        TapLocationSetting = 40,
        Templete = 41,
        CopyROIUpDown = 42,
        CopyROILeftRight = 43,
        CalcUnitPitch = 44, 
        CalcBlockGap = 45,
        UnloadAndRegisterSectionBlock = 46,
        UnloadFromSectionBlock = 47,
        CalcStripGap = 48,
        CalcInBlockGapX = 49,
        CalcInBlockGapY = 50,
        CalcInInBlockGapX = 51,
        CalcInInBlockGapY = 52,
        Inspection_List_Copy = 53,
        Inspection_List_Paste = 54,

    };

    /// <summary>   Information about the iteration symmetry.  </summary>
    /// <remarks>   suoow2, 2014-11-01. </remarks>
    public class IterationSymmetryInformation
    {
        public int StartX;
        public int StartY;
        public int JumpX;
        public int JumpY;

        public IterationSymmetryInformation() { }
        public IterationSymmetryInformation(int startX, int startY, int jumpX, int jumpY)
        {
            this.StartX = startX;
            this.StartY = startY;
            this.JumpX = jumpX;
            this.JumpY = jumpY;
        }

        public IterationSymmetryInformation Clone()
        {
            IterationSymmetryInformation clonedSymmetryValue = new IterationSymmetryInformation();
            clonedSymmetryValue.StartX = this.StartX;
            clonedSymmetryValue.StartY = this.StartY;
            clonedSymmetryValue.JumpX = this.JumpX;
            clonedSymmetryValue.JumpY = this.JumpY;

            return clonedSymmetryValue;
        }
    }
    
    /// <summary>   Information about the iteration.  </summary>
    /// <remarks>   suoow2, 2014-09-23. </remarks>
    public class IterationInformation
    {
        public int Block;
        public int Column;
        public int Row;
        public double Gap;
        public double XPitch;
        public double YPitch;

        public int inColumn;
        public int inRow;
        public double inXPitch;
        public double inYPitch;
        public double StripGap;

        public IterationInformation() { }
        public IterationInformation(int block, int column, int row, double gap, double xPitch, double YPitch, int incolumn, int inrow, double inXPitch, double inYPitch)
        {
            this.Block = block;
            this.Column = column;
            this.Row = row;
            this.Gap = gap;
            this.XPitch = xPitch;
            this.YPitch = YPitch;
            this.inColumn = incolumn;
            this.inRow = inrow;
            this.inXPitch = inXPitch;
            this.inYPitch = inYPitch;
        }

        public IterationInformation(int column, int row, double xPitch, double YPitch, double StripGap = 0.0)
        {
            this.Block = 1;
            this.Gap = 0;
            this.Column = column;
            this.Row = row;
            this.XPitch = xPitch;
            this.YPitch = YPitch;
            this.StripGap = StripGap;
        }

        public IterationInformation Clone()
        {
            IterationInformation clonedIterationValue = new IterationInformation();
            clonedIterationValue.Block = this.Block;
            clonedIterationValue.Column = this.Column;
            clonedIterationValue.Row = this.Row;
            clonedIterationValue.Gap = this.Gap;
            clonedIterationValue.XPitch = this.XPitch;
            clonedIterationValue.YPitch = this.YPitch;
            clonedIterationValue.inColumn = this.inColumn;
            clonedIterationValue.inRow = this.inRow;
            clonedIterationValue.inXPitch = this.inXPitch;
            clonedIterationValue.inYPitch = this.inYPitch;
            clonedIterationValue.StripGap = StripGap;

            return clonedIterationValue;
        }
    }

    /// <summary>   Graphics colors.  </summary>
    /// <remarks>   suoow2, 2014-09-07. </remarks>
    public static class GraphicsColors
    {
        public static readonly Color Green = Color.FromArgb(255, 0, 255, 0); // Inspection Type
        public static readonly Color Red = Color.FromArgb(255, 255, 0, 0); // Align Type
        public static readonly Color Blue = Color.FromArgb(255, 0, 0, 255); // Except Type
        public static readonly Color Yellow = Colors.Yellow;

        public static readonly Color Purple = Colors.Purple; // Undefined Section ROI.
        public static readonly Color YellowGreen = Color.FromArgb(255, 154, 205, 50); // The A type of Section.
        public static readonly Color DodgerBlue = Color.FromArgb(255, 30, 144, 255); // The B type of Section.
        public static readonly Color OrangeRed = Colors.OrangeRed; // Undefined Section ROI.
        public static readonly Color Gold = Colors.Gold; // Undefined Section ROI.
        public static readonly Color SpringGreen = Colors.SpringGreen; // Undefined Section ROI.

        // 새로 등록되는 Section의 색상을 결정하는데 사용된다.
        /// <summary> List of colors </summary>
        private static List<Color> ColorList = new List<Color>();

        // ColorList의 색상 중 하나를 무작위로 반환한다.
        public static Color GetNextColor(int anIndex)
        {
            if (anIndex >= ColorList.Count)
            {
                anIndex = 0;
            }
            return ColorList[anIndex];
        }

        // 2012-02-29, suoow2 added.
        static GraphicsColors()
        {
            // Color // Occupied
            ColorList.Add(Colors.OrangeRed);
            ColorList.Add(Colors.Gold);            
            ColorList.Add(Colors.Navy);
            ColorList.Add(Colors.Aqua);
            ColorList.Add(Colors.DeepPink);                  
            ColorList.Add(Colors.DarkCyan);
            ColorList.Add(Colors.SaddleBrown);
            ColorList.Add(Colors.DarkOrchid);
            ColorList.Add(Colors.DarkOrange);
            ColorList.Add(Colors.OliveDrab);            
            ColorList.Add(Colors.DodgerBlue);
            ColorList.Add(Colors.Green);
            ColorList.Add(Colors.PaleVioletRed);
            ColorList.Add(Colors.Teal);           
            ColorList.Add(Color.FromRgb(0x00, 0xF7, 0x1D));
        }
    }

    /// <summary>   Caption helper.  </summary>
    /// <remarks>   suoow2, 2014-11-24. </remarks>
    public static class CaptionHelper
    {
        public static  string StripAlignCaption = "Strip Align";
        public static  string UnitAlignCaption = "Unit Align";
        public static  string LocalAlignCaption = "Local Align";
        public static  string DownSetAlignCaption = "DownSet Align";

        public static  string GuideLineCaption = "GuideLine";
        public static  string FiducialUnitRegionCaption = "기준Unit";
        public static  string FiducialOuterRegionCaption = "기준외곽";
        public static  string ExceptionalMaskCaption = "검사제외";
        public static  string TapeLocationCaption = "Tape Loaction";
        public static  string DownSetRegionCaption = "다운셋 영역";


        // Unit 위치 Caption 생성.
        public static string GetRegionCaption(GraphicsRectangle graphic)
        {
            if (graphic != null)
            {
                return String.Format("X:{0} Y:{1}", graphic.IterationXPosition + 1, graphic.IterationYPosition + 1);
            }
            else return string.Empty;
        }

        public static void Captionlanguage(int language_index)
        {
            if (language_index == 0)
            {
                StripAlignCaption = "Strip Align";
                UnitAlignCaption = "Unit Align";
                LocalAlignCaption = "Local Align";
                DownSetAlignCaption = "DownSet Align";

                GuideLineCaption = "GuideLine";
                FiducialUnitRegionCaption = "기준Unit";
                FiducialOuterRegionCaption = "기준외곽";
                ExceptionalMaskCaption = "검사제외";
                TapeLocationCaption = "Tape Loaction";
                DownSetRegionCaption = "다운셋 영역";
            }
            else
            {
                StripAlignCaption = "Strip Align";
                UnitAlignCaption = "Unit Align";
                LocalAlignCaption = "Local Align";
                DownSetAlignCaption = "DownSet Align";

                GuideLineCaption = "GuideLine";
                FiducialUnitRegionCaption = "Standard Unit";
                FiducialOuterRegionCaption = "Standard Outer";
                ExceptionalMaskCaption = "Excluding Inspection";
                TapeLocationCaption = "Tape Loaction";
                DownSetRegionCaption = "DownSet Area";
            }
        }

    }

    // 2012-08-03 suoow2 Added.
    public class Int16Point
    {
    	public short X;
        public short Y;

        public Int16Point()
	    {
            X = 0;
            Y = 0;
	    }

        public Int16Point(short anX, short anY)
	    {
            X = anX;
            Y = anY;
	    }

        public override string ToString()
        {
 	         return string.Format("X:{0}, Y:{1}", X, Y);
        }

        // To 중앙선 파일
        public string ToFile()
        {
            return string.Format("X{0}Y{1}", X, Y);
        }
	}

    // 2012-08-03 suoow2 Added.
    public class Int16Rect
    {
        public short X;
        public short Y;
        public short Width;
        public short Height;

        public Int16Rect()
        {
            //
        }

        public Int16Rect(short anX, short anY, short anWidth, short anHeight)
        {
            X = anX;
            Y = anY;
            Width = anWidth;
            Height = anHeight;
        }

        public override string ToString()
        {
 	        return string.Format("X:{0}, Y:{1}, W:{2}, H:{3}", X, Y, Width, Height);
        }

        // To 중앙선 파일
        public string ToFile()
        {
            return string.Format("X{0}Y{1}W{2}H{3}", X, Y, Width, Height);
        }
    }


    public class InspectList : NotifyPropertyChanged
    {
        private string name;

        private int id;

        public string Name
        {
            get { return name; }
            set { name = value; Notify("Name"); }
        }

        public int ID
        {
            get { return id; }
            set { id = value; Notify("ID"); }
        }

    }
}
