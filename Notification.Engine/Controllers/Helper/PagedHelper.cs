using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Notification.Engine
{
    /// <summary>
    /// 
    /// </summary>
    public static class PagedHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PagedResult<T>> GetPagedAsync<T>(this IQueryable<T> query,
            int page, int pageSize)where T : class
        {
            var result = new PagedResult<T>
            {
                CurrentPage = page,
                PageSize = pageSize,
                RowCount = query.Count()
            };

            var pageCount = (double)result.RowCount / pageSize;
            result.PageCount = (int)Math.Ceiling(pageCount);

            var skip = (page - 1) * pageSize;
            result.Results = await query.Skip(skip).Take(pageSize).ToListAsync();

            return result;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class PagedResultBase
    {
        /// <summary>
        /// 
        /// </summary>
        public int CurrentPage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int RowCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int FirstRowOnPage
        {

            get { return (CurrentPage - 1) * PageSize + 1; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int LastRowOnPage
        {
            get { return Math.Min(CurrentPage * PageSize, RowCount); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagedResult<T> : PagedResultBase where T : class
    {
        /// <summary>
        /// 
        /// </summary>
        public IList<T> Results { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public PagedResult()
        {
            Results = new List<T>();
        }
    }
}