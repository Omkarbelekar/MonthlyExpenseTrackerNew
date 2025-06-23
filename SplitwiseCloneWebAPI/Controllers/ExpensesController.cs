using Newtonsoft.Json.Linq;
using SplitwiseCloneWebAPI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace SplitwiseCloneWebAPI.Controllers
{
    [EnableCors(origins: "https://nice-sea-044c05c1e.6.azurestaticapps.net", headers: "*", methods: "*")]
    //[EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ExpensesController : ApiController
    {
        private AppDbContext db = new AppDbContext();

        public class ExpenseSplitDto
        {
            public string Description { get; set; }
            public decimal Amount { get; set; }
            public int GroupId { get; set; }
            public int PaidByUserId { get; set; }
            public List<int> OwedByUserIds { get; set; }
        }

        [HttpPost]
        [Route("api/Expenses/Add")]
        public IHttpActionResult AddExpense(ExpenseSplitDto dto)
        {
            var expense = new Expense
            {
                Description = dto.Description,
                Amount = dto.Amount,
                GroupId = dto.GroupId,
                PaidByUserId = dto.PaidByUserId,
                Date = System.DateTime.Now
            };

            db.Expenses.Add(expense);
            db.SaveChanges();
            dto.OwedByUserIds.Add(dto.PaidByUserId);
            var splitAmount = dto.Amount / (dto.OwedByUserIds.Count);

            foreach (var userId in dto.OwedByUserIds)
            {
                db.ExpenseShares.Add(new ExpenseShare
                {
                    ExpenseId = expense.Id,
                    OwedByUserId = userId,
                    ShareAmount = splitAmount
                });
            }

            db.SaveChanges();
            return Ok("Expense added and split successfully.");
        }

        [HttpGet]
        [Route("api/Expenses/Group/{groupId}")]
        public IHttpActionResult GetExpensesByGroup(int groupId)
        {
            var expenses = db.Expenses
                .Where(e => e.GroupId == groupId)
                .Select(e => new {
                    e.Id,
                    e.Description,
                    e.Amount,
                    e.Date,
                    PaidBy = e.PaidByUserId,
                    Shares = e.ExpenseShares.Select(s => new {
                        s.OwedByUserId,
                        s.ShareAmount
                    })
                })
                .ToList();

            return Ok(expenses);
        }
        [HttpGet]
        [Route("api/Expenses/{userId}/RecentExpenses")]
        public IHttpActionResult GetRecentExpenses(int userId)
        {
            var expenses = db.Expenses
                             .Where(e => e.PaidByUserId == userId ||
                                         e.ExpenseShares.Any(s => s.OwedByUserId == userId))
                             .OrderByDescending(e => e.Date)
                             .Take(5)
                             .ToList();
            return Ok(expenses);
        }

        [HttpPost]
        [Route("api/Expenses/AddNewExpenseCategory")]
        public string AddNewExpenseCategory(JObject obj)
        {
            dynamic CatObj = obj;
            string resp = "";
            try
            {
                string CategoryName = CatObj.CategoryName;
                string AllowExpenseLimit = CatObj.AllowExpenseLimit;
                string AddedBy = CatObj.AddedBy;
                string Mode = CatObj.Mode;
                string CategoryMasterId = CatObj.CategoryMasterId;

                string str = "";
                if(Mode != "E")
                {
                    str += " insert into ExpenseCategoryMaster values (NEWID(),'" + CategoryName + "'," + AllowExpenseLimit + ",'" + AddedBy + "',GETDATE(),'N')";
                }
                else
                {
                    str += " update ExpenseCategoryMaster set  CategoryName = '" + CategoryName + "', AllowExpenseLimit=" + AllowExpenseLimit + ",AddedBy='" + AddedBy + "',AddedDt=GETDATE(),IsDeleted='N' where CategoryMasterId ='" + CategoryMasterId + "' ";
                }
                ExecuteMyQuery(str);
                resp = "true";
                return resp;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/Expenses/addCatExpense")]
        public string addCatExpense(JObject obj)
        {
            dynamic ExpCatObj = obj;
            string resp = "";
            try
            {
                string AddedBy = ExpCatObj.UserId;
                string Mode = ExpCatObj.Mode;
                string CategoryMasterId = ExpCatObj.CategoryMasterId;
                string ExpenseDate = ExpCatObj.ExpenseDate; // e.g., "2024-08-27"
                DateTime parsedDate = DateTime.ParseExact(ExpenseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string Purpose = ExpCatObj.Purpose;
                string ExpenseMasterId = ExpCatObj.ExpenseMasterId;
                double ExpenseAmt = Convert.ToDouble(ExpCatObj.ExpenseAmt);

                string str = "";
                if (Mode != "E")
                {
                    str += " insert into CategoryExpenseMaster values(NEWID(),'" + CategoryMasterId + "','" + parsedDate + "'," + ExpenseAmt + ",'" + Purpose + "','N','" + AddedBy + "',GETDATE()) ";
                }
                else
                {
                    str += " update CategoryExpenseMaster set CategoryMasterId='" + CategoryMasterId + "',Date='" + parsedDate + "',ExpenseAmount=" + ExpenseAmt + ",Purpose='" + Purpose + "',AddedBy='" + AddedBy + "',AddedDt=GETDATE() where ExpenseMasterId = '" + ExpenseMasterId + "' ";
                }
                ExecuteMyQuery(str);
                resp = "true";
                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpPost]
        [Route("api/Expenses/MonthlyExpensesByUser")]
        public IHttpActionResult MonthlyExpensesByUser(JObject obj)
        {
            try
            {
                dynamic DObj = obj;
                string UserId = DObj.UserId;
                string FromDate = DObj.FromDt;
                string ToDate = DObj.ToDt;
                DataTable DT = new DataTable();
                //string str = "SELECT CategoryMasterId, CategoryName, AllowExpenseLimit, AddedBy, 0 as ActualExpense " +
                //             "FROM ExpenseCategoryMaster WHERE IsDeleted = 'N' AND AddedBy = '" + UserId + "'";
                string str = "";
                str += " SELECT CategoryMasterId, CategoryName, AllowExpenseLimit, AddedBy,";
                str += " ISNULL((select SUM(ISNULL(ExpenseAmount,0)) from CategoryExpenseMaster ";
                str += " where CategoryMasterId=ExpenseCategoryMaster.CategoryMasterId and IsDelete = 'N' and AddedBy ='" + UserId + "'  and CONVERT(DATE, Date) Between '" + FromDate + "' and '" + ToDate + "'),0) as ActualExpense ";
                str += " FROM ExpenseCategoryMaster WHERE IsDeleted = 'N' AND AddedBy ='" + UserId + "'";
                
                DT = GetVSDataTable(str, false);
                return Ok(DT);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpPost]
        [Route("api/Expenses/getExepenseHistByCat")]
        public IHttpActionResult getExepenseHistByCat(JObject obj)
        {
            try
            {
                dynamic DObj = obj;
                string UserId = DObj.UserId;
                string categoryId = DObj.categoryId;
                string FromDate = DObj.FromDt;
                string ToDate = DObj.ToDt;
                DataTable DT = new DataTable();
                    
                string str = "";
                str += " select CONVERT(DATE, Date) as ExpDate,CategoryName,* from CategoryExpenseMaster inner join ExpenseCategoryMaster on CategoryExpenseMaster.CategoryMasterId = ExpenseCategoryMaster.CategoryMasterId where CategoryExpenseMaster.CategoryMasterId = '" + categoryId + "' and CategoryExpenseMaster.IsDelete = 'N' and CategoryExpenseMaster.AddedBy ='" + UserId + "' and CONVERT(DATE, Date) Between '" + FromDate + "' and '" + ToDate + "' ";
                DT = GetVSDataTable(str, false);
                return Ok(DT);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("api/Expenses/deleteCategory/{CategoryMasterId}")]
        public IHttpActionResult deleteCategory(string CategoryMasterId)
        {
            try
            {
                string str = " update ExpenseCategoryMaster set IsDeleted = 'Y' where CategoryMasterId = '" + CategoryMasterId + "'";
                ExecuteMyQuery(str);
                return Ok("true");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("api/Expenses/deleteExpense/{ExpenseMasterId}")]
        public IHttpActionResult deleteExpense(string ExpenseMasterId)
        {
            try
            {
                string str = " update CategoryExpenseMaster set IsDelete = 'Y' where ExpenseMasterId = '" + ExpenseMasterId + "'";
                ExecuteMyQuery(str);
                return Ok("true");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("api/Expenses/getExpenseData/{ExpenseMasterId}")]
        public DataTable getExpenseData(string ExpenseMasterId)
        {
            try
            {
                string str = " select * from  CategoryExpenseMaster where ExpenseMasterId = '" + ExpenseMasterId + "' and IsDelete = 'N'";
                DataTable DT = GetVSDataTable(str, false);
                return DT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        [HttpGet]
        [Route("api/Expenses/getCategoryById/{CategoryMasterId}")]
        public DataTable getCategoryById(string CategoryMasterId)
        {
            try
            {
                string str = " select * from  ExpenseCategoryMaster where CategoryMasterId = '" + CategoryMasterId + "' and IsDeleted = 'N'";
                DataTable DT = GetVSDataTable(str, false);
                return DT;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public object ExecuteMyQuery(string query)
        {
            SqlConnection MyConn = default(SqlConnection);
            string MyConnString = null;
            SqlCommand MyCmd = default(SqlCommand);
            bool ErrFlag = false;
            string ErrMsg = null;
            
            try
            {
                if (query == null)
                {
                    return false;
                }
                MyConnString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                MyConn = new SqlConnection(MyConnString);
                MyConn.Open();
                MyCmd = new SqlCommand();
                MyCmd.CommandType = CommandType.Text;
                MyCmd.CommandText = query;
                MyCmd.Connection = MyConn;
                MyCmd.ExecuteNonQuery();
                ErrFlag = false;
            }
            catch (Exception ex)
            {
                ErrFlag = true;
                ErrMsg = "ExecuteMyQuery: " + ex.Message;
            }
            finally
            {
                try
                {
                    if ((MyCmd != null))
                        MyCmd.Dispose();
                    if ((MyConn != null))
                        MyConn.Close();
                    if ((MyConn != null))
                        MyConn.Dispose();
                }
                catch (Exception ex)
                {
                    throw new Exception("ExecuteMyQuery()-" + ex.Message);
                }
            }
            return ErrFlag;
        }
        public System.Data.DataTable GetVSDataTable(string StrQuery, bool IsStoredPro)
        {
            SqlConnection MyConn = default(SqlConnection);
            string MyConnString = null;
            SqlCommand MyCmd = default(SqlCommand);
            string ErrMsg = null;
            DataTable dt = new DataTable();
            try
            {
                MyConnString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                MyConn = new SqlConnection(MyConnString);
                MyConn.Open();
                MyCmd = new SqlCommand();
                if (IsStoredPro == true)
                {
                    MyCmd.CommandType = CommandType.StoredProcedure;
                }
                else
                {
                    MyCmd.CommandType = CommandType.Text;
                }
                MyCmd.CommandText = StrQuery;
                MyCmd.Connection = MyConn;
                SqlDataReader Rd = MyCmd.ExecuteReader(CommandBehavior.CloseConnection);
                if (Rd.HasRows)
                {
                    dt.Load(Rd);
                }
            }
            catch (Exception ex)
            {
                ErrMsg = "GetVSDataTable: " + ex.Message;
            }
            finally
            {
                if ((MyConn != null))
                    MyConn.Close();
                if ((MyConn != null))
                    MyConn.Dispose();
            }
            return dt;
        }

    }
}