// http://www.codeproject.com/KB/WPF/WPF_DrawTools.aspx
// Author : Alex Fr
// This article, along with any associated source code and files, is licensed under The Code Project Open License (CPOL)

using System;
using System.Xml.Serialization;
using System.Windows;
using System.Windows.Media;

// Commented by suoow2.

namespace Common.Drawing
{
    /// <summary>
    /// Helper class used for XML serialization. Contains array of SerializedGraphicsBase instances.
    /// </summary>
    [XmlRoot("Teaching Data")]
    [XmlInclude(typeof(PropertiesGraphicsTapeLoaction))]
    public class SerializationHelper
    {
        private PropertiesGraphicsBase[] graphics;

        #region Additional Properties.
        [XmlElement(ElementName = "ModelName")]
        public string ModelName
        {
            get;
            set;
        }

        [XmlElement(ElementName = "SectionName")]
        public string SectionName
        {
            get;
            set;
        }

        [XmlElement(ElementName = "RegionWidth")]
        public int RegionWidth
        {
            get;
            set;
        }

        [XmlElement(ElementName = "RegionHeight")]
        public int RegionHeight
        {
            get;
            set;
        }
        #endregion

        public SerializationHelper()
        {
            // XML로의 저장을 위해 기본 생성자가 필요하다.
        }

        // Serialization 수행.
        public SerializationHelper(VisualCollection collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            graphics = new PropertiesGraphicsBase[collection.Count];

            int nIndex = 0;
            PropertiesGraphicsBase serializedObject;
            foreach (GraphicsBase g in collection)
            {
                if (g is GraphicsSelectionRectangle || g is GraphicsLine)
                    continue;

                if (g.RegionType != GraphicsRegionType.LocalAlign)
                {
                    serializedObject = g.CreateSerializedObject();
                    if (g.LocalAligns != null)
                    {
                        serializedObject.LocalAligns = new Point[Definitions.MAX_LOCAL_ALIGN_COUNT];
                        for (int i = 0; i < serializedObject.LocalAligns.Length; i++)
                        {
                            if (g.LocalAligns[i] != null)
                            {
                                serializedObject.LocalAligns[i] = new Point(
                                    g.LocalAligns[i].Left + (g.LocalAligns[i].Right - g.LocalAligns[i].Left) / 2.0,
                                    g.LocalAligns[i].Top + (g.LocalAligns[i].Bottom - g.LocalAligns[i].Top) / 2.0
                                    );
                            }
                            else
                            {
                                serializedObject.LocalAligns[i] = new Point(-1, -1);
                            }
                        }
                    }
                    graphics[nIndex++] = serializedObject;
                }
            }
        }

        // 추가되는 PropertiesXXX 마다 XmlArrayItem으로 등록해 주어야 한다.
        [XmlArrayItem(typeof(PropertiesGraphicsEllipse), ElementName = "Ellipse"),
         XmlArrayItem(typeof(PropertiesGraphicsLine), ElementName = "Line"),
         XmlArrayItem(typeof(PropertiesGraphicsPolyLine), ElementName = "PolyLine"),
         XmlArrayItem(typeof(PropertiesGraphicsRectangle), ElementName = "Rectangle"),
         XmlArrayItem(typeof(PropertiesGraphicsSkeletonLine), ElementName = "SkeletonLine"),
         XmlArrayItem(typeof(PropertiesGraphicsGuideLine), ElementName = "GuideLine"),
         XmlArrayItem(typeof(PropertiesGraphicsTapeLoaction), ElementName = "TapeLocation")]
        public PropertiesGraphicsBase[] Graphics
        {
            get { return graphics; }
            set { graphics = value; }
        }
    }
}
