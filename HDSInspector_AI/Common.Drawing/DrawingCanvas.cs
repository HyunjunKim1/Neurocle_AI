using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// Modified & Commented by suoow2.

namespace Common.Drawing
{
    #region Delegates.
    public delegate void ContextMenuChangeEventHandler(ContextMenuCommand aSelectedContextMenuCommand);
    public delegate void SelectedGraphicChangeEventHandler(GraphicsBase aNewGraphic);
    public delegate void SectionSizeChangeEventHandler(GraphicsRectangle aNewGraphic, double afDeltaX, double afDeltaY);
    public delegate void NotifyConstraintEventHandler(string aszMessage); // Drawing Canvas의 제약조건 값을 사용자에게 인식시키는 용도로 쓰임.
    public delegate void ChangeToolEventHandler(ToolType aToolType);
    public delegate void PitchCahnged(Point p, ToolType t);
    #endregion

    /// <summary>
    /// Canvas used as host for DrawingVisual objects. Allows to draw graphics objects using mouse.
    /// </summary>
    public class DrawingCanvas : Canvas
    {
        public static event ContextMenuChangeEventHandler ContextMenuChangeEvent;
        public static event PitchCahnged PitchCahngeEvent;
        public static event SelectedGraphicChangeEventHandler SelectedGraphicChangeEvent;
        public static event SectionSizeChangeEventHandler SectionSizeChangeEvent;
        public static event NotifyConstraintEventHandler NotifyConstraintEvent;
        public static event ChangeToolEventHandler ToolTypeChangeEvent;
        public static readonly RoutedEvent IsDirtyChangedEvent;

        #region Class Members
        private VisualCollection graphicsList;
        private List<GraphicsRectangle> fiduGraphicsList = new List<GraphicsRectangle>();
        private static List<GraphicsBase> FiduClipboardList = new List<GraphicsBase>();
        private static List<GraphicsBase> ClipboardList = new List<GraphicsBase>();

        public static readonly DependencyProperty MaxGraphicsCountProperty;
        public static readonly DependencyProperty IsBasedCanvasProperty;
        public static readonly DependencyProperty ToolProperty;
        public static readonly DependencyProperty ActualScaleProperty;
        public static readonly DependencyProperty IsDirtyProperty;
        public static readonly DependencyProperty LineWidthProperty;
        public static readonly DependencyProperty RegionTypeProperty;
        public static readonly DependencyProperty ObjectColorProperty;
        public static readonly DependencyProperty TextColorProperty;
        public static readonly DependencyProperty CanUndoProperty;
        public static readonly DependencyProperty CanRedoProperty;
        public static readonly DependencyProperty CanSelectAllProperty;
        public static readonly DependencyProperty CanUnselectAllProperty;
        public static readonly DependencyProperty CanDeleteProperty;
        public static readonly DependencyProperty CanDeleteAllProperty;
        public static readonly DependencyProperty CanMoveToFrontProperty;
        public static readonly DependencyProperty CanMoveToBackProperty;
        public static readonly DependencyProperty CanSetPropertiesProperty;
        public static readonly DependencyProperty SelectedGraphicProperty;

        public bool DrawingFinished = false;
        public static bool FixedInspectROI = false; // 검사 ROI에 대한 고정성.
        public static bool FixedSectionROI = false; // 섹션 ROI에 대한 고정성.

        private Tool[] tools;
        private ToolPointer toolPointer;

        public ContextMenu contextMenu;
        private UndoManager undoManager;
        #endregion Class Members

        public int ID;
        private int m_nColorIndex;
        private int m_nBlock;
        private bool m_useEng;
        public Point SetPosition = new Point();

        #region Constructors
        /// <summary>   Initializes a new instance of the DrawingCanvas class. </summary>
        public DrawingCanvas(bool abIsBasedCanvas , bool UseEng = false)
            : base()
        {
            this.IsBasedCanvas = abIsBasedCanvas;
            this.graphicsList = new VisualCollection(this);
            this.FocusVisualStyle = null;
            this.m_useEng = UseEng;

            InitializeDrawingTools(UseEng);
            InitializeEvent();
            CreateContextMenu(UseEng);
        }

        /// <summary>   Initializes static members of the DrawingCanvas class. </summary>
        static DrawingCanvas()
        {
            PropertyMetadata metaData;

            // MaxGraphicsCount
            metaData = new PropertyMetadata(1000);
            MaxGraphicsCountProperty = DependencyProperty.Register("MaxGraphicsCount", typeof(int), typeof(DrawingCanvas), metaData);

            // Tool
            metaData = new PropertyMetadata(ToolType.Pointer);
            ToolProperty = DependencyProperty.Register("Tool", typeof(ToolType), typeof(DrawingCanvas), metaData);

            // Is Based Canvas
            metaData = new PropertyMetadata(false);
            IsBasedCanvasProperty = DependencyProperty.Register("IsBasedCanvas", typeof(bool), typeof(DrawingCanvas), metaData);

            // ActualScale
            metaData = new PropertyMetadata(1.0 /* default value */, ActualScaleChanged /* change callback */);
            ActualScaleProperty = DependencyProperty.Register("ActualScale", typeof(double), typeof(DrawingCanvas), metaData);

            // IsDirty
            metaData = new PropertyMetadata(false);
            IsDirtyProperty = DependencyProperty.Register("IsDirty", typeof(bool), typeof(DrawingCanvas), metaData);

            // LineWidth
            metaData = new PropertyMetadata(2.0 /* default value */, LineWidthChanged);
            LineWidthProperty = DependencyProperty.Register("LineWidth", typeof(double), typeof(DrawingCanvas), metaData);

            // RegionType
            metaData = new PropertyMetadata(GraphicsRegionType.Inspection  /* default value */);
            RegionTypeProperty = DependencyProperty.Register("RegionType", typeof(GraphicsRegionType), typeof(DrawingCanvas), metaData);

            // ObjectColor
            metaData = new PropertyMetadata(Colors.Black  /* default value */, ObjectColorChanged);
            ObjectColorProperty = DependencyProperty.Register("ObjectColor", typeof(Color), typeof(DrawingCanvas), metaData);

            // TextColor
            metaData = new PropertyMetadata(Color.FromArgb(255, 0, 255, 0)  /* default value */, TextColorChanged);
            TextColorProperty = DependencyProperty.Register("TextColor", typeof(Color), typeof(DrawingCanvas), metaData);

            // CanUndo
            metaData = new PropertyMetadata(false);
            CanUndoProperty = DependencyProperty.Register("CanUndo", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanRedo
            metaData = new PropertyMetadata(false);
            CanRedoProperty = DependencyProperty.Register("CanRedo", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanSelectAll
            metaData = new PropertyMetadata(false);
            CanSelectAllProperty = DependencyProperty.Register("CanSelectAll", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanUnselectAll
            metaData = new PropertyMetadata(false);
            CanUnselectAllProperty = DependencyProperty.Register("CanUnselectAll", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanDelete
            metaData = new PropertyMetadata(false);
            CanDeleteProperty = DependencyProperty.Register("CanDelete", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanDeleteAll
            metaData = new PropertyMetadata(false);
            CanDeleteAllProperty = DependencyProperty.Register("CanDeleteAll", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanMoveToFront
            metaData = new PropertyMetadata(false);
            CanMoveToFrontProperty = DependencyProperty.Register("CanMoveToFront", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanMoveToBack
            metaData = new PropertyMetadata(false);
            CanMoveToBackProperty = DependencyProperty.Register("CanMoveToBack", typeof(bool), typeof(DrawingCanvas), metaData);

            // CanSetProperties
            metaData = new PropertyMetadata(false);
            CanSetPropertiesProperty = DependencyProperty.Register("CanSetProperties", typeof(bool), typeof(DrawingCanvas), metaData);

            // SelectedShapeProperty
            metaData = new PropertyMetadata();
            SelectedGraphicProperty = DependencyProperty.Register("SelectedGraphic", typeof(GraphicsBase), typeof(DrawingCanvas), metaData);

            // IsDirtyChanged
            IsDirtyChangedEvent = EventManager.RegisterRoutedEvent("IsDirtyChangedChanged", RoutingStrategy.Bubble, typeof(DependencyPropertyChangedEventHandler), typeof(DrawingCanvas));
        }
        #endregion Constructor

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
        }

        private void InitializeDrawingTools(bool UseEng=false)
        {
            // Create array of drawing tools
            tools = new Tool[(int)ToolType.Max];

            toolPointer = new ToolPointer(UseEng);
            tools[(int)ToolType.Move] = toolPointer;
            tools[(int)ToolType.Pointer] = toolPointer;
            tools[(int)ToolType.Rectangle] = new ToolRectangle();
            tools[(int)ToolType.Outer] = new ToolRectangle();
            tools[(int)ToolType.Ellipse] = new ToolEllipse();
            tools[(int)ToolType.Line] = new ToolLine();
            tools[(int)ToolType.PolyLine] = new ToolPolyLine();
            tools[(int)ToolType.StripAlign] = new ToolRectangle();
            tools[(int)ToolType.AlignPattern] = new ToolRectangle();
            tools[(int)ToolType.GuideLine] = new ToolGuideLine();
            tools[(int)ToolType.TapeLocation] = new ToolTapeLocation();
            tools[(int)ToolType.UnitPitch] = toolPointer;
            tools[(int)ToolType.BlockGap] = toolPointer;
            tools[(int)ToolType.InnerBlockGapX] = toolPointer;
            tools[(int)ToolType.InnerBlockGapY] = toolPointer;
            tools[(int)ToolType.StripGap] = toolPointer;
            tools[(int)ToolType.inInnerBlockGapX] = toolPointer;
            tools[(int)ToolType.inInnerBlockGapY] = toolPointer;
            // Create undo manager
            undoManager = new UndoManager(this);
        }

        private void InitializeEvent()
        {
            this.undoManager.StateChanged += undoManager_StateChanged;
            this.Loaded += DrawingCanvas_Loaded;
            this.MouseDown += DrawingCanvas_MouseDown;
            this.MouseMove += DrawingCanvas_MouseMove;
            this.MouseUp += DrawingCanvas_MouseUp;
            this.LostMouseCapture += DrawingCanvas_LostMouseCapture;
        }

        #region Dependency Properties
        #region MaxGraphicsCount
        /// <summary>   Gets or sets the number of maximum graphics. </summary>
        /// <value> The number of maximum graphics. </value>
        public int MaxGraphicsCount
        {
            get
            {
                return (int)GetValue(MaxGraphicsCountProperty);
            }
            set
            {
                SetValue(MaxGraphicsCountProperty, value);
            }
        }
        #endregion

        #region Tool
        /// <summary>   Currently active drawing tool. </summary>
        /// <value> The tool. </value>
        public ToolType Tool
        {
            get
            {
                return (ToolType)GetValue(ToolProperty);
            }
            set
            {
                if ((int)value >= 0 && (int)value < (int)ToolType.Max)
                {
                    SetValue(ToolProperty, value);

                    if ((int)value > 0)
                    {
                        // Set cursor immediately - important when tool is selected from the menu
                        tools[(int)Tool].SetCursor(this);
                    }
                }
            }
        }
        #endregion Tool

        #region IsBasedCanvas
        public bool IsBasedCanvas
        {
            get
            {
                return (bool)GetValue(IsBasedCanvasProperty);
            }
            set
            {
                SetValue(IsBasedCanvasProperty, value);
            }
        }
        #endregion

        #region ActualScale
        /// <summary>   Dependency property ActualScale. </summary>
        /// <value> The actual scale. </value>
        public double ActualScale
        {
            get
            {
                return (double)GetValue(ActualScaleProperty);
            }
            set
            {
                SetValue(ActualScaleProperty, value);
            }
        }

        /// <summary>   Callback function called when ActualScale dependency property is changed. </summary>
        /// <param name="property"> The property. </param>
        /// <param name="args">     Dependency property changed event information. </param>
        static void ActualScaleChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                DrawingCanvas d = property as DrawingCanvas;
                if (d != null)
                {
                    double scale = d.ActualScale;
                    foreach (GraphicsBase b in d.GraphicsList)
                        b.ActualScale = scale;

                    foreach (GraphicsBase b in DrawingCanvas.ClipboardList)
                        b.ActualScale = scale;

                    foreach (GraphicsBase b in DrawingCanvas.FiduClipboardList)
                        b.ActualScale = scale;
                }
            }
            catch { }
        }
        #endregion ActualScale

        #region IsDirty
        /// <summary>   Returns true if document is changed. </summary>
        /// <value> true if this object is dirty, false if not. </value>
        public bool IsDirty
        {
            get
            {
                return (bool)GetValue(IsDirtyProperty);
            }
            internal set
            {
                SetValue(IsDirtyProperty, value);

                // Raise IsDirtyChanged event.
                RoutedEventArgs newargs = new RoutedEventArgs(IsDirtyChangedEvent);
                RaiseEvent(newargs);
            }
        }
        #endregion IsDirty

        #region CanUndo
        /// <summary>   Return True if Undo operation is possible. </summary>
        /// <value> true if we can undo, false if not. </value>
        public bool CanUndo
        {
            get
            {
                return (bool)GetValue(CanUndoProperty);
            }
            set
            {
                SetValue(CanUndoProperty, value);
            }
        }
        #endregion CanUndo

        #region CanRedo
        /// <summary>   Return True if Redo operation is possible. </summary>
        /// <value> true if we can redo, false if not. </value>
        public bool CanRedo
        {
            get
            {
                return (bool)GetValue(CanRedoProperty);
            }
            set
            {
                SetValue(CanRedoProperty, value);
            }
        }
        #endregion CanRedo

        #region CanSelectAll
        /// <summary>   Return true if Select All function is available. </summary>
        /// <value> true if we can select all, false if not. </value>
        public bool CanSelectAll
        {
            get
            {
                return (bool)GetValue(CanSelectAllProperty);
            }
            internal set
            {
                SetValue(CanSelectAllProperty, value);
            }
        }
        #endregion CanSelectAll

        #region CanUnselectAll
        /// <summary>   Return true if Unselect All function is available. </summary>
        /// <value> true if we can unselect all, false if not. </value>
        public bool CanUnselectAll
        {
            get
            {
                return (bool)GetValue(CanUnselectAllProperty);
            }
            internal set
            {
                SetValue(CanUnselectAllProperty, value);
            }
        }
        #endregion CanUnselectAll

        #region CanDelete
        /// <summary>   Return true if Delete function is available. </summary>
        /// <value> true if we can delete, false if not. </value>
        public bool CanDelete
        {
            get
            {
                return (bool)GetValue(CanDeleteProperty);
            }
            internal set
            {
                SetValue(CanDeleteProperty, value);
            }
        }
        #endregion CanDelete

        #region CanDeleteAll
        /// <summary>   Return true if Delete All function is available. </summary>
        /// <value> true if we can delete all, false if not. </value>
        public bool CanDeleteAll
        {
            get
            {
                return (bool)GetValue(CanDeleteAllProperty);
            }
            internal set
            {
                SetValue(CanDeleteAllProperty, value);
            }
        }
        #endregion CanDeleteAll

        #region CanMoveToFront
        /// <summary>   Return true if Move to Front function is available. </summary>
        /// <value> true if we can move to front, false if not. </value>
        public bool CanMoveToFront
        {
            get
            {
                return (bool)GetValue(CanMoveToFrontProperty);
            }
            internal set
            {
                SetValue(CanMoveToFrontProperty, value);
            }
        }
        #endregion CanMoveToFront

        #region CanMoveToBack
        /// <summary>   Return true if Move to Back function is available. </summary>
        /// <value> true if we can move to back, false if not. </value>
        public bool CanMoveToBack
        {
            get
            {
                return (bool)GetValue(CanMoveToBackProperty);
            }
            internal set
            {
                SetValue(CanMoveToBackProperty, value);
            }
        }
        #endregion CanMoveToBack

        #region CanSetProperties
        /// <summary>
        /// Return true if currently active properties (line width, color etc.)
        /// can be applied to selected objects.
        /// </summary>
        /// <value> true if we can set properties, false if not. </value>
        public bool CanSetProperties
        {
            get
            {
                return (bool)GetValue(CanSetPropertiesProperty);
            }
            internal set
            {
                SetValue(CanSetPropertiesProperty, value);
            }
        }
        #endregion CanSetProperties

        #region SelectedGraphicProperty
        public GraphicsBase SelectedGraphic
        {
            get
            {
                return (GraphicsBase)GetValue(SelectedGraphicProperty);
            }
            set
            {
                if (SelectedGraphic != null)
                {
                    SelectedGraphic.ObjectColor = SelectedGraphic.OriginObjectColor;
                    if (SelectedGraphic.LocalAligns != null)
                    {
                        for (int nIndex = 0; nIndex < SelectedGraphic.LocalAligns.Length; nIndex++)
                        {
                            if (SelectedGraphic.LocalAligns[nIndex] != null)
                                SelectedGraphic.LocalAligns[nIndex].ObjectColor = SelectedGraphic.LocalAligns[nIndex].OriginObjectColor;
                        }
                    }
                }

                SelectedGraphicChangeEventHandler eventRunner = SelectedGraphicChangeEvent;
                if (eventRunner != null)
                {
                    eventRunner(value);
                }
                SetValue(SelectedGraphicProperty, value);

                // Set to Orange. (Selected ROI)
                if (SelectedGraphic != null)
                {
                    SelectedGraphic.ObjectColor = Colors.Orange;
                    if (!IsBasedCanvas && SelectedGraphic.LocalAligns != null)
                    {
                        for (int nIndex = 0; nIndex < SelectedGraphic.LocalAligns.Length; nIndex++)
                        {
                            if (SelectedGraphic.LocalAligns[nIndex] != null)
                                SelectedGraphic.LocalAligns[nIndex].ObjectColor = Colors.Orange;
                        }
                    }
                }
            }
        }
        #endregion

        #region LineWidth
        /// <summary>
        /// Line width of new graphics object. Setting this property is also applied to current selection.
        /// </summary>
        /// <value> The width of the line. </value>
        [DefaultValue(2.0)]
        public double LineWidth
        {
            get
            {
                return (double)GetValue(LineWidthProperty);
            }
            set
            {
                SetValue(LineWidthProperty, value);

            }
        }

        /// <summary>   Callback function called when LineWidth dependency property is changed. </summary>
        /// <param name="property"> The property. </param>
        /// <param name="args">     Dependency property changed event information. </param>
        static void LineWidthChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                DrawingCanvas d = property as DrawingCanvas;
                if (d != null)
                {
                    HelperFunctions.ApplyLineWidth(d, d.LineWidth, true);
                }
            }
            catch
            {

            }
        }
        #endregion LineWidth

        #region RegionType

        /// <summary>   Gets or sets the type of the region. </summary>
        /// <value> The type of the region. </value>
        public GraphicsRegionType RegionType
        {
            get
            {
                return (GraphicsRegionType)GetValue(RegionTypeProperty);
            }
            set
            {
                SetValue(RegionTypeProperty, value);
            }
        }

        #endregion

        #region ObjectColor

        /// <summary>
        /// Color of new graphics object. Setting this property is also applied to current selection.
        /// </summary>
        /// <value> The color of the object. </value>
        public Color ObjectColor
        {
            get
            {
                return (Color)GetValue(ObjectColorProperty);
            }
            set
            {
                SetValue(ObjectColorProperty, value);

            }
        }

        /// <summary>   Callback function called when ObjectColor dependency property is changed. </summary>
        /// <param name="property"> The property. </param>
        /// <param name="args">     Dependency property changed event information. </param>
        static void ObjectColorChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                DrawingCanvas d = property as DrawingCanvas;
                if (d != null)
                {
                    HelperFunctions.ApplyColor(d, d.ObjectColor, true);
                }
            }
            catch
            {

            }
        }
        #endregion ObjectColor

