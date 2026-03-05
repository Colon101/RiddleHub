<%@ Application Language="C#" %>
<script runat="server">
    protected void Application_BeginRequest(object sender, EventArgs e)
    {
        string path = Request.AppRelativeCurrentExecutionFilePath;
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        if (path == "~/")
        {
            Context.RewritePath("~/index.aspx");
            return;
        }

        if (path.Contains("."))
        {
            return;
        }

        string trimmed = path.TrimEnd('/');
        if (trimmed == "~")
        {
            Context.RewritePath("~/index.aspx");
            return;
        }

        string rewritten = trimmed + ".aspx";
        string physical = Server.MapPath(rewritten);

        if (System.IO.File.Exists(physical))
        {
            Context.RewritePath(rewritten);
        }
    }
</script>
