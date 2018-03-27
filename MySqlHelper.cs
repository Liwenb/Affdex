
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SMGExpression
{
    public static class MySqlHelper
    {
        private static readonly string connectionStr = "Server=192.168.1.85;Database=smg_data;Uid=root;Pwd =root;CharSet=utf8;";
        public static int ExecuteDataSet(string sql)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionStr))
                {
                    DataSet ds = new DataSet();
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(sql, connection);

                    int i = cmd.ExecuteNonQuery();
                    return i;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return 0;
            }

        }
        public static List<Time_seg> GetTime_seg()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionStr))
                {
                    DataTable dt = new DataTable();

                    connection.Open();
                    string sql = string.Format("select * from time_seg order by period");
                    MySqlCommand cmd = new MySqlCommand(sql, connection);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);

                    da.Fill(dt);

                    List<Time_seg> list = new List<Time_seg>();
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Time_seg time = new Time_seg();
                        time.Period = dt.Rows[i]["period"].ToString();
                        time.Test_id = int.Parse(dt.Rows[i]["Test_id"].ToString());
                        time.Period_name = dt.Rows[i]["period_name"].ToString();
                        time.State = int.Parse(dt.Rows[i]["state"].ToString());
                        list.Add(time);
                    }
                    return list;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
