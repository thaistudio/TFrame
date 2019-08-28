using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace TFrame
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    class TestCommand : TCommand
    {
        public TestCommand() : base("Test Command", true)
        {

        }

        protected override Result MainMethod()
        {
            Selection sel = uiDoc.Selection;
            ElementId id = sel.GetElementIds().FirstOrDefault();
            Element e = doc.GetElement(id);

            ScheduleSheetInstance ins = (ScheduleSheetInstance)e;
            var l = ins.GetSubelements();

            ElementId scheduleId = ins.ScheduleId;
            ViewSchedule masterSchedule = (ViewSchedule)(doc.GetElement(scheduleId));
            TableData tableData = masterSchedule.GetTableData();

            TableSectionData tsd1 = tableData.GetSectionData(SectionType.Body);
            TableSectionData tsd2 = tableData.GetSectionData(SectionType.Footer);
            TableSectionData tsd3 = tableData.GetSectionData(SectionType.Header);
            TableSectionData tsd4 = tableData.GetSectionData(SectionType.Summary);
            TableSectionData tsd5 = tableData.GetSectionData(SectionType.None);

            ScheduleDefinition scheDef = masterSchedule.Definition;
            IList<ScheduleFieldId> fieldIds = scheDef.GetFieldOrder();
            foreach (ScheduleFieldId fieldId in fieldIds)
            {
                ScheduleField scheField = scheDef.GetField(fieldId);
            }

            return Result.Succeeded;
        }
    }
}
