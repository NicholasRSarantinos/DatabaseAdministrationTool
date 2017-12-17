Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient

Partial Class index
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        Dim event_target As String = ""
        Dim event_argument As String = ""
        get_event_target(event_target, event_argument)

        If event_target = "login" Then

            If Request.Form("authentication_type") = "Windows Login" Then
                Session.Add("type", "auto")

                server_ip.Style("display") = "none"
                username.Style("display") = "none"
                password.Style("display") = "none"

            Else
                Session.Add("type", "manual")

                server_ip.Style("display") = "inline"
                username.Style("display") = "inline"
                password.Style("display") = "inline"

            End If

            Session.Add("database_type", Request.Form("sql_database_type"))
            Session.Add("server_ip", Request.Form("server_ip"))
            Session.Add("username", Request.Form("username"))
            Session.Add("password", Request.Form("password"))

        End If

        Dim conn_auto As Boolean = False

        If Session("type") Is Nothing Then
            Return
        Else
            If Session("type") = "auto" Then
                conn_auto = True
                If Session("database_type") = "MySQL" Then
                    MYSQL_DB_conn = New MySqlConnection("server=localhost;Integrated Security=yes;Uid=win_auth_user;Convert Zero Datetime=true;Allow Zero Datetime=true;charset=utf8;")
                Else
                    SQL_SERVER_conn = New SqlConnection("Server=.\SQLEXPRESS;Integrated Security=true;")
                End If
            Else
                If Session("server_ip") Is Nothing Or Session("server_ip") = "" Then
                    negative_event("Server cannot be empty!")
                    empty_main_page()
                    Return
                End If

                If Session("database_type") = "MySQL" Then
                    MYSQL_DB_conn = New MySqlConnection("server=" + Session("server_ip") + ";uid=" + Session("username") + ";pwd=" + Session("password") + ";Convert Zero Datetime=true;Allow Zero Datetime=true;charset=utf8;")
                Else
                    SQL_SERVER_conn = New SqlConnection("server=" + Session("server_ip") + ";User Id=" + Session("username") + ";Password=" + Session("password") + ";")
                End If

            End If
        End If

        Try
            If Session("database_type") = "MySQL" Then
                MYSQL_DB_conn.Open()
                MYSQL_DB_set_names_utf8()

                If event_target = "login" Then
                    MYSQL_DB_list_databases(True)
                Else
                    MYSQL_DB_execute_special_event(event_target, event_argument)
                End If
            Else
                SQL_SERVER_conn.Open()

                If event_target = "login" Then
                    SQL_SERVER_list_databases(True)
                Else
                    SQL_SERVER_execute_special_event(event_target, event_argument)
                End If
            End If

        Catch ex As Exception
            If conn_auto And Session("database_type") = "MySQL" And ex.Message = "Object reference not set to an instance of an object." Then
                info_event("MySQL Windows Native Authentication Plugin is not installed or configured properly. Please see report.")
                empty_main_page()
            Else
                negative_event(ex.Message)
                empty_main_page()
            End If
        End Try

    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Function encode_text(ByVal text As String) As String
        Return HttpUtility.HtmlEncode(text)
    End Function

    Private Function verify_input(ByVal input As String, ByVal reg_str As String) As Boolean
        Return Regex.IsMatch(input, reg_str)
    End Function

    Private Function str_in_arr(ByRef arr As String(), ByVal value As String) As Boolean

        For i As Integer = 0 To arr.Length - 1
            If arr(i) = value Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Sub check_illegal_chars(ByVal input As String)

        If Not verify_input(input, "^[a-zA-Z0-9_]+$") Or verify_input(input, "^[0-9]*$") Then
            Throw New System.Exception("Possible Browser Incompatibility.") 'illigal char found
        End If

    End Sub

    Public Sub positive_32_bit_number_or_throw(ByVal input As String, ByVal throw_string As String)

        Dim buff As UInteger

        If Not verify_input(input, "^[0-9]+$") Or Not UInt32.TryParse(input, buff) Then
            Throw New System.Exception(throw_string)
        End If

    End Sub

    Function convert_csv_to_params(ByVal str As String) As String

        If str Is Nothing Then
            Return "''"
        Else
            Dim result As String = "'"

            For i As Integer = 0 To str.Length - 1

                If str(i) = "," Then
                    result += "','"
                ElseIf str(i) = "'" Then
                    result += "''"
                Else
                    result += str(i)
                End If

            Next

            Return result + "'"

        End If

    End Function

    Public Function serialize(ByVal item As String) As String
        Dim arr As ArrayList = New ArrayList()
        arr.Add(item)
        Return serialize(arr)
    End Function

    Private Function serialize(ByVal array_list As ArrayList) As String
        Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
        Dim mem As New IO.MemoryStream
        bf.Serialize(mem, array_list)
        Return Convert.ToBase64String(mem.ToArray())
    End Function

    Private Function deserialize(ByVal array_string As String) As ArrayList

        If array_string = "" Then
            Return New ArrayList()
        End If

        Dim bf As New Runtime.Serialization.Formatters.Binary.BinaryFormatter
        Dim mem As New IO.MemoryStream(Convert.FromBase64String(array_string))
        Return DirectCast(bf.Deserialize(mem), ArrayList)
    End Function

    Sub empty_main_page()
        data_div.InnerHtml = ""
        on_database_list_view()
    End Sub

    Sub on_database_list_view()
        database_view.Style("display") = "inline"
        database_view.Style("visibility") = "hidden"

        table_view.Attributes.Remove("onclick")
        table_view.Style("display") = "none"
    End Sub

    Sub on_table_list_view()
        database_view.Style("display") = "inline"
        database_view.Style("visibility") = "visible"

        table_view.Attributes.Remove("onclick")
        table_view.Style("display") = "none"
    End Sub

    Sub on_row_list_view(ByVal name_of_db As String)
        database_view.Style("display") = "none"
        database_view.Style("visibility") = "visible"

        table_view.Attributes.Add("onclick", "__doPostBack('table_view', '" + serialize(name_of_db) + "')")
        table_view.Style("display") = "inline"
    End Sub

    Sub postive_event(ByVal str As String)
        event_label.InnerHtml = "<div style=""text-align:center;padding: 25px;color: white;background-color:#1cb841;"">" + str + "</div>"
    End Sub

    Sub negative_event(ByVal str As String)
        event_label.InnerHtml = "<div style=""text-align:center;padding: 25px;color: white;background-color:#F5554A;"">" + str + "</div>"
    End Sub

    Sub info_event(ByVal str As String)
        event_label.InnerHtml = "<div style=""text-align:center;padding: 25px;color: white;background-color:#29befc;"">" + str + "</div>"
    End Sub

    Sub SQL_SERVER_open_conn_if_needed()
        If SQL_SERVER_conn Is Nothing Then
            SQL_SERVER_conn.Open()
        End If
    End Sub

    Public Sub get_event_target(ByRef event_target As String, ByRef event_argument As String)

        event_target = ""
        event_argument = ""

        Me.ClientScript.GetPostBackEventReference(Me, String.Empty)

        If Me.IsPostBack Then

            If ((Me.Request("__EVENTTARGET") Is Nothing)) Then
                event_target = String.Empty
            Else
                event_target = Me.Request("__EVENTTARGET")

                If ((Me.Request("__EVENTARGUMENT") Is Nothing)) Then
                    event_argument = String.Empty
                Else
                    event_argument = Me.Request("__EVENTARGUMENT")
                End If

            End If

        End If

    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Dim SQL_SERVER_conn As SqlConnection = Nothing

    Dim SQL_SERVER_mysql_data_types(,) As String =
    {
        {"CHAR", "SIZE"},
        {"VARCHAR", "SIZE"},
        {"TEXT", "NON"},
        {"NCHAR", "SIZE"},
        {"NVARCHAR", "SIZE"},
        {"NTEXT", "NON"},
        {"BIT", "NON"},
        {"BINARY", "SIZE"},
        {"VARBINARY", "SIZE"},
        {"IMAGE", "NON"},
        {"TINYINT", "NON"},
        {"SMALLINT", "NON"},
        {"INT", "NON"},
        {"BIGINT", "NON"},
        {"DECIMAL", "M_D"},
        {"SMALLMONEY", "NON"},
        {"MONEY", "NON"},
        {"FLOAT", "SIZE"},
        {"REAL", "NON"},
        {"DATETIME", "NON"},
        {"DATETIME2", "NON"},
        {"SMALLDATETIME", "NON"},
        {"DATE", "NON"},
        {"TIME", "NON"},
        {"DATETIMEOFFSET", "NON"},
        {"ROWVERSION", "NON"},
        {"SQL_VARIANT", "NON"},
        {"XML", "NON"},
        {"UNIQUEIDENTIFIER", "NON"}
    } 'types not found in this list are treated as othertype

    Dim SQL_SERVER_data_type_alias(,) As String =
    {
        {"DEC", "DECIMAL"},
        {"NUMERIC", "DECIMAL"},
        {"SYSNAME", "NVARCHAR"},
        {"OTHERTYPE", "VARBINARY"},
        {"TIMESTAMP", "ROWVERSION"}
    } 'othertype is unkown type

    Function SQL_SERVER_return_data_type(ByVal str As String) As Data.SqlDbType

        Select Case str
            Case "CHAR"
                Return Data.SqlDbType.Char
            Case "VARCHAR"
                Return Data.SqlDbType.VarChar
            Case "TEXT"
                Return Data.SqlDbType.Text
            Case "NCHAR"
                Return Data.SqlDbType.NChar
            Case "NVARCHAR"
                Return Data.SqlDbType.NVarChar
            Case "NTEXT"
                Return Data.SqlDbType.NText
            Case "BIT"
                Return Data.SqlDbType.Bit
            Case "BINARY"
                Return Data.SqlDbType.Char
            Case "VARBINARY"
                Return Data.SqlDbType.VarChar
            Case "IMAGE"
                Return Data.SqlDbType.VarChar
            Case "TINYINT"
                Return Data.SqlDbType.TinyInt
            Case "SMALLINT"
                Return Data.SqlDbType.SmallInt
            Case "INT"
                Return Data.SqlDbType.Int
            Case "BIGINT"
                Return Data.SqlDbType.BigInt
            Case "DECIMAL"
                Return Data.SqlDbType.Decimal
            Case "SMALLMONEY"
                Return Data.SqlDbType.SmallMoney
            Case "MONEY"
                Return Data.SqlDbType.Money
            Case "FLOAT"
                Return Data.SqlDbType.Float
            Case "REAL"
                Return Data.SqlDbType.Real
            Case "DATETIME"
                Return Data.SqlDbType.DateTime
            Case "DATETIME2"
                Return Data.SqlDbType.DateTime2
            Case "SMALLDATETIME"
                Return Data.SqlDbType.SmallDateTime
            Case "DATE"
                Return Data.SqlDbType.Date
            Case "TIME"
                Return Data.SqlDbType.Time
            Case "DATETIMEOFFSET"
                Return Data.SqlDbType.DateTimeOffset
            Case "ROWVERSION"
                Return Data.SqlDbType.VarChar
            Case "SQL_VARIANT"
                Return Data.SqlDbType.Variant
            Case "XML"
                Return Data.SqlDbType.Xml
            Case "UNIQUEIDENTIFIER"
                Return Data.SqlDbType.VarChar
            Case "OTHERTYPE"
                Return Data.SqlDbType.VarChar
            Case Else
                Throw New System.Exception("UNKNOWN TYPE: " + str)
                Return Data.SqlDbType.VarChar
        End Select

    End Function

    Dim SQL_SERVER_char_like_types_arr As String() =
    {
        "CHAR",
        "VARCHAR",
        "TEXT",
        "NCHAR",
        "NVARCHAR",
        "SYSNAME",
        "NTEXT"
    }

    Dim SQL_SERVER_binary_like_types_arr As String() =
    {
        "BINARY",
        "VARBINARY",
        "IMAGE",
        "ROWVERSION",
        "TIMESTAMP",
        "OTHERTYPE"
    }

    Function SQL_SERVER_extract_column_type(ByVal str As String) As String 'needs more work

        str = str.ToUpper

        '1st we only keep the name of the type

        If str.Contains(" ") Then
            Dim parts() As String = str.Split(" ")
            str = parts(0)
        ElseIf str.Contains("(") Then
            Dim parts() As String = str.Split("(")
            str = parts(0)
        End If

        For i As Integer = 0 To SQL_SERVER_mysql_data_types.Length / 2 - 1
            If str = SQL_SERVER_mysql_data_types(i, 0) Then
                Return SQL_SERVER_mysql_data_types(i, 0)
            End If
        Next

        'if we are here it's not in the "typical" data type list. let's check if it's an alias

        For i As Integer = 0 To SQL_SERVER_data_type_alias.Length / 2 - 1
            If str = SQL_SERVER_data_type_alias(i, 0) Then
                Return SQL_SERVER_data_type_alias(i, 1)
            End If
        Next

        'if we are here the type is unkown, so

        Return "OTHERTYPE"

    End Function

    Function SQL_SERVER_prepare_for_type(ByVal type As String, ByVal value As String, ByVal convert_invalid_else_throw As Boolean) As String

        If type = "DATETIME" Or type = "DATETIME2" Or type = "SMALLDATETIME" Or type = "DATETIMEOFFSET" Then
            If Not IsDate(value) Then
                If convert_invalid_else_throw Then
                    Return "1900-01-01 00:00:00"
                Else
                    Throw New System.Exception(type + " formatting is incorrect or date or time is invalid. Example Format: YYYY-MM-DD HH:MM:SS")
                End If
            Else
                Return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss")
            End If
        ElseIf type = "DATE" Then
            If Not IsDate(value) Then
                If convert_invalid_else_throw Then
                    Return "1900-01-01"
                Else
                    Throw New System.Exception(type + " formatting is incorrect or date or time is invalid. Example Format: YYYY-MM-DD")
                End If
            Else
                Return Convert.ToDateTime(value).ToString("yyyy-MM-dd")
            End If
        End If

        Return value
    End Function

    Function SQL_SERVER_read_value(ByRef reader As SqlDataReader, ByVal column_types As String, ByVal pos As Integer) As String
        Return reader.GetValue(pos).ToString()
    End Function

    Private Function SQL_SERVER_protect_object(ByVal name As String) As String
        Return name.Replace("""", """""")
    End Function

    Private Function SQL_SERVER_add_quotes(ByVal name As String) As String
        Return """" + name + """"
    End Function

    Private Function SQL_SERVER_table_char_set() As String
        Return "" 'CHARACTER SET utf8
    End Function

    Private Function SQL_SERVER_add_item_in_where(ByVal column_type As String, ByVal column_name As String, ByVal prepared_param As String, ByRef actual_value As String, ByVal USED_IN_SEARCH As String) As String

        If actual_value Is Nothing Then
            Return SQL_SERVER_add_quotes(column_name) + " IS NULL"
        ElseIf column_type = "XML" Then
            Return SQL_SERVER_add_quotes(column_name) + " IS NOT NULL"
        ElseIf column_type = "FLOAT" Or column_type = "REAL" Then
            Return "CAST(" + SQL_SERVER_add_quotes(column_name) + " as VARCHAR(MAX)) = CAST(" + prepared_param + " as VARCHAR(MAX))"
        ElseIf USED_IN_SEARCH And str_in_arr(SQL_SERVER_char_like_types_arr, column_type) Then
            actual_value = "%" + actual_value + "%"
            Return SQL_SERVER_add_quotes(column_name) + " LIKE " + prepared_param
        ElseIf column_type = "TEXT" Or column_type = "NTEXT" Then
            Return SQL_SERVER_add_quotes(column_name) + " LIKE " + prepared_param
        ElseIf str_in_arr(SQL_SERVER_binary_like_types_arr, column_type) Then
            If column_type = "BINARY" Or column_type = "VARBINARY" Then
                Return SQL_SERVER_add_quotes(column_name) + " = CONVERT(VARBINARY(MAX), " + prepared_param + ", 2)"
            Else
                Return "CONVERT(VARBINARY(MAX), " + SQL_SERVER_add_quotes(column_name) + ", 2) = CONVERT(VARBINARY(MAX), " + prepared_param + ", 2)"
            End If
        ElseIf column_type = "UNIQUEIDENTIFIER" Then
            Return "CONVERT(VARCHAR(MAX), " + SQL_SERVER_add_quotes(column_name) + ") = " + prepared_param
        Else
            Return SQL_SERVER_add_quotes(column_name) + " = " + prepared_param
        End If

    End Function

    Private Function SQL_SERVER_modify_prepared_param_for_insert_or_update(ByVal column_type As String, ByVal prepared_param As String) As String
        If str_in_arr(SQL_SERVER_binary_like_types_arr, column_type) Then
            Return "CONVERT(VARBINARY(MAX), " + prepared_param + ", 2)"
        Else
            Return prepared_param
        End If
    End Function

    Private Sub SQL_SERVER_delete_row(ByRef args As ArrayList)
        Try
            SQL_SERVER_open_conn_if_needed()

            Dim database_name As String = args(0)
            Dim table_name As String = args(1)

            Dim del_cmd As String = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; DELETE TOP(1) FROM " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + " WHERE "

            For i As Integer = 2 To args.Count - 1 Step 3

                Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 2))
                del_cmd += SQL_SERVER_add_item_in_where(column_type_, SQL_SERVER_protect_object(args(i)), "@item_" + Convert.ToString((i - 2) / 3), args(i + 1), False)

                If i < args.Count - 3 Then
                    del_cmd += " AND "
                End If
            Next

            Dim cmd As SqlCommand = New SqlCommand(del_cmd, SQL_SERVER_conn)

            For i As Integer = 2 To args.Count - 1 Step 3

                If Not args(i + 1) Is Nothing Then
                    Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 2))
                    cmd.Parameters.Add("@item_" + Convert.ToString((i - 2) / 3), SQL_SERVER_return_data_type(column_type_)).Value = SQL_SERVER_prepare_for_type(column_type_, args(i + 1), True)
                End If

            Next

            cmd.ExecuteNonQuery()

            SQL_SERVER_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully deleted a row!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub SQL_SERVER_do_edit_row(ByRef args As ArrayList)

        Try
            SQL_SERVER_open_conn_if_needed()

            Dim database As String = args(0)
            Dim table As String = args(1)
            Dim input_base As String = args(2)

            Dim update_cmd As String = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database)) + "; UPDATE TOP(1) " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table)) + " SET "
            Dim where_cmd As String = " WHERE "

            For i As Integer = 3 To args.Count - 1 Step 3

                Dim item_no_str As String = Convert.ToString((i - 3) / 3) 'zero based
                Dim column_name As String = SQL_SERVER_protect_object(args(i))
                Dim column_old_value As String = args(i + 1)
                Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 2))
                Dim column_new_value As String = Request.Form(input_base + item_no_str)

                If column_type_ = "TIMESTAMP" Or column_type_ = "ROWVERSION" Then
                    If column_old_value <> column_new_value Then
                        Throw New System.Exception("Cannot update a TIMESTAMP/ROWVERSION column.")
                    End If
                Else
                    If column_new_value = "" Then
                        update_cmd += SQL_SERVER_add_quotes(column_name) + " = default, "
                    Else
                        update_cmd += SQL_SERVER_add_quotes(column_name) + " = " + SQL_SERVER_modify_prepared_param_for_insert_or_update(column_type_, "@new_value_" + item_no_str) + ", "
                    End If
                End If

                where_cmd += SQL_SERVER_add_item_in_where(column_type_, column_name, "@old_value_" + item_no_str, column_old_value, False) + " AND "
            Next

            update_cmd = update_cmd.Substring(0, update_cmd.Length - ", ".Length)
            where_cmd = where_cmd.Substring(0, where_cmd.Length - " AND ".Length)

            Dim cmd As SqlCommand = New SqlCommand(update_cmd + " " + where_cmd, SQL_SERVER_conn)

            For i As Integer = 3 To args.Count - 1 Step 3

                Dim item_no_str As String = Convert.ToString((i - 3) / 3) 'zero based
                Dim column_old_value As String = args(i + 1)
                Dim column_new_value As String = Request.Form(input_base + item_no_str)

                Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 2))
                Dim column_type As System.Data.SqlDbType = SQL_SERVER_return_data_type(column_type_)

                If Not column_old_value Is Nothing Then
                    cmd.Parameters.Add("@old_value_" + item_no_str, column_type).Value = SQL_SERVER_prepare_for_type(column_type_, column_old_value, True)
                End If

                If column_type_ <> "TIMESTAMP" And column_type_ <> "ROWVERSION" And column_new_value <> "" Then
                    cmd.Parameters.Add("@new_value_" + item_no_str, column_type).Value = SQL_SERVER_prepare_for_type(column_type_, column_new_value, False)
                End If
            Next

            cmd.ExecuteNonQuery()

            SQL_SERVER_list_rows(database, table, False, New ArrayList())
            postive_event("Successfully edited a row in `" + encode_text(table) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

    Private Sub SQL_SERVER_insert_row(ByRef args As ArrayList)

        Try
            Dim database_name As String = args(0)
            Dim table_name As String = args(1)

            Dim insert_cmd As String = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; INSERT INTO " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + " ("

            Dim fields As ArrayList = New ArrayList()

            For i As Integer = 2 To args.Count - 1 Step 2

                fields.Add(Request.Form("row_" + Convert.ToString((i - 2) / 2)))

                If fields((i - 2) / 2) <> "" Then
                    insert_cmd += SQL_SERVER_add_quotes(SQL_SERVER_protect_object(args(i))) + ", "
                End If
            Next

            If insert_cmd.EndsWith(", ") Then
                insert_cmd = insert_cmd.Substring(0, insert_cmd.Length - 2)
            End If

            insert_cmd += ") values ("

            For i As Integer = 2 To args.Count - 1 Step 2
                If fields((i - 2) / 2) <> "" Then
                    Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 1))

                    If column_type_ = "ROWVERSION" Or column_type_ = "TIMESTAMP" Then 'note, using alias here
                        Throw New System.Exception("Cannot insert a value into a ROWVERSION/TIMESTAMP column. They are generated automatically.")
                    ElseIf column_type_ = "UNIQUEIDENTIFIER" Then
                        If fields((i - 2) / 2) <> "NEWID()" And fields((i - 2) / 2) <> "NEWSEQUENTIALID()" Then
                            Throw New System.Exception("The only way to insert a value in a UNIQUEIDENTIFIER column is by using the keywords NEWID() or NEWSEQUENTIALID() which ask the system for a new value.")
                        Else
                            insert_cmd += fields((i - 2) / 2) + ", "
                        End If
                    Else
                        insert_cmd += SQL_SERVER_modify_prepared_param_for_insert_or_update(column_type_, "@item_" + Convert.ToString((i - 2) / 2)) + ", "
                    End If
                End If
            Next

            If insert_cmd.EndsWith(", ") Then
                insert_cmd = insert_cmd.Substring(0, insert_cmd.Length - 2)
            End If

            insert_cmd += ")"

            If insert_cmd = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; INSERT INTO " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + " () values ()" Then
                insert_cmd = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; INSERT INTO " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + " DEFAULT VALUES"
            End If

            SQL_SERVER_open_conn_if_needed()
            Dim cmd As SqlCommand = New SqlCommand(insert_cmd, SQL_SERVER_conn)

            For i As Integer = 2 To args.Count - 1 Step 2
                If fields((i - 2) / 2) <> "" Then
                    Dim column_type_ As String = SQL_SERVER_extract_column_type(args(i + 1))

                    If column_type_ <> "UNIQUEIDENTIFIER" Then
                        cmd.Parameters.Add("@item_" + Convert.ToString((i - 2) / 2), SQL_SERVER_return_data_type(column_type_)).Value = SQL_SERVER_prepare_for_type(column_type_, fields((i - 2) / 2), False)
                    End If
                End If
            Next

            cmd.ExecuteNonQuery()

            SQL_SERVER_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully added a row in `" + encode_text(table_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

    Public Sub SQL_SERVER_list_rows(ByVal database_name As String, ByVal table_name As String, ByVal show_success As Boolean, ByRef arg_arr As ArrayList)

        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; exec sp_columns " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + ";", SQL_SERVER_conn)
            Dim reader As SqlDataReader = cmd.ExecuteReader()

            Dim search_params As ArrayList = New ArrayList()
            search_params.Add(database_name)
            search_params.Add(table_name)

            Dim insert_params As ArrayList = New ArrayList()
            insert_params.Add(database_name)
            insert_params.Add(table_name)

            Dim column_names As ArrayList = New ArrayList()
            Dim column_types As ArrayList = New ArrayList()
            Dim column_types_extracted As ArrayList = New ArrayList()

            Dim column_types_special As New List(Of Data.SqlDbType)()

            Dim columns_str As String = ""

            While reader.Read()

                Dim column_name_now As String = reader.Item("COLUMN_NAME").ToString()
                Dim column_type_now As String = reader.Item("TYPE_NAME").ToString() + " (" + reader.Item("PRECISION").ToString() + ")"
                Dim column_type_now_extracted As String = SQL_SERVER_extract_column_type(Convert.ToString(column_type_now))

                column_names.Add(column_name_now)
                column_types.Add(column_type_now)

                search_params.Add(column_name_now)

                insert_params.Add(column_name_now)
                insert_params.Add(column_type_now)

                column_types_special.Add(SQL_SERVER_return_data_type(SQL_SERVER_extract_column_type(Convert.ToString(column_type_now))))
                column_types_extracted.Add(column_type_now_extracted)

                If str_in_arr(SQL_SERVER_binary_like_types_arr, column_type_now_extracted) Then
                    If column_type_now_extracted = "BINARY" Or column_type_now_extracted = "VARBINARY" Then
                        columns_str += "CONVERT(VARCHAR(MAX), " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + ", 2), "
                    Else
                        columns_str += "CONVERT(VARCHAR(MAX), CAST(" + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + " AS VARBINARY(MAX)), 2), "
                    End If
                ElseIf column_type_now_extracted = "DATETIME" Or column_type_now_extracted = "DATETIME2" Or column_type_now_extracted = "SMALLDATETIME" Then
                    columns_str += "CAST(FORMAT(" + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + ", 'yyyy-MM-dd HH:mm:ss') AS VARCHAR(MAX)), "
                ElseIf column_type_now_extracted = "DATE" Then
                    columns_str += "CAST(FORMAT(" + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + ", 'yyyy-MM-dd') AS VARCHAR(MAX)), "
                ElseIf column_type_now_extracted = "DATETIMEOFFSET" Then
                    columns_str += "CAST(FORMAT(" + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + ", 'yyyy-MM-dd HH:mm:ss') AS VARCHAR(MAX)), "
                Else
                    columns_str += SQL_SERVER_add_quotes(SQL_SERVER_protect_object(column_name_now)) + ", "
                End If

            End While

            If columns_str.Length > 0 Then
                columns_str = columns_str.Substring(0, columns_str.Length - 2)
            End If

            reader.Close()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th colspan=""" + Convert.ToString(column_names.Count) + """>Rows List in Table `" + encode_text(table_name) + "`</th>" +
                "<th style=""width:1%;"">Actions</th></tr><tr>"

            For i = 0 To column_names.Count - 1

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(table_name)
                edit_data.Add(column_names(i))
                edit_data.Add("column_list_text_" + Convert.ToString(i))

                table_ += "<th style=""text-align:left;padding-left: 30px;padding-right: 30px;"">" +
                "<img style=""display:inline-block;margin-right:10px;cursor:pointer;"" id=""column_list_edit_" + Convert.ToString(i) + """ onclick=""enable_column_edit('" + Convert.ToString(i) + "')"" src=""edit.png"">" +
                "<img style=""display:none;margin-right:10px;cursor:pointer;"" id=""column_list_accept_edit_" + Convert.ToString(i) + """ onclick=""__doPostBack('column_accept_edit', '" + serialize(edit_data) + "')"" src=""accept.png"">" +
                "<div style=""display:inline-block;min-width: 138px;padding:5px;"" id=""column_list_" + Convert.ToString(i) + """>" + encode_text(column_names(i)) + "</div>" +
                "<input runat=""server"" class=""small_fine_input"" type=""text"" style=""display:none;"" value=""" + encode_text(column_names(i)) + """ id=""column_list_text_" + Convert.ToString(i) + """ name=""column_list_text_" + Convert.ToString(i) + """/>" +
                "</th>"

            Next

            table_ += "<th></th></tr></thead><tbody><tr>"

            For i = 0 To column_names.Count - 1
                table_ += "<td><input placeholder=""" + encode_text(column_types(i)) + """ class=""fine_input"" type=""text"" id=""row_" + Convert.ToString(i) + """ name=""row_" + Convert.ToString(i) + """ runat=""server""></td>"
            Next

            table_ += "<td>"

            Dim out_values_list As ArrayList = New ArrayList()
            Dim out_column_types_sp As ArrayList = New ArrayList()

            cmd = New SqlCommand("USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; SELECT " + columns_str + " FROM " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + SQL_SERVER_generate_search_where_cause(arg_arr, out_values_list, column_types_extracted, out_column_types_sp), SQL_SERVER_conn)

            For i = 0 To out_values_list.Count - 1
                cmd.Parameters.Add("@item_" + Convert.ToString(i), SQL_SERVER_return_data_type(out_column_types_sp(i))).Value = SQL_SERVER_prepare_for_type(out_column_types_sp(i), out_values_list(i), False)
            Next

            reader = cmd.ExecuteReader()

            table_ += "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('row_search', '" + serialize(search_params) + "')"" style=""margin-right:10px;"">Search</div>" +
            "<div class=""green_button pure-button long_button"" onclick=""__doPostBack('row_insert', '" + serialize(insert_params) + "')"">Add Row</div>"

            Dim k As Integer = 0

            While reader.Read()

                table_ += "<tr>"

                Dim delete_command As ArrayList = New ArrayList()
                delete_command.Add(database_name)
                delete_command.Add(table_name)

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(table_name)
                edit_data.Add("row_list_text_" + Convert.ToString(k) + "_")

                For i = 0 To column_names.Count - 1

                    delete_command.Add(column_names(i))
                    edit_data.Add(column_names(i))

                    Dim item_now As String
                    Dim input_value As String

                    If reader.IsDBNull(i) Then
                        item_now = "<i style='color:#808080;'>NULL</i>"
                        input_value = ""
                        delete_command.Add(Nothing)
                        edit_data.Add(Nothing)
                    Else
                        item_now = SQL_SERVER_read_value(reader, SQL_SERVER_extract_column_type(column_types(i)), i)
                        delete_command.Add(item_now)
                        edit_data.Add(item_now)

                        item_now = encode_text(item_now) 'encode text
                        input_value = item_now
                    End If

                    delete_command.Add(column_types(i))
                    edit_data.Add(column_types(i))

                    table_ += "<td><div style=""display:inline;"" id=""row_list_" + Convert.ToString(k) + "_" + Convert.ToString(i) + """>" + item_now + "</div>" +
                    "<input class=""fine_input"" type=""text"" style=""display:none;"" value=""" + input_value + """ id=""row_list_text_" + Convert.ToString(k) +
                    "_" + Convert.ToString(i) + """ name=""row_list_text_" + Convert.ToString(k) + "_" + Convert.ToString(i) + """/></td>"

                Next

                table_ += "<td>" +
                "<div class=""yellow_button pure-button long_button"" id=""row_list_edit_" + Convert.ToString(k) + """ onclick=""enable_row_edit('" + Convert.ToString(k) + "', '" + Convert.ToString(column_names.Count) + "')"" style=""margin-right:10px;"">Edit Row</div>" +
                "<div class=""green_button pure-button long_button"" style=""display:none;margin-right:10px;"" id=""row_list_accept_edit_" + Convert.ToString(k) + """ onclick=""__doPostBack('row_accept_edit', '" + serialize(edit_data) + "')"">Save Edit</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('row_delete', '" + serialize(delete_command) + "')"">Delete</div>" +
                "</td></tr>"

                k += 1
            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_row_list_view(database_name)

            If show_success Then
                If arg_arr.Count = 0 Then
                    postive_event("Successfully listed all rows from table `" + encode_text(table_name) + "`!")
                Else
                    postive_event("Successfully searched table `" + encode_text(table_name) + "`!")
                End If
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Function SQL_SERVER_generate_search_where_cause(ByRef row_cmd As ArrayList, ByRef out_values_list As ArrayList,
                                                 ByRef column_types_sp As ArrayList,
                                                 ByRef out_column_types_sp As ArrayList) As String
        Dim result As String = " WHERE "

        For i As Integer = 2 To row_cmd.Count - 1

            Dim val As String = Request.Form("row_" + Convert.ToString(i - 2) + "")

            If val <> "" Then

                If result <> " WHERE " Then
                    result += " AND "
                End If

                result += SQL_SERVER_add_item_in_where(column_types_sp(i - 2), row_cmd(i), "@item_" + Convert.ToString(out_values_list.Count), val, True) + " "
                out_column_types_sp.Add(column_types_sp(i - 2))
                out_values_list.Add(val)

            End If
        Next

        If result = " WHERE " Then
            result = ""
        End If

        Return result

    End Function

    Public Sub SQL_SERVER_do_table_creation(ByVal database_name As String, ByVal cells_num As String)

        Try
            SQL_SERVER_open_conn_if_needed()

            Dim table_name As String = Request.Form("table_cell_name")

            If table_name.Length = 0 Then
                Throw New System.Exception("Table name cannot be empty.")
            End If

            Dim action As String = "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; CREATE TABLE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + " ( "

            For i = 1 To Convert.ToInt32(cells_num)

                Dim table_cell_type_arr() As String = Request.Form("type_table_cell_" + Convert.ToString(i)).Split(" ")

                If table_cell_type_arr.Length <> 2 Then
                    Throw New System.Exception("Possible Browser incompatibility.")
                End If

                Dim table_cell_type As String = table_cell_type_arr(0)
                Dim table_cell_name As String = SQL_SERVER_protect_object(Request.Form("table_cell_" + Convert.ToString(i)))
                Dim table_cell_category As String = table_cell_type_arr(1)

                If table_cell_name = "" Then
                    Throw New System.Exception("Column Name should not be empty!")
                End If

                check_illegal_chars(table_cell_type)
                check_illegal_chars(table_cell_category)

                If table_cell_category = "SIGN" Then

                    Dim sign_value As String = Request.Form("SIGN_" + Convert.ToString(i))

                    If sign_value <> "UNSIGNED" And sign_value <> "SIGNED" Then
                        Throw New System.Exception("Possible Browser incompatibility.")
                    End If

                    action += SQL_SERVER_add_quotes(table_cell_name) + " " + table_cell_type + " " + sign_value + " "

                ElseIf table_cell_category = "SIZE" Then

                    Dim size_value As String = Request.Form("SIZE_" + Convert.ToString(i))
                    positive_32_bit_number_or_throw(size_value, "Size should be a positive 32 bit number!")
                    action += SQL_SERVER_add_quotes(table_cell_name) + " " + table_cell_type + "(" + size_value + ") "

                ElseIf table_cell_category = "PARAMS" Then

                    Dim param_list As String = Request.Form("PARAMS_" + Convert.ToString(i))
                    action += SQL_SERVER_add_quotes(table_cell_name) + " " + table_cell_type + "(" + convert_csv_to_params(param_list) + ") "

                ElseIf table_cell_category = "M_D" Then

                    Dim m_value As String = Request.Form("M_VALUE_" + Convert.ToString(i))
                    Dim d_value As String = Request.Form("D_VALUE_" + Convert.ToString(i))

                    positive_32_bit_number_or_throw(m_value, "Total Digits should be a positive 32 bit number!")

                    If Not d_value = "0" Then 'if decimal digits are 0 we are ok
                        positive_32_bit_number_or_throw(d_value, "Decimal Digits should be a non negative 32 bit number!")
                    End If

                    action += SQL_SERVER_add_quotes(table_cell_name) + " " + table_cell_type + "(" + m_value + "," + d_value + ") "

                Else

                    action += SQL_SERVER_add_quotes(table_cell_name) + " " + table_cell_type

                End If

                If i = Convert.ToInt32(cells_num) Then
                    action += " )" + SQL_SERVER_table_char_set()
                Else
                    action += ", "
                End If

            Next

            Dim cmd As SqlCommand = New SqlCommand(action, SQL_SERVER_conn)

            cmd.ExecuteNonQuery()

            SQL_SERVER_list_tables(database_name, False)
            postive_event("Table `" + encode_text(table_name) + "` created successfully!")

        Catch ex As Exception
            negative_event(ex.Message)
            Return
        End Try

    End Sub

    Public Sub SQL_SERVER_create_table(ByVal database_name As String, ByVal rows_num_str As String)

        Dim rows_num As Integer = -1

        Try
            rows_num = Convert.ToInt32(rows_num_str)
        Catch ex As Exception
        End Try

        If rows_num <= 0 Then
            negative_event("Number of columns on table should be positive!")
            Return
        ElseIf rows_num > 1000 Then
            negative_event("Number of columns on table should be no more than 1000!")
            Return
        Else
            postive_event("Please fill this page to create the table!")
        End If

        Dim args As ArrayList = New ArrayList()
        args.Add(database_name)
        args.Add(Convert.ToString(rows_num))

        Dim str_ As String = "<table  class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th colspan=""4"">" +
        "<input class=""fine_input_140"" style=""margin-right:30px;"" placeholder=""Table Name"" id=""table_cell_name"" name=""table_cell_name""/>" +
        "<div class=""green_button pure-button"" style=""width:140px;"" onClick=""__doPostBack('do_create_table', '" + serialize(args) + "')"">Create Table</div>" +
        "</th></tr><th style=""padding-left:30px;text-align:left;"">Column Name</th><th style=""padding-left:30px;text-align:left;"">" +
        "Column Data Type</th><th style=""padding-left:30px;text-align:left;"">Data Type 1st Param</th><th style=""padding-left:30px;text-align:left;"">Data Type 2nd Param</th></thead><tbody>"

        For i = 1 To rows_num

            str_ +=
                "<tr><td>" +
                "<input class=""fine_input_140"" placeholder=""Column #" + Convert.ToString(i) + " Name"" id=""table_cell_" + Convert.ToString(i) + """ name=""table_cell_" + Convert.ToString(i) + """ runat=""server""/></td>" +
                "<td>" +
                "<select style=""width:154px;padding:5px;"" onchange=""type_picked(this)"" id=""type_table_cell_" + Convert.ToString(i) + """ name=""type_table_cell_" + Convert.ToString(i) + """ runat=""server"">"

            For j = 0 To SQL_SERVER_mysql_data_types.Length / 2 - 1
                str_ += "<option value=""" + SQL_SERVER_mysql_data_types(j, 0) + " " + SQL_SERVER_mysql_data_types(j, 1) + """>" + SQL_SERVER_mysql_data_types(j, 0) + "</option>"
            Next

            str_ +=
                "</select></td>" +
                "<td>" +
                "<select style=""width:154px;padding:5px;display:none;"" id=""SIGN_" + Convert.ToString(i) + """ name=""SIGN_" + Convert.ToString(i) + """><option value=""SIGNED"">SIGNED</option><option value=""UNSIGNED"">UNSIGNED</option></select>" +
                "<input class=""fine_input_140"" placeholder=""Size"" id=""SIZE_" + Convert.ToString(i) + """ name=""SIZE_" + Convert.ToString(i) + """/>" +
                "<input class=""fine_input_140"" placeholder=""CSV"" id=""PARAMS_" + Convert.ToString(i) + """ name=""PARAMS_" + Convert.ToString(i) + """ style=""display:none;""/>" +
                "<input class=""fine_input_140"" placeholder=""Total Digits"" id=""M_VALUE_" + Convert.ToString(i) + """ name=""M_VALUE_" + Convert.ToString(i) + """ style=""display:none;""/>" +
                "</td>" +
                "<td>" +
                "<input class=""fine_input_140"" placeholder=""Decimal Digits"" id=""D_VALUE_" + Convert.ToString(i) + """ name=""D_VALUE_" + Convert.ToString(i) + """ style=""visibility:hidden;""/>" +
                "</td>"

            str_ += "</tr>"
        Next

        str_ += "</tbody></table>"

        data_div.InnerHtml = str_
        on_row_list_view(database_name) 'yes, from here we want to go to table list not to database list
    End Sub

    Public Sub SQL_SERVER_list_tables(ByVal database_name As String, ByVal show_success As Boolean)

        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; SELECT name FROM sys.tables;", SQL_SERVER_conn)
            Dim reader As SqlDataReader = cmd.ExecuteReader()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight"">" +
            "<thead><tr><th>Tables List in `" + encode_text(database_name) + "`</th><th style=""width:1%;"">Actions</th></tr></thead><tbody>" +
            "<tr><td><input type=""text"" placeholder=""# Columns on New Table"" id=""new_table_size"" name=""new_table_size"" class=""fine_input"" runat=""server""></td>" +
            "<td style=""text-align:right;""><div class=""green_button pure-button long_button"" onclick=""__doPostBack('table_insert', '" + serialize(database_name) + "')"">Create New</div></td></tr>"

            Dim i As Integer = 0

            While reader.Read()

                Dim table_name_now = reader.Item("name").ToString()

                Dim arr As ArrayList = New ArrayList()
                arr.Add(database_name)
                arr.Add(table_name_now)
                Dim serialized As String = serialize(arr)

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(table_name_now)
                edit_data.Add("table_list_text_" + Convert.ToString(i))

                table_ += "<tr><td>" +
                "<div style=""display:inline;"" id=""table_list_" + Convert.ToString(i) + """>" + encode_text(table_name_now) + "</div>" +
                "<input type=""text"" style=""display:none;"" value=""" + encode_text(table_name_now) + """ id=""table_list_text_" + Convert.ToString(i) + """ class=""fine_input"" name=""table_list_text_" + Convert.ToString(i) + """/>" +
                "</td><td>" +
                "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('table_search', '" + serialized + "')"" style=""margin-right:10px;"">List Rows</div>" +
                "<div class=""yellow_button pure-button long_button"" id=""table_list_edit_" + Convert.ToString(i) + """ onclick=""enable_table_rename('" + Convert.ToString(i) + "')"" style=""margin-right:10px;"">Rename</div>" +
                "<div class=""green_button pure-button long_button"" style=""display:none;margin-right:10px;"" id=""table_list_accept_edit_" + Convert.ToString(i) + """ onclick=""__doPostBack('table_accept_edit', '" + serialize(edit_data) + "')"">Save Name</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('table_delete', '" + serialized + "')"">Delete</div>" +
                "</td></tr>"

                i += 1
            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_table_list_view()

            If show_success Then
                postive_event("Successfully listed all tables in `" + encode_text(database_name) + "`!")
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub SQL_SERVER_drop_table(ByVal database As String, ByVal table As String)
        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database)) + "; DROP TABLE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table)), SQL_SERVER_conn)
            cmd.ExecuteNonQuery()

            SQL_SERVER_list_tables(database, False)
            postive_event("Successfully deleted table `" + encode_text(table) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub SQL_SERVER_drop_database(ByVal database As String)
        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("USE ""master""; alter database " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database)) + " set single_user with rollback immediate; DROP DATABASE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database)), SQL_SERVER_conn)
            cmd.ExecuteNonQuery()

            SQL_SERVER_list_databases(False)
            postive_event("Successfully deleted database `" + encode_text(database) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub SQL_SERVER_create_database(ByVal database As String)
        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("CREATE DATABASE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database)), SQL_SERVER_conn)
            cmd.ExecuteNonQuery()

            SQL_SERVER_list_databases(False)
            postive_event("Successfully created database `" + encode_text(database) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Public Sub SQL_SERVER_list_databases(ByVal show_success As Boolean)

        Try
            SQL_SERVER_open_conn_if_needed()

            Dim cmd As SqlCommand = New SqlCommand("SELECT name FROM master.dbo.sysdatabases;", SQL_SERVER_conn)
            Dim reader As SqlDataReader = cmd.ExecuteReader()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th>" + Session("database_type") + " Database List</th><th style=""width:1%;"">Actions</th></tr></thead><tbody>"

            table_ += "<tr><td><input type=""text"" placeholder=""Name of new Database"" id=""new_database"" name=""new_database"" runat=""server"" class=""fine_input"">" +
            "</td><td style=""text-align:right;"">" +
            "<div class=""green_button pure-button long_button"" onclick=""__doPostBack('database_insert', '')"">Create DB</div>" +
            "</td></tr>"

            While reader.Read()

                Dim database_name_now = reader.Item("name").ToString()

                table_ += "<tr><td>" + encode_text(database_name_now) + "</td><td>" +
                "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('database_search', '" + serialize(database_name_now) + "')"" style=""margin-right:10px;"">List Tables</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('database_delete', '" + serialize(database_name_now) + "')"">Delete DB</div>" +
                "</td></tr>"

            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_database_list_view()

            If show_success Then
                postive_event("Listing Databases Successful!")
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

    Private Sub SQL_SERVER_do_edit_column(ByRef args As ArrayList)
        Try
            SQL_SERVER_open_conn_if_needed()

            Dim database_name As String = args(0)
            Dim table_name As String = args(1)
            Dim old_column_name As String = args(2)
            Dim new_column_name As String = Request.Form(args(3))

            If old_column_name = new_column_name Then
                Throw New System.Exception("The new column name must be different from the old!")
            ElseIf new_column_name.Length = 0 Then
                Throw New System.Exception("The new column name must be non empty!")
            End If

            Dim cmd As SqlCommand = New SqlCommand(
            "USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; EXEC sp_RENAME N'" + (SQL_SERVER_add_quotes(SQL_SERVER_protect_object(table_name)) + "." + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(old_column_name))).Replace("'", "''") + "', N'" + new_column_name.Replace("'", "''") + "', 'COLUMN'",
            SQL_SERVER_conn)

            cmd.ExecuteNonQuery()

            SQL_SERVER_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully renamed column `" + encode_text(old_column_name) + "` to `" + encode_text(new_column_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub SQL_SERVER_rename_table(ByVal database_name As String, ByVal prev_name As String, ByVal new_name As String)
        Try
            SQL_SERVER_open_conn_if_needed()

            If prev_name = new_name Then
                Throw New System.Exception("The new table name must be different from the old!")
            ElseIf new_name.Length = 0 Then
                Throw New System.Exception("The new table name must be non empty!")
            End If

            Dim cmd As SqlCommand = New SqlCommand("USE " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(database_name)) + "; EXEC sp_rename " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(prev_name)) + ", " + SQL_SERVER_add_quotes(SQL_SERVER_protect_object(new_name)) + ";", SQL_SERVER_conn)
            cmd.ExecuteNonQuery()

            SQL_SERVER_list_tables(database_name, False)
            postive_event("Successfully renamed table `" + encode_text(prev_name) + "` to `" + encode_text(new_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Public Sub SQL_SERVER_execute_special_event(ByRef eventTarget As String, ByRef eventArgument As String)

        Dim arguments As ArrayList = deserialize(eventArgument)

        If eventTarget = "database_view" Then
            If arguments.Count = 0 Then
                SQL_SERVER_list_databases(True)
            End If
        ElseIf eventTarget = "table_view" Then
            If arguments.Count = 1 Then
                SQL_SERVER_list_tables(arguments(0), True)
            End If
        ElseIf eventTarget = "database_search" Then
            If arguments.Count = 1 Then
                SQL_SERVER_list_tables(arguments(0), True)
            End If
        ElseIf eventTarget = "database_delete" Then
            If arguments.Count = 1 Then
                SQL_SERVER_drop_database(arguments(0))
            End If
        ElseIf eventTarget = "database_insert" Then
            If arguments.Count = 0 Then
                SQL_SERVER_create_database(Request.Form("new_database"))
            End If
        ElseIf eventTarget = "table_accept_edit" Then
            If arguments.Count = 3 Then
                SQL_SERVER_rename_table(arguments(0), arguments(1), Request.Form(arguments(2)))
            End If
        ElseIf eventTarget = "table_search" Then
            If arguments.Count = 2 Then
                SQL_SERVER_list_rows(arguments(0), arguments(1), True, New ArrayList())
            End If
        ElseIf eventTarget = "table_delete" Then
            If arguments.Count = 2 Then
                SQL_SERVER_drop_table(arguments(0), arguments(1))
            End If
        ElseIf eventTarget = "table_insert" Then
            If arguments.Count = 1 Then
                SQL_SERVER_create_table(arguments(0), Request.Form("new_table_size"))
            End If
        ElseIf eventTarget = "do_create_table" Then
            If arguments.Count = 2 Then
                SQL_SERVER_do_table_creation(arguments(0), arguments(1))
            End If
        ElseIf eventTarget = "row_delete" Then
            If arguments.Count >= 3 Then
                SQL_SERVER_delete_row(arguments)
            End If
        ElseIf eventTarget = "row_insert" Then
            If arguments.Count >= 3 Then
                SQL_SERVER_insert_row(arguments)
            End If
        ElseIf eventTarget = "row_search" Then
            If arguments.Count >= 3 Then
                SQL_SERVER_list_rows(arguments(0), arguments(1), True, arguments)
            End If
        ElseIf eventTarget = "row_accept_edit" Then
            If arguments.Count >= 6 Then
                SQL_SERVER_do_edit_row(arguments)
            End If
        ElseIf eventTarget = "column_accept_edit" Then
            If arguments.Count = 4 Then
                SQL_SERVER_do_edit_column(arguments)
            End If
        End If
    End Sub

    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

    Dim MYSQL_DB_conn As MySqlConnection = Nothing

    Dim MYSQL_DB_mysql_data_types(,) As String =
        {
            {"TINYINT", "SIGN"},
            {"SMALLINT", "SIGN"},
            {"MEDIUMINT", "SIGN"},
            {"INT", "SIGN"},
            {"BIGINT", "SIGN"},
            {"FLOAT", "NON"},
            {"DOUBLE", "NON"},
            {"DECIMAL", "M_D"},
            {"BIT", "SIZE"},
            {"CHAR", "SIZE"},
            {"VARCHAR", "SIZE"},
            {"TINYTEXT", "NON"},
            {"TEXT", "NON"},
            {"MEDIUMTEXT", "NON"},
            {"LONGTEXT", "NON"},
            {"BINARY", "SIZE"},
            {"VARBINARY", "SIZE"},
            {"TINYBLOB", "NON"},
            {"BLOB", "NON"},
            {"MEDIUMBLOB", "NON"},
            {"LONGBLOB", "NON"},
            {"ENUM", "PARAMS"},
            {"SET", "PARAMS"},
            {"DATETIME", "NON"},
            {"DATE", "NON"},
            {"TIMESTAMP", "NON"},
            {"TIME", "NON"},
            {"YEAR", "NON"}
        } 'don't change the order here

    Function MYSQL_DB_extract_column_type(ByVal str As String) As String

        str = str.ToUpper

        For i As Integer = 0 To MYSQL_DB_mysql_data_types.Length / 2 - 1

            If str.StartsWith(MYSQL_DB_mysql_data_types(i, 0)) Then

                If MYSQL_DB_mysql_data_types(i, 1) = "SIGN" Then
                    If str.Length >= "UNSIGNED".Length And InStrRev(str, "UNSIGNED") <> 0 Then
                        Return MYSQL_DB_mysql_data_types(i, 0) + " UNSIGNED"
                    Else
                        Return MYSQL_DB_mysql_data_types(i, 0) + " SIGNED"
                    End If
                Else
                    Return MYSQL_DB_mysql_data_types(i, 0)
                End If

            End If
        Next

        Return ""

    End Function

    Function MYSQL_DB_return_data_type(ByVal str As String) As MySql.Data.MySqlClient.MySqlDbType

        Select Case str
            Case "TINYINT UNSIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.UByte
            Case "TINYINT SIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.Byte
            Case "SMALLINT UNSIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.UInt16
            Case "SMALLINT SIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.Int16
            Case "MEDIUMINT UNSIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.UInt24
            Case "MEDIUMINT SIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.Int24
            Case "INT UNSIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.UInt32
            Case "INT SIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.Int32
            Case "BIGINT UNSIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.UInt64
            Case "BIGINT SIGNED"
                Return MySql.Data.MySqlClient.MySqlDbType.Int64
            Case "FLOAT"
                Return MySql.Data.MySqlClient.MySqlDbType.Float
            Case "DOUBLE"
                Return MySql.Data.MySqlClient.MySqlDbType.Double
            Case "DECIMAL"
                Return MySql.Data.MySqlClient.MySqlDbType.Decimal
            Case "BIT"
                Return MySql.Data.MySqlClient.MySqlDbType.Bit
            Case "CHAR"
                Return MySql.Data.MySqlClient.MySqlDbType.String
            Case "VARCHAR"
                Return MySql.Data.MySqlClient.MySqlDbType.VarChar
            Case "TINYTEXT"
                Return MySql.Data.MySqlClient.MySqlDbType.TinyText
            Case "TEXT"
                Return MySql.Data.MySqlClient.MySqlDbType.Text
            Case "MEDIUMTEXT"
                Return MySql.Data.MySqlClient.MySqlDbType.MediumText
            Case "LONGTEXT"
                Return MySql.Data.MySqlClient.MySqlDbType.LongText
            Case "BINARY"
                Return MySql.Data.MySqlClient.MySqlDbType.Binary
            Case "VARBINARY"
                Return MySql.Data.MySqlClient.MySqlDbType.VarBinary
            Case "TINYBLOB"
                Return MySql.Data.MySqlClient.MySqlDbType.TinyBlob
            Case "BLOB"
                Return MySql.Data.MySqlClient.MySqlDbType.Blob
            Case "MEDIUMBLOB"
                Return MySql.Data.MySqlClient.MySqlDbType.MediumBlob
            Case "LONGBLOB"
                Return MySql.Data.MySqlClient.MySqlDbType.LongBlob
            Case "ENUM"
                Return MySql.Data.MySqlClient.MySqlDbType.Enum
            Case "SET"
                Return MySql.Data.MySqlClient.MySqlDbType.Set
            Case "DATETIME"
                Return MySql.Data.MySqlClient.MySqlDbType.String
            Case "DATE"
                Return MySql.Data.MySqlClient.MySqlDbType.String
            Case "TIMESTAMP"
                Return MySql.Data.MySqlClient.MySqlDbType.String
            Case "TIME"
                Return MySql.Data.MySqlClient.MySqlDbType.String
            Case "YEAR"
                Return MySql.Data.MySqlClient.MySqlDbType.Year
            Case Else
                Throw New System.Exception("UNKNOWN TYPE: " + str)
                Return MySql.Data.MySqlClient.MySqlDbType.UByte
        End Select

    End Function

    Function MYSQL_DB_is_year(ByVal str As String) As Boolean 'treats year 0000 as invalid, like the vb is_date does
        'https://dev.mysql.com/doc/refman/5.7/en/year.html

        If str Is Nothing Then
            Return False
        Else
            str = str.Trim()
        End If

        If str.Length = 1 Then
            Return Char.IsDigit(str(0))
        ElseIf str.Length = 2 Then
            Return Char.IsDigit(str(0)) And Char.IsDigit(str(1))
        ElseIf str.Length = 4 Then
            Try
                Dim val As Integer = Convert.ToUInt32(str)
                Return val >= 1901 And val <= 2155
            Catch ex As Exception
                Return False
            End Try
        Else
            Return False
        End If

    End Function

    Function MYSQL_DB_is_timestamp(ByVal str As String) As Boolean 'basicly checks if the value fits in a 32bit signed int
        If Not IsDate(str) Then
            Return False
        End If

        Try
            Dim timestamp As Double = (Convert.ToDateTime(str) - New DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds
            Return timestamp >= 0 And timestamp <= 2147483647
        Catch ex As Exception
            Return False
        End Try

    End Function

    Function MYSQL_DB_convert_time_to_standard(ByVal str As String) As String 'call this for data you read from the database
        'if anything is wrong the catch will get it
        'another way is to replace the '.' with ' ' and mysql will handle the rest

        Try
            If str Is Nothing Then
                Return str
            ElseIf Not str.Contains(".") Then
                Return str
            End If

            Dim parts() As String = str.Split(".")

            If parts.Length <> 2 Then
                Return str
            End If

            Dim time_data As DateTime = Convert.ToDateTime(parts(1))
            Dim first_part As Integer = Convert.ToInt32(parts(0)) * 24

            If first_part < 0 Then
                first_part -= time_data.Hour
            Else
                first_part += time_data.Hour
            End If

            Return Convert.ToString(first_part) + ":" + Convert.ToString(time_data.Minute) + ":" + Convert.ToString(time_data.Second)

        Catch ex As Exception
            Return str
        End Try

    End Function

    Dim MYSQL_DB_char_like_types_arr As String() =
    {
        "CHAR",
        "VARCHAR",
        "TINYTEXT",
        "TEXT",
        "MEDIUMTEXT",
        "LONGTEXT"
    }

    Dim MYSQL_DB_binary_like_types_arr As String() =
    {
        "BINARY",
        "VARBINARY",
        "TINYBLOB",
        "BLOB",
        "MEDIUMBLOB",
        "LONGBLOB"
    }

    Dim MYSQL_DB_accepted_hex As String() =
        {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F"}

    Function MYSQL_DB_prepare_for_type(ByVal type As String, ByVal value As String, ByVal convert_invalid_else_throw As Boolean) As String

        If value Is Nothing Then
            Return value
        End If

        If type = "DATE" Then
            If Not IsDate(value) Then
                If convert_invalid_else_throw Then
                    Return "0000-00-00"
                Else
                    Throw New System.Exception("Date formatting is incorrect or date is invalid. Example Format: YYYY-MM-DD")
                End If
            Else
                Return Convert.ToDateTime(value).ToString("yyyy-MM-dd")
            End If
        ElseIf type = "DATETIME" Then
            If Not IsDate(value) Then
                If convert_invalid_else_throw Then
                    Return "0000-00-00 00:00:00"
                Else
                    Throw New System.Exception("DateTime formatting is incorrect or date or time is invalid. Example Format: YYYY-MM-DD HH:MM:SS")
                End If
            Else
                Return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss")
            End If
        ElseIf type = "YEAR" Then
            If Not MYSQL_DB_is_year(value) Then
                If convert_invalid_else_throw Then
                    Return "0000"
                Else
                    Throw New System.Exception("Year formatting is incorrect or year is invalid. Example Format: YYYY or YY")
                End If
            Else
                Return value.Trim()
            End If
        ElseIf type = "TIMESTAMP" Then
            If Not MYSQL_DB_is_timestamp(value) Then
                If convert_invalid_else_throw Then
                    Return "0000-00-00 00:00:00"
                Else
                    Throw New System.Exception("TIMESTAMP formatting is incorrect or date or time is invalid. Example Format: YYYY-MM-DD HH:MM:SS")
                End If
            Else
                Return Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss")
            End If
        ElseIf type = "TIME" Then
            Return MYSQL_DB_convert_time_to_standard(value)
        ElseIf type = "FLOAT" Then
            Dim result As Single
            If Single.TryParse(value, result) Then
                Return value
            Else
                Throw New System.Exception("Input string was not in a correct format or value was either too large or too small for a Float.")
            End If
        ElseIf type = "DOUBLE" Then
            Dim result As Double
            If Double.TryParse(value, result) Then
                Return value
            Else
                Throw New System.Exception("Input string was not in a correct format or value was either too large or too small for a Double.")
            End If
        ElseIf str_in_arr(MYSQL_DB_binary_like_types_arr, type) Then
            If MYSQL_DB_verify_hex_valid(value) Then
                Return value
            Else
                Throw New System.Exception("Binary string not in correct format. We expect hex strings. Example: FF0FAAAF")
            End If
        End If

        Return value
    End Function

    Function MYSQL_DB_verify_hex_valid(ByRef str As String) As Boolean

        For i As Integer = 0 To str.Length - 1
            If Not str_in_arr(MYSQL_DB_accepted_hex, Convert.ToString(str(i)).ToUpper()) Then
                Return False
            End If
        Next

        Return True

    End Function

    Function MYSQL_DB_read_value(ByRef reader As MySqlDataReader, ByVal column_types As String, ByVal pos As Integer) As String
        Try
            Dim val As String = reader.GetString(pos)

            If (column_types = "TINYINT SIGNED" Or column_types = "TINYINT UNSIGNED") Then
                If val.ToUpper() = "TRUE" Then
                    Return "1"
                ElseIf val.ToUpper() = "FALSE" Then
                    Return "0"
                End If
            End If

            Return val

        Catch ex As Exception

            If column_types = "DATE" Or column_types = "DATETIME" Or column_types = "TIMESTAMP" Then
                Return "0000/00/00 00:00:00"
            Else
                Return reader.GetValue(pos).ToString()
            End If
        End Try
    End Function

    Sub MYSQL_DB_open_conn_if_needed()
        If MYSQL_DB_conn Is Nothing Then
            MYSQL_DB_conn.Open()
            MYSQL_DB_set_names_utf8()
        End If
    End Sub

    Sub MYSQL_DB_set_names_utf8()
        Try
            Dim cmd As MySqlCommand = New MySqlCommand("SET NAMES utf8", MYSQL_DB_conn)
            cmd.ExecuteNonQuery()
        Catch ex As Exception
        End Try
    End Sub

    Private Function MYSQL_DB_protect_object(ByVal name As String) As String
        Return name.Replace("`", "``")
    End Function

    Private Function MYSQL_DB_add_item_in_where(ByVal column_type As String, ByVal column_name As String, ByVal prepared_param As String, ByRef actual_value As String, ByVal USED_IN_SEARCH As String) As String

        If actual_value Is Nothing Then
            Return "`" + column_name + "` IS NULL"
        ElseIf column_type = "FLOAT" Or column_type = "DOUBLE" Then
            Return "CAST(`" + column_name + "` as CHAR) = CAST(" + prepared_param + " as CHAR)"
        ElseIf USED_IN_SEARCH And str_in_arr(MYSQL_DB_char_like_types_arr, column_type) Then
            actual_value = "%" + actual_value + "%"
            Return "`" + column_name + "` LIKE " + prepared_param
        ElseIf str_in_arr(MYSQL_DB_binary_like_types_arr, column_type) Then
            Return "`" + column_name + "` = UNHEX(" + prepared_param + ")"
        Else
            Return "`" + column_name + "` = " + prepared_param
        End If

    End Function

    Private Function MYSQL_DB_modify_prepared_param_for_insert_or_update(ByVal column_type As String, ByVal prepared_param As String) As String
        If str_in_arr(MYSQL_DB_binary_like_types_arr, column_type) Then
            Return "UNHEX(" + prepared_param + ")"
        Else
            Return prepared_param
        End If
    End Function

    Public Sub MYSQL_DB_execute_special_event(ByRef eventTarget As String, ByRef eventArgument As String)

        Dim arguments As ArrayList = deserialize(eventArgument)

        If eventTarget = "database_view" Then
            If arguments.Count = 0 Then
                MYSQL_DB_list_databases(True)
            End If
        ElseIf eventTarget = "table_view" Then
            If arguments.Count = 1 Then
                MYSQL_DB_list_tables(arguments(0), True)
            End If
        ElseIf eventTarget = "database_search" Then
            If arguments.Count = 1 Then
                MYSQL_DB_list_tables(arguments(0), True)
            End If
        ElseIf eventTarget = "database_delete" Then
            If arguments.Count = 1 Then
                MYSQL_DB_drop_database(arguments(0))
            End If
        ElseIf eventTarget = "database_insert" Then
            If arguments.Count = 0 Then
                MYSQL_DB_create_database(Request.Form("new_database"))
            End If
        ElseIf eventTarget = "table_accept_edit" Then
            If arguments.Count = 3 Then
                MYSQL_DB_rename_table(arguments(0), arguments(1), Request.Form(arguments(2)))
            End If
        ElseIf eventTarget = "table_search" Then
            If arguments.Count = 2 Then
                MYSQL_DB_list_rows(arguments(0), arguments(1), True, New ArrayList())
            End If
        ElseIf eventTarget = "table_delete" Then
            If arguments.Count = 2 Then
                MYSQL_DB_drop_table(arguments(0), arguments(1))
            End If
        ElseIf eventTarget = "table_insert" Then
            If arguments.Count = 1 Then
                MYSQL_DB_create_table(arguments(0), Request.Form("new_table_size"))
            End If
        ElseIf eventTarget = "do_create_table" Then
            If arguments.Count = 2 Then
                MYSQL_DB_do_table_creation(arguments(0), arguments(1))
            End If
        ElseIf eventTarget = "row_delete" Then
            If arguments.Count >= 3 Then
                MYSQL_DB_delete_row(arguments)
            End If
        ElseIf eventTarget = "row_insert" Then
            If arguments.Count >= 3 Then
                MYSQL_DB_insert_row(arguments)
            End If
        ElseIf eventTarget = "row_search" Then
            If arguments.Count >= 3 Then
                MYSQL_DB_list_rows(arguments(0), arguments(1), True, arguments)
            End If
        ElseIf eventTarget = "row_accept_edit" Then
            If arguments.Count >= 6 Then
                MYSQL_DB_do_edit_row(arguments)
            End If
        ElseIf eventTarget = "column_accept_edit" Then
            If arguments.Count = 4 Then
                MYSQL_DB_do_edit_column(arguments)
            End If
        End If
    End Sub

    Private Sub MYSQL_DB_do_edit_column(ByRef args As ArrayList)
        Try
            MYSQL_DB_open_conn_if_needed()

            Dim database_name As String = args(0)
            Dim table_name As String = args(1)
            Dim old_column_name As String = args(2)
            Dim new_column_name As String = Request.Form(args(3))

            Dim cmd As MySqlCommand = New MySqlCommand("DESCRIBE `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "` `" + MYSQL_DB_protect_object(old_column_name) + "`", MYSQL_DB_conn)
            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            Dim column_type_now As String = ""

            While reader.Read()
                column_type_now = reader.GetString(1)
            End While

            reader.Close()

            cmd = New MySqlCommand("ALTER TABLE `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) +
                "` CHANGE `" + MYSQL_DB_protect_object(old_column_name) + "` `" + MYSQL_DB_protect_object(new_column_name) + "` " + column_type_now, MYSQL_DB_conn)

            cmd.ExecuteNonQuery()

            MYSQL_DB_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully renamed column `" + encode_text(old_column_name) + "` to `" + encode_text(new_column_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub MYSQL_DB_rename_table(ByVal database_name As String, ByVal prev_name As String, ByVal new_name As String)
        Try
            MYSQL_DB_open_conn_if_needed()

            If prev_name = new_name Then
                Throw New System.Exception("The new table name must be different from the old!")
            End If

            Dim cmd As MySqlCommand = New MySqlCommand("RENAME TABLE `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(prev_name) + "` TO `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(new_name) + "`", MYSQL_DB_conn)
            cmd.ExecuteNonQuery()

            MYSQL_DB_list_tables(database_name, False)
            postive_event("Successfully renamed table `" + encode_text(prev_name) + "` to `" + encode_text(new_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub MYSQL_DB_delete_row(ByRef args As ArrayList)
        Try
            MYSQL_DB_open_conn_if_needed()

            Dim database_name As String = args(0)
            Dim table_name As String = args(1)

            Dim del_cmd As String = "DELETE FROM `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "` WHERE "

            For i As Integer = 2 To args.Count - 1 Step 3

                Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 2))
                del_cmd += MYSQL_DB_add_item_in_where(column_type_, MYSQL_DB_protect_object(args(i)), "@item_" + Convert.ToString((i - 2) / 3), args(i + 1), False)

                If i < args.Count - 3 Then
                    del_cmd += " AND "
                Else
                    del_cmd += " LIMIT 1"
                End If
            Next

            Dim cmd As MySqlCommand = New MySqlCommand(del_cmd, MYSQL_DB_conn)

            For i As Integer = 2 To args.Count - 1 Step 3

                If Not args(i + 1) Is Nothing Then
                    Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 2))
                    cmd.Parameters.Add("@item_" + Convert.ToString((i - 2) / 3), MYSQL_DB_return_data_type(column_type_)).Value = MYSQL_DB_prepare_for_type(column_type_, args(i + 1), True)
                End If

            Next

            cmd.ExecuteNonQuery()

            MYSQL_DB_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully deleted a row!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Public Sub MYSQL_DB_do_table_creation(ByVal database_name As String, ByVal cells_num As String)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim table_name As String = Request.Form("table_cell_name")

            Dim action As String = " CREATE TABLE `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "` ( "

            For i = 1 To Convert.ToInt32(cells_num)

                Dim table_cell_type_arr() As String = Request.Form("type_table_cell_" + Convert.ToString(i)).Split(" ")

                If table_cell_type_arr.Length <> 2 Then
                    Throw New System.Exception("Possible Browser incompatibility.")
                End If

                Dim table_cell_type As String = table_cell_type_arr(0)
                Dim table_cell_name As String = MYSQL_DB_protect_object(Request.Form("table_cell_" + Convert.ToString(i)))
                Dim table_cell_category As String = table_cell_type_arr(1)

                check_illegal_chars(table_cell_type)
                check_illegal_chars(table_cell_category)

                If table_cell_category = "SIGN" Then

                    Dim sign_value As String = Request.Form("SIGN_" + Convert.ToString(i))

                    If sign_value <> "UNSIGNED" And sign_value <> "SIGNED" Then
                        Throw New System.Exception("Possible Browser incompatibility.")
                    End If

                    action += "`" + table_cell_name + "` " + table_cell_type + " " + sign_value + " "

                ElseIf table_cell_category = "SIZE" Then

                    Dim size_value As String = Request.Form("SIZE_" + Convert.ToString(i))
                    positive_32_bit_number_or_throw(size_value, "Size should be a positive 32 bit number!")
                    action += "`" + table_cell_name + "` " + table_cell_type + "(" + size_value + ") "

                ElseIf table_cell_category = "PARAMS" Then

                    Dim param_list As String = Request.Form("PARAMS_" + Convert.ToString(i))
                    action += "`" + table_cell_name + "` " + table_cell_type + "(" + convert_csv_to_params(param_list) + ") "

                ElseIf table_cell_category = "M_D" Then

                    Dim m_value As String = Request.Form("M_VALUE_" + Convert.ToString(i))
                    Dim d_value As String = Request.Form("D_VALUE_" + Convert.ToString(i))

                    positive_32_bit_number_or_throw(m_value, "Total Digits should be a positive 32 bit number!")

                    If Not d_value = "0" Then 'if decimal digits are 0 we are ok
                        positive_32_bit_number_or_throw(d_value, "Decimal Digits should be a non negative 32 bit number!")
                    End If

                    action += "`" + table_cell_name + "` " + table_cell_type + "(" + m_value + "," + d_value + ") "

                Else

                    action += "`" + table_cell_name + "` " + table_cell_type

                End If

                If i = Convert.ToInt32(cells_num) Then
                    action += " ) CHARACTER SET utf8"
                Else
                    action += ", "
                End If

            Next

            Dim cmd As MySqlCommand = New MySqlCommand(action, MYSQL_DB_conn)

            cmd.ExecuteNonQuery()

            MYSQL_DB_list_tables(database_name, False)
            postive_event("Table `" + encode_text(table_name) + "` created successfully!")

        Catch ex As Exception
            negative_event(ex.Message)
            Return
        End Try

    End Sub

    Public Sub MYSQL_DB_create_table(ByVal database_name As String, ByVal rows_num_str As String)

        Dim rows_num As Integer = -1

        Try
            rows_num = Convert.ToInt32(rows_num_str)
        Catch ex As Exception
        End Try

        If rows_num <= 0 Then
            negative_event("Number of columns on table should be positive!")
            Return
        ElseIf rows_num > 1000 Then
            negative_event("Number of columns on table should be no more than 1000!")
            Return
        Else
            postive_event("Please fill this page to create the table!")
        End If

        Dim args As ArrayList = New ArrayList()
        args.Add(database_name)
        args.Add(Convert.ToString(rows_num))

        Dim str_ As String = "<table  class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th colspan=""4"">" +
        "<input class=""fine_input_140"" style=""margin-right:30px;"" placeholder=""Table Name"" id=""table_cell_name"" name=""table_cell_name""/>" +
        "<div class=""green_button pure-button"" style=""width:140px;"" onClick=""__doPostBack('do_create_table', '" + serialize(args) + "')"">Create Table</div>" +
        "</th></tr><th style=""padding-left:30px;text-align:left;"">Column Name</th><th style=""padding-left:30px;text-align:left;"">" +
        "Column Data Type</th><th style=""padding-left:30px;text-align:left;"">Data Type 1st Param</th><th style=""padding-left:30px;text-align:left;"">Data Type 2nd Param</th></thead><tbody>"

        For i = 1 To rows_num

            str_ +=
                "<tr><td>" +
                "<input class=""fine_input_140"" placeholder=""Column #" + Convert.ToString(i) + " Name"" id=""table_cell_" + Convert.ToString(i) + """ name=""table_cell_" + Convert.ToString(i) + """ runat=""server""/></td>" +
                "<td>" +
                "<select style=""width:154px;padding:5px;"" onchange=""type_picked(this)"" id=""type_table_cell_" + Convert.ToString(i) + """ name=""type_table_cell_" + Convert.ToString(i) + """ runat=""server"">"

            For j = 0 To MYSQL_DB_mysql_data_types.Length / 2 - 1
                str_ += "<option value=""" + MYSQL_DB_mysql_data_types(j, 0) + " " + MYSQL_DB_mysql_data_types(j, 1) + """>" + MYSQL_DB_mysql_data_types(j, 0) + "</option>"
            Next

            str_ +=
                "</select></td>" +
                "<td>" +
                "<select style=""width:154px;padding:5px;"" id=""SIGN_" + Convert.ToString(i) + """ name=""SIGN_" + Convert.ToString(i) + """><option value=""SIGNED"">SIGNED</option><option value=""UNSIGNED"">UNSIGNED</option></select>" +
                "<input class=""fine_input_140"" placeholder=""Size"" id=""SIZE_" + Convert.ToString(i) + """ name=""SIZE_" + Convert.ToString(i) + """ style=""display:none;""/>" +
                "<input class=""fine_input_140"" placeholder=""CSV"" id=""PARAMS_" + Convert.ToString(i) + """ name=""PARAMS_" + Convert.ToString(i) + """ style=""display:none;""/>" +
                "<input class=""fine_input_140"" placeholder=""Total Digits"" id=""M_VALUE_" + Convert.ToString(i) + """ name=""M_VALUE_" + Convert.ToString(i) + """ style=""display:none;""/>" +
                "</td>" +
                "<td>" +
                "<input class=""fine_input_140"" placeholder=""Decimal Digits"" id=""D_VALUE_" + Convert.ToString(i) + """ name=""D_VALUE_" + Convert.ToString(i) + """ style=""visibility:hidden;""/>" +
                "</td>"

            str_ += "</tr>"
        Next

        str_ += "</tbody></table>"

        data_div.InnerHtml = str_
        on_row_list_view(database_name) 'yes, from here we want to go to table list not to database list
    End Sub

    Private Sub MYSQL_DB_do_edit_row(ByRef args As ArrayList)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim database As String = args(0)
            Dim table As String = args(1)
            Dim input_base As String = args(2)

            Dim update_cmd As String = "UPDATE `" + MYSQL_DB_protect_object(database) + "`.`" + MYSQL_DB_protect_object(table) + "` SET "
            Dim where_cmd As String = " WHERE "

            For i As Integer = 3 To args.Count - 1 Step 3

                Dim item_no_str As String = Convert.ToString((i - 3) / 3) 'zero based
                Dim column_name As String = MYSQL_DB_protect_object(args(i))
                Dim column_old_value As String = args(i + 1)
                Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 2))

                update_cmd += "`" + column_name + "` = " + MYSQL_DB_modify_prepared_param_for_insert_or_update(column_type_, "@new_value_" + item_no_str) + ", "
                where_cmd += MYSQL_DB_add_item_in_where(column_type_, column_name, "@old_value_" + item_no_str, column_old_value, False) + " AND "
            Next

            update_cmd = update_cmd.Substring(0, update_cmd.Length - ", ".Length)
            where_cmd = where_cmd.Substring(0, where_cmd.Length - " AND ".Length)

            Dim cmd As MySqlCommand = New MySqlCommand(update_cmd + " " + where_cmd + " LIMIT 1", MYSQL_DB_conn)

            For i As Integer = 3 To args.Count - 1 Step 3

                Dim item_no_str As String = Convert.ToString((i - 3) / 3) 'zero based
                Dim column_old_value As String = args(i + 1)
                Dim column_new_value As String = Request.Form(input_base + item_no_str)

                If column_new_value = "" Then
                    column_new_value = Nothing
                End If

                Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 2))
                Dim column_type As MySql.Data.MySqlClient.MySqlDbType = MYSQL_DB_return_data_type(column_type_)

                If Not column_old_value Is Nothing Then
                    cmd.Parameters.Add("@old_value_" + item_no_str, column_type).Value = MYSQL_DB_prepare_for_type(column_type_, column_old_value, True)
                End If

                cmd.Parameters.Add("@new_value_" + item_no_str, column_type).Value = MYSQL_DB_prepare_for_type(column_type_, column_new_value, False)

            Next

            cmd.ExecuteNonQuery()

            MYSQL_DB_list_rows(database, table, False, New ArrayList())
            postive_event("Successfully edited a row in `" + encode_text(table) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

    Private Sub MYSQL_DB_insert_row(ByRef args As ArrayList)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim database_name As String = args(0)
            Dim table_name As String = args(1)

            Dim insert_cmd As String = "INSERT INTO `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "` ("

            Dim fields As ArrayList = New ArrayList()

            For i As Integer = 2 To args.Count - 1 Step 2

                fields.Add(Request.Form("row_" + Convert.ToString((i - 2) / 2)))

                If fields((i - 2) / 2) <> "" Then
                    insert_cmd += "`" + MYSQL_DB_protect_object(args(i)) + "`, "
                End If
            Next

            If insert_cmd.EndsWith(", ") Then
                insert_cmd = insert_cmd.Substring(0, insert_cmd.Length - 2)
            End If

            insert_cmd += ") values ("

            For i As Integer = 2 To args.Count - 1 Step 2
                If fields((i - 2) / 2) <> "" Then
                    Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 1))
                    insert_cmd += MYSQL_DB_modify_prepared_param_for_insert_or_update(column_type_, "@item_" + Convert.ToString((i - 2) / 2)) + ", "
                End If
            Next

            If insert_cmd.EndsWith(", ") Then
                insert_cmd = insert_cmd.Substring(0, insert_cmd.Length - 2)
            End If

            insert_cmd += ")"

            Dim cmd As MySqlCommand = New MySqlCommand(insert_cmd, MYSQL_DB_conn)

            For i As Integer = 2 To args.Count - 1 Step 2
                If fields((i - 2) / 2) <> "" Then
                    Dim column_type_ As String = MYSQL_DB_extract_column_type(args(i + 1))
                    cmd.Parameters.Add("@item_" + Convert.ToString((i - 2) / 2), MYSQL_DB_return_data_type(column_type_)).Value = MYSQL_DB_prepare_for_type(column_type_, fields((i - 2) / 2), False)
                End If
            Next

            cmd.ExecuteNonQuery()

            MYSQL_DB_list_rows(database_name, table_name, False, New ArrayList())
            postive_event("Successfully added a row in `" + encode_text(table_name) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

    Public Sub MYSQL_DB_list_rows(ByVal database_name As String, ByVal table_name As String, ByVal show_success As Boolean, ByRef arg_arr As ArrayList)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("DESCRIBE `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "`", MYSQL_DB_conn)
            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            Dim search_params As ArrayList = New ArrayList()
            search_params.Add(database_name)
            search_params.Add(table_name)

            Dim insert_params As ArrayList = New ArrayList()
            insert_params.Add(database_name)
            insert_params.Add(table_name)

            Dim column_names As ArrayList = New ArrayList()
            Dim column_types As ArrayList = New ArrayList()
            Dim column_types_extracted As ArrayList = New ArrayList()

            Dim column_types_special As New List(Of MySql.Data.MySqlClient.MySqlDbType)()

            Dim columns_str As String = ""

            While reader.Read()

                Dim column_name_now As String = reader.GetString(0)
                Dim column_type_now As String = reader.GetString(1)
                Dim column_type_now_extracted As String = MYSQL_DB_extract_column_type(Convert.ToString(reader.GetString(1)))

                column_names.Add(column_name_now)
                column_types.Add(column_type_now)

                search_params.Add(column_name_now)

                insert_params.Add(column_name_now)
                insert_params.Add(column_type_now)

                column_types_special.Add(MYSQL_DB_return_data_type(MYSQL_DB_extract_column_type(Convert.ToString(reader.GetString(1)))))
                column_types_extracted.Add(column_type_now_extracted)

                If str_in_arr(MYSQL_DB_binary_like_types_arr, column_type_now_extracted) Then
                    columns_str += "HEX(`" + MYSQL_DB_protect_object(column_name_now) + "`), "
                Else
                    columns_str += "`" + MYSQL_DB_protect_object(column_name_now) + "`, "
                End If

            End While

            If columns_str.Length > 0 Then
                columns_str = columns_str.Substring(0, columns_str.Length - 2)
            End If

            reader.Close()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th colspan=""" + Convert.ToString(column_names.Count) + """>Rows List in Table `" + encode_text(table_name) + "`</th>" +
                "<th style=""width:1%;"">Actions</th></tr><tr>"

            For i = 0 To column_names.Count - 1

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(table_name)
                edit_data.Add(column_names(i))
                edit_data.Add("column_list_text_" + Convert.ToString(i))

                table_ += "<th style=""text-align:left;padding-left: 30px;padding-right: 30px;"">" +
                "<img style=""display:inline-block;margin-right:10px;cursor:pointer;"" id=""column_list_edit_" + Convert.ToString(i) + """ onclick=""enable_column_edit('" + Convert.ToString(i) + "')"" src=""edit.png"">" +
                "<img style=""display:none;margin-right:10px;cursor:pointer;"" id=""column_list_accept_edit_" + Convert.ToString(i) + """ onclick=""__doPostBack('column_accept_edit', '" + serialize(edit_data) + "')"" src=""accept.png"">" +
                "<div style=""display:inline-block;min-width: 138px;padding:5px;"" id=""column_list_" + Convert.ToString(i) + """>" + encode_text(column_names(i)) + "</div>" +
                "<input runat=""server"" class=""small_fine_input"" type=""text"" style=""display:none;"" value=""" + encode_text(column_names(i)) + """ id=""column_list_text_" + Convert.ToString(i) + """ name=""column_list_text_" + Convert.ToString(i) + """/>" +
                "</th>"

            Next

            table_ += "<th></th></tr></thead><tbody><tr>"

            For i = 0 To column_names.Count - 1
                table_ += "<td><input placeholder=""" + encode_text(column_types(i)) + """ class=""fine_input"" type=""text"" id=""row_" + Convert.ToString(i) + """ name=""row_" + Convert.ToString(i) + """ runat=""server""></td>"
            Next

            table_ += "<td>"

            Dim out_values_list As ArrayList = New ArrayList()
            Dim out_column_types_sp As ArrayList = New ArrayList()

            cmd = New MySqlCommand("SELECT " + columns_str + " FROM  `" + MYSQL_DB_protect_object(database_name) + "`.`" + MYSQL_DB_protect_object(table_name) + "`" + MYSQL_DB_generate_search_where_cause(arg_arr, out_values_list, column_types_extracted, out_column_types_sp), MYSQL_DB_conn)

            For i = 0 To out_values_list.Count - 1
                cmd.Parameters.Add("@item_" + Convert.ToString(i), MYSQL_DB_return_data_type(out_column_types_sp(i))).Value = MYSQL_DB_prepare_for_type(out_column_types_sp(i), out_values_list(i), False)
            Next

            reader = cmd.ExecuteReader()

            table_ += "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('row_search', '" + serialize(search_params) + "')"" style=""margin-right:10px;"">Search</div>" +
            "<div class=""green_button pure-button long_button"" onclick=""__doPostBack('row_insert', '" + serialize(insert_params) + "')"">Add Row</div>"

            Dim k As Integer = 0

            While reader.Read()

                table_ += "<tr>"

                Dim delete_command As ArrayList = New ArrayList()
                delete_command.Add(database_name)
                delete_command.Add(table_name)

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(table_name)
                edit_data.Add("row_list_text_" + Convert.ToString(k) + "_")

                For i = 0 To column_names.Count - 1

                    delete_command.Add(column_names(i))
                    edit_data.Add(column_names(i))

                    Dim item_now As String
                    Dim input_value As String

                    If reader.IsDBNull(i) Then
                        item_now = "<i style='color:#808080;'>NULL</i>"
                        input_value = ""
                        delete_command.Add(Nothing)
                        edit_data.Add(Nothing)
                    Else
                        item_now = MYSQL_DB_read_value(reader, MYSQL_DB_extract_column_type(column_types(i)), i)
                        delete_command.Add(item_now)
                        edit_data.Add(item_now)

                        item_now = encode_text(item_now) 'encode text
                        input_value = item_now
                    End If

                    delete_command.Add(column_types(i))
                    edit_data.Add(column_types(i))

                    table_ += "<td><div style=""display:inline;"" id=""row_list_" + Convert.ToString(k) + "_" + Convert.ToString(i) + """>" + item_now + "</div>" +
                    "<input class=""fine_input"" type=""text"" style=""display:none;"" value=""" + input_value + """ id=""row_list_text_" + Convert.ToString(k) +
                    "_" + Convert.ToString(i) + """ name=""row_list_text_" + Convert.ToString(k) + "_" + Convert.ToString(i) + """/></td>"

                Next

                table_ += "<td>" +
                "<div class=""yellow_button pure-button long_button"" id=""row_list_edit_" + Convert.ToString(k) + """ onclick=""enable_row_edit('" + Convert.ToString(k) + "', '" + Convert.ToString(column_names.Count) + "')"" style=""margin-right:10px;"">Edit Row</div>" +
                "<div class=""green_button pure-button long_button"" style=""display:none;margin-right:10px;"" id=""row_list_accept_edit_" + Convert.ToString(k) + """ onclick=""__doPostBack('row_accept_edit', '" + serialize(edit_data) + "')"">Save Edit</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('row_delete', '" + serialize(delete_command) + "')"">Delete</div>" +
                "</td></tr>"

                k += 1
            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_row_list_view(database_name)

            If show_success Then
                If arg_arr.Count = 0 Then
                    postive_event("Successfully listed all rows from table `" + encode_text(table_name) + "`!")
                Else
                    postive_event("Successfully searched table `" + encode_text(table_name) + "`!")
                End If
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Function MYSQL_DB_generate_search_where_cause(ByRef row_cmd As ArrayList, ByRef out_values_list As ArrayList,
                                                 ByRef column_types_sp As ArrayList,
                                                 ByRef out_column_types_sp As ArrayList) As String
        Dim result As String = " WHERE "

        For i As Integer = 2 To row_cmd.Count - 1

            Dim val As String = Request.Form("row_" + Convert.ToString(i - 2) + "")

            If val <> "" Then

                If result <> " WHERE " Then
                    result += " AND "
                End If

                result += MYSQL_DB_add_item_in_where(column_types_sp(i - 2), row_cmd(i), "@item_" + Convert.ToString(out_values_list.Count), val, True) + " "
                out_column_types_sp.Add(column_types_sp(i - 2))
                out_values_list.Add(val)

            End If
        Next

        If result = " WHERE " Then
            result = ""
        End If

        Return result

    End Function

    Private Sub MYSQL_DB_drop_table(ByVal database As String, ByVal table As String)
        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("DROP TABLE `" + MYSQL_DB_protect_object(database) + "`.`" + MYSQL_DB_protect_object(table) + "`", MYSQL_DB_conn)
            cmd.ExecuteNonQuery()

            MYSQL_DB_list_tables(database, False)
            postive_event("Successfully deleted table `" + encode_text(table) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub MYSQL_DB_drop_database(ByVal database As String)
        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("DROP DATABASE `" + MYSQL_DB_protect_object(database) + "`", MYSQL_DB_conn)
            cmd.ExecuteNonQuery()

            MYSQL_DB_list_databases(False)
            postive_event("Successfully deleted database `" + encode_text(database) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Private Sub MYSQL_DB_create_database(ByVal database As String)
        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("CREATE DATABASE `" + MYSQL_DB_protect_object(database) + "`", MYSQL_DB_conn)
            cmd.ExecuteNonQuery()

            MYSQL_DB_list_databases(False)
            postive_event("Successfully created database `" + encode_text(database) + "`!")

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Public Sub MYSQL_DB_list_tables(ByVal database_name As String, ByVal show_success As Boolean)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("SHOW TABLES IN `" + MYSQL_DB_protect_object(database_name) + "`", MYSQL_DB_conn)
            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight"">" +
            "<thead><tr><th>Tables List in `" + encode_text(database_name) + "`</th><th style=""width:1%;"">Actions</th></tr></thead><tbody>" +
            "<tr><td><input type=""text"" placeholder=""# Columns on New Table"" id=""new_table_size"" name=""new_table_size"" class=""fine_input"" runat=""server""></td>" +
            "<td style=""text-align:right;""><div class=""green_button pure-button long_button"" onclick=""__doPostBack('table_insert', '" + serialize(database_name) + "')"">Create New</div></td></tr>"

            Dim i As Integer = 0

            While reader.Read()

                Dim arr As ArrayList = New ArrayList()
                arr.Add(database_name)
                arr.Add(reader.GetString(0))
                Dim serialized As String = serialize(arr)

                Dim edit_data As ArrayList = New ArrayList()
                edit_data.Add(database_name)
                edit_data.Add(reader.GetString(0))
                edit_data.Add("table_list_text_" + Convert.ToString(i))

                table_ += "<tr><td>" +
                "<div style=""display:inline;"" id=""table_list_" + Convert.ToString(i) + """>" + encode_text(reader.GetString(0)) + "</div>" +
                "<input type=""text"" style=""display:none;"" value=""" + encode_text(reader.GetString(0)) + """ id=""table_list_text_" + Convert.ToString(i) + """ class=""fine_input"" name=""table_list_text_" + Convert.ToString(i) + """/>" +
                "</td><td>" +
                "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('table_search', '" + serialized + "')"" style=""margin-right:10px;"">List Rows</div>" +
                "<div class=""yellow_button pure-button long_button"" id=""table_list_edit_" + Convert.ToString(i) + """ onclick=""enable_table_rename('" + Convert.ToString(i) + "')"" style=""margin-right:10px;"">Rename</div>" +
                "<div class=""green_button pure-button long_button"" style=""display:none;margin-right:10px;"" id=""table_list_accept_edit_" + Convert.ToString(i) + """ onclick=""__doPostBack('table_accept_edit', '" + serialize(edit_data) + "')"">Save Name</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('table_delete', '" + serialized + "')"">Delete</div>" +
                "</td></tr>"

                i += 1
            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_table_list_view()

            If show_success Then
                postive_event("Successfully listed all tables in `" + encode_text(database_name) + "`!")
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try
    End Sub

    Public Sub MYSQL_DB_list_databases(ByVal show_success As Boolean)

        Try
            MYSQL_DB_open_conn_if_needed()

            Dim cmd As MySqlCommand = New MySqlCommand("SHOW DATABASES", MYSQL_DB_conn)
            Dim reader As MySqlDataReader = cmd.ExecuteReader()

            Dim table_ As String = "<table class=""zui-table zui-table-horizontal zui-table-zebra zui-table-highlight""><thead><tr><th>" + Session("database_type") + " Database List</th><th style=""width:1%;"">Actions</th></tr></thead><tbody>"

            table_ += "<tr><td><input type=""text"" placeholder=""Name of new Database"" id=""new_database"" name=""new_database"" runat=""server"" class=""fine_input"">" +
            "</td><td style=""text-align:right;"">" +
            "<div class=""green_button pure-button long_button"" onclick=""__doPostBack('database_insert', '')"">Create DB</div>" +
            "</td></tr>"

            While reader.Read()

                table_ += "<tr><td>" + encode_text(reader.GetString(0)) + "</td><td>" +
                "<div class=""sky_blue_button pure-button long_button"" onclick=""__doPostBack('database_search', '" + serialize(reader.GetString(0)) + "')"" style=""margin-right:10px;"">List Tables</div>" +
                "<div class=""red_button pure-button long_button"" onclick=""__doPostBack('database_delete', '" + serialize(reader.GetString(0)) + "')"">Delete DB</div>" +
                "</td></tr>"

            End While

            reader.Close()

            table_ += "</tbody></table>"

            data_div.InnerHtml = table_
            on_database_list_view()

            If show_success Then
                postive_event("Listing Databases Successful!")
            End If

        Catch ex As Exception
            negative_event(ex.Message)
        End Try

    End Sub

End Class
