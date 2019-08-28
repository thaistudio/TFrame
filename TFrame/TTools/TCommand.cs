using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TFrame
{
    public abstract class TCommand : IExternalCommand
    {
        private string _groupTransactionName;
        private bool _startTransaction;
        protected UIDocument uiDoc;
        protected Document doc;
        protected ExternalCommandData commandData;

        public TCommand(string groupTransactionName, bool startTransaction)
        {
            _groupTransactionName = groupTransactionName;
            _startTransaction = startTransaction;
        }

        public Result Execute(ExternalCommandData extCommandData, ref string message, ElementSet elements)
        {
            Result result = Result.Failed;

            uiDoc = extCommandData.Application.ActiveUIDocument;
            doc = uiDoc.Document;
            commandData = extCommandData;
            
            // Initialize 
            TransactionManager transManager = new TransactionManager(doc);
            GlobalInit();

            try
            {
                // Start group transaction
                transManager.StartGroupTransaction(_groupTransactionName);
                if (_startTransaction) transManager.StartTransaction(_groupTransactionName);

                try
                {
                    result = MainMethod();
                }
                catch (Exception ex)
                {
                    // Show ex in Revit UI
                    message = "MainMethod: " + ex.Message + ex.StackTrace;

                    // Write to error log
                    GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
                }

                if (result == Result.Succeeded)
                {
                    // End group transaction
                    if (_startTransaction) transManager.CommitTransaction();
                    transManager.AssimilateGroupTransaction();
                }
                else
                {
                    // Roll back transaction
                    if (_startTransaction) transManager.RollbackTransaction();
                    transManager.RollbackGroupTransaction();
                }
            }
            catch (Exception ex)
            {
                // Show ex in Revit UI
                message = "Transaction: " + ex.Message + ex.StackTrace;

                // Write to error log
                GlobalParams.Errors.Add(ex.Message + ex.StackTrace);
            }

            //Write to error log
            DataTools.WriteErrors(GlobalParams.ErrorPath, GlobalParams.Errors);

            return result;
        }

        protected abstract Result MainMethod();

        void GlobalInit()
        {
            GlobalParams.Doccument = doc;
            GlobalParams.ExternalCommandData = commandData;
            GlobalParams.UIDocument = uiDoc;
        }
    }

    public class TransactionManager
    {
        private Document _doc;
        private TransactionGroup _transGroup;
        private Transaction _transaction;
        public TransactionManager(Document doc)
        {
            _doc = doc;
        }

        #region Group Transaction
        /// <summary>
        /// Start group transaction
        /// </summary>
        /// <param name="groupTransactionName"></param>
        public void StartGroupTransaction(string groupTransactionName)
        {
            if (IsGroupTransactionStarted) throw new Exception("Transaction Group has already started!");
            _transGroup = new TransactionGroup(_doc, groupTransactionName);
            _transGroup.Start();
        }

        /// <summary>
        /// Merge all inside transaction in to one Revit undo
        /// </summary>
        public void AssimilateGroupTransaction()
        {
            if (!IsGroupTransactionStarted) throw new Exception("No group transaction available!");
            _transGroup.Assimilate();
        }

        /// <summary>
        /// Rollback group transaction
        /// </summary>
        public void RollbackGroupTransaction()
        {
            if (!IsGroupTransactionStarted) throw new Exception("No group transaction available!");
            _transGroup.RollBack();
        }

        /// <summary>
        /// Check if transaction group is valid
        /// </summary>
        bool IsGroupTransactionStarted
        {
            get
            {
                if (_transGroup == null) return false;
                if (_transGroup.HasStarted()) return true;
                if (_transGroup.HasEnded()) return false;
                else return true;
            }
        }
        #endregion

        #region Transaction
        /// <summary>
        /// Start transaction
        /// </summary>
        /// <param name="groupTransactionName"></param>
        public void StartTransaction(string groupTransactionName)
        {
            if (IsTransactionStarted) throw new Exception("Transaction Group has already started!");
            _transaction = new Transaction(_doc, groupTransactionName);
            _transaction.Start();
        }

        /// <summary>
        /// Merge all inside transaction in to one Revit undo
        /// </summary>
        public void CommitTransaction()
        {
            if (!IsTransactionStarted) throw new Exception("No group transaction available!");
            _transaction.Commit();
        }

        /// <summary>
        /// Rollback group transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (!IsTransactionStarted) throw new Exception("No group transaction available!");
            _transaction.RollBack();
        }

        /// <summary>
        /// Check if transaction group is valid
        /// </summary>
        bool IsTransactionStarted
        {
            get
            {
                if (_transaction == null) return false;
                if (_transaction.HasStarted()) return true;
                if (_transaction.HasEnded()) return false;
                else return true;
            }
        }
        #endregion
    }

    public class ErrorsHandling
    {

    }
}
