using System.Reflection;

namespace IISLogsManager.Database
{

#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.

    [AttributeUsage(AttributeTargets.Property)]
    public class DB_ElementName(string elementNameArg, string elementTypeArg, string elementDescription) : Attribute
    {
        private readonly string elementName = elementNameArg;
        private readonly string elementType = elementTypeArg;
        private readonly string elementDescription = elementDescription;

        public string ElementName
        {
            get { return elementName; }
        }
        public string ElementType
        {
            get { return elementType; }
        }
        public string ElementDescription
        {
            get { return elementDescription; }
        }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class DB_TableName : Attribute
    {
        private readonly string tableName;
        private readonly string primaryKeyDefinition;
        private readonly string primaryKeyName;
        private readonly string primaryKeyInitialValue;

        public DB_TableName(string tableNameArg, string primaryKeyDefinitionArg, string primaryKeyInitialValueArg = "")
        {
            tableName = tableNameArg;
            primaryKeyDefinition = primaryKeyDefinitionArg;
            primaryKeyName = primaryKeyDefinition.Split('(')[1];
            primaryKeyName = primaryKeyName.Split(')')[0];
            primaryKeyInitialValue = primaryKeyInitialValueArg;
        }

        // Define TableName property.
        // This is a read-only attribute.

        public virtual string TableName
        {
            get { return tableName; }
        }

        public virtual string PrimaryKeyDefinition
        {
            get { return primaryKeyDefinition; }
        }
        public virtual string PrimaryKeyName
        {
            get { return primaryKeyName; }
        }
        public virtual string PrimaryKeyInitialValue
        {
            get { return primaryKeyInitialValue; }
        }


        public static List<PropertyInfo> GetTableProperties(Type theType)
        {
            //$"CREATE TABLE tblname (ID int NOT NULL AUTO_INCREMENT, key_name VARCHAR(255) NOT NULL, key_value VARCHAR(255) NOT NULL, PRIMARY KEY(ID))";

            List<PropertyInfo> retProps = [];
            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return retProps;
            }

            PropertyInfo[] props = theType.GetProperties();
            foreach (PropertyInfo prop in props)
            {

                DB_ElementName elementNameAttribute = (DB_ElementName)GetCustomAttribute(prop, typeof(DB_ElementName));
                if (elementNameAttribute != null)
                {
                    retProps.Add(prop);
                }
            }

            return retProps;
        }

        public static string GetTableCreationSqlCommand(Type theType)
        {
            //$"CREATE TABLE {ConnectionParameters.DBPrefix}wptkit_key_codes (ID int NOT NULL AUTO_INCREMENT, key_name VARCHAR(255) NOT NULL, key_value VARCHAR(255) NOT NULL, PRIMARY KEY(ID))";

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;


            string sqlCommand = $"CREATE TABLE {tableName} ( ";

            PropertyInfo[] props = theType.GetProperties();
            foreach (PropertyInfo prop in props)
            {

                DB_ElementName elementNameAttribute = (DB_ElementName)GetCustomAttribute(prop, typeof(DB_ElementName));
                if (elementNameAttribute != null)
                {
                    sqlCommand += elementNameAttribute.ElementName + " " + elementNameAttribute.ElementType + " , ";
                }
            }
            string primaryKeyDefinition = myAttribute.PrimaryKeyDefinition;
            sqlCommand += primaryKeyDefinition + " )";
            if (myAttribute.PrimaryKeyInitialValue != "")
            {
                sqlCommand += $" AUTO_INCREMENT = {myAttribute.PrimaryKeyInitialValue} ";
            }
            sqlCommand += ";";
            return sqlCommand;
        }

        public static string GetTableDeletionSqlCommand(Type theType)
        {
            //$"CREATE TABLE {ConnectionParameters.DBPrefix}wptkit_key_codes (ID int NOT NULL AUTO_INCREMENT, key_name VARCHAR(255) NOT NULL, key_value VARCHAR(255) NOT NULL, PRIMARY KEY(ID))";

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;


            string sqlCommand = $"DROP TABLE IF EXISTS {tableName} ;";

            return sqlCommand;
        }

        public static string GetTableReadAllSqlCommand(Type theType, string whereClauseArg)
        {

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;
            string sqlColumnsList = "";

            PropertyInfo[] props = theType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DB_ElementName elementNameAttribute = (DB_ElementName)GetCustomAttribute(prop, typeof(DB_ElementName));
                if (elementNameAttribute != null)
                {
                    sqlColumnsList += elementNameAttribute.ElementName + " , ";
                }
            }
            sqlColumnsList = sqlColumnsList[..^3];
            string sqlRequest = $"SELECT {sqlColumnsList} FROM {tableName} {whereClauseArg} ;";
            return sqlRequest;
        }

