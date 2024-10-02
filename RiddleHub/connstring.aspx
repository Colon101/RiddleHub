<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="connstring.aspx.cs" Inherits="RiddleHub.connstring" %>

    <!DOCTYPE html>

    <html xmlns="http://www.w3.org/1999/xhtml">

    <head runat="server">
        <title></title>
    </head>

    <body>
        <form id="form1" runat="server">
            <div>
                <div id="text">
                    <asp:Literal ID="connString" runat="server"></asp:Literal>
                </div>
            </div>
            <div id="outcome">
                hi
            </div>
        </form>
        <button id="copyButton">
            Copy
        </button>
        <script>
            let connString = document.getElementById("text").textContent.replace("\n", "").trim()
            document.getElementById("copyButton").addEventListener("click", (e) => {
                navigator.clipboard.writeText(connString).then(() => {
                    document.getElementById("outcome").textContent = "Copied!"
                }).catch(err => {
                    document.getElementById("outcome").textContent = "Error:" + err;
                })
            })
        </script>
    </body>

    </html>