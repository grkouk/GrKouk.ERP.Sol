using System.Collections.Generic;

namespace GrKouk.Web.ERP.Helpers;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

public static class FilterEval
{
    // Reusable helpers for sorting and list filters specific to queries in this project
    public static (DateTime? FromDate, DateTime? ToDate) GetDateRange(string dateRange, DateTime? fromCustomFilterDate, DateTime? toCustomFilterDate)
    {
        if (string.IsNullOrEmpty(dateRange))
        {
            return (null, null);
        }
        else if (dateRange == "CUSTOM")
        {
            if (fromCustomFilterDate.HasValue && toCustomFilterDate.HasValue)
            {
                return (fromCustomFilterDate.Value, toCustomFilterDate.Value);
            }
            else
            {
                throw new ArgumentException("Custom Period filter dates are missing");
            }
        }
        else
        {
            var dfDates = DateFilter.GetDateFilterDates(dateRange); // Assuming DateFilter exists
            return (dfDates.FromDate, dfDates.ToDate);
        }
    }

    public static IQueryable<T> ApplyCompanyFilter<T>(
        IQueryable<T> query,
        string companyFilter,
        Expression<Func<T, int>> companyIdSelector)
    {
        if (int.TryParse(companyFilter, out var companyId) && companyId > 0)
        {
            var parameter = companyIdSelector.Parameters[0];
            var equalsExpression = Expression.Equal(companyIdSelector.Body, Expression.Constant(companyId));
            var lambda = Expression.Lambda<Func<T, bool>>(equalsExpression, parameter);
            return query.Where(lambda);
        }
        return query;
    }

    public static IQueryable<T> ApplySearchFilterV1<T>(
        IQueryable<T> query,
        string searchFilter,
        Expression<Func<T, string>> nameSelector,
        Expression<Func<T, string>> codeSelector,
        Expression<Func<T, string>> refCodeSelector)
    {
        if (!string.IsNullOrEmpty(searchFilter))
        {
            var parameter = nameSelector.Parameters[0];
            var nameContains = Expression.Call(nameSelector.Body, "Contains", null, Expression.Constant(searchFilter));
            var codeContains = Expression.Call(codeSelector.Body, "Contains", null, Expression.Constant(searchFilter));
            var refCodeContains = Expression.Call(refCodeSelector.Body, "Contains", null, Expression.Constant(searchFilter));
            var orExpression = Expression.OrElse(nameContains, Expression.OrElse(codeContains, refCodeContains));
            var lambda = Expression.Lambda<Func<T, bool>>(orExpression, parameter);
            return query.Where(lambda);
        }
        return query;
    }

    public static IQueryable<T> ApplySearchFilter<T>(
        IQueryable<T> query,
        string searchFilter,
        params Expression<Func<T, string>>[] selectors)
    {
        if (string.IsNullOrEmpty(searchFilter) || selectors.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "t");
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        var containsExpressions = new List<Expression>();

        foreach (var selector in selectors)
        {
            var replacer = new ParameterReplacer(parameter);
            var updatedMember = replacer.Visit(selector.Body);
            var containsExpr = Expression.Call(updatedMember, containsMethod, Expression.Constant(searchFilter));
            containsExpressions.Add(containsExpr);
        }

        if (containsExpressions.Any())
        {
            var combined = containsExpressions.Aggregate(Expression.OrElse);
            var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return query.Where(lambda);
        }

        return query;
    }
    public static IQueryable<T> ApplyPeriodDateFilter<T>(
        IQueryable<T> query,
        DateTime? fromDate,
        DateTime? toDate,
        Expression<Func<T, DateTime>> transDateSelector)
    {
        if (fromDate.HasValue && toDate.HasValue)
        {
            var parameter = transDateSelector.Parameters[0];
            var fromCondition = Expression.GreaterThanOrEqual(transDateSelector.Body, Expression.Constant(fromDate.Value));
            var toCondition = Expression.LessThanOrEqual(transDateSelector.Body, Expression.Constant(toDate.Value));
            var andExpression = Expression.AndAlso(fromCondition, toCondition);
            var lambda = Expression.Lambda<Func<T, bool>>(andExpression, parameter);
            return query.Where(lambda);
        }
        return query;
    }

