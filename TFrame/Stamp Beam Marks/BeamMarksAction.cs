using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    // Definitions:
    // Group: 1 group is a row or column of beams

    public class BeamMarksAction
    {
        public double gridBeamTolerance;

        UIDocument uidoc;
        Document doc;

        BeamTools bTools = new BeamTools();

        string gridName; // Grid name
        string circumfix; // This circumfix is used when users choose to use grid name
        

        Dictionary<Element, string> beamGrids;

        public BeamMarksAction(ExternalCommandData commandData)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = uidoc.Document;
        }

        #region Obsolete method
        // This method is obsolete. I am not sure if I want to delete it now, so I will just leave it here.
        /// <summary>
        /// Use this method to stamp beam marks from settings
        /// </summary>
        /// <param name="selectedBeams"></param>
        /// <param name="settings"></param>
        public void StampBeamMarks(List<Element> selectedBeams, List<Settings> settings)
        {
            try
            {
                bool UseGridName = false;
                foreach (Settings setting in settings)
                {
                    beamGrids = new Dictionary<Element, string>();
                    List<List<Element>> sortedBeams = SortBeams(selectedBeams, setting.BeamOrientation, setting.Rule1, setting.Rule2);
                    
                    // The next 4 ifs is to set Circumfix in case users use grid name. I need to set here because the grid name is only know inside SortBeamToGroups()
                    if (setting.UseHorGridName && setting.IsHorizontalBeams) UseGridName = true; // Hor beams, when use grid name
                    if (!setting.UseHorGridName && setting.IsHorizontalBeams) UseGridName = false; // Hor beams, when not use grid name
                    if (setting.UseVerGridName && !setting.IsHorizontalBeams) UseGridName = true; // Ver beams, when use grid name
                    if (!setting.UseVerGridName && !setting.IsHorizontalBeams) UseGridName = false; // Hor beams, when not use grid name
                  
                    foreach (List<Element> group in sortedBeams)
                    {
                        if (group.Count == 0) continue;
                        int i = setting.Suffix;
                        foreach (Element beam in group)
                        {
                            if (UseGridName) circumfix = DataTools.GetInfoFromElement<string>(beam, "GridName", DisplayUnitType.DUT_UNDEFINED); //
                            else circumfix = setting.Circumfix;

                            beam.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).
                                Set(setting.Prefix + setting.Interfix + circumfix + setting.Interfix + i.ToString()); // i.e Beam-A-1
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                GlobalParams.Errors.Add(ex.Message);
            }
        }
        #endregion

        /// <summary>
        /// Use this method to get beam mark from PartSetting. Depends on the PartSetting, marks will be returned differently
        /// </summary>
        /// <param name="ps">The PartSetting that is being passed</param>
        /// <param name="i">In case users enter numbers, i will hold the int value</param>
        /// <param name="beam">The beam that is being marked</param>
        /// <returns></returns>
        public string GetMarkFromSetting(PartSetting ps, int i, Element beam)
        {
            string mark = ps.Part; // This is the value that will be put in beam's mark
           
            if (ps.IsFloor)
            {
                Dictionary<Level, string> levelEnum = GetLevelEnums();
                int levelId = ((FamilyInstance)beam).Host.Id.IntegerValue; // Get the level of the marked beam
                ps.Part = levelEnum.FirstOrDefault(x => x.Key.Id.IntegerValue == levelId).Value;
                mark = ps.Part;
            }

            if (ps.IsGridName)
            {
                ps.Part = DataTools.GetInfoFromElement<string>(beam, "GridName", DisplayUnitType.DUT_UNDEFINED);
                mark = ps.Part;
            }

            if (ps.IsNumber)
            {
                mark = i.ToString();
            }

            return mark;
        }

        /// <summary>
        /// This method stamps marks on beams. This is called advanced because it associates changes that allow naming customization
        /// </summary>
        /// <param name="selectedBeams"></param>
        /// <param name="settings"></param>
        public void StampBeamMarksAdvanced(List<Element> selectedBeams, List<Settings> settings)
        {
            List<string> parts = new List<string>();
            List<int> numbers = new List<int>();
            string mark = string.Empty; // Beam mark
            int i = 0; // The number part in beam mark
            try
            {
                foreach (Settings setting in settings)
                {
                    numbers.Clear(); // Reset from last iteration
                    List<List<Element>> sortedBeams = SortBeams(selectedBeams, setting.BeamOrientation, setting.Rule1, setting.Rule2);

                    foreach (PartSetting ps in setting.PartSettings) // Add i to numbers in case users enter numbers
                    {
                        if (ps.IsNumber)
                        {
                            i = Convert.ToInt32(ps.Part);
                            numbers.Add(i);
                        }
                        //else numbers.Add(1); // Just add dummy number so numbers and setting.PartSettings have the same count
                    }

                    foreach (List<Element> group in sortedBeams)
                    {
                        if (group.Count == 0) continue;
                        foreach (Element beam in group)
                        {
                            for (int j = 0; j < setting.PartSettings.Count; j++)
                            {
                                mark += GetMarkFromSetting(setting.PartSettings[j], numbers[j], beam);
                            }
                           
                            beam.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(mark);// i.e Beam-A-1
                            mark = string.Empty; // Reset mark value, otherwise it will be duplicated. i.e. B-A-1-B-A-1

                            for (int j = 0; j < setting.PartSettings.Count; j++) numbers[j]++; // Increment all numbers 
                        }
                        for (int j = 0; j < setting.PartSettings.Count; j++)// Reset number for the next group
                        {
                            if (setting.PartSettings[j].IsNumber) numbers[j] = Convert.ToInt32(setting.PartSettings[j].Part);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Errors", ex.Message);
                GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
            }
        }

        /// <summary>
        /// Use this method to sort beams by orientation (horizontal/vertical).
        /// Then in each orientation, rows or columns will be sorted by rule1. i.e. In horizontal direction, rows are sorted from up to down.
        /// Finally, in each row or column, beams will be sorted by rule2. i.e. In a row, beams will be sorted from right to left
        /// </summary>
        /// <param name="selectedBeams">All selected beams</param>
        /// <param name="beamOrientation">Horizontal or Vertical</param>
        /// <param name="rule1">Sort rows or columns</param>
        /// <param name="rule2">Sort beams in each row or column</param>
        public List<List<Element>> SortBeams(List<Element> selectedBeams, BeamOrientation beamOrientation, SortingRule rule1, SortingRule rule2)
        {
            // Get horizontal or vertical beams from the selectedBeams, based on beamOrientation
            List<Element> beamsByOrientation = GetBeamsByDirection(selectedBeams, beamOrientation);

            // Sort beamsInOrientation to rows and columns. Rows or columns will be called groups.
            List<List<Element>> groups = SortBeamsToGroups(beamsByOrientation, beamOrientation);

            // Sort beams in groups using rule1 and rule2
            List<List<Element>> sortedGroups = SortBeamsByRule(groups, beamOrientation, rule1, rule2);

            return sortedGroups;
        }

        /// <summary>
        /// Return a rows and cols sorted by 2 selected rules
        /// rule1: sort the rows or columns
        /// rule2: sort the beams inside of the rows or columns
        /// </summary>
        /// <param name="beamGroups"></param>
        /// <param name="beamDirection"></param>
        /// <param name="rule1"></param>
        /// <param name="rule2"></param>
        List<List<Element>> SortBeamsByRule(List<List<Element>> beamGroups, BeamOrientation beamDirection, SortingRule rule1, SortingRule rule2)
        {
            List<List<Element>> tempGroups = new List<List<Element>>(); // This is a temporary list just to hold sortedGroup. It will be sorted after and pass to sortedGroups
            List<List<Element>> sortedGroups = new List<List<Element>>(); // The sorted groups with each group sorted
            List<Element> sortedGroup; // The sorted group

            // Horizontal beams
            if (beamDirection == BeamOrientation.Horizontal)
            {
                // Go through rule2 first: sort each group
                if (rule2 == SortingRule.LeftToRight)
                {
                    foreach (List<Element> beamGroup in beamGroups)
                    {
                        sortedGroup = beamGroup.OrderBy(x => GetBeamMidPoint(x).X).ToList();
                        tempGroups.Add(sortedGroup);
                    }
                }
                if (rule2 == SortingRule.RightToLeft)
                {
                    foreach (List<Element> beamGroup in beamGroups)
                    {
                        sortedGroup = beamGroup.OrderByDescending(x => GetBeamMidPoint(x).X).ToList();
                        tempGroups.Add(sortedGroup);
                    }
                }

                // Go through rule 1: sort all groups
                if (rule1 == SortingRule.UpDown) sortedGroups = tempGroups.OrderByDescending(x => GetBeamMidPoint(x.FirstOrDefault()).Y).ToList();
                if (rule1 == SortingRule.DownUp) sortedGroups = tempGroups.OrderBy(x => GetBeamMidPoint(x.FirstOrDefault()).Y).ToList();
            }

            // Vertical beams
            if (beamDirection == BeamOrientation.Vertical)
            {
                // Go through rule2 first: sort each group
                if (rule2 == SortingRule.UpDown)
                {
                    foreach (List<Element> beamGroup in beamGroups)
                    {
                        sortedGroup = beamGroup.OrderByDescending(x => GetBeamMidPoint(x).Y).ToList();
                        tempGroups.Add(sortedGroup);
                    }
                }
                if (rule2 == SortingRule.DownUp)
                {
                    foreach (List<Element> beamGroup in beamGroups)
                    {
                        sortedGroup = beamGroup.OrderBy(x => GetBeamMidPoint(x).Y).ToList();
                        tempGroups.Add(sortedGroup);
                    }
                }

                // Go through rule 1: sort all groups
                if (rule1 == SortingRule.LeftToRight) sortedGroups = tempGroups.OrderBy(x => GetBeamMidPoint(x.FirstOrDefault()).X).ToList();
                if (rule1 == SortingRule.RightToLeft) sortedGroups = tempGroups.OrderByDescending(x => GetBeamMidPoint(x.FirstOrDefault()).X).ToList();
            }
            return sortedGroups;
        }

        /// <summary>
        /// Use this method to get only horizontal or vertical beams from a list of beams
        /// </summary>
        /// <param name="beams"></param>
        /// <returns></returns>
        List<Element> GetBeamsByDirection(List<Element> beams, BeamOrientation beamDirection)
        {
            List<Element> horBeams = new List<Element>();
            List<Element> verBeams = new List<Element>();
            foreach (Element beam in beams)
            {
                if (GeometryTools.GetRotation(beam, true) < Math.PI / 4) horBeams.Add(beam);
                if (GeometryTools.GetRotation(beam, true) > Math.PI / 4) verBeams.Add(beam);
            }
            if (beamDirection == BeamOrientation.Horizontal) return horBeams;
            else return verBeams;
        }

        /// <summary>
        /// Use this method to sort beams to groups. Groups mean rows or columns
        /// </summary>
        /// <param name="beamsInDirection">Horizontal or vertical beams</param>
        List<List<Element>> SortBeamsToGroups(List<Element> beamsInDirection, BeamOrientation beamDirection)
        {
            

            List<List<Element>> groups = new List<List<Element>>(); // Groups of rows or columns
            List<Element> group; // A row or a column of beams

            // Get grids based on beam direction
            List<Grid> gridsInDirection = new List<Grid>();
            if (beamDirection == BeamOrientation.Horizontal)
            {
                gridsInDirection = GridTools.GetGridsByDirection(doc, GridOrientation.Horizontal);
            }
            if (beamDirection == BeamOrientation.Vertical)
            {
                gridsInDirection = GridTools.GetGridsByDirection(doc, GridOrientation.Vertical);
            }

            foreach (Grid grid in gridsInDirection)
            {
                // Get grid max and min points
                XYZ maxPoint = GridTools.GetCorrectMinMaxPoints(grid)[0];
                XYZ minPoint = GridTools.GetCorrectMinMaxPoints(grid)[1];

                // For each grid, create a row or a column
                group = new List<Element>();

                foreach (Element beam in beamsInDirection)
                {
                    // Get beam mid point and calculate distance to the grid
                    XYZ beamMidPoint = GetBeamMidPoint(beam);
                    double distance = GeometryTools.GetDistancePointToLine(beamMidPoint, maxPoint, minPoint);

                    // Grid to beam tolerance = half of beam's width
                    gridBeamTolerance = BeamTools.GetBeamDims(beam)[0];

                    // If distance < gridBeamTolerance, beam belongs to that grid
                    if (distance < gridBeamTolerance)
                    {
                        group.Add(beam);

                        // Add grid name to beam data
                        DataTools.AddInfoToElement(beam, grid.Name, "GridName", UnitType.UT_Undefined, DisplayUnitType.DUT_UNDEFINED);
                    }
                }

                groups.Add(group);
            }
            return groups;
        }

        XYZ GetBeamMidPoint(Element e)
        {
            XYZ midPoint = new XYZ(); 
            if (e != null)
            {
                XYZ p0 = BeamTools.GetBeamEnds(e)[0];
                XYZ p1 = BeamTools.GetBeamEnds(e)[1];

                midPoint = GeometryTools.GetMidPoint(p0, p1);

            }
            return midPoint;
        }

        public string GetAGridName(Document doc, BeamOrientation orientation)
        {
            GridOrientation gridOrientation;
            if (orientation == BeamOrientation.Horizontal) gridOrientation = GridOrientation.Horizontal;
            else gridOrientation = GridOrientation.Vertical;
            string gridName = GridTools.GetGridsByDirection(doc, gridOrientation).FirstOrDefault().Name;
            return gridName;
        }

        public string GetFloorName(List<Element> beams)
        {
            FamilyInstance fi = (FamilyInstance)(beams.FirstOrDefault());
            ElementId level = fi.Host.Id;
            Dictionary<Level, string> dict = GetLevelEnums();
            string levelName = dict.FirstOrDefault(x => x.Key.Id == level).Value;
            return levelName;
        }

        /// <summary>
        /// Use this method to number all levels in the document. i.e. Level 1 - 1, Level 2 - 2
        /// </summary>
        /// <returns></returns>
        Dictionary<Level, string> GetLevelEnums()
        {
            List<string> levelNames = new List<string>();
            List<Level> levels = new List<Level>();
            Dictionary<Level, string> levelEnum = new Dictionary<Level, string>();
            List<Element> elems = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).WhereElementIsNotElementType().ToList();
            foreach (Element elem in elems)
            {
                Level level = (Level)elem;
                levels.Add(level);
            }

            int i = 1;
            foreach (Level level in levels.OrderBy(x => x.Elevation))
            {
                levelEnum.Add(level, i.ToString());
                i++;
            }
            return levelEnum;
        }
    }

    public enum BeamOrientation
    {
        Horizontal,
        Vertical
    }

    public enum SortingRule
    {
        LeftToRight,
        RightToLeft,
        UpDown,
        DownUp
    }
}
