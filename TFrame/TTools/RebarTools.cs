﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;

namespace TFrame
{
    class RebarTools
    {
        /// <summary>
        /// [0 right, 1 left, 2 top, 3 bot, 4 front, 5 back] 
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="beam"></param>
        /// <param name="doc"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static double GetDistanceFromRebarToEdges(Rebar rebar, Section sec, Document doc, int i)
        {
            Element beam = doc.GetElement(rebar.GetHostId());
            //Get beam right plane
            List<double> botPlane = BeamTools.GetBeamPlane(beam, doc)[i];

            //Get rebar location
            XYZ origin = GetRebarCoorAtSection(rebar, sec, doc);

            double distToBot = GeometryTools.GetDistancePointToPlane(origin, botPlane);

            return distToBot;
        }

        public static XYZ GetRebarCoorAtSection(Rebar rebar, Section sec, Document doc)
        {
            XYZ rebarCoordinate;
            List<XYZ> rebarCoordinates = new List<XYZ>();
            double tollerance = 0.1;
            // Choose the line corresponds to the section
            Element hostElem = doc.GetElement(rebar.GetHostId());
            List<double> beamEndPlane = BeamTools.GetBeamPlane(hostElem, doc)[5];
            XYZ elemNornalVector = BeamTools.GetUnitNormalVector(BeamTools.GetBeamEnds(hostElem)[0], BeamTools.GetBeamEnds(hostElem)[1]);
            XYZ centerOfSection = GeometryTools.GetPointAtDistNormalToAVector(elemNornalVector, sec.Origin, sec.ZMaxExtra, sec.Origin.Z);
            double sectionDistToBeamEnd = GeometryTools.GetDistancePointToPlane(sec.Origin, beamEndPlane);
            List<Line> lines = GetRebarLines(rebar);
            foreach (Line line in lines)
            {
                double lineEnd0DistToBeamEnd = GeometryTools.GetDistancePointToPlane(line.GetEndPoint(0), beamEndPlane);
                double lineEnd1DistToBeamEnd = GeometryTools.GetDistancePointToPlane(line.GetEndPoint(1), beamEndPlane);
                XYZ p0 = line.GetEndPoint(0);
                XYZ p1 = line.GetEndPoint(1);
                double AC = sectionDistToBeamEnd - lineEnd1DistToBeamEnd;
                if (sectionDistToBeamEnd < lineEnd0DistToBeamEnd && sectionDistToBeamEnd > lineEnd1DistToBeamEnd)
                {

                    rebarCoordinate = GeometryTools.GetRelativePointBetween2Points(p1, p0, AC);
                    rebarCoordinates.Add(rebarCoordinate);
                    break;
                }
                else if (rebarCoordinates.Count == 0)
                {
                    if ((sectionDistToBeamEnd - tollerance) < lineEnd0DistToBeamEnd && (sectionDistToBeamEnd + tollerance) > lineEnd1DistToBeamEnd)
                    {
                        rebarCoordinate = GeometryTools.GetRelativePointBetween2Points(p1, p0, AC);
                        rebarCoordinates.Add(rebarCoordinate);
                        break;
                    }
                }
                else rebarCoordinate = null;

            }
            return rebarCoordinates[0];
        }

        public static List<Line> GetRebarLines(Rebar rebar)
        {
            IList<Curve> centerCurves = rebar.GetCenterlineCurves(true, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);

            List<Line> lines = new List<Line>();
            Dictionary<Line, double> dicLines = new Dictionary<Line, double>();
            foreach (Curve centerCurve in centerCurves)
            {
                if (centerCurve is Line)
                {
                    Line line = (Line)centerCurve;
                    lines.Add(line);
                    dicLines.Add(line, line.Length);
                }
            }
            return lines;
        }


        public static double GetRebarElevation(Rebar rebar, Section section, Document doc)
        {
            if (((RebarShape)(doc.GetElement(rebar.GetShapeId()))).RebarStyle != RebarStyle.StirrupTie)
            {
                XYZ coor = GetRebarCoorAtSection(rebar, section, doc);
                double elev = coor.Z;

                return elev;
            }
            else return 0;
        }

        public static List<double> GetRebarUniqueElevations(List<Rebar> rebars, Section section, Document doc)
        {
            List<double> elevations = new List<double>();
            foreach (Rebar rebar in rebars)
            {
                double elevation = GetRebarElevation(rebar, section, doc);
                elevations.Add(elevation);
                //if (((RebarShape)(doc.GetElement(rebar.GetShapeId()))).RebarStyle != RebarStyle.StirrupTie)
                //{
                //    double elevation = GetRebarElevation(rebar, section, doc);
                //    if (!listTools.ListMemberExists(elevations, elevation)) elevations.Add(elevation);
                //}
            }
            return elevations.OrderByDescending(x => x).ToList();
        }

        public static List<double> GetRebarUniqueDistanceToRightEdge(List<Rebar> rebars, Section sec, Document doc)
        {
            List<double> distances = new List<double>();
            foreach (Rebar rebar in rebars)
            {
                double distance = RebarTools.GetDistanceFromRebarToEdges(rebar, sec, doc, 0);
                if (!ListTools.ListMemberExists(distances, distance)) distances.Add(distance);
            }
            return distances;
        }
    }
}
