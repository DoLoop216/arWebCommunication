using FirebirdSql.Data.FirebirdClient;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace arWebCommunication
{
    class Program
    {
        private static string cs = "User=SYSDBA;Password=m;Database=c:\\poslovanje\\baze\\2020~\\firma2020.fdb;DataSource=localhost;Port=3050;Dialect=3;Charset=NONE;Role=;Connection lifetime=15;Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType =0; ";
        private static string cs1 = "Server=mysql5021.site4now.net; Database=db_a54331_crossin; Uid=a54331_crossin; Pwd=crossing123; Pooling=false;";

        static void Main(string[] args)
        {
            Console.WriteLine("===============arDev===============");
            while(true)
            {
                Console.WriteLine("Akcija: ");
                string akc = Console.ReadLine().ToLower();

                switch(akc)
                {
                    case "ar1":
                        Akc1();
                        break;
                    case "q":
                    case "exit":
                    case "e":
                        return;
                    default:
                        Console.WriteLine("Nepoznata akcija!");
                        break;
                }
            }
        }

        static void Akc1()
        {
            try
            {
                List<int> rob = new List<int>();
                List<Tuple<int, double>> fin = new List<Tuple<int, double>>();

                using (MySqlConnection con = new MySqlConnection(cs1))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("SELECT REDIRECT FROM ROBA", con))
                    {
                        MySqlDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            try
                            {
                                int t = Convert.ToInt32(dr[0]);
                                Console.WriteLine("WEB SELECT: " + t.ToString());
                                if (!rob.Contains(t))
                                    rob.Add(Convert.ToInt32(dr[0]));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
                using (FbConnection con = new FbConnection(cs))
                {
                    con.Open();
                    using (FbCommand cmd = new FbCommand("SELECT ROBAID, MAX(PRODAJNACENA) FROM ROBAUMAGACINU WHERE MAGACINID IN (26, 66, 96, 110, 116, 143) GROUP BY ROBAID", con))
                    {
                        FbDataReader dr = cmd.ExecuteReader();

                        while (dr.Read())
                        {
                            try
                            {
                                int rid = Convert.ToInt32(dr[0]);
                                double cen = Convert.ToDouble(dr[1]) / 1.2;

                                if (rob.Contains(rid))
                                {
                                    Console.WriteLine("Komecijalno add: " + rid + " sa cenom: " + cen);
                                    fin.Add(new Tuple<int, double>(rid, cen));
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }

                using (MySqlConnection con = new MySqlConnection(cs1))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("UPDATE ROBA SET PRODAJNACENA = @PC WHERE REDIRECT = @RID", con))
                    {
                        cmd.Parameters.Add("@PC", MySqlDbType.Double);
                        cmd.Parameters.Add("@RID", MySqlDbType.Int32);

                        foreach (Tuple<int, double> f in fin)
                        {
                            try
                            {
                                cmd.Parameters["@PC"].Value = f.Item2;
                                cmd.Parameters["@RID"].Value = f.Item1;

                                Console.WriteLine("WEB Update: " + f.Item1 + " sa cenom od " + f.Item2);

                                cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }
            catch (FbException fex)
            {
                Console.WriteLine(fex.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("Gotovo!");
        }
    }
}
