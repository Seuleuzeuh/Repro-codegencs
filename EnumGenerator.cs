using CodegenCS;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace ReproCodegenCS
{
    public class EnumGenerator
    {
        private class EnumDescription
        {
            public int Id { get; set; }
            public string DescriptionKey { get; set; }
            public string Libelle { get; set; }
        }

        public const string CONNECTION_STRING = "Server=tcp:test-repro.database.windows.net,1433;Initial Catalog=testcodegen;Persist Security Info=False;User ID=drizin;Password=CodegenCS3@NET;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        public void Main(ICodegenContext codegenContext)
        {
            List<EnumDescription> enumDescriptions = new List<EnumDescription>();
            using (SqlConnection sqlConnection = new SqlConnection(CONNECTION_STRING))
            {
                sqlConnection.Open();
                using (SqlCommand sqlCommand = new SqlCommand("SELECT id, descriptionKey, libelle FROM GeneratedEnum", sqlConnection))
                {
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            enumDescriptions.Add(new EnumDescription()
                            {
                                Id = reader.GetInt32(0),
                                DescriptionKey = reader.GetString(1),
                                Libelle = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            var myFirstEnumFile = codegenContext["MyFirstEnum.cs"];
            myFirstEnumFile.Write($$"""
                    namespace ReproCodegenCS
                    {
                        [DataContract(Namespace="http://MyNameSpaces.Enums")]
                        public enum MyFirstEnumValues : int
                        {
                            using System.Runtime.Serialization;
                            using System.Collections.Generic;
                            using System.Linq;

                            {{() => GetProperties(myFirstEnumFile, enumDescriptions)}}
                        }
                    }
                    """);
        }

        private void GetProperties(ICodegenOutputFile file, List<EnumDescription> enumDescriptions)
        {
            int i = 1;
            foreach (var e in enumDescriptions)
            {
                bool isLast = i == enumDescriptions.Count;
                file.WriteLine("[EnumMember]");

                file.Write($"{e.DescriptionKey} = {e.Id}");

                if (!isLast)
                {
                    file.Write(",");
                }

                file.WriteLine();
                i++;
            }
        }
    }
}
