using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SimpleAds.Data
{
    public class SimpleAdDb
    {
        private string _connectionString;

        public SimpleAdDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void AddSimpleAd(SimpleAd ad)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Ads (Title, Description, Name, PhoneNumber, Date) " +
                                      "VALUES (@title, @desc, @name, @phone, GETDATE()) SELECT @@Identity";
                command.Parameters.AddWithValue("@title", ad.Title);
                command.Parameters.AddWithValue("@desc", ad.Description);
                object name = ad.Name;
                if (name == null)
                {
                    name = DBNull.Value;
                }
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@phone", ad.PhoneNumber);
                connection.Open();
                int id = (int)(decimal)command.ExecuteScalar();
                ad.Id = id;
                connection.Close();
                if (ad.Images != null)
                {
                    AddImages(ad.Images, id);
                }
            }
        }

        public IEnumerable<SimpleAd> GetAds()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT a.*, i.FileName, i.Id as ImageId FROM Ads a " +
                                      "LEFT JOIN Images i " +
                                      "ON a.Id = i.AdId " +
                                      "ORDER BY a.Date DESC";
                connection.Open();
                Dictionary<int, SimpleAd> ads = new Dictionary<int, SimpleAd>();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int adId = (int)reader["Id"];
                    if (!ads.ContainsKey(adId))
                    {
                        SimpleAd ad = GetAdFromReader(reader);
                        ads.Add(adId, ad);
                    }

                    SimpleAd simpleAd = ads[adId];
                    List<Image> images = simpleAd.Images as List<Image>;
                    Image image = GetImageFromReader(reader);
                    if (image != null)
                    {
                        images.Add(image);
                    }
                }

                return ads.Values;
            }
        }

        public SimpleAd GetById(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "SELECT a.*, i.FileName, i.Id as ImageId FROM Ads a " +
                                      "LEFT JOIN Images i " +
                                      "ON a.Id = i.AdId " +
                                      "WHERE a.Id = @id";
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                SimpleAd ad = null;
                while (reader.Read())
                {
                    if (ad == null)
                    {
                        ad = GetAdFromReader(reader);
                    }
                    List<Image> images = ad.Images as List<Image>;
                    Image image = GetImageFromReader(reader);
                    if (image != null)
                    {
                        images.Add(image);
                    }
                }

                return ad;
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM Images WHERE AdId = @adId";
                command.Parameters.AddWithValue("@adId", id);
                connection.Open();
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = "DELETE FROM Ads WHERE Id = @id";
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
        }

        private SimpleAd GetAdFromReader(SqlDataReader reader)
        {
            SimpleAd ad = new SimpleAd
            {
                Name = reader.Get<string>("Name"),
                Description = reader.Get<string>("Description"),
                Date = reader.Get<DateTime>("Date"),
                PhoneNumber = reader.Get<string>("PhoneNumber"),
                Id = reader.Get<int>("Id"),
                Images = new List<Image>(),
                Title = reader.Get<string>("Title")
            };
            return ad;
        }

        private Image GetImageFromReader(SqlDataReader reader)
        {
            string fileName = reader.Get<string>("FileName");
            if (fileName == null)
            {
                return null;
            }
            return new Image
            {
                FileName = fileName,
                Id = reader.Get<int>("ImageId")
            };
        }

        private void AddImages(IEnumerable<Image> images, int adId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Images (FileName, AdId) VALUES (@name, @id)";
                connection.Open();
                foreach (Image image in images)
                {
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@name", image.FileName);
                    command.Parameters.AddWithValue("@id", adId);
                    command.ExecuteNonQuery();
                }
            }
        }



    }

    public static class ReaderExtensions
    {
        public static T Get<T>(this SqlDataReader reader, string name)
        {
            object value = reader[name];
            if (value == DBNull.Value)
            {
                return default(T);
            }

            return (T)value;
        }
    }


}