using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NeoNovaAPI.Data;
using NeoNovaAPI.Models.DataModels;
using System.Reflection;

namespace NeoNovaAPI.Controllers.DataControllers
{
    [Authorize(Policy = "AdminOnly")]
    [ApiController]
    [Route("api/metadata")]
    public class MetadataController : ControllerBase
    {
        private readonly NeoNovaAPIDbContext _context;

        public MetadataController(NeoNovaAPIDbContext context)
        {
            _context = context;
        }

        [HttpGet("all")]
        public ActionResult<IEnumerable<TableInfo>> GetAllTablesInfo()
        {
            var tableInfos = new List<TableInfo>();

            // Reflection to get all DbSets
            var dbSetProperties = typeof(NeoNovaAPIDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

            foreach (var prop in dbSetProperties)
            {
                var tableInfo = new TableInfo();
                tableInfo.TableName = prop.Name;

                // Using reflection to find the type of entity
                var entityType = prop.PropertyType.GenericTypeArguments[0];

                // Getting column names
                tableInfo.ColumnNames = entityType.GetProperties().Select(p => p.Name).ToList();

                // Getting number of records
                var dbSet = _context.GetType().GetProperty(prop.Name).GetValue(_context) as IQueryable<object>;
                tableInfo.NumberOfRecords = dbSet.Count();

                tableInfos.Add(tableInfo);
            }

            return Ok(tableInfos);
        }
    }
}