        #region TextColor
        /// <summary>   Gets or sets the color of the text. </summary>
        /// <value> The color of the text. </value>
        public Color TextColor
        {
            get
            {
                return (Color)GetValue(TextColorProperty);
            }
            set
            {
                SetValue(TextColorProperty, value);

            }
        }

        /// <summary>   Text color changed. </summary>
        /// <remarks>   suoow2, 2014-08-16. </remarks>
        /// <param name="property"> The property. </param>
        /// <param name="args">     Dependency property changed event information. </param>
        static void TextColorChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                DrawingCanvas d = property as DrawingCanvas;

                if (d != null)
                {
                    HelperFunctions.ApplyColor(d, d.TextColor, true);
                }
            }
            catch
            {

            }
        }
        #endregion TextColor
        #endregion Dependency Properties

        #region Routed Events
        /// <summary>
        /// IsDirtyChanged event.
        /// 
        /// If client binds to IsDirty property, this event is not required. But if client knows when
        /// IsDirty changed without binding, IsDirtyChanged is needed.
        /// </summary>
        /// <value> The is dirty changed. </value>
        public event RoutedEventHandler IsDirtyChanged
        {
            add
            {
                AddHandler(IsDirtyChangedEvent, value);
            }
            remove
            {
                RemoveHandler(IsDirtyChangedEvent, value);
            }
        }
        #endregion Routed Events

        #region Public Functions
        /// <summary>
        /// Return list of graphic objects. Used if client program needs to make its own usage of
        /// graphics objects, like save them in some persistent storage.
        /// </summary>
        /// <returns>   The list of graphic objects. </returns>
        public PropertiesGraphicsBase[] GetListOfGraphicObjects()
        {
            PropertiesGraphicsBase[] result = new PropertiesGraphicsBase[graphicsList.Count];

            int i = 0;
            foreach (GraphicsBase g in graphicsList)
            {
                result[i++] = g.CreateSerializedObject();
            }

            return result;
        }

        /// <summary>
        /// Draw all graphics objects to DrawingContext supplied by client. Can be used for printing or
        /// saving image together with graphics as single bitmap.
        /// 
        /// Selection tracker is not drawn.
        /// </summary>
        /// <param name="drawingContext">   Context for the drawing. </param>
        public void Draw(DrawingContext drawingContext)
        {
            Draw(drawingContext, false);
        }

        /// <summary>
        /// Draw all graphics objects to DrawingContext supplied by client. Can be used for printing or
        /// saving image together with graphics as single bitmap.
        /// 
        /// withSelection = true - draw selected objects with tracker.
        /// </summary>
        /// <param name="drawingContext">   Context for the drawing. </param>
        /// <param name="withSelection">    true to with selection. </param>
        public void Draw(DrawingContext drawingContext, bool withSelection)
        {
            bool oldSelection = false;

            foreach (GraphicsBase b in graphicsList)
            {
                if (!withSelection)
                {
                    // Keep selection state and unselect
                    oldSelection = b.IsSelected;
                    b.IsSelected = false;
                }

                b.Draw(drawingContext);

                if (!withSelection)
                {
                    // Restore selection state
                    b.IsSelected = oldSelection;
                }
            }
        }

        /// <summary>   Clear graphics list. </summary>        
        public void Clear()
        {
            m_nColorIndex = 0;
            fiduGraphicsList.Clear();
            graphicsList.Clear();
            FiduClipboardList.Clear();
            ClipboardList.Clear();
            ClearHistory();
            UpdateState();
        }

        /// <summary>   Save graphics to XML file. Throws: DrawingCanvasException. </summary>
        /// <remarks>   suoow2, 2014-10-15. </remarks>
        public void Save(string fileName, string strModelName, string strSectionName, int pixelWidth, int pixelHeight)
        {
            try
            {
                foreach (GraphicsBase g in graphicsList)
                {
                    g.ObjectColor = g.OriginObjectColor;
                    g.IsSelected = false;
                }

                SerializationHelper serializedGraphicsList = new SerializationHelper(graphicsList);
                serializedGraphicsList.ModelName = strModelName;
                serializedGraphicsList.SectionName = strSectionName;
                serializedGraphicsList.RegionWidth = pixelWidth;
                serializedGraphicsList.RegionHeight = pixelHeight;

                XmlSerializer xml = new XmlSerializer(typeof(SerializationHelper));

                using (Stream stream = new FileStream(fileName,
                    FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    xml.Serialize(stream, serializedGraphicsList);
                    //ClearHistory();
                    UpdateState();
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in Save(DrawingCanvas.cs)");
            }
        }

        /// <summary>   XML to GraphicsList. </summary>
        /// <remarks>   suoow2, 2014-11-29. </remarks>
        public int Load(string fileName, string strModelName, string strSectionName, int pixelWidth, int pixelHeight)
        {
            try
            {
                SerializationHelper serializedGraphicsList;
                XmlSerializer xml = new XmlSerializer(typeof(SerializationHelper));

                using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    serializedGraphicsList = (SerializationHelper)xml.Deserialize(stream);
                    //if (serializedGraphicsList.ModelName != strModelName)
                    //    return -1; // 현재 티칭중인 모델과 같아야 한다.
                    //else if (serializedGraphicsList.RegionWidth > pixelWidth || serializedGraphicsList.RegionHeight > pixelHeight)
                    //    return -2; // 현재 티칭중인 Section의 Width가 같거나 커야한다.
                }

                if (serializedGraphicsList.Graphics != null)
                {
                    fiduGraphicsList.Clear(); // 기준 영역 초기화.
                    graphicsList.Clear(); // ROI 초기화.
                    FiduClipboardList.Clear(); // 클립보드 초기화.
                    ClipboardList.Clear(); // 클립보드 초기화.

                    GraphicsBase graphic;
                    GraphicsRectangle graphicsRectangle = null;
                    foreach (PropertiesGraphicsBase g in serializedGraphicsList.Graphics)
                    {
                        // 2014-11-29, suoow2 Added. XML 복원시 Local Align과 Mother ROI로의 추적성을 고려해주어야 함.
                        graphic = g.CreateGraphics(); // XML to GraphicsBase
                        graphic.RefreshDrawing();
                        graphicsList.Add(graphic);
                        if (graphic is GraphicsRectangle)
                        {
                            if (((GraphicsRectangle)graphic).IsFiducialRegion)
                            {
                                fiduGraphicsList.Add(graphic as GraphicsRectangle);
                            }
                        }

                        // Local Align grapihcsList에 추가.
                        if (g.LocalAligns != null)
                        {
                            graphic.LocalAligns = new GraphicsRectangle[g.LocalAligns.Length];
                            for (int i = 0; i < g.LocalAligns.Length; i++)
                            {
                                if (g.LocalAligns[i].X == -1 || g.LocalAligns[i].Y == -1)
                                    continue;

                                graphicsRectangle = new GraphicsRectangle(
                                    g.LocalAligns[i].X - 15, g.LocalAligns[i].Y - 15,
                                    g.LocalAligns[i].X + 15, g.LocalAligns[i].Y + 15,
                                    this.LineWidth, GraphicsRegionType.LocalAlign, GraphicsColors.Red, this.ActualScale);
                                graphicsRectangle.Caption = CaptionHelper.LocalAlignCaption;

                                // 상호 참조.
                                graphicsRectangle.MotherROI = graphic;
                                graphic.LocalAligns[i] = graphicsRectangle;

                                graphicsList.Add(graphicsRectangle);
                            }
                            g.LocalAligns = null;
                        }
                    }

                    // Update clip for all loaded objects.
                    RefreshClip();

                    // Mother ROI로의 추적성을 확보한다.
                    if (IsBasedCanvas)
                    {
                        foreach (GraphicsRectangle g in GraphicsRectangleList)
                        {
                            if (g.RegionType == GraphicsRegionType.UnitRegion && !g.IsFiducialRegion)
                            {
                                foreach (GraphicsRectangle fiduGraphic in fiduGraphicsList)
                                {
                                    if (g.MotherROIID == fiduGraphic.ID)
                                    {
                                        g.MotherROI = fiduGraphic;
                                    }
                                }
                            }
                        }
                    }
                    ClearHistory();
                    UpdateState();
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in Load(DrawingCanvas.cs)");
            }
            return 0;
        }

        /// <summary>   Select all. </summary>
        public void SelectAll()
        {
            HelperFunctions.SelectAll(this);
            UpdateState();
        }

        /// <summary>   Unselect all. </summary>
        public void UnselectAll()
        {
            HelperFunctions.UnselectAll(this);
            UpdateState();
        }

        /// <summary>   Delete selection. </summary>
        public void Delete()
        {
            // 선택된 ROI를 삭제한다.
            // 2014-12-02, suoow2 Added. 실수로 인한 ROI 삭제를 방지하기 위함.            

            if (SelectionCount > 0)
            {
                if (IsBasedCanvas) // 전체 영상에서의 삭제 요청인 경우.
                {
                    string szQuestionMessage = "Are you sure you want to delete the selected section?\n\n";
                    int nDeleteStripSectionCount = 0;
                    int nDeleteOuterSectionCount = 0;
                    int nDeleteUnitSectionCount = 0;

                    bool bContainsUnitRegion = false;
                    bool bContainsOuterRegion = false;
                    foreach (GraphicsRectangle graphic in SelectionRectangle)
                    {
                        if (graphic.RegionType == GraphicsRegionType.StripAlign)
                            nDeleteStripSectionCount++;
                        else if (graphic.RegionType == GraphicsRegionType.OuterRegion)
                            bContainsOuterRegion = true;
                        else if (graphic.RegionType == GraphicsRegionType.UnitRegion)
                            bContainsUnitRegion = true;
                    }

                    if (bContainsUnitRegion)
                    {
                        foreach (GraphicsRectangle graphic in FiduGraphicsList)
                        {
                            if (graphic.RegionType == GraphicsRegionType.UnitRegion)
                            {
                                nDeleteUnitSectionCount++;
                            }
                        }
                    }

                    if (bContainsOuterRegion)
                    {
                        foreach (GraphicsRectangle graphic in FiduGraphicsList)
                        {
                            if (graphic.RegionType == GraphicsRegionType.OuterRegion)
                            {
                                nDeleteOuterSectionCount++;
                            }
                        }
                    }

                    if (nDeleteStripSectionCount > 0)
                        szQuestionMessage += string.Format("Strip Section : {0} units\n", nDeleteStripSectionCount);
                    if (nDeleteUnitSectionCount > 0)
                        szQuestionMessage += string.Format("Unit Section : {0} units\n", nDeleteUnitSectionCount);
                    if (nDeleteOuterSectionCount > 0)
                        szQuestionMessage += string.Format("Outer Section : {0} units", nDeleteOuterSectionCount);

                    if ((nDeleteStripSectionCount + nDeleteOuterSectionCount + nDeleteUnitSectionCount) > 0 && MessageBoxResult.Yes != MessageBox.Show(szQuestionMessage, "Information", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }
                else // Section 영상에서의 삭제 요청인 경우.
                {
                    if (MessageBoxResult.Yes != MessageBox.Show(String.Format(ResourceStringHelper.GetInformationMessage("M074",m_useEng), SelectionCount), "Information", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }
            }

            if (IsBasedCanvas) //전체 영상
            {
                bool bNeedClearUnitRegion = false;
                bool bNeedClearOuterRegion = false;

                foreach (GraphicsRectangle selectedRectangle in SelectionRectangle)
                {
                    if (selectedRectangle.RegionType == GraphicsRegionType.UnitRegion)
                    {
                        bNeedClearUnitRegion = true;
                    }

                    if (selectedRectangle.RegionType == GraphicsRegionType.OuterRegion)
                    {
                        bNeedClearOuterRegion = true;
                    }


                    if (selectedRectangle.RegionType == GraphicsRegionType.StripAlign)
                    {
                        FiduGraphicsList.Remove(selectedRectangle);
                    }

                }

                // Unit ROI 삭제
                if (bNeedClearUnitRegion)
                {
                    for (int nIndex = 0; nIndex < fiduGraphicsList.Count; nIndex++)
                    {
                        if (fiduGraphicsList[nIndex].RegionType == GraphicsRegionType.UnitRegion)
                        {
                            fiduGraphicsList.RemoveAt(nIndex);
                            nIndex--;
                        }
                    }

                    // 이부분 전부 selected를 true로 만들기 때문에 모든 ROI를 선택되게함.
                    // 그래서 이거 전체가 삭제됨 걍
                    foreach (GraphicsBase g in graphicsList)
                    {
                        if (g.RegionType == GraphicsRegionType.UnitRegion)
                        {
                            g.selected = true; // g.IsSelected로 처리하는 경우 RefreshDrawing이 호출되어 성능 저하 발생.
                        }
                    }
                }

                // Outer ROI 삭제
                if (bNeedClearOuterRegion)
                {
                    for (int nIndex = 0; nIndex < fiduGraphicsList.Count; nIndex++)
                    {
                        if (fiduGraphicsList[nIndex].RegionType == GraphicsRegionType.OuterRegion)
                        {
                            fiduGraphicsList.RemoveAt(nIndex);
                            nIndex--;
                        }
                    }

                    foreach (GraphicsBase g in graphicsList)
                    {
                        if (g.RegionType == GraphicsRegionType.OuterRegion)
                        {
                            g.selected = true; // g.IsSelected로 처리하는 경우 RefreshDrawing이 호출되어 성능 저하 발생.
                        }
                    }
                }


            }
            else
            {
                // Local Align은 개별 영상에서만 존재한다.
                #region Local Align 삭제
                foreach (GraphicsBase targetROI in Selection)
                {
                    if (targetROI.LocalAligns != null) //  Local Align을 갖는 ROI에 해당된다.
                    {
                        if (targetROI.LocalAligns.Length != 0)
                        {
                            foreach (GraphicsRectangle g in GraphicsRectangleList)
                            {
                                if (g.MotherROI == targetROI)
                                {
                                    g.IsSelected = true;
                                }
                            }
                        }
                    }
                    else if (targetROI.RegionType == GraphicsRegionType.LocalAlign) // Local Align에 해당된다.
                    {
                        foreach (GraphicsBase parentROI in graphicsList)
                        {
                            if (parentROI == ((GraphicsRectangle)targetROI).MotherROI)
                            {
                                for (int i = 0; i < parentROI.LocalAligns.Length; i++)
                                {
                                    if (parentROI.LocalAligns[i] != null &&
                                        parentROI.LocalAligns[i] == targetROI)
                                    {
                                        parentROI.LocalAligns[i] = null;
                                        break; // for
                                    }
                                }
                                break; // foreach
                            }
                        }
                    }
                    else if(targetROI.RegionType == GraphicsRegionType.DownSetAlign)
                    {
                        foreach (GraphicsBase parentROI in graphicsList)
                        {
                            if (parentROI == ((GraphicsRectangle)targetROI).MotherROI)
                            {
                                for (int i = 0; i < parentROI.DownSetAligns.Length; i++)
                                {
                                    if (parentROI.DownSetAligns[i] != null &&
                                        parentROI.DownSetAligns[i] == targetROI)
                                    {
                                        parentROI.DownSetAligns[i] = null;
                                        break; // for
                                    }
                                }
                                break; // foreach
                            }
                        }
                    }
                }
                #endregion
            }

            // 각 단계를 통해 선택된 ROI를 모두 삭제한다.
            if (!IsBasedCanvas)
                undoManager.AddCommandToHistory(new CommandChangeState(this));
            HelperFunctions.DeleteSelection(this);
            UpdateState();
        }

        /// <summary>   Delete selection. </summary>
        /// modify - hjkim : 선택된 ROI만 삭제되도록 변경 //
        public void SelectDelete()
        {
            //modify - hjkim : 인라인 검사용 Section 선택 삭제 기능 추가
            // 선택된 ROI를 삭제한다.

            if (SelectionCount <= 0) return;

            if (IsBasedCanvas)
            {
                string msg = $"Are you sure you want to delete {SelectionCount} selected regions ?";

                if (MessageBoxResult.Yes != MessageBox.Show(msg, "Delete", MessageBoxButton.YesNo)) return;

                // 선택된 StripAlign Unit, Outer만 삭제시키자.
                // List로 복사해서 순회
                // 기준유닛이 선택되면 고놈 삭제해버리자.
                foreach (GraphicsRectangle rect in SelectionRectangle.ToList())
                {
                    if (FiduGraphicsList.Contains(rect))
                        FiduGraphicsList.Remove(rect);
                }

                for (int i = fiduGraphicsList.Count - 1; i >= 0; i--)
                {
                    var r = fiduGraphicsList[i];
                    if (SelectionRectangle.Contains(r) &&
                        (r.RegionType == GraphicsRegionType.UnitRegion || r.RegionType == GraphicsRegionType.OuterRegion))
                        fiduGraphicsList.RemoveAt(i);
                }
            }
            else
            {
                // Section 이미지 내부 삭제 처리
                foreach (GraphicsBase roi in Selection)
                {
                    if (roi.RegionType == GraphicsRegionType.LocalAlign || roi.RegionType == GraphicsRegionType.DownSetAlign)
                    {
                        var parent = ((GraphicsRectangle)roi).MotherROI;
                        if (parent != null)
                        {
                            if (roi.RegionType == GraphicsRegionType.LocalAlign)
                            {
                                for (int i = 0; i < parent.LocalAligns.Length; i++)
                                {
                                    if (parent.LocalAligns[i] == roi)
                                        parent.LocalAligns[i] = null;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < parent.DownSetAligns.Length; i++)
                                {
                                    if (parent.DownSetAligns[i] == roi)
                                        parent.DownSetAligns[i] = null;
                                }
                            }
                        }
                    }
                }
            }

            // 실제 삭제 수행
            undoManager.AddCommandToHistory(new CommandChangeState(this));
            HelperFunctions.DeleteSelection(this);  // Selection 에 있는 것만 삭제
            UpdateState();
        }
        /// <summary>   Delete all. </summary>
        public void DeleteAll()
        {
            if (graphicsList.Count > 0)
            {
                if (IsBasedCanvas)
                {
                    if (MessageBoxResult.Yes != MessageBox.Show(ResourceStringHelper.GetInformationMessage("M075",m_useEng), "Information", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }
                else
                {
                    if (MessageBoxResult.Yes != MessageBox.Show(ResourceStringHelper.GetInformationMessage("M076",m_useEng), "Information", MessageBoxButton.YesNo))
                    {
                        return;
                    }
                }

                if (!IsBasedCanvas)
                    undoManager.AddCommandToHistory(new CommandChangeState(this));
                else
                {
                    fiduGraphicsList.Clear();
                }
                HelperFunctions.DeleteAll(this);
                UpdateState();
            }
        }

        /// <summary>   Move selection to the front of Z-order. </summary>
        public void MoveToFront()
        {
            HelperFunctions.MoveSelectionToFront(this);
            UpdateState();
        }

        public void MoveToFront(GraphicsRegionType regionType)
        {
            HelperFunctions.MoveToFront(this, regionType);
            UpdateState();
        }

        /// <summary>   Move selection to the end of Z-order. </summary>
        public void MoveToBack()
        {
            HelperFunctions.MoveSelectionToBack(this);
            UpdateState();
        }

        /// <summary>   Apply currently active properties to selected objects. </summary>
        public void SetProperties()
        {
            HelperFunctions.ApplyProperties(this);
            UpdateState();
        }

        /// <summary>   Set clip for all graphics objects. </summary>
        public void RefreshClip()
        {
            foreach (GraphicsBase b in graphicsList)
            {
                b.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

                // Good chance to refresh actual scale
                b.ActualScale = this.ActualScale;
            }

            foreach (GraphicsBase b in ClipboardList)
            {
                b.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

                // Good chance to refresh actual scale
                b.ActualScale = this.ActualScale;
            }

            foreach (GraphicsBase b in FiduClipboardList)
            {
                b.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth, this.ActualHeight));

                // Good change to refresh actual scale
                b.ActualScale = this.ActualScale;
            }
        }

        /// <summary>   Remove clip for all graphics objects. </summary>
        public void RemoveClip()
        {
            foreach (GraphicsBase b in graphicsList)
                b.Clip = null;

            foreach (GraphicsBase b in ClipboardList)
                b.Clip = null;

            foreach (GraphicsBase b in FiduClipboardList)
                b.Clip = null;
        }

        /// <summary>   Undo. </summary>
        public void Undo()
        {
            if (!CanUndo)
            {
                return;
            }

            undoManager.Undo();

            #region Local Align 및 반복설정된 자식 ROI의 경우 다시 부모 ROI와 연결해주어야 한다. ## suoow2. 2014-11-28 added. ##
            List<GraphicsRectangle> orphanRoiList = new List<GraphicsRectangle>();
            List<GraphicsRectangle> motherRoiList = new List<GraphicsRectangle>();
            List<GraphicsRectangle> localAlignList = new List<GraphicsRectangle>();
            foreach (GraphicsRectangle g in GraphicsRectangleList)
            {
                if (g.RegionType == GraphicsRegionType.UnitRegion || g.RegionType == GraphicsRegionType.OuterRegion)
                {
                    if (!g.IsFiducialRegion)
                    {
                        orphanRoiList.Add(g);
                    }
                    else
                    {
                        motherRoiList.Add(g);
                    }
                }
                else if (g.RegionType == GraphicsRegionType.LocalAlign)
                {
                    localAlignList.Add(g);
                    //g.LocalAligns = null;
                }
            }

            foreach (GraphicsRectangle orphanROI in orphanRoiList)
            {
                foreach (GraphicsRectangle motherROI in motherRoiList)
                {
                    // 비교 조건은 반복 설정 값이 같은 것으로 충분하다.
                    if (orphanROI.SymmetryValue == motherROI.SymmetryValue &&
                        orphanROI.IterationValue == motherROI.IterationValue)
                    {
                        orphanROI.MotherROI = motherROI;
                        orphanROI.IsFiducialRegion = false;
                        orphanROI.Caption = CaptionHelper.GetRegionCaption(orphanROI);
                    }
                }
            }

            foreach (GraphicsBase graphic in graphicsList) 
            {
                graphic.LocalAligns = null;
                foreach (GraphicsRectangle localAlignROI in localAlignList)
                {
                    if (localAlignROI.MotherROI.ID == graphic.ID)
                    {
                        if (graphic.LocalAligns == null)
                        {
                            graphic.LocalAligns = new GraphicsRectangle[4];
                        }

                        for (int i = 0; i < graphic.LocalAligns.Length; i++)
                        {
                            if (graphic.LocalAligns[i] == null)
                            {
                                graphic.LocalAligns[i] = localAlignROI;
                                localAlignROI.MotherROI = graphic;

                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            UpdateState();
        }

        /// <summary>   Redo. </summary>
        public void Redo()
        {
            if (!CanRedo)
            {
                return;
            }

            undoManager.Redo();
            UpdateState();
        }
        #endregion Public Functions

        #region Internal Properties
        // GraphicsList 인덱서
        public GraphicsBase this[int index]
        {
            get
            {
                if (index >= 0 && index < Count)
                {
                    return (GraphicsBase)graphicsList[index];
                }

                return null;
            }
        }

        public int Count
        {
            get
            {
                return graphicsList.Count;
            }
        }

        public int SelectionCount
        {
            get
            {
                int n = 0;

                foreach (GraphicsBase g in this.graphicsList)
                {
                    if (g.IsSelected)
                    {
                        n++;
                    }
                }

                return n;
            }
        }
        public List<GraphicsBase> BackupGraphicsList = new List<GraphicsBase>();
        public VisualCollection GraphicsList
        {
            get
            {
                return graphicsList;
            }
        }

        public List<GraphicsRectangle> FiduGraphicsList
        {
            get
            {
                return fiduGraphicsList;
            }
        }

        // GraphicsList 중에서 Rectangle Graphic만 반환한다.
        public IEnumerable<GraphicsRectangle> GraphicsRectangleList
        {
            get
            {
                foreach (GraphicsBase g in graphicsList)
                {
                    if (g is GraphicsRectangle)
                    {
                        yield return g as GraphicsRectangle;
                    }
                }
            }
        }

        public IEnumerable<GraphicsBase> Selection
        {
            get
            {
                foreach (GraphicsBase o in graphicsList)
                {
                    if (o.IsSelected)
                    {
                        yield return o;
                    }
                }
            }
        }

        public IEnumerable<GraphicsBase> SelectionRectangle
        {
            get
            {
                foreach (GraphicsBase o in graphicsList)
                {
                    if (o is GraphicsRectangle && o.IsSelected)
                    {
                        yield return o;
                    }
                }
            }
        }
        #endregion Internal Properties

        #region Visual Children Overrides
        protected override int VisualChildrenCount
        {
            get
            {
                return graphicsList.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return graphicsList[index];
        }
        #endregion Visual Children Overrides

        #region Mouse Event Handlers
        /// <summary>
        /// Mouse down. Left button down event is passed to active tool. Right button down event is
        /// handled in this class.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Mouse button event information. </param>
        public void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            if ((Tool == ToolType.UnitPitch || Tool == ToolType.BlockGap || Tool == ToolType.InnerBlockGapY || Tool == ToolType.InnerBlockGapX || Tool == ToolType.inInnerBlockGapY || Tool == ToolType.inInnerBlockGapX
                || Tool == ToolType.StripGap) && IsBasedCanvas)
            {
                DrawingFinished = false;
               // SetPosition = Mouse.GetPosition(this);
                tools[(int)Tool].OnMouseDown(this, e);

                UpdateState();
                //UpdateSelectedGraphic();
                return;
            }

            // Drawing Canvas에 그릴 수 있는 도형의 개수를 제한한다.
            if (FiduGraphicsList.Count >= MaxGraphicsCount)
            {
                Tool = ToolType.Pointer;
            }

            this.Focus();

            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // 2014-04-07. suoow2.
                    if (Tool == ToolType.Pointer)
                    {
                        MoveToBack();
                    }
                }

                DrawingFinished = false;
                tools[(int)Tool].OnMouseDown(this, e);

                UpdateState();
                UpdateSelectedGraphic();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                Point point = Mouse.GetPosition(this);

                List<GraphicsBase> hittedList = new List<GraphicsBase>();
                foreach (GraphicsBase graphic in graphicsList)
                {
                    if (graphic.MakeHitTest(point) >= 0)
                    {
                        hittedList.Add(graphic);
                    }
                }

                if (hittedList.Count > 0)
                {
                    SelectedGraphic = hittedList[hittedList.Count - 1]; // 가장 앞에 놓인 GraphicsBase를 SelectedGraphic으로 선정한다.
                }
                else
                {
                    SelectedGraphic = null;
                }

                if (Tool == ToolType.PolyLine)
                {
                    ToolPolyLine toolPolyLine = tools[(int)ToolType.PolyLine] as ToolPolyLine;

                    if (toolPolyLine != null)
                    {
                        if (toolPolyLine.CanShowContextMenu)
                        {
                            tools[(int)Tool].OnMouseDown(this, e);
                            ShowContextMenu(e);
                        }
                    }
                }
                else
                {
                    if (!IsBasedCanvas)
                    {
                        tools[(int)Tool].OnMouseUp(this, e);
                        foreach (GraphicsBase graphic in graphicsList)
                        {
                            if (graphic.MakeHitTest(point) >= 0)
                            {
                                tools[(int)Tool].OnMouseDown(this, e);
                                break;
                            }
                        }
                    }
                    ShowContextMenu(e);
                }
            }
        }

        /// <summary>
        /// Mouse move. Moving without button pressed or with left button pressed is passed to active tool.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Mouse event information. </param>
        public void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            if (e.MiddleButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                tools[(int)Tool].OnMouseMove(this, e);

                UpdateState();
            }
            else
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        /// <summary>   Mouse up event. Left button up event is passed to active tool. </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Mouse button event information. </param>
        public void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (tools[(int)Tool] == null)
            {
                return;
            }

            if (IsBasedCanvas)
            {
                if (Tool == ToolType.UnitPitch || Tool == ToolType.BlockGap || Tool == ToolType.InnerBlockGapX || Tool == ToolType.InnerBlockGapY ||
                     Tool == ToolType.inInnerBlockGapX || Tool == ToolType.inInnerBlockGapY || Tool == ToolType.StripGap)
                {
                   // Point p = Mouse.GetPosition(this);
                   // SetPosition.X = (p.X - SetPosition.X) * this.ActualScale;
                   // SetPosition.Y = (p.Y - SetPosition.Y) * this.ActualScale;
                    DrawingFinished = true;
                    tools[(int)Tool].OnMouseUp(this, e);
                    //PitchCahnged pc = PitchCahngeEvent;
                    //pc(SetPosition, Tool);
                    UpdateState();
                    UnselectAll();
                    Tool = ToolType.Pointer;
                    return;
                }

                // Drawing Canvas에 그릴 수 있는 도형의 개수를 제한한다.
                if (fiduGraphicsList.Count >= MaxGraphicsCount)
                {
                    Tool = ToolType.Pointer;
                }
            }
            else
            {
                if (graphicsList.Count >= MaxGraphicsCount)
                {
                    Tool = ToolType.Pointer;
                }
            }

            if (SelectedGraphic is GraphicsSelectionRectangle)
            {
                if (((GraphicsSelectionRectangle)SelectedGraphic).WidthProperty == 0 &&
                   ((GraphicsSelectionRectangle)SelectedGraphic).HeightProperty == 0)
                {
                    SelectedGraphic = null;
                }
            }
            if (Tool != ToolType.PolyLine && e.ChangedButton == MouseButton.Left)
            {
                DrawingFinished = true;
                tools[(int)Tool].OnMouseUp(this, e);

                UpdateState();
            }
            else if (Tool == ToolType.PolyLine && e.ChangedButton == MouseButton.Right)
            {
                try
                {
                    DrawingFinished = true;
                    tools[(int)ToolType.PolyLine].OnMouseUp(this, e);

                    GraphicsPolyLine polyLine = graphicsList[graphicsList.Count - 1] as GraphicsPolyLine;
                    #region Unit Align ROI Top Most로 설정시 아래 코드 원복필요. suoow2.
                    //int nIndex = graphicsList.Count - 1;
                    //do
                    //{
                    //    polyLine = graphicsList[nIndex] as GraphicsPolyLine;
                    //    if (nIndex == 0)
                    //    {
                    //        graphicsList.RemoveAt(nIndex);
                    //        break;
                    //    }
                    //    if (polyLine == null) nIndex--;
                    //} while (polyLine == null);
                    #endregion

                    if (polyLine != null)
                    {
                        if (polyLine.Points.Length > 3)
                        {
                            List<Point> points = new List<Point>();

                            points.Add(polyLine.Points[0]);
                            int nPoints = polyLine.Points.Length;
                            for (int i = 1; i < nPoints - 1; i++)
                            {
                                if (polyLine.Points[i] != polyLine.Points[i + 1])
                                {
                                    points.Add(polyLine.Points[i]);
                                }
                            }
                            graphicsList.RemoveAt(graphicsList.Count - 1);

                            points.Add(Mouse.GetPosition(this));

                            polyLine = new GraphicsPolyLine(
                                points,
                                this.LineWidth,
                                this.RegionType,
                                this.ObjectColor,
                                this.ActualScale);

                            // 완성된 다각형을 리스트에 추가한다.
                            SelectedGraphic = polyLine;
                            graphicsList.Add(polyLine);
                            polyLine.IsSelected = true;

                            undoManager.AddCommandToHistory(new CommandAdd(polyLine));
                        }
                        else
                        {
                            graphicsList.RemoveAt(graphicsList.Count - 1);
                        }
                    }
                }
                catch
                {
                    ShowContextMenu(e);
                }

                UpdateState();
            }
        }
        #endregion Mouse Event Handlers

        #region Other Event Handlers
        /// <summary>   Initialization after control is loaded. </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Routed event information. </param>
        public void DrawingCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            this.Focusable = true;      // to handle keyboard messages
        }

        public void SymmetryGraphic(Point aRightBottom, FilpType aFlip)
        {
            foreach (GraphicsBase roi in this.graphicsList)
            {
                if (roi.IsSelected == true)
                {
                    if (roi is GraphicsRectangleBase)
                    {
                        #region Rectangle, Ellipse Symmetry
                        GraphicsRectangleBase graphicsRectangleBase = roi as GraphicsRectangleBase;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            graphicsRectangleBase.Top = aRightBottom.Y - graphicsRectangleBase.Top;
                            graphicsRectangleBase.Bottom = aRightBottom.Y - graphicsRectangleBase.Bottom;
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            graphicsRectangleBase.Left = aRightBottom.X - graphicsRectangleBase.Left;
                            graphicsRectangleBase.Right = aRightBottom.X - graphicsRectangleBase.Right;
                        }
                        graphicsRectangleBase.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsPolyLine)
                    {
                        #region PolyLine Symmetry
                        GraphicsPolyLine graphicsPolyLine = roi as GraphicsPolyLine;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            for (int i = 0; i < graphicsPolyLine.Points.Length; i++)
                            {
                                graphicsPolyLine.Points[i].Y = aRightBottom.Y - graphicsPolyLine.Points[i].Y;
                            }
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            for (int i = 0; i < graphicsPolyLine.Points.Length; i++)
                            {
                                graphicsPolyLine.Points[i].X = aRightBottom.X - graphicsPolyLine.Points[i].X;
                            }
                        }
                        graphicsPolyLine.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsLine)
                    {
                        #region GraphicsLine Symmetry
                        GraphicsLine graphicsLine = roi as GraphicsLine;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            Point point = new Point();
                            point.Y = aRightBottom.Y - graphicsLine.Start.Y;
                            point.X = graphicsLine.Start.X;
                            graphicsLine.Start = point;

                            point = new Point();
                            point.Y = aRightBottom.Y - graphicsLine.End.Y;
                            point.X = graphicsLine.End.X;
                            graphicsLine.End = point;
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            Point point = new Point();
                            point.X = aRightBottom.X - graphicsLine.Start.X;
                            point.Y = graphicsLine.Start.Y;
                            graphicsLine.Start = point;

                            point = new Point();
                            point.X = aRightBottom.X - graphicsLine.End.X;
                            point.Y = graphicsLine.End.Y;
                            graphicsLine.End = point;
                        }
                        graphicsLine.RefreshDrawing();
                        #endregion
                    }

                    if (roi.LocalAligns != null)
                    {
                        for (int nIndex = 0; nIndex < roi.LocalAligns.Length; nIndex++)
                        {
                            if (roi.LocalAligns[nIndex] != null)
                            {
                                #region Rectangle, Ellipse Symmetry
                                if (aFlip == FilpType.UPDOWN)
                                {
                                    roi.LocalAligns[nIndex].Top = aRightBottom.Y - roi.LocalAligns[nIndex].Top;
                                    roi.LocalAligns[nIndex].Bottom = aRightBottom.Y - roi.LocalAligns[nIndex].Bottom;
                                }
                                else if (aFlip == FilpType.LEFTRIGHT)
                                {
                                    roi.LocalAligns[nIndex].Left = aRightBottom.X - roi.LocalAligns[nIndex].Left;
                                    roi.LocalAligns[nIndex].Right = aRightBottom.X - roi.LocalAligns[nIndex].Right;
                                }
                                roi.LocalAligns[nIndex].RefreshDrawing();
                                #endregion
                            }
                        }
                    }
                }
            }
        }

        public void CopytoGraphic(Point aRightBottom, FilpType aFlip)
        {
            CopyGraphics();
            List<GraphicsBase> SourceClipboardList = (IsBasedCanvas) ? FiduClipboardList : ClipboardList;
            if (SourceClipboardList != null && SourceClipboardList.Count > 0)
            {
                UnselectAll();
                List<GraphicsBase> newGraphicsClipboardList = new List<GraphicsBase>();

                foreach (GraphicsBase b in GraphicsList)
                {
                    b.ObjectColor = b.OriginObjectColor;
                }

                foreach (GraphicsBase roi in SourceClipboardList)
                {
                    GraphicsBase graphic = roi.CreateSerializedObject().CreateGraphics();
                    graphic.ID = graphic.GetHashCode();
                    graphic.graphicsActualScale = this.ActualScale;
                    graphic.OriginObjectColor = roi.OriginObjectColor;
                    graphic.graphicsLineWidth = roi.LineWidth;
                    graphic.selected = true;
                    graphic.RefreshDrawing();

                    if (roi.LocalAligns != null)
                    {
                        graphic.LocalAligns = new GraphicsRectangle[roi.LocalAligns.Length];
                        for (int nIndex = 0; nIndex < roi.LocalAligns.Length; nIndex++)
                        {
                            if (roi.LocalAligns[nIndex] != null)
                            {
                                // Create Local Align.
                                GraphicsRectangle localAlign = CreateLocalAlign(graphic, nIndex, roi.LocalAligns[nIndex].boundaryRect);
                                localAlign.MotherROI = graphic;
                                graphic.LocalAligns[nIndex] = localAlign;
                                #region Rectangle, Ellipse Symmetry
                                if (aFlip == FilpType.UPDOWN)
                                {
                                    graphic.LocalAligns[nIndex].Top = aRightBottom.Y - roi.LocalAligns[nIndex].Top;
                                    graphic.LocalAligns[nIndex].Bottom = aRightBottom.Y - roi.LocalAligns[nIndex].Bottom;
                                }
                                else if (aFlip == FilpType.LEFTRIGHT)
                                {
                                    graphic.LocalAligns[nIndex].Left = aRightBottom.X - roi.LocalAligns[nIndex].Left;
                                    graphic.LocalAligns[nIndex].Right = aRightBottom.X - roi.LocalAligns[nIndex].Right;
                                }
                                graphic.LocalAligns[nIndex].RefreshDrawing();
                                #endregion
                            }
                        }
                    }

                    if (roi is GraphicsRectangleBase)
                    {
                        #region Rectangle, Ellipse Symmetry
                        GraphicsRectangleBase graphicsRectangleBase = roi as GraphicsRectangleBase;
                        GraphicsRectangleBase graphicsRectangleBase2 = graphic as GraphicsRectangleBase;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            graphicsRectangleBase2.Top = aRightBottom.Y - graphicsRectangleBase.Top;
                            graphicsRectangleBase2.Bottom = aRightBottom.Y - graphicsRectangleBase.Bottom;
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            graphicsRectangleBase2.Left = aRightBottom.X - graphicsRectangleBase.Left;
                            graphicsRectangleBase2.Right = aRightBottom.X - graphicsRectangleBase.Right;
                        }
                        graphicsRectangleBase2.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsPolyLine)
                    {
                        #region PolyLine Symmetry
                        GraphicsPolyLine graphicsPolyLine = roi as GraphicsPolyLine;
                        GraphicsPolyLine graphicsPolyLine2 = graphic as GraphicsPolyLine;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            for (int i = 0; i < graphicsPolyLine.Points.Length; i++)
                            {
                                graphicsPolyLine2.Points[i].Y = aRightBottom.Y - graphicsPolyLine.Points[i].Y;
                            }
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            for (int i = 0; i < graphicsPolyLine.Points.Length; i++)
                            {
                                graphicsPolyLine2.Points[i].X = aRightBottom.X - graphicsPolyLine.Points[i].X;
                            }
                        }
                        graphicsPolyLine2.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsLine)
                    {
                        #region GraphicsLine Symmetry
                        GraphicsLine graphicsLine = roi as GraphicsLine;
                        GraphicsLine graphicsLine2 = graphic as GraphicsLine;
                        if (aFlip == FilpType.UPDOWN)
                        {
                            Point point = new Point();
                            point.Y = aRightBottom.Y - graphicsLine.Start.Y;
                            point.X = graphicsLine.Start.X;
                            graphicsLine2.Start = point;

                            point = new Point();
                            point.Y = aRightBottom.Y - graphicsLine.End.Y;
                            point.X = graphicsLine.End.X;
                            graphicsLine2.End = point;
                        }
                        else if (aFlip == FilpType.LEFTRIGHT)
                        {
                            Point point = new Point();
                            point.X = aRightBottom.X - graphicsLine.Start.X;
                            point.Y = graphicsLine.Start.Y;
                            graphicsLine2.Start = point;

                            point = new Point();
                            point.X = aRightBottom.X - graphicsLine.End.X;
                            point.Y = graphicsLine.End.Y;
                            graphicsLine2.End = point;
                        }
                        graphicsLine2.RefreshDrawing();
                        #endregion
                    }

                    if (CanDraw(graphic))
                    {
                        if (IsBasedCanvas)
                        {
                            if (fiduGraphicsList.Count < MaxGraphicsCount)
                            {
                                graphicsList.Add(graphic);
                                if (graphic.RegionType == GraphicsRegionType.OuterRegion && graphic is GraphicsRectangle)
                                {
                                    FiduGraphicsList.Add(graphic as GraphicsRectangle);
                                }
                            }
                        }
                        else
                        {
                            if (graphicsList.Count < MaxGraphicsCount)
                            {
                                graphicsList.Add(graphic);
                                if (graphic.LocalAligns != null)
                                {
                                    int nLocalAligns = graphic.LocalAligns.Length;
                                    for (int nIndex = 0; nIndex < nLocalAligns; nIndex++)
                                    {
                                        if (graphic.LocalAligns[nIndex] != null)
                                        {
                                            if (CanDraw(graphic.LocalAligns[nIndex]))
                                                graphicsList.Add(graphic.LocalAligns[nIndex]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    newGraphicsClipboardList.Add(graphic);
                }
                SourceClipboardList.Clear();
                if (IsBasedCanvas)
                {
                    FiduClipboardList = newGraphicsClipboardList;
                }
                else
                {
                    ClipboardList = newGraphicsClipboardList;
                }

                undoManager.AddCommandToHistory(new CommandAdd(this));
                UpdateState();
            }
        }

        public void RotateGraphic(Point aCenterPoint, double aAngle)
        {
            double _Sin = Math.Sin(aAngle * Math.PI / 180);
            double _Cos = Math.Cos(aAngle * Math.PI / 180);

            foreach (GraphicsBase roi in this.graphicsList)
            {
                if (roi.IsSelected)
                {
                    if (roi is GraphicsRectangleBase)
                    {
                        #region Rectangle,Ellipse Rotation
                        GraphicsRectangleBase graphicsRectangleBase = roi as GraphicsRectangleBase;

                        //좌표계 변환
                        graphicsRectangleBase.Left -= aCenterPoint.X;
                        graphicsRectangleBase.Top -= aCenterPoint.Y;
                        graphicsRectangleBase.Right -= aCenterPoint.X;
                        graphicsRectangleBase.Bottom -= aCenterPoint.Y;

                        //회전
                        double rotateLeft = graphicsRectangleBase.Left * _Cos - graphicsRectangleBase.Top * _Sin;
                        double rotateTop = graphicsRectangleBase.Left * _Sin + graphicsRectangleBase.Top * _Cos;
                        double rotateRight = graphicsRectangleBase.Right * _Cos - graphicsRectangleBase.Bottom * _Sin;
                        double rotateBottom = graphicsRectangleBase.Right * _Sin + graphicsRectangleBase.Bottom * _Cos;

                        //좌표계 복구
                        graphicsRectangleBase.Left = aCenterPoint.X + Math.Min(rotateRight, rotateLeft);
                        graphicsRectangleBase.Top = aCenterPoint.Y + Math.Min(rotateTop, rotateBottom);
                        graphicsRectangleBase.Right = aCenterPoint.X + Math.Max(rotateRight, rotateLeft);
                        graphicsRectangleBase.Bottom = aCenterPoint.Y + Math.Max(rotateTop, rotateBottom);

                        graphicsRectangleBase.Move(0, 0, aCenterPoint.X * 2, aCenterPoint.Y * 2);
                        graphicsRectangleBase.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsPolyLine)
                    {
                        #region PolyLine Rotation
                        double rotateX = 0.0;
                        double rotateY = 0.0;
                        GraphicsPolyLine graphicsPolyLine = roi as GraphicsPolyLine;

                        // 회전
                        for (int i = 0; i < graphicsPolyLine.Points.Length; i++)
                        {
                            graphicsPolyLine.Points[i].X -= aCenterPoint.X;
                            graphicsPolyLine.Points[i].Y -= aCenterPoint.Y;

                            rotateX = graphicsPolyLine.Points[i].X * _Cos - graphicsPolyLine.Points[i].Y * _Sin;
                            rotateY = graphicsPolyLine.Points[i].X * _Sin + graphicsPolyLine.Points[i].Y * _Cos;

                            graphicsPolyLine.Points[i].X = aCenterPoint.X + rotateX;
                            graphicsPolyLine.Points[i].Y = aCenterPoint.Y + rotateY;
                        }

                        graphicsPolyLine.Move(0, 0, aCenterPoint.X * 2, aCenterPoint.Y * 2);
                        graphicsPolyLine.RefreshDrawing();
                        #endregion
                    }
                    else if (roi is GraphicsLine)
                    {
                        #region Line Rotation
                        GraphicsLine graphicsLine = roi as GraphicsLine;
                        // 시작점
                        Point point = new Point();
                        point.X = graphicsLine.Start.X - aCenterPoint.X;
                        point.Y = graphicsLine.Start.Y - aCenterPoint.Y;

                        double rotateX = point.X * _Cos - point.Y * _Sin;
                        double rotateY = point.X * _Sin + point.Y * _Cos;

                        point.X = aCenterPoint.X + rotateX;
                        point.Y = aCenterPoint.Y + rotateY;
                        graphicsLine.Start = point;

                        // 끝점
                        point = new Point();
                        point.X = graphicsLine.End.X - aCenterPoint.X;
                        point.Y = graphicsLine.End.Y - aCenterPoint.Y;

                        rotateX = point.X * _Cos - point.Y * _Sin;
                        rotateY = point.X * _Sin + point.Y * _Cos;

                        point.X = aCenterPoint.X + rotateX;
                        point.Y = aCenterPoint.Y + rotateY;
                        graphicsLine.End = point;

                        graphicsLine.Move(0, 0, aCenterPoint.X * 2, aCenterPoint.Y * 2);
                        graphicsLine.RefreshDrawing();
                        #endregion
                    }

                    if (roi.LocalAligns != null)
                    {
                        for (int nIndex = 0; nIndex < roi.LocalAligns.Length; nIndex++)
                        {
                            if (roi.LocalAligns[nIndex] != null)
                            {
                                #region Local Align Rotation
                                //좌표계 변환
                                roi.LocalAligns[nIndex].Left -= aCenterPoint.X;
                                roi.LocalAligns[nIndex].Top -= aCenterPoint.Y;
                                roi.LocalAligns[nIndex].Right -= aCenterPoint.X;
                                roi.LocalAligns[nIndex].Bottom -= aCenterPoint.Y;

                                //회전
                                double rotateLeft = roi.LocalAligns[nIndex].Left * _Cos - roi.LocalAligns[nIndex].Top * _Sin;
                                double rotateTop = roi.LocalAligns[nIndex].Left * _Sin + roi.LocalAligns[nIndex].Top * _Cos;
                                double rotateRight = roi.LocalAligns[nIndex].Right * _Cos - roi.LocalAligns[nIndex].Bottom * _Sin;
                                double rotateBottom = roi.LocalAligns[nIndex].Right * _Sin + roi.LocalAligns[nIndex].Bottom * _Cos;

                                //좌표계 복구
                                roi.LocalAligns[nIndex].Left = aCenterPoint.X + Math.Min(rotateRight, rotateLeft);
                                roi.LocalAligns[nIndex].Top = aCenterPoint.Y + Math.Min(rotateTop, rotateBottom);
                                roi.LocalAligns[nIndex].Right = aCenterPoint.X + Math.Max(rotateRight, rotateLeft);
                                roi.LocalAligns[nIndex].Bottom = aCenterPoint.Y + Math.Max(rotateTop, rotateBottom);

                                roi.LocalAligns[nIndex].Move(0, 0, aCenterPoint.X * 2, aCenterPoint.Y * 2);
                                roi.LocalAligns[nIndex].RefreshDrawing();
                                #endregion
                            }
                        }
                    }
                }
            }
        }

        // 2014-09-01, suoow2.
        public void UpdateSelectedGraphic()
        {
            Point point = Mouse.GetPosition(this);

            List<GraphicsBase> hittedList = new List<GraphicsBase>();
            foreach (GraphicsBase graphic in graphicsList)
            {
                if (graphic.MakeHitTest(point) >= 0)
                {
                    hittedList.Add(graphic);
                }
            }

            if (hittedList.Count > 0)
            {
                SelectedGraphic = hittedList[hittedList.Count - 1]; // 가장 앞에 놓인 GraphicsBase를 SelectedGraphic으로 선정한다.
            }
            else
            {
                SelectedGraphic = null;
            }
        }

        /// <summary>   Context menu item is clicked. </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Routed event information. </param>
        private void contextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;

            if (item == null)
            {
                return;
            }

            ContextMenuCommand command = (ContextMenuCommand)item.Tag;
            ContextMenuChangeEventHandler eventRunner = ContextMenuChangeEvent;

            switch (command)
            {
                case ContextMenuCommand.CalcUnitPitch:
                    UnselectAll();
                    this.Tool = ToolType.UnitPitch;

                    break;
                case ContextMenuCommand.CalcBlockGap:
                    UnselectAll();
                    this.Tool = ToolType.BlockGap;
                    break;
                case ContextMenuCommand.CalcInBlockGapX:
                    UnselectAll();
                    this.Tool = ToolType.InnerBlockGapX;
                    break;
                case ContextMenuCommand.CalcInBlockGapY:
                    UnselectAll();
                    this.Tool = ToolType.InnerBlockGapY;
                    break;
                case ContextMenuCommand.CalcStripGap:
                    UnselectAll();
                    this.Tool = ToolType.StripGap;
                    break;
                case ContextMenuCommand.CalcInInBlockGapX:
                    UnselectAll();
                    this.Tool = ToolType.inInnerBlockGapX;
                    break;
                case ContextMenuCommand.CalcInInBlockGapY:
                    UnselectAll();
                    this.Tool = ToolType.inInnerBlockGapY;
                    break;
                case ContextMenuCommand.SetFiducialRegion:
                    ChangeFiducialRegion(CaptionHelper.FiducialUnitRegionCaption);
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.SetFiducialRegion);
                    }
                    break;
                case ContextMenuCommand.ShowFiducialRegion:
                    UnselectAll();
                    foreach (GraphicsRectangle g in GraphicsRectangleList)
                        if (g.IsFiducialRegion)
                            g.IsSelected = true;
                    break;
                case ContextMenuCommand.RetrySearchRegion:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.RetrySearchRegion);
                    }
                    break;
                case ContextMenuCommand.UnloadFromSection:
                    UnloadFromSection(false);
                    break;
                case ContextMenuCommand.UnloadFromSectionBlock:
                    UnloadFromSection(true);
                    break;
                case ContextMenuCommand.RegisterSection:
                    RegisterSection();
                    break;
                case ContextMenuCommand.UnloadAndRegisterSection:
                    UnloadFromSection(false);
                    RegisterSection();
                    break;
                case ContextMenuCommand.UnloadAndRegisterSectionBlock:
                    UnloadFromSection(true);
                    RegisterSection();
                    break;
                case ContextMenuCommand.ShowSelectedSectionGroup:
                    ShowSelectedSectionGroup();
                    break;
                case ContextMenuCommand.CopySectionSetting:
                    SectionSettingStorage.OriginSetting.CopySectionSetting(this);
                    break;
                case ContextMenuCommand.PasteSectionSetting:
                    SectionSettingStorage.OriginSetting.PasteSectionSetting(this);
                    break;
                case ContextMenuCommand.SetExceptInspectionRegion:
                    CommandChangeState setExceptCommandChangeState = new CommandChangeState(SelectedGraphic);
                    SelectedGraphic.RegionType = GraphicsRegionType.Except;
                    SelectedGraphic.OriginObjectColor = SelectedGraphic.ObjectColor = GraphicsColors.Blue;
                    SelectedGraphic.Caption = CaptionHelper.ExceptionalMaskCaption;
                    setExceptCommandChangeState.NewState(SelectedGraphic);
                    AddCommandToHistory(setExceptCommandChangeState);
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.SetExceptInspectionRegion);
                    }
                    break;

                case ContextMenuCommand.UnSetExceptInspectionRegion:
                    CommandChangeState unsetExceptCommandChangeState = new CommandChangeState(SelectedGraphic);
                    SelectedGraphic.RegionType = GraphicsRegionType.Inspection;
                    SelectedGraphic.OriginObjectColor = SelectedGraphic.ObjectColor = GraphicsColors.Green;
                    SelectedGraphic.Caption = "";
                    unsetExceptCommandChangeState.NewState(SelectedGraphic);
                    AddCommandToHistory(unsetExceptCommandChangeState);
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.UnSetExceptInspectionRegion);
                    }
                    break;
                //DownSet 영역 지정 및 해제
                case ContextMenuCommand.SetDownSetRegion:
                    CommandChangeState setDownSetCommandChangeState = new CommandChangeState(SelectedGraphic);
                    SelectedGraphic.RegionType = GraphicsRegionType.DownSetRegion;
                    SelectedGraphic.OriginObjectColor = SelectedGraphic.ObjectColor = GraphicsColors.OrangeRed;
                    SelectedGraphic.Caption = CaptionHelper.DownSetRegionCaption;
                    setDownSetCommandChangeState.NewState(SelectedGraphic);
                    AddCommandToHistory(setDownSetCommandChangeState);
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.SetDownSetRegion);
                    }
                    break;

                case ContextMenuCommand.UnSetDownSetRegion:
                    CommandChangeState unsetDownSetCommandChangeState = new CommandChangeState(SelectedGraphic);
                    SelectedGraphic.RegionType = GraphicsRegionType.Inspection;
                    SelectedGraphic.OriginObjectColor = SelectedGraphic.ObjectColor = GraphicsColors.Green;
                    SelectedGraphic.Caption = "";
                    unsetDownSetCommandChangeState.NewState(SelectedGraphic);
                    AddCommandToHistory(unsetDownSetCommandChangeState);
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.UnSetDownSetRegion);
                    }
                    break;
                case ContextMenuCommand.SelectAll:
                    SelectAll();
                    break;
                case ContextMenuCommand.UnselectAll:
                    UnselectAll();
                    break;
                case ContextMenuCommand.Delete:
                    if (SelectionCount > 0)
                    {
                        Delete();
                    }
                    break;
                case ContextMenuCommand.DeleteAll:
                    DeleteAll();
                    break;
                case ContextMenuCommand.MoveToFront:
                    MoveToFront();
                    break;
                case ContextMenuCommand.MoveToBack:
                    MoveToBack();
                    break;
                case ContextMenuCommand.Undo:
                    Undo();
                    break;
                case ContextMenuCommand.Redo:
                    Redo();
                    break;
                case ContextMenuCommand.SerProperties:
                    SetProperties();
                    break;
                case ContextMenuCommand.RotateROI90:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.RotateROI90);
                    }
                    break;
                case ContextMenuCommand.RotateROI180:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.RotateROI180);
                    }
                    break;
                case ContextMenuCommand.SymmetryROIUpDown:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.SymmetryROIUpDown);
                    }
                    break;
                case ContextMenuCommand.SymmetryROILeftRight:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.SymmetryROILeftRight);
                    }
                    break;
                case ContextMenuCommand.CopyROIUpDown:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.CopyROIUpDown);
                    }
                    break;
                case ContextMenuCommand.CopyROILeftRight:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.CopyROILeftRight);
                    }
                    break;
                case ContextMenuCommand.LocalAlign:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.LocalAlign);
                    }
                    break;
                case ContextMenuCommand.GuideLineSetting:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.GuideLineSetting);
                    }
                    break;
                case ContextMenuCommand.TapLocationSetting:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.TapLocationSetting);
                    }
                    break;
                case ContextMenuCommand.Templete:
                    if (eventRunner != null)
                    {
                        eventRunner(ContextMenuCommand.Templete);
                    }
                    break;



            }
        }

        /// <summary>   Mouse capture is lost. </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Mouse event information. </param>
        public void DrawingCanvas_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                CancelCurrentOperation();
                UpdateState();
            }
        }

        // 반복 설정 취소
        private void UndoIterationGraphics(GraphicsRectangle aLeftTopGraphic)
        {
            GraphicsRectangle motherROI = (aLeftTopGraphic.MotherROI == null) ? aLeftTopGraphic : aLeftTopGraphic.MotherROI as GraphicsRectangle;
            int nLength = graphicsList.Count;
            for (int i = 0; i < nLength; i++)
            {
                if (graphicsList[i] == aLeftTopGraphic)
                {
                    continue;
                }
                if (graphicsList[i] is GraphicsRectangle && ((GraphicsRectangle)graphicsList[i]).MotherROI == motherROI)
                {
                    graphicsList.RemoveAt(i);
                    i--;
                    nLength--;
                }
            }
            if (aLeftTopGraphic.MotherROI != null)
            {
                graphicsList.Remove(aLeftTopGraphic.MotherROI);
            }
        }

        // 반복 설정
        public void IterationGraphics(GraphicsRectangle fiducialGraphic, SymmetryType aSymmetryType = SymmetryType.Matrix)
        {
            #region set local variables.
            int unitColumns = fiducialGraphic.IterationValue.Column;
            int unitRows = fiducialGraphic.IterationValue.Row;
            double unitXPitch = fiducialGraphic.IterationValue.XPitch;
            double unitYPitch = fiducialGraphic.IterationValue.YPitch;

            int blocknum = fiducialGraphic.BlockIterationValue.Block;
            int blockColumns = fiducialGraphic.BlockIterationValue.Column;
            int blockRows = fiducialGraphic.BlockIterationValue.Row;
            double blockgap = fiducialGraphic.BlockIterationValue.Gap;
            double blockXPitch = fiducialGraphic.BlockIterationValue.XPitch;
            double blockYPitch = fiducialGraphic.BlockIterationValue.YPitch;

            int unitRowPerBlockNum = ((double)unitRows / (double)blocknum > unitRows / blocknum) ? unitRows / blocknum + 1 : unitRows / blocknum;
            int unitRowPerBlockRow = ((double)unitRowPerBlockNum / (double)blockRows > unitRowPerBlockNum / blockRows) ? unitRowPerBlockNum / blockRows + 1 : unitRowPerBlockNum / blockRows;
            int unitColumnPerBlockColumn = ((double)unitColumns / (double)blockColumns > unitColumns / blockColumns) ? unitColumns / blockColumns + 1 : unitColumns / blockColumns;
            #endregion

            double columnSpace = 0;
            double rowSpace = 0;

            // 반복 설정된 ROI중 최좌측상단의 ROI를 기준으로 지정한다.
            if (!(fiducialGraphic.IterationXPosition == 0 && fiducialGraphic.IterationYPosition == 0))
            {
                foreach (GraphicsRectangle g in GraphicsRectangleList)
                {
                    if (g.RegionType == GraphicsRegionType.UnitRegion)
                    {
                        if (g.MotherROI == fiducialGraphic &&
                            g.IterationXPosition == 0 &&
                            g.IterationYPosition == 0)
                        {
                            GraphicsRectangle mostLeftTopGraphic = g;
                            mostLeftTopGraphic.SymmetryValue = fiducialGraphic.SymmetryValue;
                            mostLeftTopGraphic.IterationValue = fiducialGraphic.IterationValue;
                            mostLeftTopGraphic.BlockIterationValue = fiducialGraphic.BlockIterationValue;

                            fiducialGraphic = mostLeftTopGraphic;
                            break;
                        }
                    }
                }
            }
            UndoIterationGraphics(fiducialGraphic);

            Debug.Assert(fiducialGraphic.IterationXPosition == 0);
            Debug.Assert(fiducialGraphic.IterationYPosition == 0);

            for (int row = 0; row < unitRows; row++)
            {
                if (row % unitRowPerBlockRow == 0 && row >= 1)
                {
                    if (row >= 1 && row % unitRowPerBlockNum == 0)
                    {
                        rowSpace += blockgap;
                    }
                    else rowSpace += blockYPitch;
                }

                for (int column = 0; column < unitColumns; column++)
                {
                    if (column == 0 && row == 0) continue;

                    if (column == 0)
                    {
                        columnSpace = 0;
                    }
                    else
                    {
                        columnSpace += unitXPitch;
                    }

                    if (column % unitColumnPerBlockColumn ==0 && column >=1)
                    {
                            columnSpace += blockXPitch;
                    }

                    GraphicsRectangle graphic = new GraphicsRectangle(fiducialGraphic.Left + columnSpace,
                        fiducialGraphic.Top + rowSpace,
                        fiducialGraphic.Right + columnSpace,
                        fiducialGraphic.Bottom + rowSpace,
                        LineWidth,
                        fiducialGraphic.RegionType,
                        GraphicsColors.Green,
                        fiducialGraphic.ActualScale,
                        fiducialGraphic.SymmetryValue,
                        fiducialGraphic.IterationValue,
                        fiducialGraphic.BlockIterationValue,
                        false,
                        fiducialGraphic);
                    graphic.IsValidRegion = fiducialGraphic.IsValidRegion;
                    graphic.IterationXPosition = column;
                    graphic.IterationYPosition = row;
                    graphic.Caption = CaptionHelper.GetRegionCaption(graphic);

                    graphicsList.Add(graphic);
                }
                columnSpace = 0;
                rowSpace += unitYPitch;
            }

            if (fiducialGraphic.RegionType == GraphicsRegionType.UnitRegion)
            {
                fiducialGraphic.Caption = CaptionHelper.FiducialUnitRegionCaption;
            }
            else if (fiducialGraphic.RegionType == GraphicsRegionType.OuterRegion)
            {
                fiducialGraphic.Caption = CaptionHelper.FiducialOuterRegionCaption;
            }
            fiducialGraphic.IsValidRegion = true;
            fiducialGraphic.IsFiducialRegion = true;
            fiducialGraphic.MotherROI = null;

            UpdateState();
            UnselectAll();
        }

        public List<GraphicsBase> CloneGraphics()
        {
            List<GraphicsBase> SourceClipboardList = new List<GraphicsBase>();
            try
            {
                SourceClipboardList.Clear();
                foreach (GraphicsBase b in graphicsList)
                {
                    if (b.RegionType == GraphicsRegionType.LocalAlign || b.RegionType == GraphicsRegionType.DownSetAlign)
                        continue;
                    if (b is GraphicsSelectionRectangle || b is GraphicsLine || b is GraphicsSkeletonLine)
                        continue;

                    GraphicsBase graphic = b.CreateSerializedObject().CreateGraphics();
                    
                    graphic.OriginObjectColor = graphic.ObjectColor = b.OriginObjectColor;
                    graphic.IsSelected = false;

                    SourceClipboardList.Add(graphic);
                }
                return SourceClipboardList;
            }
            catch
            {
                Debug.WriteLine("Exception occured in CopyGraphics(DrawingCanvas.cs)");
                return SourceClipboardList;
            }
        }

        /// <summary>   Copies the graphics. </summary>
        /// <remarks>   suoow2, 2014-08-18. </remarks>
        public void CopyGraphics()
        {
            try
            {
                if (SelectionCount > 0)
                {
                    List<GraphicsBase> SourceClipboardList = (IsBasedCanvas) ? FiduClipboardList : ClipboardList;
                    if (SourceClipboardList != null)
                    {
                        SourceClipboardList.Clear();
                        foreach (GraphicsBase b in Selection)
                        {
                            if (b.RegionType == GraphicsRegionType.LocalAlign || b.RegionType == GraphicsRegionType.DownSetAlign)
                                continue;
                            if (b is GraphicsSelectionRectangle || b is GraphicsLine || b is GraphicsSkeletonLine)
                                continue;

                            GraphicsBase graphic = b.CreateSerializedObject().CreateGraphics();
                            
                            graphic.OriginObjectColor = graphic.ObjectColor = b.OriginObjectColor;
                            graphic.IsSelected = false;

                            SourceClipboardList.Add(graphic);
                        }
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in CopyGraphics(DrawingCanvas.cs)");
            }
        }

        /// <summary>   Paste graphics. </summary>
        /// <remarks>   suoow2, 2014-08-18. </remarks>
        public void PasteGraphics()
        {
            try
            {
                List<GraphicsBase> SourceClipboardList = (IsBasedCanvas) ? FiduClipboardList : ClipboardList;
                if (SourceClipboardList != null && SourceClipboardList.Count > 0)
                {
                    UnselectAll();
                    List<GraphicsBase> newGraphicsClipboardList = new List<GraphicsBase>();

                    foreach (GraphicsBase b in GraphicsList)
                    {
                        b.ObjectColor = b.OriginObjectColor;
                    }
                    bool bNeedMoveGraphics = (graphicsList.Count > 0);

                    foreach (GraphicsBase b in SourceClipboardList)
                    {
                        if (b.RegionType == GraphicsRegionType.LocalAlign || b.RegionType == GraphicsRegionType.DownSetAlign)
                            continue;
                        GraphicsBase graphic = b.CreateSerializedObject().CreateGraphics();
                        graphic.ID = graphic.GetHashCode();
                        graphic.graphicsActualScale = this.ActualScale;
                        graphic.graphicsLineWidth = b.LineWidth;
                        graphic.selected = true;
                        graphic.RefreshDrawing();

                        if (bNeedMoveGraphics)
                            graphic.Move(5, 5, this.ActualWidth, this.ActualHeight);

                        if (CanDraw(graphic))
                        {
                            if (IsBasedCanvas)
                            {
                                if (fiduGraphicsList.Count < MaxGraphicsCount)
                                {
                                    graphicsList.Add(graphic);
                                    if (graphic.RegionType == GraphicsRegionType.OuterRegion && graphic is GraphicsRectangle)
                                    {
                                        FiduGraphicsList.Add(graphic as GraphicsRectangle);
                                    }
                                }
                            }
                            else
                            {
                                if (graphicsList.Count < MaxGraphicsCount)
                                {
                                    graphicsList.Add(graphic);
                                    if (graphic.LocalAligns != null)
                                    {
                                        int nLocalAligns = graphic.LocalAligns.Length;
                                        for (int nIndex = 0; nIndex < nLocalAligns; nIndex++)
                                        {
                                            if (graphic.LocalAligns[nIndex] != null)
                                            {
                                                graphic.LocalAligns[nIndex].Move(5, 5, this.ActualWidth, this.ActualHeight);
                                                if (CanDraw(graphic.LocalAligns[nIndex]))
                                                    graphicsList.Add(graphic.LocalAligns[nIndex]);
                                            }
                                        }
                                    }

                                    if (graphic.DownSetAligns != null)
                                    {
                                        int nDownSetAligns = graphic.DownSetAligns.Length;
                                        for (int nIndex = 0; nIndex < nDownSetAligns; nIndex++)
                                        {
                                            if (graphic.DownSetAligns[nIndex] != null)
                                            {
                                                graphic.DownSetAligns[nIndex].Move(5, 5, this.ActualWidth, this.ActualHeight);
                                                if (CanDraw(graphic.DownSetAligns[nIndex]))
                                                    graphicsList.Add(graphic.DownSetAligns[nIndex]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        newGraphicsClipboardList.Add(graphic);
                    }
                    SourceClipboardList.Clear();
                    if (IsBasedCanvas)
                    {
                        FiduClipboardList = newGraphicsClipboardList;
                    }
                    else
                    {
                        ClipboardList = newGraphicsClipboardList;
                    }

                    undoManager.AddCommandToHistory(new CommandAdd(this));
                    UpdateState();
                }
            }
            catch
            {
                Debug.WriteLine("Exception occured in PasteGraphics(DrawingCanvas.cs)");
            }
        }

        /// <summary>   Move graphics. </summary>
        /// <remarks>   suoow2, 2014-08-19. </remarks>
        public void MoveGraphics(double deltaX, double deltaY)
        {
            HelperFunctions.MoveGraphics(this, deltaX, deltaY);
            UpdateState();
        }

        /// <summary>
        /// UndoManager state is changed. Refresh CanUndo, CanRedo and IsDirty properties.
        /// </summary>
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        private void undoManager_StateChanged(object sender, EventArgs e)
        {
            this.CanUndo = undoManager.CanUndo;
            this.CanRedo = undoManager.CanRedo;

            // Set IsDirty only if it is actually changed.
            // Setting IsDirty raises event for client.
            if (undoManager.IsDirty != this.IsDirty)
            {
                this.IsDirty = undoManager.IsDirty;
            }
        }
        #endregion Other Event Handlers

        #region Other Functions
        /// <summary>   Create context menu. </summary>
        private void CreateContextMenu(bool UseEng = false)
        {
            contextMenu = new ContextMenu();
            MenuItem menuItem;

            #region 전체영상에서의 Context Menu.
            if (IsBasedCanvas) // 전체 영상에서의 Context Menu.
            {
                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Specify reference area" : "기준 영역 지정";
                menuItem.Tag = ContextMenuCommand.SetFiducialRegion;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Re-explore the area" : "영역 재탐색";
                menuItem.Tag = ContextMenuCommand.RetrySearchRegion;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Section Release & Register" : "섹션 해제 & 등록";
                menuItem.Tag = ContextMenuCommand.UnloadAndRegisterSectionBlock;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Section Release & Registration (Block Exception)" : "섹션 해제 & 등록(블록 예외)";
                menuItem.Tag = ContextMenuCommand.UnloadAndRegisterSection;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Section registration" : "섹션 등록";
                menuItem.Tag = ContextMenuCommand.RegisterSection;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Section release" : "섹션 해제";
                menuItem.Tag = ContextMenuCommand.UnloadFromSectionBlock;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Section release (block exception)" : "섹션 해제(블록 예외)";
                menuItem.Tag = ContextMenuCommand.UnloadFromSection;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Display reference area" : "기준 영역 표시";
                menuItem.Tag = ContextMenuCommand.ShowFiducialRegion;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Display same section area" : "동일 섹션 영역 표시";
                menuItem.Tag = ContextMenuCommand.ShowSelectedSectionGroup;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy section settings" : "섹션 설정 복사";
                menuItem.Tag = ContextMenuCommand.CopySectionSetting;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng? "Paste section settings(current location)" : "현재 위치에 섹션 설정 붙여넣기";
                menuItem.Tag = ContextMenuCommand.PasteSectionSetting;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Unit Pitch Calculation" : "Unit Pitch 계산";
                menuItem.Tag = ContextMenuCommand.CalcUnitPitch;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Block Gap Calculation" : "Block Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcBlockGap;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Calculating Block X Gap within Block" : "Block내 Block X Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcInBlockGapX;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Calculating Block Y Gap within a Block" : "Block내 Block Y Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcInBlockGapY;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();

                menuItem.Header = UseEng ? "Strip Gap Calculation" : "Strip Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcStripGap;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Calculating Block X Gap within Block within Block" : "Block내 Block 내 Block X Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcInInBlockGapX;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Calculating Block Y Gap within Block within Block" : "Block내 Block 내 Block Y Gap 계산";
                menuItem.Tag = ContextMenuCommand.CalcInInBlockGapY;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);
                menuItem = new MenuItem();
            }
            #endregion
            #region 섹션에서의 Context Menu.
            else
            {
                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Set the inspection exclusion area" : "검사 제외영역 지정";
                menuItem.Tag = ContextMenuCommand.SetExceptInspectionRegion;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Remove inspection exclusion area" : "검사 제외영역 지정해제";
                menuItem.Tag = ContextMenuCommand.UnSetExceptInspectionRegion;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                //DownSet 영역 지정 및 해제
                //contextMenu.Items.Add(new Separator());

                //menuItem = new MenuItem();
                //menuItem.Header = "다운셋 영역 지정";
                //menuItem.Tag = ContextMenuCommand.SetDownSetRegion;
                //menuItem.Click += contextMenuItem_Click;
                //contextMenu.Items.Add(menuItem);

                //menuItem = new MenuItem();
                //menuItem.Header = "다운셋 영역 지정해제";
                //menuItem.Tag = ContextMenuCommand.UnSetDownSetRegion;
                //menuItem.Click += contextMenuItem_Click;
                //contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Add Inspection Item" : "검사 방법 추가";
                menuItem.Tag = ContextMenuCommand.AddInspectItem;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Add Templete" : "Templete 추가";
                menuItem.Tag = ContextMenuCommand.Templete;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = "Local Align";
                menuItem.Tag = ContextMenuCommand.LocalAlign;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Reset Guidelines" : "가이드 라인 재설정";
                menuItem.Tag = ContextMenuCommand.GuideLineSetting;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Set Tape Location" : "Tape Location 설정";
                menuItem.Tag = ContextMenuCommand.TapLocationSetting;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer1" : "뷰어1 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView11;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer2" : "뷰어2 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView12;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer3" : "뷰어3 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView21;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer4" : "뷰어4 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView22;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer5" : "뷰어5 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView31;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy ROI of Viewer6" : "뷰어6 ROI 복사";
                menuItem.Tag = ContextMenuCommand.CopyROIToView32;
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());

                // 10대 과제
                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Copy inspection parameter" : "검사 파라미터 복사";
                menuItem.Tag = ContextMenuCommand.Inspection_List_Copy;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Paste inspection parameter" : "검사 파라미터 붙여넣기";
                menuItem.Tag = ContextMenuCommand.Inspection_List_Paste;
                menuItem.Click += contextMenuItem_Click;
                contextMenu.Items.Add(menuItem);

                #region ROI 회전
                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "Rotate ROI" : "ROI 회전";
                menuItem.Tag = ContextMenuCommand.RotateROI;

                MenuItem subItem = new MenuItem();
                subItem.Header = UseEng ? "Rotate 90˚" : "90˚회전";
                subItem.Tag = ContextMenuCommand.RotateROI90;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);

                subItem = new MenuItem();
                subItem.Header = UseEng ? "Rotate 180˚" : "180˚회전";
                subItem.Tag = ContextMenuCommand.RotateROI180;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);
                contextMenu.Items.Add(menuItem);
                #endregion

                #region ROI 대칭
                menuItem = new MenuItem();
                menuItem.Header = UseEng ? "ROI symmetry" : "ROI 대칭";
                menuItem.Tag = ContextMenuCommand.SymmetryROI;

                subItem = new MenuItem();
                subItem.Header = UseEng ? "Up-down symmetry move" : "상하 대칭 이동";
                subItem.Tag = ContextMenuCommand.SymmetryROIUpDown;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);

                subItem = new MenuItem();
                subItem.Header = UseEng ? "Left-right symmetry move" : "좌우 대칭 이동";
                subItem.Tag = ContextMenuCommand.SymmetryROILeftRight;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);

                subItem = new MenuItem();
                subItem.Header = UseEng ? "Up-down symmetry copy" : "상하 대칭 복사";
                subItem.Tag = ContextMenuCommand.CopyROIUpDown;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);

                subItem = new MenuItem();
                subItem.Header = UseEng ? "Left-right symmetry copy" : "좌우 대칭 복사";
                subItem.Tag = ContextMenuCommand.CopyROILeftRight;
                subItem.Click += contextMenuItem_Click;
                menuItem.Items.Add(subItem);
                contextMenu.Items.Add(menuItem);
                #endregion
            }
            #endregion

            // 공통 메뉴
            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem();
            menuItem.Header = UseEng ? "Select all": "전체 선택";
            menuItem.Tag = ContextMenuCommand.SelectAll;
            menuItem.Click += contextMenuItem_Click;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = UseEng ? "Delete all" : "전체 삭제";
            menuItem.Tag = ContextMenuCommand.DeleteAll;
            menuItem.Click += contextMenuItem_Click;
            contextMenu.Items.Add(menuItem);
        }

        /// <summary>   Show context menu. </summary>
        /// <param name="e">    Mouse button event information. </param>
        private void ShowContextMenu(MouseButtonEventArgs e)
        {
            // Change current selection if necessary

            Point point = e.GetPosition(this);
            SetPosition = point;

            GraphicsBase o = null;

            for (int i = graphicsList.Count - 1; i >= 0; i--)
            {
                if (((GraphicsBase)graphicsList[i]).MakeHitTest(point) == 0)
                {
                    o = (GraphicsBase)graphicsList[i];
                    break;
                }
            }

            if (o != null)
            {
                if (!o.IsSelected)
                {
                    UnselectAll();
                }

                // Select clicked object
                o.IsSelected = true;
            }
            else
            {
                UnselectAll();
            }

            UpdateState();

            MenuItem item;

            /// Enable/disable menu items.
            foreach (object obj in contextMenu.Items)
            {
                item = obj as MenuItem;

                if (item != null)
                {
                    ContextMenuCommand command = (ContextMenuCommand)item.Tag;

                    switch (command)
                    {
                        case ContextMenuCommand.SetFiducialRegion:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic is GraphicsRectangle &&
                                              SelectedGraphic.RegionType == GraphicsRegionType.UnitRegion &&
                                              ((GraphicsRectangle)SelectedGraphic).IsValidRegion &&
                                              !((GraphicsRectangle)SelectedGraphic).IsFiducialRegion);
                            break;
                        case ContextMenuCommand.ShowFiducialRegion:
                            item.IsEnabled = (fiduGraphicsList.Count > 0);
                            break;
                        case ContextMenuCommand.RetrySearchRegion:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic is GraphicsRectangle &&
                                              SelectedGraphic.RegionType == GraphicsRegionType.UnitRegion);
                            break;
                        case ContextMenuCommand.UnloadFromSection:
                        case ContextMenuCommand.UnloadFromSectionBlock:
                            item.IsEnabled = CanUnloadFromSection();
                            break;
                        case ContextMenuCommand.ShowSelectedSectionGroup:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic is GraphicsRectangle &&
                                              SelectedGraphic.RegionType == GraphicsRegionType.UnitRegion &&
                                              ((GraphicsRectangle)SelectedGraphic).IsValidRegion && SelectionCount == 1);
                            break;

                        case ContextMenuCommand.CopySectionSetting:
                            item.IsEnabled = (fiduGraphicsList.Count > 0 && graphicsList.Count > 0);
                            break;
                        case ContextMenuCommand.PasteSectionSetting:
                            item.IsEnabled = (SelectionCount == 0 && SectionSettingStorage.OriginSetting.CanPasteSetting(this, Mouse.GetPosition(this)));
                            break;

                        case ContextMenuCommand.RegisterSection:
                            item.IsEnabled = CanRegisterSection();
                            break;

                        case ContextMenuCommand.UnloadAndRegisterSection:
                        case ContextMenuCommand.UnloadAndRegisterSectionBlock:
                            item.IsEnabled = CanUnloadAndRegisterSection();
                            break;

                        case ContextMenuCommand.SetDownSetRegion:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic.RegionType == GraphicsRegionType.Inspection);
                            break;
                        case ContextMenuCommand.UnSetDownSetRegion:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic.RegionType == GraphicsRegionType.DownSetRegion);
                            break;
                        case ContextMenuCommand.SetExceptInspectionRegion:
                            item.IsEnabled = (SelectedGraphic != null &&
                                              SelectedGraphic.RegionType == GraphicsRegionType.Inspection);
                            break;
                        case ContextMenuCommand.UnSetExceptInspectionRegion:
                            item.IsEnabled = (SelectedGraphic != null &&
                                              SelectedGraphic.RegionType == GraphicsRegionType.Except);
                            break;
                        case ContextMenuCommand.SelectAll:
                            item.IsEnabled = CanSelectAll;
                            break;
                        case ContextMenuCommand.UnselectAll:
                            item.IsEnabled = CanUnselectAll;
                            break;
                        case ContextMenuCommand.Delete:
                            item.IsEnabled = CanDelete;
                            break;
                        case ContextMenuCommand.DeleteAll:
                            item.IsEnabled = CanDeleteAll;
                            break;
                        case ContextMenuCommand.MoveToFront:
                            item.IsEnabled = CanMoveToFront;
                            break;
                        case ContextMenuCommand.MoveToBack:
                            item.IsEnabled = CanMoveToBack;
                            break;
                        case ContextMenuCommand.Undo:
                            item.IsEnabled = CanUndo;
                            break;
                        case ContextMenuCommand.Redo:
                            item.IsEnabled = CanRedo;
                            break;
                        case ContextMenuCommand.SerProperties:
                            item.IsEnabled = CanSetProperties;
                            break;

                        case ContextMenuCommand.CopyROIToView11:
                        case ContextMenuCommand.CopyROIToView12:
                        case ContextMenuCommand.CopyROIToView21:
                        case ContextMenuCommand.CopyROIToView22:
                        case ContextMenuCommand.CopyROIToView31:
                        case ContextMenuCommand.CopyROIToView32:
                            item.IsEnabled = (SelectedGraphic != null && SelectionCount > 0);
                            break;

                        case ContextMenuCommand.GuideLineSetting:
                            item.IsEnabled = (SelectedGraphic != null && SelectionCount == 1 && SelectedGraphic.RegionType == GraphicsRegionType.GuideLine);
                            break;
                        case ContextMenuCommand.TapLocationSetting:
                            item.IsEnabled = (SelectedGraphic == null || (SelectedGraphic != null || SelectionCount == 1 && SelectedGraphic.RegionType == GraphicsRegionType.TapeLoaction));
                            break;
                        case ContextMenuCommand.RotateROI:
                        case ContextMenuCommand.RotateROI90:
                        case ContextMenuCommand.RotateROI180:
                        case ContextMenuCommand.SymmetryROI:
                        case ContextMenuCommand.SymmetryROIUpDown:
                        case ContextMenuCommand.SymmetryROILeftRight:
                            item.IsEnabled = (SelectedGraphic != null &&
                                              ((SelectedGraphic.RegionType == GraphicsRegionType.Inspection) || (SelectedGraphic.RegionType == GraphicsRegionType.Except)) &&
                                              (SelectionCount > 0));
                            break;

                        case ContextMenuCommand.AddInspectItem:
                            item.IsEnabled = (SelectedGraphic != null && SelectionCount == 1 &&
                                              !(SelectedGraphic.RegionType == GraphicsRegionType.UnitRegion ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.OuterRegion ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.UnitAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.StripAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.LocalAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.GuideLine ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.TapeLoaction));
                            break;
                        case ContextMenuCommand.Templete:
                            item.IsEnabled = (SelectedGraphic != null && SelectionCount == 1 &&
                                              !(SelectedGraphic.RegionType == GraphicsRegionType.UnitRegion ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.OuterRegion ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.UnitAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.StripAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.LocalAlign ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.GuideLine ||
                                               SelectedGraphic.RegionType == GraphicsRegionType.TapeLoaction));
                            break;
                        case ContextMenuCommand.LocalAlign:
                            item.IsEnabled = (SelectedGraphic != null && SelectedGraphic is GraphicsBase &&
                                              ((SelectedGraphic.RegionType == GraphicsRegionType.Inspection) || (SelectedGraphic.RegionType == GraphicsRegionType.Except)) &&
                                              (SelectionCount == 1));
                            break;

                        //10대 과제
                        case ContextMenuCommand.Inspection_List_Copy:

                            int count = 0;

                            foreach (GraphicsBase rectGraphic in Selection)
                            {
                                if (rectGraphic.RegionType == GraphicsRegionType.Inspection || rectGraphic.RegionType == GraphicsRegionType.Except)
                                {
                                    count++;
                                }

                                if (count > 1) break;
                            }

                            if (count == 1)
                            {
                                item.IsEnabled = false;
                            }
                            else item.IsEnabled = false;

                            break;

                    }
                }
            }

            contextMenu.IsOpen = true;
        }

        /// <summary>
        /// Cancel currently executed operation: add new object or group selection.
        /// Called when mouse capture is lost or Esc is pressed.
        /// </summary>
        public void CancelCurrentOperation()
        {
            if (Tool == ToolType.Pointer)
            {
                if (graphicsList.Count > 0)
                {
                    if (graphicsList[graphicsList.Count - 1] is GraphicsSelectionRectangle)
                    {
                        // Delete selection rectangle if it exists
                        graphicsList.RemoveAt(graphicsList.Count - 1);
                    }
                    else
                    {
                        // Pointer tool moved or resized graphics object.
                        // Add this action to the history
                        toolPointer.AddChangeToHistory(this);
                    }
                }
            }
            else if (Tool > ToolType.Pointer && Tool < ToolType.Max)
            {
                // Delete last graphics object which is currently drawn
                if (graphicsList.Count > 0)
                {
                    if (Tool == ToolType.PolyLine)
                    {
                        ToolPolyLine toolPolyLine = tools[(int)ToolType.PolyLine] as ToolPolyLine;
                        if (toolPolyLine != null)
                        {
                            toolPolyLine.IsFirstClicked = true;
                        }

                        ChangeToolEventHandler eventRunner = ToolTypeChangeEvent;
                        if (eventRunner != null)
                        {
                            eventRunner(ToolType.Pointer);
                        }
                    }
                    graphicsList.RemoveAt(graphicsList.Count - 1);
                }
            }

            Tool = ToolType.Pointer;

            this.ReleaseMouseCapture();
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>   Add command to history. </summary>
        /// <param name="command">  The command. </param>
        internal void AddCommandToHistory(CommandBase command)
        {
            undoManager.AddCommandToHistory(command);
        }

        /// <summary>   Clear Undo history. </summary>
        public void ClearHistory()
        {
            undoManager.ClearHistory();
        }

        /// <summary>
        /// Update state of Can* dependency properties used for Edit commands. This function calls after
        /// any change in drawing canvas state, caused by user commands. Helps to keep client controls
        /// state up-to-date, in the case if Can* properties are used for binding.
        /// </summary>
        private void UpdateState()
        {
            bool hasObjects = (this.Count > 0);
            bool hasSelectedObjects = (this.SelectionCount > 0);

            CanSelectAll = hasObjects;
            CanUnselectAll = hasObjects;
            CanDelete = hasSelectedObjects;
            CanDeleteAll = hasObjects;

            CanMoveToFront = hasSelectedObjects;
            CanMoveToBack = hasSelectedObjects;

            // 필요없는 설정이므로 호출하지 않도록 한다.
            // CanSetProperties = HelperFunctions.CanApplyProperties(this);
        }
        #endregion Other Functions

        // 2014-10-15, suoow2. : Section의 기준 영역을 교체한다.
        private void ChangeFiducialRegion(string aszCaption)
        {
            GraphicsRectangle newFiducialGraphic = SelectedGraphic as GraphicsRectangle;
            if (newFiducialGraphic != null)
            {
                GraphicsRectangle oldFiducialGraphic = newFiducialGraphic.MotherROI as GraphicsRectangle;
                if (oldFiducialGraphic != null)
                {
                    oldFiducialGraphic.IsFiducialRegion = false;
                    oldFiducialGraphic.Caption = CaptionHelper.GetRegionCaption(oldFiducialGraphic);

                    foreach (GraphicsRectangle g in GraphicsRectangleList)
                    {
                        if (g.MotherROI == oldFiducialGraphic)
                        {
                            g.MotherROI = newFiducialGraphic;
                        }
                    }
                    newFiducialGraphic.IsFiducialRegion = true;
                    newFiducialGraphic.Caption = aszCaption;
                    newFiducialGraphic.MotherROI = null;
                    oldFiducialGraphic.MotherROI = newFiducialGraphic;

                    // Update FiduGraphicsList.
                    fiduGraphicsList.Remove(oldFiducialGraphic);
                    fiduGraphicsList.Add(newFiducialGraphic);
                }
            }
        }

        // 2012-02-28, suoow2 : 선택된 ROI 중에서 Section Region 초기화가 가능한 ROI가 있는지 판별한다.
        private bool CanUnloadFromSection()
        {
            bool bCanUnload = false;
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic.RegionType == GraphicsRegionType.UnitRegion && rectGraphic.IsValidRegion)
                {
                    bCanUnload = true;
                    break;
                }
            }

            return bCanUnload;
        }

        // 2012-02-28, suoow2 : Section Region으로 설정된 ROI의 상태를 초기화시킨다.
        private void UnloadFromSection(bool bBlock)
        {
            List<GraphicsRectangle> tempMotherGraphicList = new List<GraphicsRectangle>();
            List<GraphicsRectangle> tempGraphicList = new List<GraphicsRectangle>();
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic.RegionType != GraphicsRegionType.UnitRegion) // Strip Align, Outer Region은 대상에서 제외.
                {
                    rectGraphic.IsSelected = false;
                }
            }

            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                rectGraphic.OriginObjectColor = rectGraphic.ObjectColor = GraphicsColors.Purple;
                if (rectGraphic.IsFiducialRegion)
                {
                    tempMotherGraphicList.Add(rectGraphic);
                    fiduGraphicsList.Remove(rectGraphic);
                }

                rectGraphic.IsFiducialRegion = false;
                rectGraphic.IsValidRegion = false;
                rectGraphic.Caption = CaptionHelper.GetRegionCaption(rectGraphic);
                rectGraphic.MotherROI = null;

                if (rectGraphic.BlockIterationValue.Block > 1 && bBlock)
                {
                    int y = (rectGraphic.IterationValue.Row / rectGraphic.BlockIterationValue.Block);
                    for (int i = 1; i < rectGraphic.BlockIterationValue.Block; i++)
                    {
                        foreach (GraphicsRectangle rectg in GraphicsRectangleList)
                        {
                            if (rectg.RegionType == GraphicsRegionType.UnitRegion && rectg.IterationXPosition == rectGraphic.IterationXPosition && (rectGraphic.IterationYPosition + (y*i)) == rectg.IterationYPosition)
                            {
                                rectg.OriginObjectColor = rectg.ObjectColor = GraphicsColors.Purple;
                               // rectg.IsSelected = true;
                                tempGraphicList.Add(rectg);
                                if (rectg.IsFiducialRegion)
                                {
                                    tempMotherGraphicList.Add(rectg);
                                    fiduGraphicsList.Remove(rectg);
                                }

                                rectg.IsFiducialRegion = false;
                                rectg.IsValidRegion = false;
                                rectg.Caption = CaptionHelper.GetRegionCaption(rectg);
                                rectg.MotherROI = null;
                            }
                        }
                    }
                }
                
            }
            foreach (GraphicsRectangle fiduGraphic in tempGraphicList)
            {
                foreach (GraphicsRectangle g in GraphicsRectangleList)
                {
                    if (g == fiduGraphic)
                    {
                        g.IsSelected = true;
                    }
                }
            }
            // Section 해제 Unit 중 기준 영역이 포함되어 있는 경우 새로운 기준 영역을 선정한다.
            GraphicsRectangle newFiduGraphic = null;
            foreach (GraphicsRectangle fiduGraphic in tempMotherGraphicList)
            {
                foreach (GraphicsRectangle g in GraphicsRectangleList)
                {
                    if (g == fiduGraphic)
                        continue;

                    if (g.MotherROI == fiduGraphic)
                    {
                        if (newFiduGraphic == null)
                        {
                            g.MotherROI = null;
                            newFiduGraphic = g;
                            newFiduGraphic.IsFiducialRegion = true;
                            newFiduGraphic.Caption = CaptionHelper.FiducialUnitRegionCaption;

                            fiduGraphicsList.Add(g);
                        }
                        else
                        {
                            g.MotherROI = newFiduGraphic;
                        }
                    }
                }
                newFiduGraphic = null;
            }
        }

        // 2012-02-28, suoow2 : 수동으로 Section 등록이 가능한지 판별한다.
        private bool CanRegisterSection()
        {
            bool bCanRegister = false;
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic.RegionType == GraphicsRegionType.UnitRegion && 
                    !rectGraphic.IsFiducialRegion && rectGraphic.MotherROI == null)
                {
                    bCanRegister = true;
                    break;
                }
            }
            if (SelectedGraphic == null)
            {
                bCanRegister = false;
            }

            return bCanRegister;
        }

        private bool CanUnloadAndRegisterSection()
        {
            // 섹션 해제 & 등록 가능 여부.
            // 이상적인 로직은 CanUnloadFromSection() && CanRegisterSection()이나 Mother ROI 비교 부분에서 차이가 발생하여 별도의 메서드로 분리함.
            bool bCanUnload = CanUnloadFromSection();
            bool bCanRegister = false;
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic.RegionType == GraphicsRegionType.UnitRegion)
                {
                    bCanRegister = true;
                    break;
                }
            }
            if (SelectedGraphic == null)
                bCanRegister = false;

            return bCanUnload && bCanRegister;
        }

        // 2012-02-28, suoow2 : 수동으로 Section을 등록한다.
        private void RegisterSection()
        {
            Color sectionColor = GraphicsColors.GetNextColor(m_nColorIndex++);
            bool bNeedNotify = false;
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic.RegionType != GraphicsRegionType.UnitRegion) // Strip Align, Outer Region은 대상에서 제외.
                {
                    rectGraphic.IsSelected = false;
                    bNeedNotify = true;
                }
                else
                {
                    if (rectGraphic.IsFiducialRegion || rectGraphic.MotherROI != null)
                    {
                        rectGraphic.IsSelected = false; // 기준 Unit Region은 대상에서 제외.
                        bNeedNotify = true;
                    }
                }
            }

            // 새로운 섹션 등록시 최좌측상단의 영역이 기준 영역으로 잡히도록 한다.
            GraphicsRectangle fiducialRegion = null;
            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (fiducialRegion == null)
                    fiducialRegion = rectGraphic;
                else
                {
                    if (fiducialRegion.IterationYPosition >= rectGraphic.IterationYPosition)
                    {
                        if (fiducialRegion.IterationXPosition > rectGraphic.IterationXPosition)
                            fiducialRegion = rectGraphic;
                    }
                }
            }
            if (fiducialRegion != null)
            {
                fiducialRegion.IsFiducialRegion = true;
                fiducialRegion.IsValidRegion = true;
                fiducialRegion.MotherROI = null;
                fiducialRegion.Caption = CaptionHelper.FiducialUnitRegionCaption;
                fiducialRegion.OriginObjectColor = fiducialRegion.ObjectColor = sectionColor;

                fiduGraphicsList.Add(fiducialRegion);
            }

            foreach (GraphicsRectangle rectGraphic in SelectionRectangle)
            {
                if (rectGraphic == fiducialRegion)
                    continue;

                rectGraphic.OriginObjectColor = rectGraphic.ObjectColor = sectionColor;
                rectGraphic.IsValidRegion = true;
                rectGraphic.Caption = CaptionHelper.GetRegionCaption(rectGraphic);
                rectGraphic.MotherROI = fiducialRegion;
            }

            if (bNeedNotify)
            {
                NotifyConstraintEventHandler eventRunner = NotifyConstraintEvent;
                if (eventRunner != null)
                {
                    eventRunner("등록 가능한 영역을 섹션으로 등록하였습니다.");
                }
            }
        }

        private void ShowSelectedSectionGroup()
        {
            if (IsBasedCanvas)
            {
                GraphicsRectangle fiducialGraphic = SelectedGraphic as GraphicsRectangle;
                if (fiducialGraphic != null)
                {
                    if (fiducialGraphic.MotherROI != null)
                    {
                        fiducialGraphic = fiducialGraphic.MotherROI as GraphicsRectangle;
                        if (fiducialGraphic == null)
                            return;
                    }

                    fiducialGraphic.IsSelected = true;
                    foreach (GraphicsRectangle g in GraphicsRectangleList)
                    {
                        if (g.MotherROI == fiducialGraphic)
                        {
                            g.IsSelected = true;
                        }
                    }
                }
            }
        }
       
        // 10대 과제 - LocalAlign 크기 조정 작업
        public GraphicsRectangle CreateLocalAlign(GraphicsBase aGraphic, int anIndex, Rect? aRect = null)
        {
            if (aRect != null)
            {
                // return Local Align.
                GraphicsRectangle localAlign = new GraphicsRectangle(((Rect)aRect).Left, ((Rect)aRect).Top, ((Rect)aRect).Right, ((Rect)aRect).Bottom,
                                                                     LineWidth, GraphicsRegionType.LocalAlign, Colors.Red, aGraphic.ActualScale);
                localAlign.Caption = CaptionHelper.LocalAlignCaption;

                return localAlign;
            }
            else
            {
                double Left = 0.0;
                double Top = 0.0;
                double Right = 0.0;
                double Bottom = 0.0;

                int h_Size = (int)aGraphic.boundaryRect.Height / 10;
                int w_Size = (int)aGraphic.boundaryRect.Width  / 10;
                int Size = w_Size < h_Size ? w_Size : h_Size;

                int h_offest = (int)aGraphic.boundaryRect.Height / 10;
                int w_offest = (int)aGraphic.boundaryRect.Width / 10;
                int offest = w_offest < h_offest ? w_offest : h_offest;


                switch (anIndex) // Count가 그려질 Local Align의 위치를 결정한다.
                {
                    case 0: // 좌상귀
                        Left = aGraphic.boundaryRect.Left + offest;
                        Top = aGraphic.boundaryRect.Top + offest;
                        Right = Left + Size;
                        Bottom = Top + Size;

                        break;
                    case 1: // 좌하귀                      
                        Left = aGraphic.boundaryRect.Left + offest;
                        Bottom = aGraphic.boundaryRect.Bottom - offest;
                        Right = Left + Size;
                        Top = Bottom - Size;

                        break;
                    case 2: // 우하귀

                        Right = aGraphic.boundaryRect.Right - offest;
                        Bottom = aGraphic.boundaryRect.Bottom - offest;
                        Left = Right - Size;
                        Top = Bottom - Size;

                        break;
                    case 3: // 우상귀
                        Top = aGraphic.boundaryRect.Top + offest;
                        Right = aGraphic.boundaryRect.Right - offest;
                        Left = Right - Size;
                        Bottom = Top + Size;
                        break;
                }
                //Right = Left + 80; // Width is 80
                //Bottom = Top + 80; // Height is 80

                // return Local Align.
                GraphicsRectangle localAlign = new GraphicsRectangle(Left, Top, Right, Bottom, this.LineWidth, GraphicsRegionType.LocalAlign, Colors.Red, aGraphic.ActualScale);
                localAlign.Caption = CaptionHelper.LocalAlignCaption;

                return localAlign;
            }
        }

        /// <summary>   Queries if we can draw 'graphic'. </summary>
        /// <remarks>   suoow2, 2014-10-16. </remarks>
        public bool CanDraw(GraphicsBase graphic, double deltaScale = 1.0)
        {
            int nWidth = Convert.ToInt32(this.Width);
            int nHeight = Convert.ToInt32(this.Height);

            double fSideMargin = 5.0;
            if (nWidth < fSideMargin * 2 || nHeight < fSideMargin * 2) // Section의 최소 사이즈.
            {
                return false;
            }
            
            #region Try draw Local Align.
            if (graphic.LocalAligns != null)
            {
                for (int nIndex = 0; nIndex < graphic.LocalAligns.Length; nIndex++)
                {
                    if (graphic.LocalAligns[nIndex] != null)
                    {
                        if ((int)Math.Round(graphic.LocalAligns[nIndex].Right) >= nWidth)
                        {
                            double fMargin = graphic.LocalAligns[nIndex].Right - nWidth + fSideMargin;
                            if ((int)Math.Round(graphic.LocalAligns[nIndex].Right) - fMargin >= nWidth)
                            {
                                return false; // 좌표 보정 후에도 폭을 넘어서는 경우 false 값 반환.
                            }

                            graphic.LocalAligns[nIndex].Right -= fMargin;
                            if (graphic.LocalAligns[nIndex].Right < 0)
                            {
                                graphic.LocalAligns[nIndex].Right = fSideMargin;
                            }

                            graphic.LocalAligns[nIndex].Left -= fMargin;
                            if (graphic.LocalAligns[nIndex].Left < 0)
                            {
                                graphic.LocalAligns[nIndex].Left = 0;
                            }
                        }
                        if ((int)Math.Round(graphic.LocalAligns[nIndex].Bottom) >= nHeight)
                        {
                            double fMargin = graphic.LocalAligns[nIndex].Bottom - nHeight + fSideMargin;
                            if ((int)Math.Round(graphic.LocalAligns[nIndex].Bottom) - fMargin >= nHeight)
                            {
                                return false; // 좌표 보정 후에도 폭을 넘어서는 경우 false 값 반환.
                            }

                            graphic.LocalAligns[nIndex].Bottom -= fMargin;
                            if (graphic.LocalAligns[nIndex].Bottom < 0)
                            {
                                graphic.LocalAligns[nIndex].Bottom = fSideMargin;
                            }

                            graphic.LocalAligns[nIndex].Top -= fMargin;
                            if (graphic.LocalAligns[nIndex].Top < 0)
                            {
                                graphic.LocalAligns[nIndex].Top = 0;
                            }

                            graphic.LocalAligns[nIndex].CalcBoundaryRect();
                            graphic.LocalAligns[nIndex].RefreshDrawing();
                        }
                    }
                }
            }
            #endregion Try draw Local Align.

            if (graphic is GraphicsRectangleBase)
            {
                #region Try draw GraphicsRectangleBase.
                GraphicsRectangleBase rectBaseGraphic = (GraphicsRectangleBase)graphic;
                if ((int)Math.Round(rectBaseGraphic.Right - rectBaseGraphic.Left) >= nWidth ||
                    (int)Math.Round(rectBaseGraphic.Bottom - rectBaseGraphic.Top) >= nHeight)
                {
                    if (deltaScale < 1.0)
                    {
                        rectBaseGraphic.Left *= deltaScale;
                        rectBaseGraphic.Top *= deltaScale;
                        rectBaseGraphic.Right *= deltaScale;
                        rectBaseGraphic.Bottom *= deltaScale;

                        if ((int)Math.Round(rectBaseGraphic.Right - rectBaseGraphic.Left) >= nWidth ||
                            (int)Math.Round(rectBaseGraphic.Bottom - rectBaseGraphic.Top) >= nHeight)
                            return false;
                    }
                    else return false;
                }
                else
                {
                    if ((int)Math.Round(rectBaseGraphic.Right) >= nWidth)
                    {
                        double fMargin = rectBaseGraphic.Right - nWidth + fSideMargin;
                        if ((int)Math.Round(rectBaseGraphic.Right) - fMargin >= nWidth)
                        {
                            return false; // 좌표 보정 후에도 폭을 넘어서는 경우 false 값 반환.
                        }

                        rectBaseGraphic.Right -= fMargin;
                        if (rectBaseGraphic.Right < 0)
                        {
                            rectBaseGraphic.Right = fSideMargin;
                        }

                        rectBaseGraphic.Left -= fMargin;
                        if (rectBaseGraphic.Left < 0)
                        {
                            rectBaseGraphic.Left = 0;
                        }
                    }
                    if ((int)Math.Round(rectBaseGraphic.Bottom) >= nHeight)
                    {
                        double fMargin = rectBaseGraphic.Bottom - nHeight + fSideMargin;
                        if ((int)Math.Round(rectBaseGraphic.Bottom) - fMargin >= nHeight)
                        {
                            return false; // 좌표 보정 후에도 폭을 넘어서는 경우 false 값 반환.
                        }

                        rectBaseGraphic.Bottom -= fMargin;
                        if (rectBaseGraphic.Bottom < 0)
                        {
                            rectBaseGraphic.Bottom = fSideMargin;
                        }

                        rectBaseGraphic.Top -= fMargin;
                        if (rectBaseGraphic.Top < 0)
                        {
                            rectBaseGraphic.Top = 0;
                        }
                    }
                }
                rectBaseGraphic.CalcBoundaryRect();
                rectBaseGraphic.RefreshDrawing();

                #endregion Try draw GraphicsRectangleBase.
            }
            else if (graphic is GraphicsLine)
            {
                #region Try draw GraphicsLine.
                GraphicsLine lineGraphic = (GraphicsLine)graphic;
                double distanceX = Math.Abs(lineGraphic.Start.X - lineGraphic.End.X);
                double distanceY = Math.Abs(lineGraphic.Start.Y - lineGraphic.End.Y);
                double length = Math.Sqrt((distanceX * distanceX) + (distanceY + distanceY));

                if ((int)Math.Round(lineGraphic.Start.X) >= nWidth)
                    lineGraphic.Start = new Point(nWidth - fSideMargin, lineGraphic.Start.Y);

                if ((int)Math.Round(lineGraphic.Start.Y) >= nHeight)
                    lineGraphic.End = new Point(lineGraphic.Start.X, nHeight - fSideMargin);

                if ((int)Math.Round(lineGraphic.End.X) >= nWidth)
                    lineGraphic.End = new Point(nWidth - fSideMargin, lineGraphic.End.Y);

                if ((int)Math.Round(lineGraphic.End.Y) >= nHeight)
                    lineGraphic.End = new Point(lineGraphic.End.X, nHeight - fSideMargin);

                lineGraphic.CalcBoundaryRect();
                lineGraphic.RefreshDrawing();
                #endregion Try draw GraphicsLine.
            }
            else if (graphic is GraphicsPolyLine)
            {
                #region Try draw GraphicsPolyLine.
                GraphicsPolyLine polyLineGraphic = (GraphicsPolyLine)graphic;
                int points = polyLineGraphic.Points.Length;
                double farLeft = nWidth;
                double farTop = nHeight;
                double farRight = 0;
                double farBottom = 0;

                for (int index = 0; index < points; index++)
                {
                    farLeft = (farLeft < polyLineGraphic.Points[index].X) ? farLeft : polyLineGraphic.Points[index].X;
                    farTop = (farTop < polyLineGraphic.Points[index].Y) ? farTop : polyLineGraphic.Points[index].Y;
                    farRight = (farRight > polyLineGraphic.Points[index].X) ? farRight : polyLineGraphic.Points[index].X;
                    farBottom = (farBottom > polyLineGraphic.Points[index].Y) ? farBottom : polyLineGraphic.Points[index].Y;
                }

                if ((int)Math.Round(farRight - farLeft) >= nWidth || (int)Math.Round(farBottom - farTop) >= nHeight)
                {
                    if (deltaScale < 1.0)
                    {
                        for (int index = 0; index < points; index++)
                        {
                            polyLineGraphic.Points[index].X *= deltaScale;
                            polyLineGraphic.Points[index].Y *= deltaScale;
                        }

                        for (int index = 0; index < points; index++)
                        {
                            farLeft = (farLeft < polyLineGraphic.Points[index].X) ? farLeft : polyLineGraphic.Points[index].X;
                            farTop = (farTop < polyLineGraphic.Points[index].Y) ? farTop : polyLineGraphic.Points[index].Y;
                            farRight = (farRight > polyLineGraphic.Points[index].X) ? farRight : polyLineGraphic.Points[index].X;
                            farBottom = (farBottom > polyLineGraphic.Points[index].Y) ? farBottom : polyLineGraphic.Points[index].Y;
                        }

                        if ((int)Math.Round(farRight - farLeft) >= nWidth || (int)Math.Round(farBottom - farTop) >= nHeight)
                            return false;
                    }
                    else return false;
                }
                else
                {
                    for (int index = 0; index < points; index++)
                    {
                        if ((int)Math.Round(polyLineGraphic.Points[index].X) >= nWidth)
                            polyLineGraphic.Points[index].X = nWidth - fSideMargin;
                        if ((int)Math.Round(polyLineGraphic.Points[index].Y) >= nHeight)
                            polyLineGraphic.Points[index].Y = nHeight - fSideMargin;
                    }
                }
                polyLineGraphic.CalcBoundaryRect();
                polyLineGraphic.RefreshDrawing();

                #endregion Try draw GraphicsPolyLine.
            }
            return true;
        }

        public int ValidIterationValue(GraphicsRectangle aGraphicRectangle, int anPixelWidth = -1, int anPixelHeight = -1)
        {
            #region set local variables.
            if (anPixelWidth == -1)
            {
                anPixelWidth = (int)this.Width;
            }
            if (anPixelHeight == -1)
            {
                anPixelHeight = (int)this.Height;
            }
            
            int unitColumns = aGraphicRectangle.IterationValue.Column;
            int unitRows = aGraphicRectangle.IterationValue.Row;

            int blockColumns = aGraphicRectangle.BlockIterationValue.Column;
            int blockRows = aGraphicRectangle.BlockIterationValue.Row;

            double unitXPitch = aGraphicRectangle.IterationValue.XPitch;
            double unitYPitch = aGraphicRectangle.IterationValue.YPitch;

            double blockXGap = aGraphicRectangle.BlockIterationValue.XPitch;
            double blockYGap = aGraphicRectangle.BlockIterationValue.YPitch;
            #endregion

            if (aGraphicRectangle.Left + (aGraphicRectangle.Right - aGraphicRectangle.Left - unitXPitch) + (unitColumns * unitXPitch) + ((blockColumns - 1) * blockXGap) > anPixelWidth)
                return -1; // 복사 결과가 이미지의 가로 길이를 벗어나는 경우.

            if (aGraphicRectangle.Top + (aGraphicRectangle.Bottom - aGraphicRectangle.Top - unitYPitch) + (unitRows * unitYPitch) + ((blockRows - 1) * blockYGap) > anPixelHeight)
                return -2; // 복사 결과가 이미지의 세로 길이를 벗어나는 경우.

            return 0; // Success
        }

        // Overloading //
        public int ValidIterationValue(GraphicsRectangle aGraphicRectangle, int aFovY, int anResolution , double scale, int anPixelWidth = -1, int anPixelHeight = -1)
        {
            #region set local variables.
            if (anPixelWidth == -1)
            {
                anPixelWidth = (int)this.Width;
            }
            if (anPixelHeight == -1)
            {
                anPixelHeight = (int)this.Height;
            }

            int unitColumns = aGraphicRectangle.IterationValue.Column;
            int unitRows = aGraphicRectangle.IterationValue.Row;

            int blockColumns = aGraphicRectangle.BlockIterationValue.Column;
            int blockRows = aGraphicRectangle.BlockIterationValue.Row;

            double unitXPitch = aGraphicRectangle.IterationValue.XPitch;
            double unitYPitch = aGraphicRectangle.IterationValue.YPitch;

            double blockXGap = aGraphicRectangle.BlockIterationValue.XPitch;
            double blockYGap = aGraphicRectangle.BlockIterationValue.YPitch;

            double ImageHeight = (aFovY * anResolution) / 1000 * scale;
            double ActualUnitRows = ImageHeight / anResolution * 1000 / unitYPitch;

            #endregion

            if (aGraphicRectangle.Left + (aGraphicRectangle.Right - aGraphicRectangle.Left - unitXPitch) + (unitColumns * unitXPitch) + ((blockColumns - 1) * blockXGap) > anPixelWidth)
                return -1; // 복사 결과가 이미지의 가로 길이를 벗어나는 경우.

            if ((ActualUnitRows * unitYPitch) > anPixelHeight)
                return -2; // 복사 결과가 이미지의 세로 길이를 벗어나는 경우.

            return 0; // Success
        }

        public void NotifySectionSizeChanged(GraphicsRectangle aNewGraphic, double afDeltaX, double afDeltaY)
        {
            if (aNewGraphic != null)
            {
                SectionSizeChangeEventHandler eventRunner = SectionSizeChangeEvent;
                if (eventRunner != null)
                {
                    eventRunner(aNewGraphic, afDeltaX, afDeltaY);
                }
            }
        }

        public void RevertResize()
        {
            if (toolPointer != null)
            {
                toolPointer.RevertResize();
            }
        }
    }
}
