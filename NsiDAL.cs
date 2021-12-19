using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace UIModules
{
    public class NsiDAL : BaseDAL
    {
        #region OrgGetList
        public static DALResult OrgGetList(long id_user, int page, int size, ArrayList id_source, string mask)
        {
            DALResult result = new DALResult();
            PostgreDataBase db = new PostgreDataBase();
            try
            {
                string sql = "select * from nsi.org_get_list(@id_user, @page, @size, @id_source, @mask);";
                DbCommand comm = db.CreateCommand(sql);
                db.AddCommandParam(ref comm, "@id_user", id_user, DbType.Int64);
                db.AddCommandParam(ref comm, "@page", page, DbType.Int32);
                db.AddCommandParam(ref comm, "@size", size, DbType.Int32);
                db.AddCommandParam(ref comm, "@id_source", string.Join(",", id_source.ToArray()), DbType.String);
                db.AddCommandParam(ref comm, "@mask", mask, DbType.String);
                DbDataReader reader = db.CreateReader(comm);
                OrgList list = new OrgList();
                while (reader.Read())
                {
                    list.Add(new OrgEntry(reader));
                }
                result.Result = list;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Error = ex.Message;
                result.Status = StatusCode.S_DATABASE_ERROR;
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }
        #endregion

        #region OrgGetInfo
        public static DALResult OrgGetInfo(long id_user, string org_number)
        {
            DALResult result = new DALResult();
            PostgreDataBase db = new PostgreDataBase();
            try
            {
                string sql = "select * from nsi.org_get_info(@id_user, @org_number);";
                DbCommand comm = db.CreateCommand(sql);
                db.AddCommandParam(ref comm, "@id_user", id_user, DbType.Int64);
                db.AddCommandParam(ref comm, "@org_number", org_number, DbType.String);
                DbDataReader reader = db.CreateReader(comm);
                OrgEntry item = new OrgEntry();
                if (reader.Read())
                {
                    item = new OrgEntry(reader);
                }
                result.Result = item;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Error = ex.Message;
                result.Status = StatusCode.S_DATABASE_ERROR;
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }
        #endregion

        #region OrgCreate
        public static DALResult OrgCreate(long id_user, OrgEntry item)
        {
            DALResult result = new DALResult();
            PostgreDataBase db = new PostgreDataBase();
            try
            {
                string sql = "select * from nsi.org_create(@id_user, @id_source, @org_number, @inn, @kpp, @ogrn, @short_name, @full_name, @fact_address, @post_address, @contact, @timezone, @is_actual);";
                DbCommand comm = db.CreateCommand(sql);
                db.AddCommandParam(ref comm, "@id_user", id_user, DbType.Int64);
                db.AddCommandParam(ref comm, "@id_source", item.Source[0], DbType.Int32);
                db.AddCommandParam(ref comm, "@org_number", item.OrgNumber, DbType.String);
                db.AddCommandParam(ref comm, "@inn", item.Inn, DbType.String);
                db.AddCommandParam(ref comm, "@kpp", item.Kpp, DbType.String);
                db.AddCommandParam(ref comm, "@ogrn", item.Ogrn, DbType.String);
                db.AddCommandParam(ref comm, "@short_name", item.ShortName, DbType.String);
                db.AddCommandParam(ref comm, "@full_name", item.FullName, DbType.String);
                db.AddCommandParam(ref comm, "@fact_address", item.FactAddress, DbType.String);
                db.AddCommandParam(ref comm, "@post_address", item.PostAddress, DbType.String);
                db.AddCommandParam(ref comm, "@contact", item.Contact, DbType.String);
                db.AddCommandParam(ref comm, "@timezone", item.Timezone, DbType.String);
                db.AddCommandParam(ref comm, "@is_actual", item.Actual, DbType.Int32);
                int res = Convert.ToInt32(comm.ExecuteScalar());
                if (res > 0)
                {
                    result.Result = res;
                    result.Status = StatusCode.S_OK;
                }
                else
                {
                    result.HasError = true;
                    result.Status = StatusCode.S_DATABASE_ERROR;
                }
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Error = ex.Message;
                result.Status = StatusCode.S_DATABASE_ERROR;
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }
        #endregion

        #region ContractorCreate
        public static DALResult ContractorCreate(long id_user, ContractorSimpleEntry item)
        {
            DALResult result = new DALResult();
            PostgreDataBase db = new PostgreDataBase();
            try
            {
                string sql = "select * from nsi.contractor_create(@id_user, @inn, @reg_number, @short_name, @full_name, @id_district, @id_region, @data);";
                DbCommand comm = db.CreateCommand(sql);
                db.AddCommandParam(ref comm, "@id_user", id_user, DbType.Int64);
                db.AddCommandParam(ref comm, "@inn", item.Inn, DbType.String);
                db.AddCommandParam(ref comm, "@reg_number", item.RegNumber, DbType.String);
                db.AddCommandParam(ref comm, "@short_name", item.ShortName, DbType.String);
                db.AddCommandParam(ref comm, "@full_name", item.FullName, DbType.String);
                db.AddCommandParam(ref comm, "@data", item.Data, DbType.String);
                db.AddCommandParam(ref comm, "@id_district", item.DistrictId, DbType.Int32);
                db.AddCommandParam(ref comm, "@id_region", item.RegionId, DbType.Int32);
                int res = Convert.ToInt32(comm.ExecuteScalar());
                if (res > 0)
                {
                    result.Result = res;
                    result.Status = StatusCode.S_OK;
                }
                else
                {
                    result.HasError = true;
                    result.Status = StatusCode.S_DATABASE_ERROR;
                }
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.Error = ex.Message;
                result.Status = StatusCode.S_DATABASE_ERROR;
            }
            finally
            {
                db.Dispose();
            }
            return result;
        }
        #endregion
    }
}