        public static string GetTableDeleteRowSqlCommand(Type theType, int rowID)
        {
            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }
            string tableName = myAttribute.TableName;
            string sqlRequest = $"DELETE  FROM {tableName}  WHERE {myAttribute.PrimaryKeyName} = '{rowID}' ;";
            return sqlRequest;
        }

        public static string GetTableDeleteAllRowSqlCommand(Type theType)
        {

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;
            string sqlRequest = $"DELETE FROM  {tableName} ";

            return sqlRequest;
        }

        public static string GetTablInsertRowSqlCommand(object theObject)
        {
            Type theType = theObject.GetType();

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;
            string sqlRequest = $"INSERT INTO  {tableName} ";

            string sqlColumnsList = " ( ";
            string sqlValuesList = " ( ";

            PropertyInfo[] props = theType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DB_ElementName elementNameAttribute = (DB_ElementName)GetCustomAttribute(prop, typeof(DB_ElementName));
                if (elementNameAttribute != null)
                {
                    sqlColumnsList += elementNameAttribute.ElementName + " , ";
                    if (prop.PropertyType == typeof(double))
                    {
                        // On the server decimal is dot . mnot colon ,
                        sqlValuesList += "'" + prop.GetValue(theObject, null)?.ToString().Replace(",", ".").Trim() + "' , ";
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        DateTime objVal = (DateTime)(prop.GetValue(theObject, null) ?? DateTime.MinValue);
                        string formatForMySql = objVal.ToString("yyyy-MM-dd HH:mm:ss");
                        sqlValuesList += "'" + formatForMySql + "' , ";
                    }
                    else if (prop.PropertyType == typeof(TimeOnly))
                    {
                        TimeOnly objVal = (TimeOnly)(prop.GetValue(theObject, null) ?? TimeOnly.MinValue);
                        string formatForMySql = objVal.ToString("HH:mm:ss");
                        sqlValuesList += "'" + formatForMySql + "' , ";
                    }
                    else if (prop.PropertyType == typeof(DateOnly))
                    {
                        DateOnly objVal = (DateOnly)(prop.GetValue(theObject, null) ?? DateOnly.MinValue);
                        string formatForMySql = objVal.ToString("yyyy-MM-dd");
                        sqlValuesList += "'" + formatForMySql + " ' , ";
                    }
                    else
                    {
                        string sqlValue = (prop.GetValue(theObject, null)?.ToString().Trim()) ?? "";
                        if (sqlValue.Contains('\''))
                        {
                            sqlValue = sqlValue.Replace("'", "\\" + "'");
                        }
                        //sqlValuesList += "'" + prop.GetValue(theObject, null)?.ToString().Replace("\"", "_").Replace("'", " ").Replace(",", ".").Trim() + "' , ";
                        sqlValuesList += "'" + sqlValue + "' , ";
                    }
                }
            }
            sqlColumnsList = sqlColumnsList[..^3] + " ) ";
            sqlValuesList = sqlValuesList[..^3] + " ) ";

            sqlRequest += sqlColumnsList + " VALUES " + sqlValuesList + " ; ";

            return sqlRequest;
        }

        public static string GetTableUpdateRowSqlCommand(object theObject)
        {
            Type theType = theObject.GetType();

            DB_TableName myAttribute = (DB_TableName)GetCustomAttribute(theType, typeof(DB_TableName));

            if (myAttribute == null)
            {
                return "";
            }

            string tableName = myAttribute.TableName;
            string sqlRequest = $"UPDATE  {tableName} SET ";

            string sqlSetValues = "";

            PropertyInfo[] props = theType.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DB_ElementName elementNameAttribute = (DB_ElementName)GetCustomAttribute(prop, typeof(DB_ElementName));
                // skip if not in database (no database attribute)
                if (elementNameAttribute != null)
                {
                    sqlSetValues += elementNameAttribute.ElementName + " = ";
                    if (prop.PropertyType == typeof(double))
                    {
                        // On the server decimal is dot . mnot colon ,
                        sqlSetValues += "'" + prop.GetValue(theObject, null)?.ToString().Replace(",", ".").Trim() + "' , ";
                    }
                    else if (prop.PropertyType == typeof(DateTime))
                    {
                        DateTime objVal = (DateTime)(prop.GetValue(theObject, null) ?? DateTime.MinValue);
                        string formatForMySql = objVal.ToString("yyyy-MM-dd HH:mm:ss");
                        sqlSetValues += "'" + formatForMySql + "' , ";
                    }
                    else if (prop.PropertyType == typeof(TimeOnly))
                    {
                        TimeOnly objVal = (TimeOnly)(prop.GetValue(theObject, null) ?? TimeOnly.MinValue);
                        string formatForMySql = objVal.ToString("HH:mm:ss");
                        sqlSetValues += "'" + formatForMySql + "' , ";
                    }
                    else if (prop.PropertyType == typeof(DateOnly))
                    {
                        DateOnly objVal = (DateOnly)(prop.GetValue(theObject, null) ?? DateOnly.MinValue);
                        string formatForMySql = objVal.ToString("yyyy-MM-dd");
                        sqlSetValues += "'" + formatForMySql + "' , ";
                    }
                    else
                    {
                        string sqlValue = (prop.GetValue(theObject, null)?.ToString().Trim()) ?? "";
                        if (sqlValue.Contains('\''))
                        {
                            sqlValue = sqlValue.Replace("'", "\\" + "'");
                        }
                        //sqlSetValues += "'" + prop.GetValue(theObject, null)?.ToString().Replace("\"", "_").Replace("'", "_").Replace(",", ".").Trim() + "' , ";
                        sqlSetValues += "'" + sqlValue + "' , ";
                    }
                }
            }
            sqlSetValues = sqlSetValues[..^3];

            PropertyInfo propertyInfo = theType.GetProperty($"ID");

#pragma warning disable CS8605 // Conversion unboxing d'une valeur peut-être null.
            int theID = (int)propertyInfo.GetValue(theObject, null);
#pragma warning restore CS8605 // Conversion unboxing d'une valeur peut-être null.

            sqlRequest += sqlSetValues + $" WHERE {myAttribute.PrimaryKeyName} = {theID} ; ";

            return sqlRequest;
        }
    }


#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
}