    public static IQueryable<T> ApplyBeforePeriodDateFilter<T>(
        IQueryable<T> query,
        DateTime? fromDate,
        Expression<Func<T, DateTime>> transDateSelector)
    {
        if (fromDate.HasValue)
        {
            var parameter = transDateSelector.Parameters[0];
            var beforeCondition = Expression.LessThan(transDateSelector.Body, Expression.Constant(fromDate.Value));
            var lambda = Expression.Lambda<Func<T, bool>>(beforeCondition, parameter);
            return query.Where(lambda);
        }
        return query.Where(t => false); // Empty query if no fromDate
    }
    public static IQueryable<T> ApplyCompanyListFilter<T>(
        IQueryable<T> query,
        List<int> firmIds,
        int allCompaniesId,
        Expression<Func<T, int>> companyIdSelector)
    {
        if (firmIds != null && firmIds.Count > 0 && !firmIds.Contains(allCompaniesId))
        {
            var parameter = companyIdSelector.Parameters[0];
            var companyIdProperty = companyIdSelector.Body;
            var firmIdsConstant = Expression.Constant(firmIds);
            var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
            var containsExpression = Expression.Call(firmIdsConstant, containsMethod, companyIdProperty);
            var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
            return query.Where(lambda);
        }
        return query;
    }

    // New: Apply int list filter from JSON with a specific 'all' sentinel id (e.g., 0)
    public static IQueryable<T> ApplyIntListFilterFromJson<T>(
        IQueryable<T> query,
        string jsonList,
        int allId,
        Expression<Func<T, int>> idSelector)
    {
        if (string.IsNullOrWhiteSpace(jsonList)) return query;
        List<int> ids;
        try
        {
            ids = JsonSerializer.Deserialize<List<int>>(jsonList) ?? new List<int>();
        }
        catch
        {
            return query; // ignore invalid JSON
        }
        if (ids.Count == 0 || ids.Contains(allId)) return query;

        var parameter = idSelector.Parameters[0];
        var property = idSelector.Body;
        var idsConst = Expression.Constant(ids);
        var containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });
        var containsExpression = Expression.Call(idsConst, containsMethod, property);
        var lambda = Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        return query.Where(lambda);
    }

    // New: Sorting for TransactorTransaction
    public static IQueryable<GrKouk.Erp.Domain.Shared.TransactorTransaction> ApplyTransactorTransSorting(
        IQueryable<GrKouk.Erp.Domain.Shared.TransactorTransaction> query,
        string sortData)
    {
        if (string.IsNullOrWhiteSpace(sortData)) return query;
        switch (sortData.ToLower())
        {
            case "datesort:asc":
                return query.OrderBy(p => p.TransDate);
            case "datesort:desc":
                return query.OrderByDescending(p => p.TransDate);
            case "transactornamesort:asc":
                return query.OrderBy(p => p.Transactor.Name);
            case "transactornamesort:desc":
                return query.OrderByDescending(p => p.Transactor.Name);
            case "seriescodesort:asc":
                return query.OrderBy(p => p.TransTransactorDocSeries.Code);
            case "seriescodesort:desc":
                return query.OrderByDescending(p => p.TransTransactorDocSeries.Code);
            case "companycodesort:asc":
                return query.OrderBy(p => p.Company.Code);
            case "companycodesort:desc":
                return query.OrderByDescending(p => p.Company.Code);
            case "sectioncodesort:asc":
                return query.OrderBy(p => p.Section.Code);
            case "sectioncodesort:desc":
                return query.OrderByDescending(p => p.Section.Code);
            case "refnumbersort:asc":
                return query.OrderBy(p => p.TransRefCode);
            case "refnumbersort:desc":
                return query.OrderByDescending(p => p.TransRefCode);
            default:
                return query;
        }
    }
    
    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return _parameter;
        }
    }
}