using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace TFrame
{
    class BeamDimensionData : DataBinding
    {
        private BeamDimensionData()
        {

        }

        private static BeamDimensionData formData;
        private static readonly object ThreadLock = new object();
        public static BeamDimensionData Singleton
        {
            get
            {
                lock (ThreadLock)
                {
                    if (formData == null)
                    {
                        formData = new BeamDimensionData();
                    }
                    return formData;
                }
            }
        }

        private DimensionType _dimType;
        public DimensionType DimensionType
        {
            get { return _dimType; }
            set { SetPropertyValue(ref _dimType, value); }
        }

        private string _breakLineFamilyDirectory = GlobalParams.BreakLineDirectory;
        public string BreakLineFamilyDirectory
        {
            get { return _breakLineFamilyDirectory; }
            set { SetPropertyValue(ref _breakLineFamilyDirectory, value); }
        }

        private bool _OK;
        public bool OK
        {
            get { return _OK; }
            set { SetPropertyValue(ref _OK, value); }
        }
    }
}
