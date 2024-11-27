
//using IMSWebApi.Data;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace IMSWebApi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ReportsController : ControllerBase
//    {
//        private readonly InventoryDBaseContext _context;

//        // Injecting the DbContext via constructor for dependency injection
//        public ReportsController(InventoryDBaseContext context)
//        {
//            _context = context;
//        }

//        // GET api/reports/inventoryreport
//        [HttpGet("inventoryreport")]
//        public async Task<ActionResult> GetReport()
//        {
//            try
//            {
//                // LINQ query to get SupplierId, Supplier Name, ProductId, StockLevel, ReorderLevel, and Quantity from related tables
//                var reportData = await (from supplier in _context.Suppliers
//                                        join product in _context.Products on supplier.SupplierId equals product.SupplierId
//                                        join order in _context.Orders on product.ProductId equals order.ProductId
//                                        select new
//                                        {
//                                            supplier.SupplierId,
//                                            SupplierName = supplier.Name,
//                                            product.ProductId,
//                                            StockLevel = product.StockLevel,
//                                            ReorderLevel = product.ReorderLevel,
//                                            Quantity = order.Quantity
//                                        }).ToListAsync();

//                // If no data is returned, return a specific message
//                if (reportData == null || reportData.Count == 0)
//                {
//                    return NotFound("No report data available.");
//                }

//                // Return the result as JSON
//                return Ok(reportData);
//            }
//            catch (Exception ex)
//            {
//                // Log the exception for debugging
//                // In a production environment, you could use a logging framework like Serilog or NLog
//                Console.WriteLine($"Error: {ex.Message}");
//                return StatusCode(500, $"Internal server error: {ex.Message}");
//            }
//        }
//    }
//}

using IMSWebApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IMSWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly InventoryDBaseContext _context;

        // Injecting the DbContext via constructor for dependency injection
        public ReportsController(InventoryDBaseContext context)
        {
            _context = context;
        }

        // Injecting the DbContext via constructor for dependency injection

        // GET api/reports/inventoryreport
        [HttpGet("stocklevelreport")]
        public async Task<ActionResult> GetStockLevelReport()
        {
            try
            {
                // Step 1: Perform a left join between Suppliers and Products
                var supplierProducts = await (from supplier in _context.Suppliers
                                              join product in _context.Products
                                              on supplier.SupplierId equals product.SupplierId into supplierProductGroup
                                              from product in supplierProductGroup.DefaultIfEmpty()
                                              select new
                                              {
                                                  SupplierId = supplier.SupplierId,
                                                  SupplierName = supplier.Name,
                                                  ProductId = product != null ? product.ProductId : 0, // Use 0 for missing ProductId
                                                  ProductName = product != null ? product.Name : "N/A", // Use "N/A" for missing ProductName
                                                  StockLevel = product != null ? product.StockLevel : 0, // Use 0 for missing StockLevel
                                                  ReorderLevel = product != null ? product.ReorderLevel : 0 // Use 0 for missing ReorderLevel
                                              }).ToListAsync();

                // Step 2: Fetch order quantities grouped by ProductId
                var orderQuantities = await _context.Orders
                    .GroupBy(o => o.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        Quantity = g.Sum(o => o.Quantity)
                    })
                    .ToListAsync();

                // Step 3: Merge supplier-products data with order quantities
                var stockLevelReport = supplierProducts.GroupJoin(
                    orderQuantities,
                    sp => sp.ProductId,
                    oq => oq.ProductId,
                    (sp, oqGroup) => new
                    {
                        sp.SupplierId,
                        sp.SupplierName,
                        sp.ProductId,
                        sp.ProductName,
                        sp.StockLevel,
                        sp.ReorderLevel,
                        Quantity = oqGroup.Any() ? oqGroup.Sum(oq => oq.Quantity) : 0 // Use 0 if no orders
                    }).ToList();

                // Step 4: Return the report or a "NotFound" message if empty
                if (!stockLevelReport.Any())
                {
                    return NotFound("No inventory report data available.");
                }

                return Ok(stockLevelReport);
            }
            catch (Exception ex)
            {
                // Log and return error details
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/reports/inventoryreport
        //[HttpGet("inventoryreport")]
        //public async Task<ActionResult> GetReport()
        //{
        //    try
        //    {
        //        // LINQ query to get SupplierId, Supplier Name, ProductId, StockLevel, ReorderLevel, and Quantity from related tables
        //        var reportData = await (from supplier in _context.Suppliers
        //                                join product in _context.Products on supplier.SupplierId equals product.SupplierId
        //                                join order in _context.Orders on product.ProductId equals order.ProductId
        //                                select new
        //                                {
        //                                    supplier.SupplierId,
        //                                    SupplierName = supplier.Name,
        //                                    product.ProductId,
        //                                    StockLevel = product.StockLevel,
        //                                    ReorderLevel = product.ReorderLevel,
        //                                    Quantity = order.Quantity
        //                                }).ToListAsync();

        //        // If no data is returned, return a specific message
        //        if (reportData == null || reportData.Count == 0)
        //        {
        //            return NotFound("No report data available.");
        //        }

        //        // Return the result as JSON
        //        return Ok(reportData);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception for debugging
        //        // In a production environment, you could use a logging framework like Serilog or NLog
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}



        //[HttpGet("salesreport")]
        //public async Task<ActionResult> GetSalesReport()
        //{
        //    try
        //    {
        //        // Step 1: Aggregate orders in the database (this remains an IQueryable)
        //        var salesReport = await _context.Orders
        //            .GroupBy(o => o.ProductId) // Group orders by ProductId
        //            .Select(g => new
        //            {
        //                ProductId = g.Key,
        //                TotalQuantitySold = g.Sum(o => o.Quantity),
        //                TotalRevenue = g.Sum(o => (decimal)o.Quantity * o.Product.Price)
        //            })
        //            .ToListAsync(); // Materialize the result as a list of aggregated sales data

        //        // Step 2: Fetch the product details separately from the database
        //        var products = await _context.Products.ToListAsync();

        //        // Step 3: Join the sales report data with the product details in memory
        //        var result = salesReport
        //            .Join(products,
        //                  report => report.ProductId,
        //                  product => product.ProductId,
        //                  (report, product) => new
        //                  {
        //                      product.ProductId,
        //                      product.Name,
        //                      report.TotalQuantitySold,
        //                      report.TotalRevenue
        //                  })
        //            .ToList(); // Perform the final in-memory join and materialize the result

        //        // Check if the report is empty and return NotFound if necessary
        //        if (result == null || !result.Any())
        //        {
        //            return NotFound("No sales data available.");
        //        }

        //        // Return the sales report
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception to the console (can be replaced with logging service)
        //        Console.WriteLine($"Error: {ex.Message}");

        //        // Return internal server error status with the exception message
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

        [HttpGet("salesreport")]
        public async Task<ActionResult> GetSalesReport()
        {
            try
            {
                // Step 1: Perform a left join between Suppliers and Products
                var supplierProducts = await (from supplier in _context.Suppliers
                                              join product in _context.Products on supplier.SupplierId equals product.SupplierId into productGroup
                                              from product in productGroup.DefaultIfEmpty()
                                              select new
                                              {
                                                  SupplierId = supplier.SupplierId,
                                                  SupplierName = supplier.Name,
                                                  ProductId = product != null ? product.ProductId : 0, // Handle products that don't exist
                                                  ProductName = product != null ? product.Name : null
                                              }).ToListAsync();

                // Step 2: Fetch order quantities grouped by ProductId
                var productSales = await _context.Orders
                    .GroupBy(o => o.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalQuantitySold = g.Sum(o => o.Quantity)
                    })
                    .ToListAsync();

                // Step 3: Merge the supplierProducts data with productSales
                var salesReport = supplierProducts
                    .GroupJoin(
                        productSales,
                        sp => sp.ProductId,
                        ps => ps.ProductId,
                        (sp, psGroup) => new
                        {
                            sp.SupplierId,
                            sp.SupplierName,
                            sp.ProductId,
                            sp.ProductName,
                            TotalQuantitySold = psGroup.Any() ? psGroup.First().TotalQuantitySold : 0
                        })
                    .ToList();

                // Step 4: Check if the sales report is empty
                if (!salesReport.Any())
                {
                    return NotFound("No sales data available.");
                }

                // Step 5: Return the result as JSON
                return Ok(salesReport);
            }
            catch (Exception ex)
            {
                // Log and return the exception details
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        //[HttpGet("supplier-performance")]
        //public async Task<ActionResult> GetSupplierPerformanceReport()
        //{
        //    try
        //    {
        //        // Step 1: Get total products supplied by each supplier
        //        var supplierPerformance = await _context.Suppliers
        //            .Select(s => new
        //            {
        //                s.SupplierId,                  // Supplier ID
        //                s.Name,                         // Supplier Name
        //                TotalProductsSupplied = s.Products.Count() // Count the products supplied by the supplier
        //            })
        //            .ToListAsync();

        //        // Step 2: Find the supplier with the maximum products supplied
        //        var maxSupplier = supplierPerformance.OrderByDescending(sp => sp.TotalProductsSupplied)
        //                                              .FirstOrDefault();

        //        // Step 3: Add a flag to indicate which supplier has supplied the most products
        //        var result = supplierPerformance.Select(s => new
        //        {
        //            s.SupplierId,
        //            s.Name,
        //            s.TotalProductsSupplied,
        //            IsTopSupplier = s.SupplierId == maxSupplier?.SupplierId // Flag to indicate top supplier
        //        }).ToList();

        //        // Check if no data is found
        //        if (result == null || !result.Any())
        //        {
        //            return NotFound("No supplier performance data available.");
        //        }

        //        // Return the result as JSON
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any exceptions and return a 500 Internal Server Error
        //        Console.WriteLine($"Error: {ex.Message}");
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        [HttpGet("supplierperformance")]
        public async Task<ActionResult> GetSupplierPerformance()
        {
            try
            {
                // Step 1: Perform the join between Suppliers, Products, and Orders
                var supplierSalesData = await (from supplier in _context.Suppliers
                                               join product in _context.Products on supplier.SupplierId equals product.SupplierId
                                               join order in _context.Orders on product.ProductId equals order.ProductId
                                               group new { product, order } by new
                                               {
                                                   supplier.SupplierId,
                                                   supplier.Name
                                               } into supplierGroup
                                               select new
                                               {
                                                   SupplierId = supplierGroup.Key.SupplierId,
                                                   SupplierName = supplierGroup.Key.Name,
                                                   TotalRevenue = supplierGroup.Sum(g => g.order.Quantity * g.product.Price)
                                               }).ToListAsync();

                // Step 2: Ensure all suppliers are included, even those with no sales
                var allSuppliers = await _context.Suppliers
                    .Select(s => new
                    {
                        s.SupplierId,
                        SupplierName = s.Name,
                        TotalRevenue = 0m // Default revenue for suppliers with no sales
                    }).ToListAsync();

                // Step 3: Merge the sales data with all suppliers to include those without sales
                var supplierPerformance = allSuppliers.GroupJoin(
                    supplierSalesData,
                    s => s.SupplierId,
                    sd => sd.SupplierId,
                    (s, sdGroup) => new
                    {
                        s.SupplierId,
                        s.SupplierName,
                        TotalRevenue = sdGroup.Any() ? sdGroup.First().TotalRevenue : s.TotalRevenue
                    }).ToList();

                // Step 4: Check if the supplier performance report is empty
                if (!supplierPerformance.Any())
                {
                    return NotFound("No supplier performance data available.");
                }

                // Step 5: Return the result as JSON
                return Ok(supplierPerformance);
            }
            catch (Exception ex)
            {
                // Log and return the exception details
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
    
