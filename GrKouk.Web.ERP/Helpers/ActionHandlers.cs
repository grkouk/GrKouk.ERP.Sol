using System;
using GrKouk.Erp.Definitions;
using GrKouk.Erp.Domain.CashFlow;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Erp.Domain.Shared;

namespace GrKouk.Web.ERP.Helpers
{
    public static class ActionHandlers
    {
        /// <summary>
        /// Manipulates item Trans based on material nature
        /// </summary>
        /// <param name="itemNature"></param>
        /// <param name="itemTrans"></param>
        /// <param name="itemTransDef"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ItemNatureHandler(WarehouseItemNatureEnum itemNature, WarehouseTransaction itemTrans,
            TransWarehouseDef itemTransDef)
        {
            switch (itemNature)
            {
                case WarehouseItemNatureEnum.WarehouseItemNatureUndefined:
                    throw new ArgumentOutOfRangeException();
                case WarehouseItemNatureEnum.WarehouseItemNatureMaterial:
                    itemTrans.InventoryAction = itemTransDef.MaterialInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.MaterialInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                case WarehouseItemNatureEnum.WarehouseItemNatureService:
                    itemTrans.InventoryAction = itemTransDef.ServiceInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.ServiceInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                case WarehouseItemNatureEnum.WarehouseItemNatureExpense:
                    itemTrans.InventoryAction = itemTransDef.ExpenseInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.ExpenseInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                case WarehouseItemNatureEnum.WarehouseItemNatureIncome:
                    itemTrans.InventoryAction = itemTransDef.IncomeInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.IncomeInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                case WarehouseItemNatureEnum.WarehouseItemNatureFixedAsset:
                    itemTrans.InventoryAction = itemTransDef.FixedAssetInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.FixedAssetInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                case WarehouseItemNatureEnum.WarehouseItemNatureRawMaterial:
                    itemTrans.InventoryAction = itemTransDef.RawMaterialInventoryAction;
                    itemTrans.InventoryValueAction = itemTransDef.RawMaterialInventoryValueAction;
                    itemTrans.InvoicedVolumeAction = itemTransDef.MaterialInvoicedVolumeAction;
                    itemTrans.InvoicedValueAction = itemTransDef.MaterialInvoicedValueAction;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Manipulates warehouse item transaction object based on the inventory action
        /// </summary>
        /// <param name="inventoryAction"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <param name="itemTrans"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ItemInventoryActionHandler(InventoryActionEnum inventoryAction, double q1, double q2,
            WarehouseTransaction itemTrans)
        {
            switch (inventoryAction)
            {
                case InventoryActionEnum.InventoryActionEnumNoChange:
                    itemTrans.TransactionType =
                        WarehouseTransactionTypeEnum.WarehouseTransactionTypeIgnore;
                    itemTrans.TransQ1 = 0;
                    itemTrans.TransQ2 = 0;
                    break;
                case InventoryActionEnum.InventoryActionEnumImport:

                    itemTrans.TransactionType =
                        WarehouseTransactionTypeEnum.WarehouseTransactionTypeImport;
                    itemTrans.Quontity1 = q1;
                    itemTrans.Quontity2 = q2;
                    itemTrans.TransQ1 = (decimal)itemTrans.Quontity1;
                    itemTrans.TransQ2 = (decimal)itemTrans.Quontity2;
                    break;
                case InventoryActionEnum.InventoryActionEnumExport:

                    itemTrans.TransactionType =
                        WarehouseTransactionTypeEnum.WarehouseTransactionTypeExport;
                    itemTrans.Quontity1 = q1;
                    itemTrans.Quontity2 = q2;
                    itemTrans.TransQ1 = (decimal)itemTrans.Quontity1;
                    itemTrans.TransQ2 = (decimal)itemTrans.Quontity2;
                    break;
                case InventoryActionEnum.InventoryActionEnumNegativeImport:

                    itemTrans.TransactionType =
                        WarehouseTransactionTypeEnum.WarehouseTransactionTypeImport;
                    itemTrans.Quontity1 = q1;
                    itemTrans.Quontity2 = q2;
                    itemTrans.TransQ1 = (decimal)itemTrans.Quontity1 * -1;
                    itemTrans.TransQ2 = (decimal)itemTrans.Quontity2 * -1;
                    break;
                case InventoryActionEnum.InventoryActionEnumNegativeExport:

                    itemTrans.TransactionType =
                        WarehouseTransactionTypeEnum.WarehouseTransactionTypeExport;
                    itemTrans.Quontity1 = q1;
                    itemTrans.Quontity2 = q2;
                    itemTrans.TransQ1 = (decimal)itemTrans.Quontity1 * -1;
                    itemTrans.TransQ2 = (decimal)itemTrans.Quontity2 * -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Manipulates warehouse item transaction object based on the inventory value action
        /// </summary>
        /// <param name="inventoryValueAction"></param>
        /// <param name="itemTrans"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ItemInventoryValueActionHandler(InventoryValueActionEnum inventoryValueAction,
            WarehouseTransaction itemTrans)
        {
            switch (inventoryValueAction)
            {
                case InventoryValueActionEnum.InventoryValueActionEnumNoChange:
                    itemTrans.TransNetAmount = 0;
                    itemTrans.TransFpaAmount = 0;
                    itemTrans.TransDiscountAmount = 0;
                    break;
                case InventoryValueActionEnum.InventoryValueActionEnumIncrease:
                    itemTrans.TransNetAmount = itemTrans.AmountNet;
                    itemTrans.TransFpaAmount = itemTrans.AmountFpa;
                    itemTrans.TransDiscountAmount = itemTrans.AmountDiscount;
                    break;
                case InventoryValueActionEnum.InventoryValueActionEnumDecrease:
                    itemTrans.TransNetAmount = itemTrans.AmountNet;
                    itemTrans.TransFpaAmount = itemTrans.AmountFpa;
                    itemTrans.TransDiscountAmount = itemTrans.AmountDiscount;
                    break;
                case InventoryValueActionEnum.InventoryValueActionEnumNegativeIncrease:
                    itemTrans.TransNetAmount = itemTrans.AmountNet * -1;
                    itemTrans.TransFpaAmount = itemTrans.AmountFpa * -1;
                    itemTrans.TransDiscountAmount = itemTrans.AmountDiscount * -1;

                    break;
                case InventoryValueActionEnum.InventoryValueActionEnumNegativeDecrease:
                    itemTrans.TransNetAmount = itemTrans.AmountNet * -1;
                    itemTrans.TransFpaAmount = itemTrans.AmountFpa * -1;
                    itemTrans.TransDiscountAmount = itemTrans.AmountDiscount * -1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// Manipulates Transactor transaction object based on cfaAction 
        /// </summary>
        /// <param name="finAction"></param>
        /// <param name="transaction"></param>
        public static void TransactorFinAction(FinActionsEnum finAction, TransactorTransaction transaction)
        {
            switch (finAction)
            {
                case FinActionsEnum.FinActionsEnumNoChange:
                    transaction.FinancialAction = FinActionsEnum.FinActionsEnumNoChange;
                    transaction.TransDiscountAmount = 0;
                    transaction.TransFpaAmount = 0;
                    transaction.TransNetAmount = 0;
                    break;
                case FinActionsEnum.FinActionsEnumDebit:
                    transaction.FinancialAction = FinActionsEnum.FinActionsEnumDebit;
                    transaction.TransDiscountAmount = transaction.AmountDiscount;
                    transaction.TransFpaAmount = transaction.AmountFpa;
                    transaction.TransNetAmount = transaction.AmountNet;
                    break;
                case FinActionsEnum.FinActionsEnumCredit:
                    transaction.FinancialAction = FinActionsEnum.FinActionsEnumCredit;
                    transaction.TransDiscountAmount = transaction.AmountDiscount;
                    transaction.TransFpaAmount = transaction.AmountFpa;
                    transaction.TransNetAmount = transaction.AmountNet;
                    break;
                case FinActionsEnum.FinActionsEnumNegativeDebit:
                    transaction.FinancialAction = FinActionsEnum.FinActionsEnumNegativeDebit;
                    transaction.TransDiscountAmount = transaction.AmountDiscount * -1;
                    transaction.TransFpaAmount = transaction.AmountFpa * -1;
                    transaction.TransNetAmount = transaction.AmountNet * -1;
                    break;
                case FinActionsEnum.FinActionsEnumNegativeCredit:
                    transaction.FinancialAction = FinActionsEnum.FinActionsEnumNegativeCredit;
                    transaction.TransDiscountAmount = transaction.AmountDiscount * -1;
                    transaction.TransFpaAmount = transaction.AmountFpa * -1;
                    transaction.TransNetAmount = transaction.AmountNet * -1;
                    break;
                default:
                    break;
            }
        }

         public static void CashFlowFinAction(CashFlowAccountActionsEnum cfaAction, CashFlowAccountTransaction transaction)
        {
            switch (cfaAction)
            {
                case CashFlowAccountActionsEnum.CfaActionNoChange:
                    transaction.CfaAction = CashFlowAccountActionsEnum.CfaActionNoChange;
                   
                    transaction.TransAmount = 0;
                    break;
                case CashFlowAccountActionsEnum.CfaActionDeposit:
                    transaction.CfaAction = CashFlowAccountActionsEnum.CfaActionDeposit;
                    transaction.TransAmount = transaction.Amount;
                    break;
                case CashFlowAccountActionsEnum.CfaActionWithdraw:
                    transaction.CfaAction = CashFlowAccountActionsEnum.CfaActionWithdraw;
                    transaction.TransAmount = transaction.Amount;
                    break;
                case CashFlowAccountActionsEnum.CfaActionNegativeDeposit:
                    transaction.CfaAction = CashFlowAccountActionsEnum.CfaActionNegativeDeposit;
                    transaction.TransAmount = transaction.Amount * -1;
                    break;
                case CashFlowAccountActionsEnum.CfaActionNegativeWithdraw:
                    transaction.CfaAction = CashFlowAccountActionsEnum.CfaActionNegativeWithdraw;
                    transaction.TransAmount = transaction.Amount * -1;
                    break;
                default:
                    break;
            }
        }
    }
}