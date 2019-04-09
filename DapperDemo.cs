using Model.Library;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DAL
{
    class DapperDemo
    {
        public int InsertMaterial(Material material)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@ID", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);
            parameters.AddDynamicParams(material);

            string insert_sql = "INSERT INTO [dbo].[Material] ([Content]) VALUES (@Content);SELECT @ID = SCOPE_IDENTITY();";
            using (SqlConnection connection = new SqlConnection(ConnectionConfig.SqlConnectionStr))
            {
                connection.Execute(insert_sql, parameters);
                int id = parameters.Get<int>("@ID");
                return id;
            }
        }

        public void EditMaterial(Material material)
        {
            string update_sql = "UPDATE [dbo].[Material] SET [Content] = @Content WHERE [Id] = @Id";
            using (SqlConnection connection = new SqlConnection(ConnectionConfig.SqlConnectionStr))
            {
                connection.Execute(update_sql, material);
            }
        }

        public List<Material> SearchMaterial(string key, int start, int length, out int total, out int filtered)
        {
            string select_sql = @"WITH    DataSource
                                              AS ( SELECT   [Id] ,
                                                            [Content] ,
                                                            ROW_NUMBER() OVER ( ORDER BY [Id] DESC ) AS RN
                                                   FROM     [dbo].[Material](NOLOCK)
                                                   WHERE    [Content] LIKE N'%' + @Key + N'%'
                                                 )
                                        SELECT  [Id] ,
                                                [Content]
                                        FROM    DataSource
                                        WHERE   RN >= @Start AND RN < @End;";
            using (SqlConnection connection = new SqlConnection(ConnectionConfig.SqlConnectionStr))
            {
                List<Material> list = connection.Query<Material>(select_sql, new { Key = key, Start = start + 1, End = start + 1 + length }).ToList();

                total = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM [dbo].[Material](NOLOCK)");
                filtered = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM [dbo].[Material](NOLOCK) WHERE [Content] LIKE N'%' + @Key + N'%'", new { Key = key });
                return list;
            }
        }
        public Material SearchMaterial(int id)
        {
            string select_sql = @"SELECT [Id]
                                      ,[Content]
                                  FROM [dbo].[Material](NOLOCK)
                                  WHERE [Id] = @Id";
            using (SqlConnection connection = new SqlConnection(ConnectionConfig.SqlConnectionStr))
            {
                Material material = connection.QueryFirstOrDefault<Material>(select_sql, new { Id = id });

                return material;
            }
        }
    }
}
