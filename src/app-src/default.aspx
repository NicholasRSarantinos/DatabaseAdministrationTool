<%@ Page Language="VB" AutoEventWireup="false" CodeFile="default.aspx.vb" ValidateRequest="false" Inherits="index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head id="Head1" runat="server">
        <title>Database Admin Pro</title>
        <meta charset="UTF-8"/>
		<style>
			#container {
				width: 90%;
				margin: 0 auto;
			}
			
			#data_div {
				white-space: nowrap;
			}

			#second_div {
				white-space: nowrap;
			}

			#first_div {
				white-space: nowrap;
			}

			#third_div {
				white-space: nowrap;
			}

			.zui-table {
			border: solid 1px #DDEEEE;
			border-collapse: collapse;
			border-spacing: 0;
			font: normal 13px Arial, sans-serif;
			width: 100%;
			white-space: nowrap;
			cursor:default;
			}

			.zui-table thead th {
				background-color: #DDEFEF;
				color: #336B6B;
				padding-right: 20px;
				padding-left: 20px;
				height:45px;
				text-align: left;
				text-shadow: 1px 1px 1px #fff;
				text-align:center;
			}

			.zui-table tbody td {
				border: solid 1px #DDEEEE;
				color: #333;
				padding-left: 30px;
				padding-right: 30px;
				height:50px;
				text-shadow: 1px 1px 1px #fff;
			}

			.zui-table-horizontal tbody td {
				border-left: none;
				border-right: none;
			}

			.zui-table-zebra tbody tr:nth-child(odd) {
				background-color: #fff;
			}

			.zui-table-zebra tbody tr:nth-child(even) {
				background-color: #EEF7EE;
			}

			.zui-table-highlight tbody tr:hover {
				background-color: #CCE7E7;
			}

			.long_button {
				width:92px;
			}

			.centered_table_column {
				text-align: center;
			}

			.create_button {
				-moz-box-shadow:inset 0px 1px 0px 0px #7a8eb9;
				-webkit-box-shadow:inset 0px 1px 0px 0px #7a8eb9;
				box-shadow:inset 0px 1px 0px 0px #7a8eb9;
				background:-webkit-gradient(linear, left top, left bottom, color-stop(0.05, #637aad), color-stop(1, #5972a7));
				background:-moz-linear-gradient(top, #637aad 5%, #5972a7 100%);
				background:-webkit-linear-gradient(top, #637aad 5%, #5972a7 100%);
				background:-o-linear-gradient(top, #637aad 5%, #5972a7 100%);
				background:-ms-linear-gradient(top, #637aad 5%, #5972a7 100%);
				background:linear-gradient(to bottom, #637aad 5%, #5972a7 100%);
				filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#637aad', endColorstr='#5972a7',GradientType=0);
				background-color:#637aad;
				border:1px solid #314179;
				display:inline-block;
				cursor:pointer;
				color:#ffffff;
				font-family:Arial;
				font-size:12px;
				font-weight:bold;
				padding:4px 8px;
				text-decoration:none;
			}

			.create_button:hover {
				background:-webkit-gradient(linear, left top, left bottom, color-stop(0.05, #5972a7), color-stop(1, #637aad));
				background:-moz-linear-gradient(top, #5972a7 5%, #637aad 100%);
				background:-webkit-linear-gradient(top, #5972a7 5%, #637aad 100%);
				background:-o-linear-gradient(top, #5972a7 5%, #637aad 100%);
				background:-ms-linear-gradient(top, #5972a7 5%, #637aad 100%);
				background:linear-gradient(to bottom, #5972a7 5%, #637aad 100%);
				filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#5972a7', endColorstr='#637aad',GradientType=0);
				background-color:#5972a7;
			}

			.create_button:active {
				position:relative;
				top:1px;
			}
			
			.small_fine_input {
				width: 134px;
				padding: 5px;
			}

			.fine_input {
				width: 160px;
				padding: 5px;
			}
			
			.fine_input_140 {
				width: 140px;
				padding: 5px;
			}

			.pure-button {
				display:inline-block;
				zoom:1;
				line-height:normal;
				white-space:nowrap;
				vertical-align:middle;
				text-align:center;
				cursor:pointer;
				-webkit-user-drag:none;
				-webkit-user-select:none;
				-moz-user-select:none;
				-ms-user-select:none;
				user-select:none;
				-webkit-box-sizing:border-box;
				-moz-box-sizing:border-box;
				box-sizing:border-box;
			}
			
			.pure-button::-moz-focus-inner {
				padding:0;
				border:0;
			}
			
			.pure-button {
				font-family:inherit;
				font-size:100%;
				padding:.5em 1em;
				color:#444;
				color:rgba(0,0,0,.8);
				border:1px solid #999;
				border:0 rgba(0,0,0,0);
				background-color:#E6E6E6;
				text-decoration:none;
				border-radius:2px;
			}
			
			.pure-button-hover,.pure-button:hover,.pure-button:focus {
				filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#00000000', endColorstr='#1a000000', GradientType=0);
				background-image:-webkit-gradient(linear,0 0,0 100%,from(transparent),color-stop(40%,rgba(0,0,0,.05)),to(rgba(0,0,0,.1)));
				background-image:-webkit-linear-gradient(transparent,rgba(0,0,0,.05) 40%,rgba(0,0,0,.1));
				background-image:-moz-linear-gradient(top,rgba(0,0,0,.05) 0,rgba(0,0,0,.1));
				background-image:-o-linear-gradient(transparent,rgba(0,0,0,.05) 40%,rgba(0,0,0,.1));
				background-image:linear-gradient(transparent,rgba(0,0,0,.05) 40%,rgba(0,0,0,.1));
			}
			
			.pure-button:focus {
				outline:0;
			}
			
			.pure-button-active,.pure-button:active {
				box-shadow:0 0 0 1px rgba(0,0,0,.15) inset,0 0 6px rgba(0,0,0,.2) inset;border-color:#000;
			}
			
			.pure-button[disabled],.pure-button-disabled,.pure-button-disabled:hover,.pure-button-disabled:focus,.pure-button-disabled:active {
				border:0;
				background-image:none;
				filter:progid:DXImageTransform.Microsoft.gradient(enabled=false);
				filter:alpha(opacity=40);
				-khtml-opacity:.4;
				-moz-opacity:.4;
				opacity:.4;
				cursor:not-allowed;
				box-shadow:none;
			}
			
			.pure-button-hidden {
				display:none;
			}
			
			.pure-button::-moz-focus-inner {
				padding:0;
				border:0;
			}
			
			.pure-button-primary,.pure-button-selected,a.pure-button-primary,a.pure-button-selected {
				background-color:#0078e7;
				color:#fff;
			}

			.green_button, .red_button, .orange_button, .sky_blue_button, .yellow_button, .blue_button {
				color: white;
				border-radius: 1px;
				text-shadow: 0 1px 1px rgba(0, 0, 0, 0.2);
			}

			.green_button {
				background: rgb(28, 184, 65);
			}
			
			.red_button {
				background: rgb(202, 60, 60);
			}
			
			.orange_button {
				background: rgb(223, 117, 20);
			}
			
			.sky_blue_button {
				background: rgb(66, 184, 221);
			}

			.yellow_button {
				background: #F2A900;
			 }

			.blue_button {
				background: rgb(0, 120, 231);
			}

			.top {
				margin: 0;
				padding: 0;
				background-color: #29befc;
				white-space: nowrap;
				position: absolute;
				top: 0;
				left: 0;
				right: 0;
				bottom: 0;
				width:100%;
				height:130px;
			}
		</style>
		
		<script>
			var data_type_categories = ["SIGN", "M_D", "", "PARAM"];

			function hide_and_clearall(current_column)
			{
				document.getElementById("SIGN_" + current_column).style.display = "none";
				document.getElementById("SIGN_" + current_column).selectedIndex = 0;

				document.getElementById("SIZE_" + current_column).style.display = "none";
				document.getElementById("SIZE_" + current_column).value = "";

				document.getElementById("PARAMS_" + current_column).style.display = "none";
				document.getElementById("PARAMS_" + current_column).value = "";

				document.getElementById("M_VALUE_" + current_column).style.display = "none";
				document.getElementById("M_VALUE_" + current_column).value = "";

				document.getElementById("D_VALUE_" + current_column).style.visibility = "hidden";
				document.getElementById("D_VALUE_" + current_column).value = "";
			}

			function enable_item(full_name)
			{
				document.getElementById(full_name).style.display = "inline";
				document.getElementById(full_name).style.visibility = "visible";
			}

			function type_picked(this_item)
			{
				var full_item_id = this_item.id;
				var full_item_value = this_item.value;

				var current_column = full_item_id.substring(parseInt(full_item_id.lastIndexOf("_")) + 1, full_item_id.length);
				var current_type = full_item_value.substring(parseInt(full_item_value.lastIndexOf(" ")) + 1, full_item_value.length);

				hide_and_clearall(current_column);

				if (current_type == "SIGN") enable_item("SIGN_" + current_column);
				else if (current_type == "SIZE") enable_item("SIZE_" + current_column);
				else if (current_type == "PARAMS") enable_item("PARAMS_" + current_column);
				else if (current_type == "M_D")
				{
					enable_item("M_VALUE_" + current_column);
					enable_item("D_VALUE_" + current_column);
				}
			}

			function login_picked(this_item)
			{
				if (this_item == "Windows Login")
				{
					document.getElementById("server_ip").style.display = "none";
					document.getElementById("username").style.display = "none";
					document.getElementById("password").style.display = "none";
				}
				else
				{
					document.getElementById("server_ip").style.display = "inline";
					document.getElementById("username").style.display = "inline";
					document.getElementById("password").style.display = "inline";
				}
			}

			function enable_table_rename(this_id)
			{
				document.getElementById("table_list_edit_" + this_id).style.display = "none";
				document.getElementById("table_list_" + this_id).style.display = "none";
				document.getElementById("table_list_accept_edit_" + this_id).style.display = "inline-block";
				document.getElementById("table_list_text_" + this_id).style.display = "inline";
			}

			function enable_row_edit(this_id, fields_no)
			{
				document.getElementById("row_list_edit_" + this_id).style.display = "none";
				document.getElementById("row_list_accept_edit_" + this_id).style.display = "inline-block";

				var size = parseInt(fields_no);

				for (var i = 0; i < size; i++)
				{
					document.getElementById("row_list_" + this_id + "_" + i.toString()).style.display = "none";
					document.getElementById("row_list_text_" + this_id + "_" + i.toString()).style.display = "inline";
				}
			}

			function enable_column_edit(this_id)
			{
				document.getElementById("column_list_edit_" + this_id).style.display = "none";
				document.getElementById("column_list_" + this_id).style.display = "none";
				document.getElementById("column_list_accept_edit_" + this_id).style.display = "inline-block";
				document.getElementById("column_list_text_" + this_id).style.display = "inline";
			}

		</script>
    </head>

    <body style="background-color:#f6f6f6;">
        <div class="top" style="min-width:1000px;">
	        <div style="height:90px;background-color: #272727;">
		        <div style="width:88%;margin:auto;">

			        <div style="float:left;color:#FFFFFF;height:90px;line-height:90px;"><img src="app_logo.png" width="300" height="50" style="vertical-align:middle;"/></div>

			        <div style="float:right;line-height: 90px;">
                        <div id="database_view" class="sky_blue_button pure-button" runat="server" onclick="__doPostBack('database_view', '')" style="visibility:hidden;display:inline;">Database List</div>
                        <div id="table_view" class="sky_blue_button pure-button" runat="server" style="display:none;">Table List</div>
			        </div>

		        </div>
	        </div>
        </div>

        <div style="margin-bottom:160px;"></div>

        <form id="basic_form" runat="server" style="margin-bottom:30px;min-width:1000px;">
	        <div runat="server" id="container">

                <div style="text-align:center;white-space:nowrap;">

                    <select style="margin-right:10px;width:140px;padding:5px;" runat="server" id="sql_database_type" name="sql_database_type">
                        <option value="MySQL">MySQL</option><option value="SQL Server">SQL Server</option>
                    </select>

                    <select style="margin-right:10px;width:140px;padding:5px;" runat="server" id="authentication_type" name="authentication_type" onchange="login_picked(this.value)">
                        <option value="Manual Login">Manual Login</option><option value="Windows Login">Windows Login</option>
                    </select>
                    <input placeholder="Database Server" runat="server" id="server_ip" name="server_ip" style="margin-right:10px;width:120px;padding:5px;display:inline;"/>
                    <input placeholder="User Name" runat="server" id="username" name="username" style="margin-right:10px;width:120px;padding:5px;display:inline;"/>
                    <input placeholder="Password" runat="server" id="password" name="password" style="margin-right:10px;width:120px;padding:5px;display:inline;"/>
                    <div id="create_button" class="green_button pure-button" runat="server" onclick="__doPostBack('login', '')" style="width:140px;padding:6px;">Login</div>
                </div>

                <div style="margin-top:30px;margin-bottom:30px;text-align:center;white-space:normal;word-wrap: break-word;" id="event_label" runat="server">
                    <div style="text-align:center;padding: 25px;color: white;background-color:#1cb841;">Select Windows Login to login using Windows Authentication.</div>
                </div>

                <div style="overflow-y:auto;">
                    <div runat="server" id="data_div" style="display:inline;"></div>
                </div>
            </div>
	    </form>
    </body>
</html>
