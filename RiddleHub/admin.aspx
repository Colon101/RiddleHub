<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="admin.aspx.cs" Inherits="RiddleHub.admin" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>RiddleHub Admin</title>
    <style>
        body {
            margin: 0;
            padding: 20px;
            font-family: Arial, sans-serif;
            background: #f2f4f7;
            color: #1f2937;
        }

        .layout {
            max-width: 1200px;
            margin: 0 auto;
        }

        .card {
            background: #fff;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
            padding: 16px;
            margin-bottom: 16px;
        }

        h1, h2 {
            margin: 0 0 12px 0;
        }

        h1 {
            font-size: 28px;
        }

        h2 {
            font-size: 20px;
        }

        .status {
            padding: 10px 12px;
            border-radius: 8px;
            margin-bottom: 12px;
            background: #eef2ff;
            border: 1px solid #c7d2fe;
            white-space: pre-wrap;
        }

        .controls {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
            align-items: center;
        }

        .controls form {
            margin: 0;
        }

        button {
            border: none;
            border-radius: 8px;
            padding: 8px 12px;
            cursor: pointer;
            background: #111827;
            color: #fff;
        }

        button.warn {
            background: #991b1b;
        }

        input[type="text"], input[type="number"], input[type="password"], textarea, select {
            border: 1px solid #cbd5e1;
            border-radius: 8px;
            padding: 8px 10px;
            font-size: 14px;
        }

        textarea {
            width: 100%;
            min-height: 120px;
            resize: vertical;
            font-family: Consolas, Monaco, monospace;
        }

        .table-wrap {
            overflow-x: auto;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            font-size: 13px;
        }

        th, td {
            text-align: left;
            padding: 8px;
            border-bottom: 1px solid #e5e7eb;
            vertical-align: top;
        }

        th {
            background: #f8fafc;
        }

        .muted {
            color: #64748b;
        }

        .summary-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
            gap: 10px;
        }

        .summary-box {
            background: #f8fafc;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 12px;
        }

        .summary-box .label {
            font-size: 12px;
            color: #64748b;
        }

        .summary-box .value {
            font-size: 24px;
            font-weight: 700;
            margin-top: 4px;
        }
    </style>
</head>
<body>
    <div class="layout">
        <% if (!IsAdminConfigured) { %>
            <div class="card">
                <h1>RiddleHub Admin Panel</h1>
                <p class="muted">Admin password is not configured.</p>
                <%= StatusHtml %>
                <p class="muted">Set <code>RIDDLEHUB_ADMIN_PASSWORD</code> in environment variables or in a <code>.env</code> file.</p>
            </div>
        <% } else if (!IsAdminAuthenticated) { %>
            <div class="card" style="max-width: 460px; margin: 60px auto;">
                <h1>Admin Login</h1>
                <p class="muted">Enter admin password from <code>.env</code>.</p>
                <%= StatusHtml %>
                <form method="post" action="admin">
                    <input type="hidden" name="action" value="admin_login" />
                    <div class="controls" style="gap: 10px; align-items: center;">
                        <label for="adminPassword">Password:</label>
                        <input id="adminPassword" type="password" name="admin_password" required="required" />
                        <button type="submit">Login</button>
                    </div>
                </form>
            </div>
        <% } else { %>
            <div class="card">
                <h1>RiddleHub Admin Panel</h1>
                <p class="muted">Standalone admin page. Not part of iframe navigation flow.</p>
                <%= StatusHtml %>
                <div class="controls">
                    <form method="post" action="admin">
                        <input type="hidden" name="action" value="refresh" />
                        <button type="submit">Refresh</button>
                    </form>
                    <form method="post" action="admin">
                        <input type="hidden" name="action" value="seed_demo" />
                        <button type="submit">Seed Demo Data</button>
                    </form>
                    <form method="post" action="admin">
                        <input type="hidden" name="action" value="clear_riddles" />
                        <button class="warn" type="submit">Delete All Riddles</button>
                    </form>
                    <form method="post" action="admin">
                        <input type="hidden" name="action" value="clear_all" />
                        <button class="warn" type="submit">Reset Users + Riddles</button>
                    </form>
                    <form method="post" action="admin">
                        <input type="hidden" name="action" value="admin_logout" />
                        <button type="submit">Logout</button>
                    </form>
                </div>
            </div>

            <div class="card">
                <h2>Summary</h2>
                <%= SummaryHtml %>
            </div>

            <div class="card">
                <h2>Users</h2>
                <form method="post" action="admin" class="controls" style="margin-bottom: 10px;">
                    <input type="hidden" name="action" value="delete_user" />
                    <label for="deleteUser">Delete username:</label>
                    <input id="deleteUser" type="text" name="username" required="required" />
                    <button class="warn" type="submit">Delete User</button>
                </form>
                <%= UsersTableHtml %>
            </div>

            <div class="card">
                <h2>Riddles</h2>
                <form method="post" action="admin" class="controls" style="margin-bottom: 10px;">
                    <input type="hidden" name="action" value="delete_riddle" />
                    <label for="deleteRiddle">Delete riddle id:</label>
                    <input id="deleteRiddle" type="number" name="riddle_id" min="1" required="required" />
                    <button class="warn" type="submit">Delete Riddle</button>
                </form>
                <%= RiddlesTableHtml %>
            </div>

            <div class="card">
                <h2>SQL Runner</h2>
                <form method="post" action="admin">
                    <input type="hidden" name="action" value="run_sql" />
                    <div class="controls" style="margin-bottom: 10px;">
                        <label for="sqlMode">Mode:</label>
                        <select id="sqlMode" name="sql_mode">
                            <option value="query" <%= SqlMode == "query" ? "selected=\"selected\"" : "" %>>Query (SELECT)</option>
                            <option value="exec" <%= SqlMode == "exec" ? "selected=\"selected\"" : "" %>>Execute (INSERT/UPDATE/DELETE)</option>
                        </select>
                        <button type="submit">Run SQL</button>
                    </div>
                    <textarea name="sql" placeholder="SELECT TOP 20 * FROM dbo.[riddle] ORDER BY riddle_id DESC;"><%= EncodedSqlInput %></textarea>
                </form>
                <div style="margin-top: 10px;">
                    <%= SqlResultHtml %>
                </div>
            </div>
        <% } %>
    </div>
</body>
</html>
