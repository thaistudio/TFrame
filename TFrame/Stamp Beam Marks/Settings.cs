using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace TFrame.Stamp_Beam_Marks
{
    /// <summary>
    /// This class holds settings from the UI and transfer them to command class
    /// </summary>
    public class Settings
    {
        public Settings(BeamMarks bm)
        {
            Document doc = bm.doc;
            BeamMarksAction bmAction = new BeamMarksAction(bm._commandData);
            HorGridName = bmAction.GetAGridName(doc, BeamOrientation.Horizontal);
            VerGridName = bmAction.GetAGridName(doc, BeamOrientation.Vertical);
            LevelName = bmAction.GetFloorName(bm.selBeams);
        }

        public string SettingName { get; set; }
        public BeamOrientation BeamOrientation { get; set; }
        public SortingRule Rule1 { get; set; }
        public SortingRule Rule2 { get; set; }
        public int Suffix { get; set; }
        public string Prefix { get; set; }
        public string Circumfix { get; set; }
        public string Interfix { get; set; }
        public bool UseHorGridName { get; set; }
        public bool UseVerGridName { get; set; }
        public string HorGridName { get; private set; }
        public string VerGridName { get; private set; }
        public string LevelName { get; private set; }
        public bool IsHorizontalBeams { get; set; }

        public List<PartSetting> PartSettings { get; set; }
    }

    public class PartSetting
    {
        public string Part { get; set; }
        public bool IsText { get; set; }
        public bool IsFloor { get; set; }
        public bool IsGridName { get; set; }
        public bool IsNumber { get; set; }
        public bool IsHorizontal { get; set; }
        public int Index { get; set; }
    }

    public enum NamingParts
    {
        Text,
        Floor,
        GridName,
        Number
    }
}
