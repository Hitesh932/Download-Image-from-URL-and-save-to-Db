using Image_Downloads.Entity;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;

namespace ImageDownloader
{
    class Program
    {
       
        public void Download()
        {
            string SKU_No, Image_Url;
            string conStr = ConfigurationManager.AppSettings["ADOConStr"];
            SqlConnection _SqlConnection = new SqlConnection(conStr);
            string query = "select SKU_No,Image_Url from Product_Image where SKU_No is not null";
            
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = _SqlConnection;
            cmd.CommandText = query;
            _SqlConnection.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.HasRows)
            {
                foreach (var item in dr)
                {
                    SKU_No = (string)dr["SKU_No"];
                    Image_Url = (string)dr["Image_Url"];
                    string filename = string.Format(@"{0}.png", SKU_No);
                    string saveLocation = @"C:\ITPlusPoint\product\" + filename;
                    byte[] imageBytes;
                    HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(Image_Url);
                    WebResponse imageResponse = imageRequest.GetResponse();

                    Stream responseStream = imageResponse.GetResponseStream();

                    using (BinaryReader br = new BinaryReader(responseStream))
                    {
                        imageBytes = br.ReadBytes(500000);
                        br.Close();
                    }
                    responseStream.Close();
                    imageResponse.Close();

                    FileStream fs = new FileStream(saveLocation, FileMode.Create);
                    BinaryWriter bw = new BinaryWriter(fs);
                    Console.WriteLine("image saved");
                    
                   try
                   {
                        bw.Write(imageBytes);
                        Save(filename,SKU_No);
                    }
                    finally
                    {
                        fs.Close();
                        bw.Close();

                    }
                   
                }
            }
        }
        public void Save(string filename,string sku)
        {
            string conStr = ConfigurationManager.AppSettings["ADOConStr"];
            SqlConnection _SqlConnection = new SqlConnection(conStr);
            string query2 = " update[dbo].[Product_Image] set Product_image =  @filename where SKU_No = @sku_no ";
           
            using (SqlCommand cmd2 = new SqlCommand(query2, _SqlConnection))
            {
                cmd2.Parameters.Add("@filename", SqlDbType.VarChar, 50).Value = filename;
                cmd2.Parameters.Add("@sku_no", SqlDbType.NVarChar,255).Value = sku;
                _SqlConnection.Open();
                cmd2.ExecuteNonQuery();
                _SqlConnection.Close();
            }           
        }
        static void Main(string[] args)
        {
          Program oProgram = new Program();
          oProgram.Download();
        }
            
    }
}